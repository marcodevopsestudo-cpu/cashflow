variable "name" {
  type = string
}

variable "location" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "topic_name" {
  type = string
}

variable "subscription_name" {
  type = string
}

variable "max_delivery_count" {
  type    = number
  default = 10
}

variable "tags" {
  type    = map(string)
  default = {}
}
