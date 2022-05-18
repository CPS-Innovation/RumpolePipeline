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
    "AzureWebJobsStorage"                     = azurerm_storage_account.sa.primary_connection_string
    "FUNCTIONS_WORKER_RUNTIME"                = "dotnet"
    "StorageConnectionAppSetting"             = azurerm_storage_account.sa.primary_connection_string 
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
    "BlobServiceUrl"                          = "https://sacps${var.env != "prod" ? var.env : ""}rumpolepipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                = "documents"
    "StubBlobStorageConnectionString"         = var.stub_blob_storage_connection_string,
    "AuthorizationClaim"                      = "application.create" //TODO reference app role value
  }
  site_config {
    always_on      = true
    ip_restriction = []
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
      id_token_issuance_enabled     = true
    }
  }

  app_role {
    allowed_member_types  = ["Application", "User"]
    description          = "Creators have the ability to create resources"
    display_name         = "Create"
    enabled              = true
    id                   = "86CD7E91-7949-47EB-A148-9B81C249C55C"
    value                = "application.create"
  }
}

data "azurerm_function_app_host_keys" "ak_pdf_generator" {
  name                = "fa-${local.resource_name}-pdf-generator"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_function_app.fa_pdf_generator]
}
