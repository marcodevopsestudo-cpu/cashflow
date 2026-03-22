alter table transactions
    add column if not exists processing_status integer not null default 1;

alter table transactions
    add column if not exists consolidated_at_utc timestamp without time zone null;

alter table transactions
    add column if not exists last_batch_id uuid null;

alter table transactions
    add column if not exists processing_attempt_count integer not null default 0;

create index if not exists ix_transactions_processing_status on transactions(processing_status);
