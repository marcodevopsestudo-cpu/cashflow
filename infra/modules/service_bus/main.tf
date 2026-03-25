resource "azurerm_servicebus_namespace" "this" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "Standard"
  tags                = var.tags
}

resource "azurerm_servicebus_topic" "this" {
  name         = var.topic_name
  namespace_id = azurerm_servicebus_namespace.this.id
}

resource "azurerm_servicebus_subscription" "this" {
  for_each           = toset(var.subscription_names)
  name               = each.value
  topic_id           = azurerm_servicebus_topic.this.id
  max_delivery_count = var.max_delivery_count
}

resource "azurerm_servicebus_namespace_authorization_rule" "app" {
  name         = "transaction-service"
  namespace_id = azurerm_servicebus_namespace.this.id

  listen = true
  send   = true
  manage = false
}
