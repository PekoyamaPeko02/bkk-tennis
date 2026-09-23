---
description: Take the next open item from docs/BACKLOG.md, build it with tests, commit, tick it off.
---

You are the lead developer for BKK Tennis working autonomously. Read `CLAUDE.md` and `docs/BACKLOG.md`.

1. Pick the FIRST item in `docs/BACKLOG.md` that is `- [ ]` and not marked `BLOCKED`. If none remain, reply
   exactly `BACKLOG EMPTY` and stop. $ARGUMENTS
2. Make sure you are on branch `dev/autopilot` (create it from the current branch if needed). Pull nothing, push nothing.
3. Plan briefly, then implement the item end-to-end (backend + frontend + SQL in `db/init/` as needed — add a new
   numbered SQL file for schema changes, never edit applied ones except seed data).
4. Delegate testing to the `test-engineer` subagent: tell it the item, the acceptance criteria and changed files.
   If it reports bugs, fix them and ask it to re-run until green.
5. Run the full gate: `dotnet build backend/BkkTennis.sln && dotnet test backend/BkkTennis.sln && pnpm -r run typecheck && pnpm -r run build`.
6. Commit with a conventional message (author per CLAUDE.md), change the item to `- [x]` and append one line of
   notes under it (what was done / follow-ups). Commit the backlog update too.
7. If an item needs a human decision, credentials, or a paid service: mark it `BLOCKED: <reason>` in the backlog,
   commit, and move on to the next item instead of stopping.
8. Continue with the next item in the same session until the backlog is empty or you run out of context;
   then write a 5-line handoff at the bottom of `docs/BACKLOG.md` under `## Handoff` and commit.
