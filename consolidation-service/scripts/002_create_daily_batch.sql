create schema if not exists transaction;

create table if not exists transaction.daily_batch
(
    batch_id uuid primary key,
    correlation_id varchar(64) not null,
    status integer not null,
    transaction_count integer not null,
    retry_count integer not null default 0,
    last_error text null,
    created_at_utc timestamp without time zone not null,
    started_at_utc timestamp without time zone null,
    completed_at_utc timestamp without time zone null
);

create index if not exists ix_daily_batch_status
    on transaction.daily_batch(status);
