create schema if not exists transaction;

create table if not exists transaction.transaction_processing_error
(
    id bigserial primary key,
    batch_id uuid not null,
    transaction_id uuid null,
    correlation_id varchar(64) not null,
    error_code varchar(128) not null,
    error_message text not null,
    stack_trace text null,
    created_at_utc timestamp without time zone not null,
    retry_count integer not null,
    status varchar(64) not null
);

create index if not exists ix_transaction_processing_error_batch_id
    on transaction.transaction_processing_error(batch_id);

create index if not exists ix_transaction_processing_error_transaction_id
    on transaction.transaction_processing_error(transaction_id);
