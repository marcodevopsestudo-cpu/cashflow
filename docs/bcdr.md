# BC/DR and resilience notes

## Objectives

- Keep transaction ingestion available during failures in downstream consumers.
- Recover quickly from regional or service outages.
- Minimize data loss while preserving event ordering and idempotency.

## Current resilience posture

- The transaction write path is decoupled from the balance consolidation flow.
- The outbox pattern preserves publish intent in PostgreSQL.
- Service Bus topic decouples future consumers from the write service.
- Application Insights provides a starting point for operational visibility.

## Gaps to close

- No implemented balance consolidation service yet.
- No documented regional failover process yet.
- No VNet/private endpoint hardening yet.
- No runbook for PostgreSQL failover or restore yet.

## Recommended BC/DR roadmap

### Phase 1
- Document recovery runbooks.
- Create health dashboards and alerts.
- Add backup/restore validation for PostgreSQL.
- Define RTO/RPO targets for the challenge environment.

### Phase 2
- Introduce VNet integration and subnet isolation.
- Enable private database access.
- Add regional pairing strategy for Function, Storage, Service Bus and PostgreSQL.
- Add dead-letter handling and replay tooling for integration events.

### Phase 3
- Create a formal failover document with decision trees and owner matrix.
- Add periodic DR drills.
- Add workload-level SLOs and error budgets.

## Suggested failover strategy

- **Functions**: redeploy from GitHub Actions into a secondary region using the same artifacts and infrastructure modules.
- **PostgreSQL**: evaluate flexible server high availability and backup restore strategy for the target budget.
- **Service Bus**: use premium features or documented replay strategy depending on target environment criticality.
- **Key Vault/Storage**: plan paired-region design and recovery validation.

## Metrics to monitor

- Function execution duration and failures.
- Outbox backlog size and age.
- Publish retry count and last error.
- Postgres availability, CPU, storage and connection saturation.
- Service Bus active messages, dead-letter messages and throttling.
