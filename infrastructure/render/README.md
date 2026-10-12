# Render and Neon production operations contract

ADR-034 governs M18 deployment. This document records the provider-facing plan only; it contains no service IDs, domains, credentials, keys, or backup destinations.

## Environment isolation

| Environment | API | Database | Secrets and JWT keys | Intended use |
|---|---|---|---|---|
| Development | Local process/container | Local PostgreSQL | Local user-secrets/environment | Development only |
| Staging | Dedicated Render Web Service | Dedicated Neon project/database | Dedicated Render environment and Secret File | Release candidate and restore/smoke verification |
| Production | Dedicated non-Free Render Web Service | Dedicated Neon project/database | Dedicated Render environment and Secret File | Public operation |

Development, CI, and Staging must never receive Production database credentials or Production data. Render Free may be used only for Development/private beta where its limitations are acceptable.

## Runtime boundary

- Deploy the existing backend Docker image to Render.
- Render terminates HTTPS with its managed certificate. Do not add a second TLS proxy for M18.
- Configure explicit, environment-specific allowed hosts and CORS origins. Flutter mobile requires no browser origin.
- Process only the minimum forwarded headers required behind Render, with bounded forwarding depth. Forwarded IP is an operational signal, never authorization.
- Store the Neon TLS-required connection string in Render runtime environment variables.
- Mount the current RSA private key from a Render Secret File. Runtime configuration identifies the active `kid`; verifier configuration includes every public key accepted during rotation.
- Keep Ollama optional. Provider absence must use deterministic fallback and must not affect readiness.

## Deployment and migrations

1. CI succeeds on `dev`.
2. Open and approve the `dev -> main` PR after required release checks.
3. Build one versioned backend/container artifact and its SBOM.
4. Deploy the candidate configuration/artifact to Staging.
5. Apply EF migrations as an explicit idempotent Staging step; stop on failure.
6. Run health, core-journey, ownership-negative, AI-fallback, migration, log-redaction, and release-security checks.
7. Obtain project-owner Production approval.
8. Apply the same reviewed migration set to Production before sending traffic to the dependent application version; stop on failure.
9. Deploy the approved artifact and run post-deploy liveness/readiness and safe smoke checks.

The application must not lazily migrate on requests. Application rollback uses Render's previous known-good deployment only when schema-compatible. EF migrations are not automatically downgraded; incompatible schema incidents use a forward fix or verified provider restore.

## Key rotation

1. Generate a new RSA pair outside the repository and assign a new `kid`.
2. Deploy verification configuration accepting old and new public keys.
3. Replace the signing Secret File/configuration with the new private key and active `kid`.
4. Retain the old public key for at least 15 minutes plus 30 seconds clock skew.
5. Remove the old public key and securely destroy/revoke the old private key.

Routine signing-key rotation does not revoke refresh families. A compromise response may revoke all families.

## Backup and restore

- Enable at least seven days of Neon restore history for Production where the selected plan supports it.
- Produce an encrypted logical backup weekly, store it outside Render/Neon application runtime, and retain four weekly copies.
- Target RPO <= 24 hours outside PITR and RTO <= 4 hours; these are engineering objectives, not SLAs.
- Before public release, restore into Staging or an isolated recovery database and verify schema, representative User/World ownership data, and application connectivity.
- Repeat before major schema-risk releases or at least quarterly during public operation.

## Monitoring and release evidence

Use Render CPU/memory/request metrics, `/health/live`, `/health/ready`, sanitized structured logs/correlation IDs, and Render/Neon notifications. Enable notifications for service health/restarts/deploy failures, database connectivity, supported sustained error-rate signals, and resource exhaustion. Full distributed tracing and paid monitoring are not M18 requirements.

Release evidence records CI results, security scan/SBOM/container scan results, migration output, Staging smoke results, restore-drill date/result, performance results or accepted deviation, Production approval, deployed artifact version, and post-deploy checks. Critical risk blocks release; High risk requires the written ADR-034 acceptance fields and project-owner approval.
