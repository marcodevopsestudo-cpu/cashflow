resource "random_string" "suffix" {
  length  = 6
  upper   = false
  special = false
}
resource "azurerm_key_vault" "this" {
  name = substr(
    lower(
      replace("${random_string.suffix.result}-${var.name}", "_", "-")
    ),
    0,
    24
  )

  location            = var.location
  resource_group_name = var.resource_group_name
  tenant_id           = var.tenant_id
  sku_name            = "standard"

  soft_delete_retention_days = 7
  purge_protection_enabled   = true

  enable_rbac_authorization = true

  tags = var.tags
}


resource "azurerm_role_assignment" "kv_admin" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.current_principal_object_id
}
