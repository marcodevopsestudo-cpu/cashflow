# Cashflow infra - transaction-service

Esta base Terraform cria a infra inicial do `transaction-service` com:

- Resource Group
- Storage Account
- App Service Plan (Consumption / Linux)
- Function App (.NET isolated)
- Application Insights
- PostgreSQL Flexible Server + database `cashflow`
- Service Bus Namespace + topic `transaction-events`
- Key Vault com secrets de conexão
- App Registration do `transaction-service`
- App Registration para GitHub OIDC
- Federated identity credential para o branch `main`

## Como usar

```bash
cd infra/envs/dev
cp terraform.tfvars.example terraform.tfvars
terraform init
terraform plan
terraform apply
```

## Observações importantes

- A connection string do PostgreSQL e do Service Bus são gravadas no Key Vault.
- A Function App lê esses valores via Key Vault references (`@Microsoft.KeyVault(...)`).
- A Function usa System Assigned Managed Identity.
- O Service Principal da API foi configurado com `app_role_assignment_required = true`.
- Existe uma app role inicial: `TransactionService.Access`.

## Próximo passo depois do apply

1. Criar um usuário de teste ou convidar um guest user.
2. Atribuir o usuário ao Enterprise Application da API.
3. Atribuir a role `Transaction Service Access`.
4. Testar no Postman com Authorization Code + PKCE.
