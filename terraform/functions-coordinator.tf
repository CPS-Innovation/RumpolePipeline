#################### Functions ####################

resource "azurerm_function_app" "fa_coordinator" {
  name                       = "fa-${local.resource_name}-coordinator"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.asp.id 
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  os_type                    = "linux"
  version                    = "~4"
  app_settings = {
    "AzureWebJobsStorage"                     = azurerm_storage_account.sa.primary_connection_string
    "FUNCTIONS_WORKER_RUNTIME"                = "dotnet"
    "StorageConnectionAppSetting"             = azurerm_storage_account.sa.primary_connection_string 
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
    "PdfGeneratorUrl"                         = "https://fa-${local.resource_name}-pdf-generator.azurewebsites.net/api/generate?code=${data.azurerm_function_app_host_keys.ak_pdf_generator.default_function_key}"
    "PdfGeneratorScope"                       = "api://fa-${local.resource_name}-pdf-generator/.default"
    "TextExtractorUrl"                        = "https://fa-${local.resource_name}-text-extractor.azurewebsites.net/api/extract?code=${data.azurerm_function_app_host_keys.ak_text_extractor.default_function_key}"
    "TextExtractorScope"                      = "api://fa-${local.resource_name}-text-extractor/.default"
    "OnBehalfOfTokenTenantId"                 = data.azurerm_client_config.current.tenant_id
    "OnBehalfOfTokenClientId"                 = azuread_application.fa_coordinator.application_id
    "OnBehalfOfTokenClientSecret"             = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_fa_coordinator_client_secret.id})"
    "CoordinatorOrchestratorTimeoutSecs"      = "600"
    "CallingAppTenantId"                      = data.azurerm_client_config.current.tenant_id
    "CallingAppValidAudience"                 = var.auth_details.coordinator_valid_audience
    "CallingAppValidScopes"                   = var.auth_details.coordinator_valid_scopes
    "CallingAppValidRoles"                    = var.auth_details.coordinator_valid_roles
  }
  https_only                 = true

  site_config {
    always_on      = true
    ip_restriction = []
    ftps_state     = "FtpsOnly"
    http2_enabled = true

    cors {
      allowed_origins = []
    }
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }

  # depends_on = [
  #   data.azurerm_function_app_host_keys.ak_pdf_generator,
  #   data.azurerm_function_app_host_keys.ak_text_extractor,
  #   data.azurerm_function_app_host_keys.ak_indexer
  # ]
}

data "azurerm_function_app_host_keys" "ak_coordinator" {
  name                = "fa-${local.resource_name}-coordinator"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_function_app.fa_coordinator]
}

resource "azuread_application" "fa_coordinator" {
  display_name               = "fa-${local.resource_name}-coordinator"
  identifier_uris            = ["api://fa-${local.resource_name}-coordinator"]

  api {
      oauth2_permission_scope {
        admin_consent_description  = "Allow an application to access function app on behalf of the signed-in user."
        admin_consent_display_name = "Access function app"
        enabled                    = true
        id                         = var.coordinator_details.user_impersonation_scope_id
        type                       = "Admin"
        value                      = "user_impersonation"
    }
  }
  
  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph
    
    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # read user
      type = "Scope"
    }
  }

  required_resource_access {
    resource_app_id = var.pdf_generator_details.application_registration_id # Pdf Generator

    resource_access {
      id   = var.pdf_generator_details.user_impersonation_scope_id # user impersonation
      type = "Scope"
    }

    resource_access {
      id   = var.pdf_generator_details.application_create_role_id # pdf generator role
      type = "Role"
    }
  }

  required_resource_access {
    resource_app_id = var.text_extractor_details.application_registration_id # Text Extractor

    resource_access {
      id   = var.text_extractor_details.application_text_extraction_role_id # text extraction role
      type = "Role"
    }
  }
  
  web {
    redirect_uris = [
      "https://fa-${local.resource_name}-coordinator.azurewebsites.net/.auth/login/aad/callback",
      "https://getpostman.com/oauth2/callback"
    ]

    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }
}

resource "azuread_application_pre_authorized" "fapre_fa_coordinator" {
  application_object_id = azuread_application.fa_coordinator.id
  authorized_app_id     = var.gateway_details.application_registration_id
  permission_ids        = [var.coordinator_details.user_impersonation_scope_id]
}

resource "azuread_application_password" "faap_fa_coordinator_app_service" {
  application_object_id = azuread_application.fa_coordinator.id
  end_date_relative     = "17520h"

  depends_on = [
    azuread_application.fa_coordinator
  ]
}
