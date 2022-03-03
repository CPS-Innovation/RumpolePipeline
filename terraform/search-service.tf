resource "azurerm_search_service" "ss" {
    name                  = "ss-${local.resource_name}"
    resource_group_name   = azurerm_resource_group.rg.name
    location              = azurerm_resource_group.rg.location
    sku                   = "basic" # just temporary - probably way too low!
}