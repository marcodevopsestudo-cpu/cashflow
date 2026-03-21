variable "name" { type = string }
variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "service_plan_id" { type = string }
variable "storage_account_name" { type = string }

variable "key_vault_reference_identity_id" {
     type = string
     default = null
     }
variable "app_settings" { type = map(string) }
variable "auth_settings" {
  type = object({
    enabled                = bool
    client_id              = string
    tenant_auth_endpoint   = string
    unauthenticated_action = string
  })
}
variable "tags" { type = map(string) }

variable "project_name" { type = string }
variable "environment" { type = string}

variable "application_insights_connection_string" {
  type = string
  sensitive = true
}
