create schema if not exists transaction;

create table if not exists transaction.transactions (
    transaction_id uuid primary key,
    account_id varchar(100) not null,
    kind varchar(20) not null,
    amount numeric(18,2) not null,
    currency varchar(10) not null,
    transaction_date_utc timestamptz not null,
    description varchar(500) null,
    correlation_id varchar(120) not null,
    status varchar(40) not null,
    created_at_utc timestamptz not null,
    updated_at_utc timestamptz null
);

create index if not exists ix_transactions_account_id on transaction.transactions(account_id);
create index if not exists ix_transactions_correlation_id on transaction.transactions(correlation_id);
