resource "azurerm_storage_account" "sa" {
  name                = "sacps${var.env != "prod" ? var.env : ""}rumpolepipeline"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  account_kind              = "StorageV2"
  account_replication_type  = "LRS"
  account_tier              = "Standard"
  enable_https_traffic_only = true

  min_tls_version = "TLS1_2"

  network_rules {
    default_action = "Allow"
  }

  identity {
    type = "SystemAssigned"
  }
}

data "azuread_service_principal" "sp_gateway" {
  display_name = "fa-${local.resource_name}-gateway"
}

resource "azurerm_storage_account_customer_managed_key" "rumpole_storage_pipeline_cmk" {
  storage_account_id = azurerm_storage_account.sa.id
  key_vault_id       = azurerm_key_vault.kv.id
  key_name           = azurerm_key_vault_key.kvap_sa_customer_managed_key.name

  depends_on = [
    azurerm_role_assignment.kv_role_client_kvc,
    azurerm_role_assignment.kv_role_sa_kvcseu,
    azurerm_key_vault_key.kvap_sa_customer_managed_key,
    azurerm_storage_account.sa,
    azurerm_role_assignment.kv_role_terraform_sp
  ]
}

resource "azurerm_role_assignment" "ra_blob_delegator_text_extractor" {
  scope                = azurerm_storage_account.sa.id
  role_definition_name = "Storage Blob Delegator"
  principal_id         = azurerm_function_app.fa_text_extractor.identity[0].principal_id
}

resource "azurerm_storage_container" "container" {
  name                  = "documents"
  storage_account_name  = azurerm_storage_account.sa.name
  container_access_type = "private"
}

resource "azurerm_role_assignment" "ra_blob_data_contributor" {
  scope                = azurerm_storage_container.container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_function_app.fa_pdf_generator.identity[0].principal_id
}

resource "azurerm_role_assignment" "ra_blob_data_contributor_text_extractor" {
  scope                = azurerm_storage_container.container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_function_app.fa_text_extractor.identity[0].principal_id
}

resource "azurerm_role_assignment" "ra_blob_data_reader" {
  scope                = azurerm_storage_container.container.resource_manager_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = data.azuread_service_principal.sp_gateway.object_id
}