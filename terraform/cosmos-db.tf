#################### Cosmos Db ####################

resource "azurerm_cosmosdb_account" "cdba" {
  name                = "cdb-${local.resource_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  key_vault_key_id    = azurerm_key_vault_key.kvap_cmos_customer_managed_key.versionless_id
  default_identity_type = "SystemAssignedIdentity"
  capabilities {
    name = "EnableServerless"
  }
  consistency_policy {
    consistency_level       = "BoundedStaleness"
    max_interval_in_seconds = 10
    max_staleness_prefix    = 200
  }

  geo_location {
    //prefix            = "cdb-${local.resource_name}-geoloc"
    location          = azurerm_resource_group.rg.location
    failover_priority = 0
  }

  identity {
    type="SystemAssigned"
  }
  
  depends_on = [
    azurerm_role_assignment.kv_role_cmos_kvcseu
  ]
}

resource "azurerm_cosmosdb_sql_database" "cdb" {
  name                = "cdb-rumpole-pipeline"
  resource_group_name = azurerm_cosmosdb_account.cdba.resource_group_name
  account_name        = azurerm_cosmosdb_account.cdba.name
  #throughput          = 400  # do not use if serverless
}

resource "azurerm_cosmosdb_sql_container" "cdbdc" {
  name                = "documents"
  resource_group_name = azurerm_cosmosdb_account.cdba.resource_group_name
  account_name        = azurerm_cosmosdb_account.cdba.name
  database_name       = azurerm_cosmosdb_sql_database.cdb.name
  partition_key_path  = "/caseId"
  default_ttl = 157680000 // 5 years
}
