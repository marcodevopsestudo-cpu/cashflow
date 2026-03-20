output "id" { value = azurerm_postgresql_flexible_server.this.id }
output "fqdn" { value = azurerm_postgresql_flexible_server.this.fqdn }
output "database_name" { value = azurerm_postgresql_flexible_server_database.app.name }
output "connection_string" {
  value     = "Host=${azurerm_postgresql_flexible_server.this.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.app.name};Username=${var.admin_username};Password=${var.admin_password};Ssl Mode=Require;Trust Server Certificate=true"
  sensitive = true
}
