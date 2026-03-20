output "id" { value = azurerm_servicebus_namespace.this.id }
output "namespace_name" { value = azurerm_servicebus_namespace.this.name }
output "namespace_fqdn" { value = "${azurerm_servicebus_namespace.this.name}.servicebus.windows.net" }
output "topic_name" { value = azurerm_servicebus_topic.this.name }
output "primary_connection_string" {
  value     = azurerm_servicebus_namespace_authorization_rule.app.primary_connection_string
  sensitive = true
}
