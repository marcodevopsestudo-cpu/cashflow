# Requirements validation

## Challenge requirements covered by this service

- C# implementation.
- Separate daily consolidation responsibility.
- Resilience so transaction intake does not depend on consolidation availability.
- Tests for the application layer and orchestration flow.
- Clear documentation and runnable structure.
- Good practices such as layered architecture, MediatR, FluentValidation, structured logging, and bounded retry.

## Agreed implementation scope

The implemented scope intentionally stops here:

- no System.Reactive pipeline;
- no extra HTTP API in the consolidator;
- no unnecessary multi-tenant or merchant dimension in the balance key;
- no infinite retry behavior.

## Explicit simplifications

- the daily balance key is the date itself;
- the balance endpoint stays in Transaction Service and only reads the materialized table;
- the consolidator processes compact batch messages containing transaction ids;
- manual review uses database tables instead of an additional external workflow engine.
