
alter table transaction.transactions
    add column if not exists consolidated_at_utc timestamp without time zone null;

alter table transaction.transactions
    add column if not exists last_batch_id uuid null;

alter table transaction.transactions
    add column if not exists processing_attempt_count integer not null default 0;


