output "namespace_name" {
  value = azurerm_servicebus_namespace.this.name
}

output "topic_name" {
  value = azurerm_servicebus_topic.this.name
}

output "subscription_name" {
  value = azurerm_servicebus_subscription.this.name
}

output "namespace_id" {
  value = azurerm_servicebus_namespace.this.id
}

output "topic_id" {
  value = azurerm_servicebus_topic.this.id
}

output "subscription_id" {
  value = azurerm_servicebus_subscription.this.id
}
