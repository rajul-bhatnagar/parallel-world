# Prompt 05 — Finalize Database Design

```text
Read docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, and docs/architecture/DATABASE.md.

Act as a senior PostgreSQL and EF Core data modeller.
Validate the model for:
- strict world ownership
- actor modelling
- post/reply/repost relationships
- one active direct conversation per player-character pair
- directional relationships
- romantic-state history
- memory source references
- simulation idempotency
- cursor-pagination indexes
- cache-count correctness
- deletion behaviour
- future account registration

List Critical, High, Medium, and Low findings before editing.
Do not create migrations or code yet.
```
