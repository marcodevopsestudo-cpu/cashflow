# Database design

## Tables

### `daily_balance`

Stores one row per day.

| Column | Purpose |
|---|---|
| `balance_date` | natural key for the daily balance |
| `total_credits` | accumulated credit amount |
| `total_debits` | accumulated debit amount |
| `balance` | net result |
| `updated_at_utc` | last refresh timestamp |

### `daily_batch`

Stores the lifecycle of every batch message.

| Column | Purpose |
|---|---|
| `batch_id` | unique identifier for the batch |
| `correlation_id` | tracing key |
| `status` | pending, processing, succeeded, failed, ignored |
| `transaction_count` | number of transactions in the batch |
| `retry_count` | bounded retry count |
| `last_error` | last failure reason |
| `created_at_utc` | row creation time |
| `started_at_utc` | first processing time |
| `completed_at_utc` | finalization time |

### `transaction_processing_error`

Stores items that require manual review.

| Column | Purpose |
|---|---|
| `id` | surrogate key |
| `batch_id` | failing batch |
| `transaction_id` | optional failing transaction |
| `correlation_id` | tracing key |
| `error_code` | short error category |
| `error_message` | human-readable summary |
| `stack_trace` | captured failure details |
| `created_at_utc` | insertion timestamp |
| `retry_count` | number of retries consumed |
| `status` | manual-review state |

## Indexes

- `daily_balance(balance_date)` as primary key;
- `daily_batch(batch_id)` as primary key;
- `transaction_processing_error(batch_id)` for investigation queries;
- `transaction_processing_error(transaction_id)` for item lookups.
