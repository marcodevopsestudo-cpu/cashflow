resource "azuread_application" "this" {
  display_name = var.display_name

  web {
    homepage_url  = var.homepage_url
    redirect_uris = var.redirect_uris

    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = false
    }
  }

  required_resource_access {
    resource_app_id = var.api_client_id

    resource_access {
      id   = var.api_scope_id
      type = "Scope"
    }
  }
}

resource "azuread_service_principal" "this" {
  client_id = azuread_application.this.client_id
}

resource "azuread_application_password" "this" {
  application_id = azuread_application.this.id
  display_name   = var.secret_display_name
}
