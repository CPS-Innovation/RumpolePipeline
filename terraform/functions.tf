#################### Functions ####################

resource "azurerm_function_app" "fa_coordinator" {
  name                       = "fa-${local.resource_name}-coordinator"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.asp.id 
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  os_type                    = "linux"
  version                    = "~3"
  app_settings = {
    "AzureWebJobsStorage"                     = azurerm_storage_account.sa.primary_connection_string
    "FUNCTIONS_WORKER_RUNTIME"                = "dotnet"
    "StorageConnectionAppSetting"             = azurerm_storage_account.sa.primary_connection_string 
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.ai.instrumentation_key
    "OnBehalfOfTokenTenantId"                 = data.azurerm_client_config.current.tenant_id
    "OnBehalfOfTokenClientId"                 = azuread_application.fa_coordinator.application_id
    "OnBehalfOfTokenClientSecret"             = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_fa_coordinator_client_secret.versionless_id})"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
  }
  site_config {
    always_on      = true
    ip_restriction = []
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
      client_id         = azuread_application.fa_coordinator.application_id
      client_secret     = azuread_application_password.faap_fa_coordinator_app_service.value
      allowed_audiences = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-coordinator"]
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }

  depends_on = [
    azurerm_key_vault_secret.kvs_fa_coordinator_client_secret
  ]
}

resource "azuread_application" "fa_coordinator" {
  display_name               = "fa-${local.resource_name}-coordinator"
  #oauth2_allow_implicit_flow = false
  identifier_uris            = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-coordinator"]
  owners                     = ["4acc9fb2-3e32-4109-b3d1-5fcd3a253e4e"]
  #reply_urls                 = ["https://fa-${local.resource_name}-coordinator.azurewebsites.net/.auth/login/aad/callback"]

  # Please note: oauth2_permissions with a user impersonation value is created by default by Terraform
  # Creating another causes a duplication error

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # read user
      type = "Scope"
    }

    resource_access {
      id   = "5f8c59db-677d-491f-a6b8-5f174b11ec1d" # read all groups (requires admin consent?)
      type = "Scope"
    }
  }
}

resource "azuread_application_password" "faap_fa_coordinator_app_service" {
  application_object_id = azuread_application.fa_coordinator.id
  #description           = "Default function app password"
  end_date_relative     = "17520h"
  #value                 = "__faap_rumpole_pipeline_app_service_password__"

  depends_on = [
    azuread_application.fa_coordinator
  ]
}
