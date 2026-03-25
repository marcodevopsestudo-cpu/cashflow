output "client_id" { value = azuread_application.this.client_id }
output "application_object_id" { value = azuread_application.this.object_id }
output "service_principal_object_id" { value = azuread_service_principal.this.object_id }
output "federated_identity_credential" {value = azuread_application_federated_identity_credential.main.id}
