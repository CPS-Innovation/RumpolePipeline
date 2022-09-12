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
    "AzureWebJobsStorage"                     = azurerm_storage_account.sa.primary_connection_string
    "FUNCTIONS_WORKER_RUNTIME"                = "dotnet"
    "StorageConnectionAppSetting"             = azurerm_storage_account.sa.primary_connection_string 
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
    "computerVisionClient__ServiceUrl"         = azurerm_cognitive_account.computer_vision_service.endpoint
    "computerVisionClient__ServiceKey"         = azurerm_cognitive_account.computer_vision_service.primary_access_key
    "searchClient__EndpointUrl"                = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "searchClient__AuthorizationKey"           = azurerm_search_service.ss.primary_key
    "searchClient__IndexName"                  = jsondecode(file("search-index-definition.json")).name
    "BlobServiceClientUrl"                     = azurerm_storage_account.sa.primary_blob_endpoint
    "blob__BlobContainerName"                  = azurerm_storage_container.container.name
    "blob__BlobExpirySecs"                     = 3600
    "blob__UserDelegationKeyExpirySecs"        = 3600
    "CallingAppTenantId"                      = data.azurerm_client_config.current.tenant_id
    "CallingAppValidAudience"                 = var.auth_details.text_extractor_valid_audience
    "CallingAppValidScopes"                   = var.auth_details.text_extractor_valid_scopes
    "CallingAppValidRoles"                    = var.auth_details.text_extractor_valid_roles
  }
  https_only                 = true

  auth_settings {
    enabled = true
  }

  site_config {
    always_on      = true
    ip_restriction = []
    ftps_state     = "FtpsOnly"
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

  app_role {
    allowed_member_types  = ["Application"]
    description          = "Readers have the ability to read resources"
    display_name         = "Read"
    enabled              = true
    id                   = var.text_extractor_details.application_text_extraction_role_id
    value                = "application.extracttext"
  }
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

  depends_on = [
    azuread_application.fa_text_extractor
  ]
}