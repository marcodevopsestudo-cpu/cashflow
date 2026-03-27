# Infrastructure Overview

## Overview

Infrastructure is provisioned using Terraform and deployed on Microsoft Azure.

The goal is to ensure:

- reproducibility
- consistency across environments
- automated provisioning
- secure configuration

---

## Resources Created

The infrastructure includes:

### Core Resources

- Resource Group
- Storage Account (for Azure Functions runtime)

### Compute

- Azure Function App (Transaction Service)
- Azure Function App (Consolidation Service)

### Data

- Azure PostgreSQL Flexible Server
- Database for application data

### Messaging

- Azure Service Bus Namespace
- Topic for event publishing
- (Subscription may be manually created or extended via Terraform)

### Security

- Azure Key Vault (for secrets)
- Microsoft Entra ID applications

### Observability

- Application Insights

---

## Architecture Role

The infrastructure supports:

- decoupled communication (Service Bus)
- durable persistence (PostgreSQL)
- secure secret management (Key Vault)
- monitoring and diagnostics (Application Insights)

---

## CI/CD Integration

Infrastructure and application deployment are integrated with:

- GitHub Actions
- OIDC authentication (no stored credentials)

Pipeline responsibilities:

- build
- test
- run migrations
- deploy services

---

## How to Apply Infrastructure

```bash
terraform init
terraform plan
terraform apply
```
