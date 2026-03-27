---

# 5) `docs/non-functional-requirements.md`

```md
# Non-Functional Requirements

This document explains how the current architecture addresses the main non-functional concerns of the challenge.

The goal is not only to list technical mechanisms, but to make the architectural reasoning explicit.

---

## 1. Core Architectural Principle

The main non-functional design decision is the separation between:

- the **transaction write path**;
- the **daily balance consolidation path**.

This separation exists because the system must preserve transaction-service availability even when consolidation is failing, delayed or temporarily unavailable.

That principle drives most of the architecture.

---

## 2. Availability

### Requirement Intent

Transaction ingestion should remain available even when downstream consolidation is degraded.

### Current Architectural Response

The write path depends primarily on:

- HTTP/API execution;
- validation;
- PostgreSQL persistence;
- durable outbox registration.

It does **not** depend on:

- immediate balance recalculation;
- immediate worker execution;
- immediate successful consolidation.

### Practical Impact

If consolidation fails:

- valid transactions can still be accepted;
- the source of truth remains durable;
- the read model may lag;
- recovery can happen later.

### Assessment

This is one of the strongest non-functional characteristics of the current solution.

---

## 3. Throughput and Concurrency

### Requirement Intent

The system should support peak load without making balance processing the bottleneck of every write request.

### Current Architectural Response

The write path was intentionally designed to perform only bounded synchronous work:

- validate request;
- persist transaction;
- register outbox event.

Heavy or expandable work is shifted to the asynchronous side.

### Why This Helps

This reduces coupling between request latency and:

- worker throughput;
- batch processing duration;
- downstream queue consumption speed;
- projection update cost.

### Practical Outcome

Under concurrency:

- the API remains simpler and faster;
- asynchronous backlog can absorb pressure over time;
- consolidation lag is preferable to blocking all callers.

### Assessment

The design is appropriate for burst traffic and concurrent ingestion, especially for an MVP focused on architectural soundness.

---

## 4. Scalability

### Requirement Intent

The system should scale more safely than a tightly coupled synchronous implementation.

### Current Architectural Response

The architecture supports scalability through:

- stateless API behavior;
- asynchronous processing separation;
- message buffering;
- projection-based query model.

### Scaling Perspective

#### Transaction Service

Can scale independently around write-path demand.

#### Consolidation Service

Can evolve independently according to projection workload and backlog.

#### Read Path

Uses a pre-aggregated table, reducing repeated calculation cost.

### Assessment

The architecture is more scalable than an inline synchronous consolidation model and provides a sound foundation for future scale-out decisions.

---

## 5. Resilience

### Requirement Intent

The system should tolerate failures without immediately losing accepted transactions or taking down the entire flow.

### Current Architectural Response

Resilience is addressed through:

- decoupling between write and processing paths;
- outbox-based durable integration intent;
- asynchronous processing;
- retry-oriented worker behavior;
- isolation of failed items for manual follow-up.

### Service Bus Failure Scenario

If Service Bus is unavailable temporarily:

- accepted transactions can still remain stored;
- outbox entries remain pending;
- consolidation is delayed;
- normal flow can resume later.

### Assessment

The current resilience posture is strong for challenge scope and clearly demonstrates architectural maturity.

---

## 6. Controlled Data Loss Risk

### Requirement Intent

The architecture should minimize the risk of losing accepted business events.

### Current Architectural Response

The system favors durable acceptance through:

- persistent source transaction storage;
- durable outbox registration;
- asynchronous continuation rather than inline distributed dependency.

### Important Clarification

This does **not** mean the current MVP is already formally certified for strict financial-grade loss guarantees.

It means the architecture was intentionally designed to minimize avoidable loss windows in the implemented scope.

### Assessment

The solution demonstrates the right design direction and core reliability mechanisms.

---

## 7. Eventual Consistency

### Requirement Intent

The system may tolerate slight delay in consolidated balance visibility if that improves availability and scalability.

### Current Architectural Response

The `daily_balance` read model is updated asynchronously.

Implications:

- recently accepted transactions may not appear instantly in daily balance queries;
- the lag window depends on asynchronous publication and processing;
- the source transaction remains authoritative even while the read model catches up.

### Why This Is Acceptable

The architecture explicitly values:

- write-path availability;
- decoupling;
- recoverability;
- operational flexibility

over strict real-time read-model freshness.

### Assessment

This is an intentional and appropriate trade-off.

---

## 8. Observability

### Requirement Intent

The platform should provide sufficient visibility for a distributed asynchronous solution.

### Current Architectural Response

The platform already includes observability foundations through Application Insights and structured operational flow.

Key visibility dimensions include:

- request health;
- error tracking;
- asynchronous processing status;
- backlog and lag visibility;
- failure diagnosis.

### Current Limitation

Observability is foundational, but not yet fully mature in terms of:

- dashboards;
- alerting;
- SLO monitoring;
- explicit freshness indicators for the read model.

### Assessment

Good MVP foundation, with clear next steps for production maturity.

---

## 9. Security

### Requirement Intent

The platform should avoid insecure operational shortcuts and support modern cloud security practices.

### Current Architectural Response

The solution already demonstrates:

- Key Vault secret usage;
- Azure authentication from CI/CD through OIDC;
- identity-aware cloud deployment foundations;
- separation between code, secrets and infrastructure.

### Current Limitation

Further hardening is still needed in areas such as:

- network isolation;
- more advanced RBAC review;
- policy enforcement;
- explicit rotation and compliance controls.

### Assessment

Strong architectural direction for challenge scope, not yet fully hardened enterprise security posture.

---

## 10. Operability

### Requirement Intent

The solution should be operable, deployable and recoverable beyond local development.

### Current Architectural Response

Operability is strengthened through:

- Terraform-based infrastructure provisioning;
- GitHub Actions-based CI/CD;
- automated build/test/deploy flow;
- migration support through DB Migrator;
- explicit processing separation.

### Assessment

This is a major strength of the solution because it demonstrates that the architecture is not only theoretically designed but also operationally deployable.

---

## 11. Business Continuity and Disaster Recovery

### Requirement Intent

The platform should have a path toward operational continuity under infrastructure incidents.

### Current Architectural Response

The current architecture already supports an important continuity principle:

- accepted transactions should remain durable even when downstream processing is unavailable.

However, full BC/DR maturity is not yet complete.

### Remaining Gaps

- formal backup policy validation;
- tested restore procedures;
- failover design;
- DR runbooks;
- RTO/RPO definition;
- continuity drills.

### Assessment

The current state is appropriate for an MVP and clearly identifies the next maturity steps.

---

## 12. Summary

The solution responds well to the most important non-functional concerns by making one central decision:

> separate reliable transaction acceptance from asynchronous consolidated balance processing.

That decision improves:

- availability;
- resilience;
- scalability;
- concurrency behavior;
- operational flexibility.

The main remaining work is not about changing the architectural direction, but about increasing production maturity around monitoring, DR, security hardening and operational tooling.
