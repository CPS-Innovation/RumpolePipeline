#################### Functions ####################

resource "azurerm_function_app" "fa_text_extractor" {
  name                       = "fa-${local.resource_name}-text-extractor"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.asp.id
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  os_type                    = "linux"
  version                    = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                = "dotnet"
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
    "AzureWebJobsStorage"                     = azurerm_storage_account.sa.primary_connection_string
    "BlobServiceContainerName"                = azurerm_storage_container.container.name
    "BlobExpirySecs"                          = 3600
    "BlobUserDelegationKeyExpirySecs"         = 3600
    "BlobServiceUrl"                          = azurerm_storage_account.sa.primary_blob_endpoint
    "CallingAppTenantId"                      = data.azurerm_client_config.current.tenant_id
    "CallingAppValidAudience"                 = "api://fa-${local.resource_name}-text-extractor"
    "ComputerVisionClientServiceKey"          = azurerm_cognitive_account.computer_vision_service.primary_access_key
    "ComputerVisionClientServiceUrl"          = azurerm_cognitive_account.computer_vision_service.endpoint
    "SearchClientAuthorizationKey"            = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                 = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                   = jsondecode(file("search-index-definition.json")).name
    "IsRunningLocally"                        = "false"
  }
  https_only                 = true

  site_config {
    always_on      = true
    ip_restriction = []
    ftps_state     = "FtpsOnly"
    http2_enabled = true
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "RedirectToLoginPage"
    default_provider              = "AzureActiveDirectory"
    active_directory {
      client_id         = azuread_application.fa_text_extractor.application_id
      client_secret     = azuread_application_password.faap_fa_text_extractor_app_service.value
      allowed_audiences = ["api://fa-${local.resource_name}-text-extractor"]
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }
}

resource "azuread_application" "fa_text_extractor" {
  display_name               = "fa-${local.resource_name}-text-extractor"
  identifier_uris            = ["api://fa-${local.resource_name}-text-extractor"]

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # read user
      type = "Scope"
    }
  }

  web {
    redirect_uris = ["https://fa-${local.resource_name}-text-extractor.azurewebsites.net/.auth/login/aad/callback"]

    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }
}

resource "azuread_application_app_role" "fa_text_extractor_app_role" {
  application_object_id = azuread_application.fa_text_extractor.id
  allowed_member_types  = ["Application"]
  description           = "Can parse document texts using the ${local.resource_name} Polaris Text Extractor"
  display_name          = "Parse document texts in ${local.resource_name}"
  is_enabled            = true
  value                 = "application.extracttext"
}

resource "azuread_service_principal" "fa_text_extractor" {
  application_id = azuread_application.fa_text_extractor.application_id
}

data "azurerm_function_app_host_keys" "ak_text_extractor" {
  name                = "fa-${local.resource_name}-text-extractor"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_function_app.fa_text_extractor]
}

resource "azuread_application_password" "faap_fa_text_extractor_app_service" {
  application_object_id = azuread_application.fa_text_extractor.id
  end_date_relative     = "17520h"

  depends_on = [azuread_application.fa_text_extractor]
}