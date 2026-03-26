create schema if not exists transaction;

create table if not exists transaction.daily_balance
(
    balance_date date primary key,
    total_credits numeric(18, 2) not null default 0,
    total_debits numeric(18, 2) not null default 0,
    balance numeric(18, 2) not null default 0,
    updated_at_utc timestamp without time zone not null
);
