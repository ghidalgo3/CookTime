---
applyTo: '**/*.cs'
---

1. ALWAYS wrap blocks with braces `{}` even for single-line statements.
1. ALWAYS use the filesystem to determine namespace names.
1. PREFER to use records for classes with properties and no methods.
1. AVOID using regions (`#region` / `#endregion`).
1. AVOID writing docstrings for small methods that are self-explanatory.
1. AVOID adding unnecessary empty lines.
1. When Nuget packages are modified, the docker containers must be rebuilt.
1. When a file is nearing 350 lines of code, ask how to split it into smaller files.