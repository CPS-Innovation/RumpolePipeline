#################### Resource Group ####################

resource "azurerm_resource_group" "rg" {
  name     = "rg-${local.resource_name}"
  location = "UK South"
}

resource "azurerm_log_analytics_workspace" "law" {
  name                = var.default_workspace
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}
