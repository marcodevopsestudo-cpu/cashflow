variable "display_name" {
  type = string
}

variable "homepage_url" {
  type    = string
  default = "https://oauth.pstmn.io"
}

variable "redirect_uris" {
  type    = list(string)
  default = ["https://oauth.pstmn.io/v1/callback"]
}

variable "api_client_id" {
  type = string
}

variable "api_scope_id" {
  type = string
}

variable "secret_display_name" {
  type    = string
  default = "postman-client-secret"
}
