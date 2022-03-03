#################### Key Vault ####################

resource "azurerm_key_vault" "kv" {
  name                = "kv-${local.resource_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  sku_name = "standard"
}

resource "azurerm_key_vault_access_policy" "kvap_fa_coordinator" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_function_app.fa_coordinator.identity[0].principal_id

  secret_permissions = [
    "Get",
  ]
}

resource "azurerm_key_vault_access_policy" "kvap_terraform_sp" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = "4acc9fb2-3e32-4109-b3d1-5fcd3a253e4e"#data.azuread_service_principal.terraform_service_principal.object_id

  secret_permissions = [
    "Get",
    "Set"
  ]
}

# resource "azurerm_key_vault_secret" "kvs_rumpole_pipeline_as_client_secret" {
#   name         = "AppServiceRegistrationClientSecret"
#   value        = azuread_application_password.asap_web_rumpole_app_service.value
#   key_vault_id = azurerm_key_vault.kv.id
#   depends_on = [
#     azurerm_key_vault_access_policy.kvap_terraform_sp
#   ]
# }

resource "azurerm_key_vault_secret" "kvs_fa_coordinator_client_secret" {
  name         = "FunctionAppRegistrationClientSecret"
  value        = azuread_application_password.faap_fa_coordinator_app_service.value
  key_vault_id = azurerm_key_vault.kv.id
  depends_on = [
    azurerm_key_vault_access_policy.kvap_terraform_sp
  ]
}
