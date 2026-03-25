output "resource_group_name" {
  value = module.rg.name
}

output "function_app_name" {
  value = module.function.name
}

output "function_app_principal_id" {
  value = module.function.principal_id
}

output "function_default_hostname" {
  value = module.function.default_hostname
}

output "consolidation_function_app_name" {
  value = module.consolidation_function.name
}

output "consolidation_function_app_principal_id" {
  value = module.consolidation_function.principal_id
}

output "consolidation_function_default_hostname" {
  value = module.consolidation_function.default_hostname
}

output "key_vault_name" {
  value = module.key_vault.name
}

output "postgres_server_fqdn" {
  value = module.postgres.fqdn
}

output "service_bus_namespace" {
  value = module.service_bus.namespace_name
}

output "service_bus_topic_name" {
  value = module.service_bus.topic_name
}

output "service_bus_subscription_names" {
  value = module.service_bus.subscription_names
}

output "entra_api_client_id" {
  value = module.entra_api.client_id
}

output "entra_api_service_principal_object_id" {
  value = module.entra_api.service_principal_object_id
}

output "transaction_service_access_role_id" {
  value = module.entra_api.app_role_id
}

output "github_oidc_client_id" {
  value = module.github_oidc.client_id
}

output "github_oidc_tenant_id" {
  value = var.tenant_id
}

output "github_oidc_subscription_id" {
  value = var.subscription_id
}

output "entra_api_identifier_uri" {
  value = module.entra_api.identifier_uri
}

output "entra_api_scope_value" {
  value = module.entra_api.oauth2_scope_value
}

output "postman_client_id" {
  value = module.entra_postman_client_app.client_id
}

output "postman_client_secret" {
  value     = module.entra_postman_client_app.client_secret
  sensitive = true
}
