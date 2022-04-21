resource "azurerm_storage_account" "sa" {
  name                = "sacps${var.env != "prod" ? var.env : ""}rumpolepipeline"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  account_kind              = "StorageV2"
  account_replication_type  = "LRS"
  account_tier              = "Standard"
  enable_https_traffic_only = true

  network_rules {
    default_action = "Allow"
  }
}

resource "azurerm_storage_container" "container" {
  name                  = "documents"
  storage_account_name  = azurerm_storage_account.sa.name
  container_access_type = "private"
}