terraform {
  backend "azurerm" {
    resource_group_name  = "rg-cashflow-tfstate"
    storage_account_name = "SEU_STORAGE_ACCOUNT_DO_BOOTSTRAP"
    container_name       = "tfstate"
    key                  = "cashflow/dev/terraform.tfstate"
  }
}