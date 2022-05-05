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

    "ocrService__ServiceUrl"                   = azurerm_cognitive_account.computer_vision_service.endpoint
    "ocrService__ServiceKey"                   = azurerm_cognitive_account.computer_vision_service.primary_access_key

    "searchDataStorage__EndpointUrl"           = azurerm_cosmosdb_account.cdba.endpoint
    "searchDataStorage__AuthorizationKey"      = azurerm_cosmosdb_account.cdba.primary_key
    "searchDataStorage__DatabaseName"          = azurerm_cosmosdb_sql_database.cdb.name
    "searchDataStorage__ContainerName"         = azurerm_cosmosdb_sql_container.cdbdc.name
    "searchDataStorage__Enabled"               = "false"

    "searchDataIndex__EndpointUrl"             = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "searchDataIndex__AuthorizationKey"        = azurerm_search_service.ss.primary_key
    "searchDataIndex__IndexName"               = jsondecode(file("search-index-definition.json")).name
    "searchDataIndex__Enabled"                 = "true"
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

data "azurerm_function_app_host_keys" "ak_text_extractor" {
  name                = "fa-${local.resource_name}-text-extractor"
  resource_group_name = azurerm_resource_group.rg.name
    depends_on = [azurerm_function_app.fa_text_extractor]
}