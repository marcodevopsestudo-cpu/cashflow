# Operational behavior

## Correlation and observability

Every batch carries the following telemetry identifiers:

- `CorrelationId`: generated upstream and propagated end-to-end;
- `BatchId`: identifies the logical consolidation unit;
- `MessageId`: identifies the broker message.

These values are included in structured logs so they can be searched in Application Insights.

## Retry policy

The service applies exponential backoff with jitter and a bounded retry count.

- max retries: 3
- base delay: 2 seconds
- jitter: 100ms to 750ms

There is no infinite reprocessing loop.

## Manual review

When the workflow still fails after retry exhaustion:

- the batch is marked as `Failed`;
- transactions are marked as `PendingManualReview`;
- records are written to `transaction_processing_error`.

## Consistency model

The balance endpoint is **eventually consistent**. A transaction is not expected to appear immediately in the balance endpoint until the batch has been processed.

## Scale considerations

The service is prepared for horizontal scale because:

- processing is event-driven;
- the read model is materialized;
- duplicate processing is controlled through `daily_batch`;
- broker messages contain compact batch payloads.
