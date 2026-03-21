locals {
  prefix         = "${var.project_name}-${var.environment}"
  compact_prefix = replace(local.prefix, "-", "")
  function_name  = coalesce(var.function_app_name_override, "func-${local.prefix}-tx")

  default_tags = {
    project     = var.project_name
    environment = var.environment
    managed_by  = "terraform"
    workload    = "transaction-service"
  }

  tags = merge(local.default_tags, var.tags)
}

module "rg" {
  source   = "./modules/resource_group"
  name     = "rg-${local.prefix}"
  location = var.location
  tags     = local.tags
}

module "storage" {
  source              = "./modules/storage_account"
  resource_group_name = module.rg.name
  location            = module.rg.location
  project_name        = var.project_name
  environment         = var.environment
  tags                = local.tags
}

module "plan" {
  source              = "./modules/app_service_plan"
  name                = "asp-${local.prefix}"
  resource_group_name = module.rg.name
  location            = module.rg.location
  os_type             = "Linux"
  sku_name            = "Y1"
  tags                = local.tags
}

module "appinsights" {
  source              = "./modules/application_insights"
  name                = "appi-${local.prefix}"
  resource_group_name = module.rg.name
  location            = module.rg.location
  application_type    = "web"
  tags                = local.tags
}

module "service_bus" {
  source              = "./modules/service_bus"
  name                = substr("sb${local.compact_prefix}", 0, 50)
  topic_name          = var.servicebus_topic_name
  resource_group_name = module.rg.name
  location            = module.rg.location
  tags                = local.tags
}

module "postgres" {
  source                = "./modules/postgresql"
  name                  = substr("psql-${local.prefix}", 0, 63)
  database_name         = "cashflow"
  resource_group_name   = module.rg.name
  location              = module.rg.location
  admin_username        = var.postgres_admin_username
  admin_password        = var.postgres_admin_password
  postgres_version      = "16"
  sku_name              = "B_Standard_B1ms"
  storage_mb            = 32768
  backup_retention_days = 7
  zone                  = "1"
  tags                  = local.tags
  start_ip_address      = var.start_ip_address
  end_ip_address        = var.end_ip_address
}

module "key_vault" {
  source                      = "./modules/key_vault"
  name                        = substr("kv-${local.prefix}", 0, 24)
  resource_group_name         = module.rg.name
  location                    = module.rg.location
  tenant_id                   = var.tenant_id
  current_principal_object_id = data.azurerm_client_config.current.object_id
  tags                        = local.tags
}

module "entra_api" {
  source                         = "./modules/entra_api_app"
  display_name                   = "api-${local.prefix}-transaction-service"
  identifier_uri                 = "api://api-${local.prefix}-transaction-service"
  app_role_display_name          = "Transaction Service Access"
  app_role_value                 = "TransactionService.Access"
  requested_access_token_version = 2
}

module "github_oidc" {
  source          = "./modules/github_oidc_app"
  display_name    = "gh-${local.prefix}-deploy"
  github_owner    = var.github_owner
  github_repo     = var.github_repo
  github_branch   = var.github_branch
  credential_name = "github-${var.github_branch}"
}

resource "azurerm_key_vault_secret" "postgres_connection_string" {
  name         = "postgres-connection-string"
  value        = module.postgres.connection_string
  key_vault_id = module.key_vault.id

  depends_on = [module.key_vault]
}

resource "azurerm_key_vault_secret" "servicebus_connection_string" {
  name         = "servicebus-connection-string"
  value        = module.service_bus.primary_connection_string
  key_vault_id = module.key_vault.id

  depends_on = [module.key_vault]
}

module "function" {
  source                                 = "./modules/function_app"
  name                                   = local.function_name
  resource_group_name                    = module.rg.name
  location                               = module.rg.location
  service_plan_id                        = module.plan.id
  storage_account_name                   = module.storage.name
  storage_account_access_key             = module.storage.primary_access_key
  application_insights_connection_string = module.appinsights.connection_string

  app_settings = {
    # Core
    FUNCTIONS_WORKER_RUNTIME    = "dotnet-isolated"
    FUNCTIONS_EXTENSION_VERSION = "~4"
    ASPNETCORE_ENVIRONMENT      = var.environment

    # REQUIRED
    AzureWebJobsStorage = module.storage.primary_connection_string

    # Service Bus
    ServiceBus__TopicName               = module.service_bus.topic_name
    ServiceBus__FullyQualifiedNamespace = module.service_bus.namespace_fqdn
    ServiceBus__UseManagedIdentity      = "true"

    # (fallback opcional - igual seu local)
    ServiceBus__ConnectionString = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.servicebus_connection_string.versionless_id})"

    # Database
    ConnectionStrings__Postgres = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.postgres_connection_string.versionless_id})"

    # Auth config (igual seu local)
    Authorization__Enabled                = "true"
    Authorization__AllowedAppIds__0       = module.entra_api.client_id
    Authorization__RequiredRoles__0       = "TransactionService.Access"
    Authorization__AllowedAudiences__0    = "api://${module.entra_api.client_id}"
    Authorization__AllowedIssuers__0      = "https://login.microsoftonline.com/${var.tenant_id}/v2.0"

    # Observabilidade
    APPLICATIONINSIGHTS_CONNECTION_STRING = module.appinsights.connection_string

    # Feature flags
    AzureWebJobsFeatureFlags = "EnableWorkerIndexing"
  }

  auth_settings = {
    enabled                = true
    client_id              = module.entra_api.client_id
    tenant_auth_endpoint   = "https://login.microsoftonline.com/${var.tenant_id}/v2.0"
    unauthenticated_action = "Return401"
  }

  key_vault_reference_identity_id = null

  tags = local.tags

  depends_on = [
    azurerm_key_vault_secret.postgres_connection_string,
    azurerm_key_vault_secret.servicebus_connection_string
  ]
}

module "rbac_service_bus_assignment" {
  source                = "./modules/role_assignments"
  principal_id          = module.function.principal_id
  role_definition_name  = "Azure Service Bus Data Sender"
  service_scope         = module.service_bus.id

}
module "rbac_storage_blob_assignment" {
  source                = "./modules/role_assignments"
  principal_id          = module.function.principal_id
  role_definition_name  = "Storage Blob Data Contributor"
  service_scope         = module.storage.id
}
module "rbac_key_vault_assignment" {
  source                = "./modules/role_assignments"
  principal_id          = module.function.principal_id
  role_definition_name = "Key Vault Secrets User"
  service_scope         = module.key_vault.id
}
module "rbac_service_principal_assignment" {
  source                = "./modules/role_assignments"
  principal_id          =  module.github_oidc.service_principal_object_id
  role_definition_name  = "Contributor"
  service_scope         = "/subscriptions/${var.subscription_id}"
}

module "rbac_key_vault_assignment_principal_github_oidc" {
  source                = "./modules/role_assignments"
  principal_id          = module.github_oidc.service_principal_object_id
  role_definition_name = "Key Vault Secrets User"
  service_scope         = module.key_vault.id
}

