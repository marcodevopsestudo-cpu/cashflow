
resource "azurerm_role_assignment" "this" {
  scope                = var.service_scope
  role_definition_name = var.role_definition_name
  principal_id         = var.principal_id
}

