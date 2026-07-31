# Security and Authentication Evolution

## Version 1 guest flow

1. Flutter creates a cryptographically random installation public identifier.
2. App calls guest-session endpoint.
3. Backend creates or resolves guest user and device installation.
4. Backend issues short-lived access token and rotated refresh token.
5. Flutter stores tokens in secure storage.
6. Every protected request uses the access token.

The installation identifier is not a password and must not be the sole authorization mechanism after token issuance.

## Registration upgrade

A guest account can be upgraded to email/password or external login without creating a new world or player profile.

## Required controls

- Passwords use an established password hasher, never custom cryptography.
- Refresh tokens are hashed at rest and rotated.
- Reuse detection revokes a compromised token family.
- Rate-limit sensitive authentication and AI-generation endpoints.
- API keys for AI/storage/push providers remain server-side.
- Logs redact tokens, secrets, prompts containing private data where appropriate, and provider keys.
- World ownership is checked in application queries, not only in UI.
- SQL uses EF parameterization; raw SQL requires review.
- Uploaded media requires type/size validation when introduced.

## Threats explicitly considered

- Guessing another user's world ID
- Extracting secrets from APK
- Replaying create requests
- Refresh-token theft
- Prompt injection through player text
- AI output containing unsafe or malformed data
- Duplicate background execution
- Excessive AI spending

## AI boundary

Player content and retrieved memories are untrusted prompt input. The backend must delimit them, apply output constraints, and never execute commands or mechanics from generated text.
