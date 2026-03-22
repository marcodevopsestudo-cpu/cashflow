# Consolidation workflow

## Sequence diagram

```mermaid
sequenceDiagram
    participant SB as Service Bus Topic
    participant FN as Azure Function
    participant MR as MediatR Handler
    participant WF as Consolidation Workflow
    participant PG as PostgreSQL

    SB->>FN: Batch message (BatchId, CorrelationId, TransactionIds)
    FN->>MR: ProcessConsolidationBatchCommand
    MR->>WF: ExecuteAsync
    WF->>PG: Upsert daily_batch as pending/processing
    WF->>PG: Load pending transactions by ids
    WF->>WF: Aggregate by date
    WF->>PG: Upsert daily_balance
    WF->>PG: Mark transactions as consolidated
    WF->>PG: Mark daily_batch as succeeded
```

## Failure path

```mermaid
sequenceDiagram
    participant WF as Consolidation Workflow
    participant RP as Retry Policy
    participant PG as PostgreSQL

    WF->>RP: Execute protected operation
    RP-->>WF: Exception after bounded retries
    WF->>PG: Mark daily_batch as failed
    WF->>PG: Insert transaction_processing_error rows
    WF->>PG: Mark transactions as PendingManualReview
```

## Step contract

1. `RegisterBatchStep`
   - prevents duplicate successful processing;
   - sets the batch to `Processing`.
2. `LoadTransactionsStep`
   - reads only pending transactions.
3. `AggregateTransactionsStep`
   - groups by `DateOnly`;
   - sums credits and debits separately.
4. `UpsertDailyBalanceStep`
   - inserts missing days;
   - updates existing days atomically.
5. `FinalizeBatchStep`
   - marks transactions as consolidated;
   - closes the batch successfully.
