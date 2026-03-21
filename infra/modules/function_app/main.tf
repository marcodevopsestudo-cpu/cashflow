resource "azurerm_linux_function_app" "this" {
  name                = var.name
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = var.service_plan_id

  storage_account_name          = var.storage_account_name
  storage_uses_managed_identity = true

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }

    application_insights_connection_string = var.application_insights_connection_string
    minimum_tls_version                    = "1.2"
  }

  auth_settings_v2 {
    auth_enabled           = var.auth_settings.enabled
    unauthenticated_action = var.auth_settings.unauthenticated_action
    default_provider       = "azureactivedirectory"

    active_directory_v2 {
      client_id            = var.auth_settings.client_id
      tenant_auth_endpoint = var.auth_settings.tenant_auth_endpoint
    }

    login {}
  }

  key_vault_reference_identity_id = var.key_vault_reference_identity_id
  app_settings                    = var.app_settings
  tags                            = var.tags
}
