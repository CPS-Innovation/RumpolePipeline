#################### Functions ####################

resource "azurerm_function_app" "fa_pdf_generator" {
  name                       = "fa-${local.resource_name}-pdf-generator"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.aspw.id
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  version                    = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                = "dotnet"
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
    "AzureWebJobsStorage"                     = azurerm_storage_account.sa.primary_connection_string
    "BlobServiceUrl"                          = "https://sacps${var.env != "prod" ? var.env : ""}rumpolepipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                = "documents"
    "CallingAppTenantId"                      = data.azurerm_client_config.current.tenant_id
    "CallingAppValidAudience"                 = "api://fa-${local.resource_name}-pdf-generator"
    "StubBlobStorageConnectionString"         = var.stub_blob_storage_connection_string
    "FeatureFlags_EvaluateDocuments"          = "false"
    "DocumentExtractionBaseUrl"               = ""
    "SearchClientAuthorizationKey"            = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                 = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                   = jsondecode(file("search-index-definition.json")).name
    "FakeCmsDocumentsRepository"              = "cms-documents"
    "FakeCmsDocumentsRepository2"             = "cms-documents-2"
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
      client_id         = azuread_application.fa_pdf_generator.application_id
      client_secret     = azuread_application_password.faap_fa_pdf_generator_app_service.value
      allowed_audiences = ["api://fa-${local.resource_name}-pdf-generator"]
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }
}

resource "azuread_application" "fa_pdf_generator" {
  display_name               = "fa-${local.resource_name}-pdf-generator"
  identifier_uris            = ["api://fa-${local.resource_name}-pdf-generator"]

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # read user
      type = "Scope"
    }
  }

  web {
    redirect_uris = ["https://fa-${local.resource_name}-pdf-generator.azurewebsites.net/.auth/login/aad/callback"]

    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }
}

resource "azuread_application_oauth2_permission_scope" "fa_pdf_generator_scope" {
  application_object_id      = azuread_application.fa_pdf_generator.id
  admin_consent_description  = "Allow the calling application to make requests of the ${local.resource_name} PDF Generator"
  admin_consent_display_name = "Call the ${local.resource_name} PDF Generator"
  is_enabled                 = true
  type                       = "Admin"
  value                      = "user_impersonation"
  user_consent_description   = "Interact with the ${local.resource_name} Polaris PDF Generator on-behalf of the calling user"
  user_consent_display_name  = "Interact with the ${local.resource_name} Polaris PDF Generator"
}

resource "azuread_application_app_role" "fa_pdf_generator_app_role" {
  application_object_id = azuread_application.fa_pdf_generator.id
  allowed_member_types  = ["Application"]
  description           = "Can create PDF resources using the ${local.resource_name} PDF Generator"
  display_name          = "Create PDF resources"
  is_enabled            = true
  value                 = "application.create"
}

resource "azuread_service_principal" "fa_pdf_generator" {
  application_id = azuread_application.fa_pdf_generator.application_id
}

data "azurerm_function_app_host_keys" "ak_pdf_generator" {
  name                = "fa-${local.resource_name}-pdf-generator"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_function_app.fa_pdf_generator]
}

resource "azuread_application_pre_authorized" "fapre_fa_pdf-generator" {
  application_object_id = azuread_application.fa_pdf_generator.id
  authorized_app_id     = data.azuread_application.fa_gateway.application_id
  permission_ids        = [azuread_application_oauth2_permission_scope.fa_pdf_generator_scope.id]
}

resource "azuread_application_pre_authorized" "fapre_fa_pdf-generator2" {
  application_object_id = azuread_application.fa_pdf_generator.id
  authorized_app_id     = azuread_application.fa_coordinator.application_id
  permission_ids        = [azuread_application_oauth2_permission_scope.fa_pdf_generator_scope.id]
}

resource "azuread_application_password" "faap_fa_pdf_generator_app_service" {
  application_object_id = azuread_application.fa_pdf_generator.id
  end_date_relative     = "17520h"

  depends_on = [
    azuread_application.fa_pdf_generator
  ]
}