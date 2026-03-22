create table if not exists transaction.outbox_messages (
    outbox_message_id uuid primary key,
    event_name varchar(200) not null,
    event_version integer not null,
    aggregate_id varchar(120) not null,
    payload jsonb not null,
    correlation_id varchar(120) not null,
    occurred_on_utc timestamptz not null,
    created_at_utc timestamptz not null,
    processed_on_utc timestamptz null,
    error varchar(4000) null,
    retry_count integer not null default 0
);

create index if not exists ix_outbox_messages_processed_on_utc
    on transaction.outbox_messages(processed_on_utc);

create index if not exists ix_outbox_messages_created_at_utc
    on transaction.outbox_messages(created_at_utc);
