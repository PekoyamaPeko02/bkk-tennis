#!/usr/bin/env bash
# Runs Claude Code headless in a loop over docs/BACKLOG.md until the backlog is empty
# or Claude Code stops (usage/rate limit, error). Each round is a fresh session that runs /next.
#
#   ./scripts/autopilot.sh              # loop until empty / limit
#   ./scripts/autopilot.sh 3            # at most 3 rounds
#
# Prereqs: `claude` CLI logged in, Docker running (db on :5433), run from repo root.
set -uo pipefail
cd "$(dirname "$0")/.."

MAX_ROUNDS="${1:-100}"
LOG_DIR=".claude/logs"; mkdir -p "$LOG_DIR"
docker compose up -d db >/dev/null 2>&1 || echo "warn: could not start db (is Docker running?)"

if [[ -n "$(git status --porcelain)" ]]; then
  echo "❌ Working tree has uncommitted changes — commit or stash them first."; exit 1
fi
git switch dev/autopilot 2>/dev/null || git switch -c dev/autopilot

for ((i = 1; i <= MAX_ROUNDS; i++)); do
  if ! grep -q '^- \[ \] \*\*' docs/BACKLOG.md; then
    echo "✅ Backlog empty — done."; break
  fi
  ts="$(date +%Y%m%d-%H%M%S)"; log="$LOG_DIR/round-$i-$ts.log"
  echo "▶ Round $i ($(date '+%H:%M')) — log: $log"

  claude -p "/next" --permission-mode acceptEdits --max-turns 200 2>&1 | tee "$log"
  code=${PIPESTATUS[0]}

  if grep -qiE 'usage limit|rate limit|limit reached|quota' "$log"; then
    echo "⏸  Hit a usage/rate limit — stopping. Re-run this script after the limit resets."; break
  fi
  if grep -q 'BACKLOG EMPTY' "$log"; then echo "✅ Backlog empty — done."; break; fi
  if [[ $code -ne 0 ]]; then echo "❌ claude exited with $code — stopping. See $log"; break; fi
done

echo; git --no-pager log --oneline -15
