# Recipe Recommendations And Cook History

## Summary

Implemented a v1 recipe recommendation and cook-history feature for CookTime.

The feature adds:

- A cook-history tracker for signed-in users.
- A recipe recommendation API based on ingredient similarity, ownership, favorites, and novelty.
- A compact "I cooked this" recipe-page UI.
- A separate History page for viewing and deleting cook events.
- Admin-only recommendation score breakdowns.

## Backend Changes

Added migration:

- `src/CookTime/Scripts/017_recipe_cook_history_and_recommendations.sql`

The migration creates `cooktime.recipe_cook_events` with:

- `id`
- `user_id`
- `recipe_id`
- `cooked_at`
- `created_date`

It also adds indexes and SQL functions for:

- Creating cook events.
- Listing cook history for one recipe.
- Updating and deleting cook events with ownership checks.
- Recommending recipes from a source recipe.

Added DTOs:

- `CookHistoryEventDto`
- `CookHistoryEventWithRecipeDto`
- `CookHistoryUpsertRequest`
- `RecommendationScoreBreakdownDto`
- `RecipeRecommendationDto`

Added `CookTimeDB` methods for:

- `CreateCookHistoryEventAsync`
- `GetCookHistoryAsync(userId, recipeId)`
- `GetCookHistoryAsync(userId)`
- `UpdateCookHistoryEventAsync`
- `DeleteCookHistoryEventAsync`
- `GetRecipeRecommendationsAsync`

Added API endpoints:

- `GET /api/multipartrecipe/{recipeId}/recommendations?limit=6`
- `GET /api/multipartrecipe/{recipeId}/cook-history`
- `POST /api/multipartrecipe/{recipeId}/cook-history`
- `GET /api/cook-history`
- `PATCH /api/cook-history/{eventId}`
- `DELETE /api/cook-history/{eventId}`

Fixed an Npgsql parameter issue:

- Replaced named date parameters with unnamed typed parameters because positional SQL placeholders cannot be mixed with named parameters.

## Recommendation Scoring

V1 recommendation inputs:

- Ingredient similarity using Jaccard similarity over distinct ingredient ids.
- Ownership boost for recipes owned by the signed-in user.
- Favorites boost for recipes in the user's `Favorites` list.
- Novelty boost based on last cooked date.
- Diet scoring is deferred and remains `0`.

Default weights:

- Ingredient similarity: `0.60`
- Owned by user: `0.15`
- Favorited by user: `0.15`
- Novelty: `0.10`
- Diet match: `0.00`

Anonymous users receive similarity-based recommendations only.

## Frontend Changes

Added client types and service methods in:

- `src/CookTime/client-app/src/shared/CookTime/CookTime.types.ts`
- `src/CookTime/client-app/src/shared/CookTime/CookTime.service.ts`

Added recipe-page components:

- `CookHistorySection.tsx`
- `RecommendedRecipes.tsx`
- `RecipeInsights.css`

Recipe details page changes:

- "I cooked this" now appears after the recipe components (instructions and ingredients).
- Recipe details only shows a compact cook-history summary and button.
- It no longer shows every cook event on the recipe page.
- Render order: Header → Image → Fields → Components → Cook History → Nutrition → Reviews → Recommended Recipes.

Recommendation UI:

- Recommendation cards are visible to all users.
- Score, reasons, and breakdown accordion are visible only to users with the `Administrator` role.

Added History page:

- `src/CookTime/client-app/src/pages/History.tsx`
- Route: `/history`
- User menu link: `History`

The History page:

- Lists the signed-in user's cook events.
- Links each event back to the recipe.
- Lets users delete cook events.
- Shows an error if history cannot be loaded.

## Docker And Routing Fixes

Fixed a Docker Compose API proxy mismatch:

- API container listens on port `5001`.
- Webapp proxy was pointing at `http://api:5000`.
- Updated webapp proxy target to `http://api:5001`.
- Updated API port mapping to `5001:5001`.

Important local-dev note:

- Use `http://localhost:3000/history` for the React dev app.
- `http://localhost:5001/history` is served by the API container's static `wwwroot` bundle and may be stale unless `scripts/build` has copied a fresh frontend build.

Recommended container refresh after compose changes:

```bash
docker compose up -d --force-recreate api webapp
```

## Tests And Verification

Added tests:

- `src/CookTimeTests/TestCookHistoryAndRecommendations.cs`
- Additional API smoke checks in `src/CookTimeTests/API/ApiIntegrationTests.cs`

Verification commands that passed during implementation:

```bash
dotnet build src/CookTime/CookTime.csproj --no-restore
dotnet build src/CookTimeTests/CookTimeTests.csproj --no-restore
npx eslint src/components/Recipe/CookHistorySection.tsx
npx eslint src/components/Recipe/RecommendedRecipes.tsx
npx eslint src/pages/History.tsx
git diff --check
```

Known verification limitations:

- `npm run typecheck` still fails on pre-existing unrelated TypeScript errors.
- Focused `dotnet test` could start only with sandbox escalation, then failed locally because PostgreSQL was not running on `127.0.0.1:5432`.

## Follow-up Notes

Potential next improvements:

- Add edit-date support on the History page if desired.
- Add filtering or grouping by month on the History page.
- Move the user-wide cook-history query into a SQL function if the project wants all DB read behavior in migration scripts.
- Implement the deferred diet scoring once CookTime has an active diet preference model.
