---
applyTo: '**/*.sql'
---
1. When modifying tables, read the 000_migration_tracker.sql to understand how migrations are tracked.
1. If you alter a table or function, you must generate a new migration script (e.g., 00X_description.sql) to apply those changes.