This preserves the structure and substance already present in your current consolidation README, while organizing it better and removing weak spots like the old broken internal doc links. Your current file already includes the worker role, batch messages, MediatR flow, exponential backoff, manual review, the example contract, and local settings. :contentReference[oaicite:1]{index=1}

---

## 2) File: `README.md`

### Directory

`/infra/README.md`

````md
# Infrastructure

This directory contains the Terraform code used to provision the Azure resources required by the cashflow solution.

The infrastructure supports:

- the Transaction Service;
- the Consolidation Service;
- shared platform capabilities such as observability, messaging, identity and secret management.

---

## Provisioned Azure Resources

The Terraform code provisions the main cloud resources used by the solution, including:

- Resource Group
- Storage Account for Azure Functions
- Linux Function Apps
- Application Insights
- Azure Database for PostgreSQL Flexible Server
- Azure Service Bus namespace and topic
- Key Vault
- Microsoft Entra ID application registrations used by the solution
- Microsoft Entra ID application registration used by GitHub Actions OIDC

This infrastructure reflects the architectural goals of the project:

- serverless execution;
- low operational overhead;
- secure service-to-service integration;
- resilient asynchronous communication.

---

## Architectural Role of the Infrastructure

The infrastructure is not only deployment support; it is part of the design.

### Availability and decoupling

Azure Service Bus allows the write path and the consolidation path to operate independently.

### Observability

Application Insights provides centralized logs and telemetry for the Function Apps.

### Security

The solution uses Azure-native identity and secret management patterns, including:

- Microsoft Entra ID
- Managed Identity
- Key Vault
- RBAC
- GitHub OIDC for CI/CD authentication

### Cost model

Azure Functions supports the serverless, pay-per-use model adopted by the solution.

---

## Directory Structure

At a high level, this directory contains:

- reusable Terraform modules;
- bootstrap resources for remote state;
- environment-specific configuration;
- example variable files and backend configuration templates.

---

## State Management

Use the bootstrap project to create the remote backend resources and then initialize the target environment with a `backend.hcl` file.

```bash
cd infra/bootstrap/tfstate
terraform init
terraform apply -var="subscription_id=" -var="tenant_id="

cd ../../envs/dev
cp backend.dev.hcl.example backend.hcl
terraform init -backend-config=backend.hcl -reconfigure
```
````
