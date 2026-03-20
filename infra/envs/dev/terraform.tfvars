subscription_id         = "7a6f3b04-9b9d-4728-bdc4-d0d5e9aa9ced"
tenant_id               = "6dbb1d5c-d749-4eda-9302-4967a4675d09"
location                = "brazilsouth"
project_name            = "cashflow"
environment             = "dev"
github_owner            = "marcodevopsestudo-cpu"
github_repo             = "cashflow"
github_branch           = "main"
postgres_admin_username = "pgcashflowadmin"
postgres_admin_password = "CHANGE-ME"
servicebus_topic_name   = "transaction-events"

tags = {
  project     = "cashflow"
  environment = "dev"
  managed_by  = "terraform"
}
