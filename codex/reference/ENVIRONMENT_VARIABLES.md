# Environment Variables

Never commit actual values.

```text
ASPNETCORE_ENVIRONMENT
ConnectionStrings__Default
Auth__Issuer
Auth__Audience
Auth__SigningKey
Auth__AccessTokenMinutes
Auth__RefreshTokenDays
AI__Provider
AI__ApiKey
AI__Model
AI__TimeoutSeconds
AI__DailyBudget
Storage__Provider
Storage__Connection
Push__Provider
Push__Credentials
Observability__Endpoint
```

Flutter receives only public configuration such as API base URL and build environment. It must not receive provider secrets or signing keys.
