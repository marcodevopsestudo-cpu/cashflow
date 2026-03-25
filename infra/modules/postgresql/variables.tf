variable "name" { type = string }
variable "database_name" { type = string }
variable "resource_group_name" { type = string }
variable "location" { type = string }
variable "admin_username" { type = string }
variable "admin_password" {
  type      = string
  sensitive = true
}
variable "postgres_version" { type = string }
variable "sku_name" { type = string }
variable "storage_mb" { type = number }
variable "backup_retention_days" { type = number }
variable "zone" { type = string }
variable "tags" { type = map(string) }
variable "start_ip_address" { type = string }
variable "end_ip_address" { type = string }
