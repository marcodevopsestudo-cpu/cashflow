output "id" {
  value = azurerm_servicebus_namespace.this.id
}

output "namespace_name" {
  value = azurerm_servicebus_namespace.this.name
}

output "namespace_id" {
  value = azurerm_servicebus_namespace.this.id
}

output "namespace_fqdn" {
  value = "${azurerm_servicebus_namespace.this.name}.servicebus.windows.net"
}

output "topic_name" {
  value = azurerm_servicebus_topic.this.name
}

output "topic_id" {
  value = azurerm_servicebus_topic.this.id
}

output "subscription_names" {
  value = [for subscription in values(azurerm_servicebus_subscription.this) : subscription.name]
}

output "subscription_ids" {
  value = { for key, subscription in azurerm_servicebus_subscription.this : key => subscription.id }
}

output "primary_connection_string" {
  value     = azurerm_servicebus_namespace_authorization_rule.app.primary_connection_string
  sensitive = true
}
