variable "name" { type = string }
variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "tenant_id" { type = string }
variable "current_principal_object_id" { type = string }
variable "tags" { type = map(string) }
variable "environment" { type = string }
