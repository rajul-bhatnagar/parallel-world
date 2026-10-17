# Production release and recovery runbook

This runbook implements ADR-034. Staging and Production are separate Render Web Services backed by separate Neon projects. Runtime values are configured in Render; no value from Production is copied to Development, tests, or Staging.

## Provision once

1. Create the services from `render.yaml`. Both managed Staging and Production use a plan that supports the pre-deploy migration step; keep public Production on a non-Free plan. Render Free remains suitable only for development/private-beta instances that do not use this release path.
2. Set each service's Neon connection string with `sslmode=require`, bounded `Maximum Pool Size` (at most 200), and the environment's own database credentials.
3. Add a Render Secret File named `jwt-private.pem`. Set the matching public key and `kid` as environment variables. Never place private key text in an environment variable or Blueprint.
4. Set `Deployment__AllowedHosts__0` to the exact Render/custom API host. Add CORS origins only for an approved browser client; the Flutter mobile app needs none.
5. Enable Neon restore history for at least seven days where the selected plan supports it. Configure an encrypted weekly logical backup outside Render/Neon and retain four weekly copies.
6. Enable Render and Neon notifications for failed deploys, unhealthy/restarting services, database connectivity, sustained errors where supported, and resource exhaustion.

## Release

1. Require green CI on `dev`, then merge a reviewed `dev -> main` pull request.
2. Confirm dependency, secret, SAST, SBOM, and container results contain no unaccepted reachable Critical or High finding. A High acceptance records issue, exposure, mitigation, owner, and expiry; Critical findings block release.
3. Deploy Staging. The pre-deploy migration bundle must succeed before the service receives traffic.
4. Verify `/health/live`, `/health/ready`, authentication, world/feed, messaging, catch-up, and log redaction. Run `m18-load.js` against representative non-Production test data.
5. Record the migration result, image revision, smoke evidence, performance result, known risks, and project-owner approval.
6. Manually deploy Production, then repeat safe health and smoke checks. Never run the load profile against Production without separate approval.

## Migration failure and rollback

- A failed pre-deploy migration stops deployment. Diagnose against an isolated copy; do not start the new application version and do not auto-downgrade EF migrations.
- For a schema-compatible application failure, restore Render's previous known-good deployment.
- For an incompatible or destructive schema failure, stop writes and use the approved forward fix or provider restore plan. Record the incident and evidence.

## Backup restore rehearsal

At least quarterly and before a major schema-risk release, restore a logical backup or Neon restore point into Staging or an isolated recovery database. Verify schema/migration history, representative User and World data, and API readiness/connectivity. Record elapsed recovery time and confirm the engineering targets: RPO at most 24 hours outside PITR and RTO at most four hours.

## JWT rotation

Deploy the new public key plus the previous public key, then switch signing to the new Secret File and `kid`. Retain the previous public key for at least 15 minutes 30 seconds, remove it, and destroy the old private key. Ordinary rotation does not revoke refresh families; incident response may require logout-all/family revocation.
