#################### Functions ####################

resource "azurerm_function_app" "fa_indexer" {
  name                       = "fa-${local.resource_name}-indexer"
  enabled                    = false
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
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
    "searchDataStorage__EndpointUrl"           = azurerm_cosmosdb_account.cdba.endpoint
    "searchDataStorage__AuthorizationKey"      = azurerm_cosmosdb_account.cdba.primary_key
    "searchDataStorage__DatabaseName"          = azurerm_cosmosdb_sql_database.cdb.name
    "searchDataStorage__ContainerName"         = azurerm_cosmosdb_sql_container.cdbdc.name

    "searchDataIndex__EndpointUrl"             = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "searchDataIndex__AuthorizationKey"        = azurerm_search_service.ss.primary_key
    "searchDataIndex__IndexName"               = jsondecode(file("search-index-definition.json")).name

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

data "azurerm_function_app_host_keys" "ak_indexer" {
  name                = "fa-${local.resource_name}-indexer"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_function_app.fa_indexer]
}

# resource "azuread_application" "fa_indexer" {
#   display_name               = "fa-${local.resource_name}-indexer"
#   #oauth2_allow_implicit_flow = false
#   #identifier_uris            = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-coordinator"]
#   #owners                     = ["4acc9fb2-3e32-4109-b3d1-5fcd3a253e4e"]
#   #reply_urls                 = ["https://fa-${local.resource_name}-coordinator.azurewebsites.net/.auth/login/aad/callback"]

#   # Please note: oauth2_permissions with a user impersonation value is created by default by Terraform
#   # Creating another causes a duplication error

#   # required_resource_access {
#   #   resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

#   #   resource_access {
#   #     id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # read user
#   #     type = "Scope"
#   #   }

#   #   resource_access {
#   #     id   = "5f8c59db-677d-491f-a6b8-5f174b11ec1d" # read all groups (requires admin consent?)
#   #     type = "Scope"
#   #   }
#   # }
# }