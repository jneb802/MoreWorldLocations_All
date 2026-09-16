#!/bin/sh
# Runs the suite on both runtimes:
#   net10.0 — fast local loop via dotnet test
#   net48   — the mod's actual target, under Mono (closest to Valheim's runtime)
# Bare `dotnet test` aborts on the net48 target on macOS; use this script, or
# `dotnet test -f net10.0` for the fast loop alone.
#
# set -e so the script exits with the runner's own status: a suite is gated on
# the exit code, never on a grep of the output.
set -e
cd "$(dirname "$0")"

echo "== net10.0 (dotnet test) =="
dotnet test -f net10.0 "$@"

echo "== net48 (mono + xunit console) =="
dotnet build -f net48 >/dev/null
XUNIT_CONSOLE="$HOME/.nuget/packages/xunit.runner.console/2.8.1/tools/net48/xunit.console.exe"
mono "$XUNIT_CONSOLE" bin/Debug/net48/MoreWorldLocations.Tests.dll
