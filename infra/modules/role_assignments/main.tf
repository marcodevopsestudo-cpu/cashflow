resource "azurerm_role_assignment" "service_bus_sender" {
  scope                = var.service_bus_scope
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = var.principal_id
}

resource "azurerm_role_assignment" "storage_blob_data_contributor" {
  scope                = var.storage_scope
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.principal_id
}

resource "azurerm_role_assignment" "kv_admin" {
  scope                =var.key_vault_scope
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.principal_id
}
