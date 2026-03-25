variable "subscription_id" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "location" {
  type    = string
  default = "brazilsouth"
}

variable "project_name" {
  type    = string
  default = "cashflow"
}

variable "environment" {
  type    = string
  default = "dev"
}

variable "github_owner" {
  type    = string
  default = "marcodevopsestudo-cpu"
}

variable "github_repo" {
  type    = string
  default = "cashflow"
}

variable "github_branch" {
  type    = string
  default = "main"
}

variable "postgres_admin_username" {
  type = string
}

variable "postgres_admin_password" {
  type      = string
  sensitive = true
}

# -----------------------------
# Service Bus
# -----------------------------

variable "servicebus_topic_name" {
  type    = string
  default = "transaction-events"
}

variable "servicebus_subscription_name" {
  type    = string
  default = "consolidation-service"
}

# -----------------------------
# Function App
# -----------------------------

variable "function_app_name_override" {
  type    = string
  default = null
}

# -----------------------------
# Networking
# -----------------------------

variable "start_ip_address" {
  type = string
}

variable "end_ip_address" {
  type = string
}

# -----------------------------
# Tags
# -----------------------------

variable "tags" {
  type    = map(string)
  default = {}
}
