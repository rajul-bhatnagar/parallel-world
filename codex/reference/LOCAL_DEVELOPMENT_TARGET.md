# Local Development Target

Recommended local setup:

```text
Flutter emulator/device
        -> local ASP.NET Core API
        -> PostgreSQL in Docker
        -> fake/stub AI provider by default
```

Real AI provider calls should be opt-in through local secrets. Automated tests never call a real AI provider.

Seed a deterministic development world so UI and simulation defects can be reproduced.
