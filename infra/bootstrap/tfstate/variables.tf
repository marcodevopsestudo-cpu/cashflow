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

variable "resource_group_name" {
  type    = string
  default = "rg-cashflow-tfstate"
}

variable "container_name" {
  type    = string
  default = "tfstate"
}

variable "tags" {
  type = map(string)
  default = {
    project    = "cashflow"
    managed_by = "terraform"
    purpose    = "tfstate"
  }
}