variable "display_name" {
  type = string
}

variable "identifier_uri" {
  type = string
}

variable "requested_access_token_version" {
  type    = number
  default = 2
}

variable "homepage_url" {
  type    = string
  default = "https://localhost"
}

variable "redirect_uris" {
  type    = list(string)
  default = ["https://oauth.pstmn.io/v1/callback"]
}

variable "scope_value" {
  type    = string
  default = "access_as_user"
}

variable "scope_admin_consent_display_name" {
  type    = string
  default = "Access transaction service"
}

variable "scope_admin_consent_description" {
  type    = string
  default = "Allows the application to access transaction service on behalf of the signed-in user."
}

variable "scope_user_consent_display_name" {
  type    = string
  default = "Access transaction service"
}

variable "scope_user_consent_description" {
  type    = string
  default = "Allow this application to access transaction service on your behalf."
}

variable "app_role_display_name" {
  type = string
}

variable "app_role_value" {
  type = string
}
