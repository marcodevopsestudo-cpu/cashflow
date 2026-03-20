resource "random_uuid" "app_role_id" {}

resource "azuread_application" "this" {
  display_name    = var.display_name
  identifier_uris = [var.identifier_uri]

  api {
    requested_access_token_version = var.requested_access_token_version
  }

  app_role {
    allowed_member_types = ["User"]
    description          = var.app_role_display_name
    display_name         = var.app_role_display_name
    enabled              = true
    id                   = random_uuid.app_role_id.result
    value                = var.app_role_value
  }

  web {
    homepage_url  = "https://localhost"
    redirect_uris = ["https://oauth.pstmn.io/v1/callback"]

    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = false
    }
  }
}

resource "azuread_service_principal" "this" {
  client_id                    = azuread_application.this.client_id
  app_role_assignment_required = true
}
