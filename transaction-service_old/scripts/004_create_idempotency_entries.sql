create table if not exists transaction.idempotency_entries
(
    idempotency_key varchar(120) primary key,
    request_hash varchar(200) not null,
    transaction_id uuid null,
    created_at_utc timestamp without time zone not null
);

create unique index if not exists ux_idempotency_entries_key
    on transaction.idempotency_entries (idempotency_key);

create index if not exists ix_idempotency_entries_transaction_id
    on transaction.idempotency_entries (transaction_id);
