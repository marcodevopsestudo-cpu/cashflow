output "client_id" {
  value = azuread_application.this.client_id
}

output "object_id" {
  value = azuread_application.this.object_id
}

output "service_principal_object_id" {
  value = azuread_service_principal.this.object_id
}

output "identifier_uri" {
  value = var.identifier_uri
}

output "oauth2_scope_id" {
  value = random_uuid.scope_id.result
}

output "oauth2_scope_value" {
  value = var.scope_value
}

output "app_role_id" {
  value = random_uuid.app_role_id.result
}
