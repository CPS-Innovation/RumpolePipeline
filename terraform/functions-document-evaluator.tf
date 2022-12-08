#################### Functions ####################

resource "azurerm_function_app" "fa_document_evaluator" {
  name                       = "fa-${local.resource_name}-document-evaluator"
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
    "BlobServiceUrl"                          = azurerm_storage_account.sa.primary_blob_endpoint
    "BlobServiceContainerName"                = azurerm_storage_container.container.name
    "SearchClientAuthorizationKey"            = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                 = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                   = jsondecode(file("search-index-definition.json")).name
    "UpdateSearchIndexQueueUrl"               = "https://sacps${local.resource_name}rumpolepipeline.queue.core.windows.net/{0}"
    "EvaluateExistingDocumentQueueName"       = var.queue_config.evaluate_existing_documents_queue_name
    "UpdateSearchIndexByVersionQueueName"     = var.queue_config.update_search_index_by_version_queue_name
    "UpdateSearchIndexByBlobNameQueueName"    = var.queue_config.update_search_index_by_blob_name_queue_name
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

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }
}