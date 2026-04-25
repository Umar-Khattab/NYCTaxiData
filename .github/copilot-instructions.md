# Copilot Instructions

## Project Guidelines
- Use strict CQRS with record request types, MediatR handlers returning Result<TResponse>, FluentValidation for input-only checks, no service layer, marker interfaces for cacheable queries/transactional commands, and keep Domain layer pure (entities/enums/repositories only).