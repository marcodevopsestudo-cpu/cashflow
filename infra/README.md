# Infrastructure

Terraform provisions the Azure resources required by the transaction service:

- Resource Group
- Storage Account for Azure Functions
- Linux Consumption Function App
- Application Insights
- Azure Database for PostgreSQL Flexible Server
- Azure Service Bus namespace and topic
- Microsoft Entra ID application for the API
- Microsoft Entra ID application for GitHub OIDC
- Key Vault for connection strings and deployment secrets

## State management

Use the bootstrap project to create the remote backend resources and then initialize the environment with a `backend.hcl` file.

```bash
cd infra/bootstrap/tfstate
terraform init
terraform apply -var="subscription_id=<subscription>" -var="tenant_id=<tenant>"

cd ../../envs/dev
cp backend.dev.hcl.example backend.hcl
terraform init -backend-config=backend.hcl -reconfigure
```

## Notes

- The repository intentionally keeps `backend.tf` with a partial configuration so the same code can be reused across environments.
- `terraform.tfvars.example` is provided as a safe template. Keep real `terraform.tfvars` files out of source control.
