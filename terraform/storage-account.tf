resource "azurerm_storage_account" "sa" {
  name                = "sacps${var.env != "prod" ? var.env : ""}rumpolepipeline"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  account_kind              = "StorageV2"
  account_replication_type  = "RAGRS"
  account_tier              = "Standard"
  enable_https_traffic_only = true

  network_rules {
    default_action = "Allow"
  }
}
