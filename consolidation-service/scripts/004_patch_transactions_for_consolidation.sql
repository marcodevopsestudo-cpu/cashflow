-- ========================================
-- REMOVE COLUNAS ANTIGAS (SE EXISTIREM)
-- ========================================

alter table transaction.transactions
    drop column if exists processing_status;

alter table transaction.transactions
    drop column if exists processing_attempt_count;

alter table transaction.transactions
    drop column if exists last_batch_id;

alter table transaction.transactions
    drop column if exists consolidated_at_utc;


-- ========================================
-- CRIA NOVAS COLUNAS NORMALIZADAS
-- ========================================

alter table transaction.transactions
    add column if not exists consolidation_status integer not null default 0;

alter table transaction.transactions
    add column if not exists consolidation_attempt_count integer not null default 0;

alter table transaction.transactions
    add column if not exists last_consolidation_batch_id uuid null;

alter table transaction.transactions
    add column if not exists consolidated_at_utc timestamp without time zone null;
