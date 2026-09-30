#!/usr/bin/env bash
# Copied unchanged from ValheimTesting tools/test-runners/run-tests.sh (see its README there).
#   MoreWorldLocations.Tests/run-tests.sh MoreWorldLocations.Tests/MoreWorldLocations.Tests.csproj
# Run a mod's test project on modern .NET and on .NET Framework, the runtime
# family of the game's Mono, and report each.
#
#   run-tests.sh [options] <Tests.csproj> [dotnet test arguments]
#
#   -f, --framework TFM     run only this target framework; repeat for more
#                           (default: net10.0 and net48)
#   -c, --configuration C   build configuration (default Debug)
#   --filter EXPR           dotnet test filter for every framework; for .NET
#                           Framework translated to xunit console options
#   --netfx-parallel MODE   xunit console -parallel for .NET Framework:
#                           none, collections, assemblies, all or default (the
#                           assembly's own setting). Default: none under Mono,
#                           default on Windows
#
# Modern frameworks (net10.0) run with `dotnet test`. .NET Framework ones
# (net48) are built with `dotnet build` and run by the xunit v2 console runner
# (the project's xunit.runner.console package reference) under Mono on macOS
# and Linux. Other arguments after the project (or after `--`) go to
# `dotnet test` only.
#
# Every selected framework runs even when an earlier one failed; a summary
# with one line per framework ends the output.
#
# Exit code: 0 every selected framework passed; 1 a build or a test run
# failed, or the console runner ran no tests; 2 usage, a filter the console
# runner cannot express, or a framework the project does not target (nothing
# is run); 3 a prerequisite is missing (Mono, or xunit.runner.console) and
# nothing failed.
#
# Environment:
#   MONO           the Mono executable (default: mono on PATH)
#   XUNIT_CONSOLE  xunit.console.exe to use instead of the package's own
#
# PowerShell twin: run-tests.ps1. Keep the two in step. See
# tools/test-runners/README.md.
set -euo pipefail

usage() { sed -n '5,16p' "$0" >&2; exit 2; }

frameworks=()
config=Debug
filter=
has_filter=0
parallel=
project=
extra=()
while [ $# -gt 0 ]; do
  case "$1" in
    -f|--framework) [ $# -ge 2 ] || usage; frameworks+=("$2"); shift 2 ;;
    -c|--configuration) [ $# -ge 2 ] || usage; config=$2; shift 2 ;;
    --filter) [ $# -ge 2 ] || usage; filter=$2; has_filter=1; shift 2 ;;
    --netfx-parallel) [ $# -ge 2 ] || usage; parallel=$2; shift 2 ;;
    --) shift; extra+=("$@"); break ;;
    *)
      if [ -n "$project" ]; then extra+=("$1")
      elif [ "${1#-}" != "$1" ]; then echo "ERROR: unknown option $1" >&2; usage
      else project=$1
      fi
      shift ;;
  esac
done
[ -n "$project" ] || usage
[ ${#frameworks[@]} -gt 0 ] || frameworks=(net10.0 net48)
case "$parallel" in
  ''|none|collections|assemblies|all|default) ;;
  *) echo "ERROR: --netfx-parallel takes none, collections, assemblies, all or default" >&2; exit 2 ;;
esac

# net48, net472, net462: .NET Framework (no dot). net10.0 and other modern
# frameworks run under dotnet test.
is_netfx() { [[ "$1" =~ ^net[0-9]+$ ]]; }

on_windows=0
case "$(uname -s)" in MINGW*|MSYS*|CYGWIN*) on_windows=1 ;; esac

# Translate a dotnet test filter into xunit console options, or fail. The
# console ORs options of one kind and ANDs different kinds, so only terms of
# one kind joined by | translate exactly: FullyQualifiedName~text (or a bare
# text, dotnet test's shorthand for it), FullyQualifiedName=name, and
# Trait=value for any trait name. Test properties the console cannot select by
# (DisplayName, Name, ClassName, ...) are refused: as a trait they would match
# nothing. Property names compare case-insensitively, as dotnet test's do.
xunit_filter=()
lower() { printf '%s' "$1" | tr '[:upper:]' '[:lower:]'; }
translate_filter() {
  local expr=$1 term key value kind='' this
  local terms=()
  case "$expr" in *'&'* | *'('* | *')'* | *'!'*) return 1 ;; esac
  IFS='|' read -r -a terms <<< "$expr"
  [ ${#terms[@]} -gt 0 ] || return 1
  for term in "${terms[@]}"; do
    term=$(printf '%s' "$term" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')
    [ -n "$term" ] || return 1
    if [[ "$term" == *'~'* ]]; then
      key=${term%%'~'*}; value=${term#*'~'}
      [ "$(lower "$key")" = fullyqualifiedname ] || return 1
      this=method; xunit_filter+=(-method "*$value*")
    elif [[ "$term" == *'='* ]]; then
      key=${term%%=*}; value=${term#*=}
      [ -n "$key" ] && [ -n "$value" ] || return 1
      case "$(lower "$key")" in
        fullyqualifiedname) this=method; xunit_filter+=(-method "$value") ;;
        displayname|name|classname|testcategory|priority|id) return 1 ;;
        *) this=trait; xunit_filter+=(-trait "$key=$value") ;;
      esac
    else
      this=method; xunit_filter+=(-method "*$term*")
    fi
    [ -z "$kind" ] || [ "$kind" = "$this" ] || return 1
    kind=$this
  done
}

any_netfx=0
for tfm in "${frameworks[@]}"; do is_netfx "$tfm" && any_netfx=1; done

if [ "$any_netfx" = 1 ] && [ "$has_filter" = 1 ] && ! translate_filter "$filter"; then
  echo "ERROR: the xunit console runner cannot express the filter '$filter'." >&2
  echo "  It takes terms of one kind joined by |: FullyQualifiedName~text, FullyQualifiedName=name or Trait=value." >&2
  echo "  Simplify it, or run only the modern framework with --framework net10.0." >&2
  exit 2
fi

mono=${MONO:-mono}
if [ "$any_netfx" = 1 ] && [ "$on_windows" = 0 ] && ! command -v "$mono" >/dev/null 2>&1; then
  echo "ERROR: .NET Framework tests need Mono on macOS and Linux, and '$mono' is not on PATH." >&2
  echo "  Install it (macOS: brew install mono; Debian/Ubuntu: sudo apt-get install mono-complete)," >&2
  echo "  or run only the modern framework with --framework net10.0." >&2
  exit 3
fi

# One evaluated MSBuild property of the project; on failure, shows why and returns 1.
property() {
  local out status=0
  out=$(dotnet msbuild "$project" -nologo "-getProperty:$1" "${@:2}") || status=$?
  if [ "$status" != 0 ]; then printf 'dotnet msbuild -getProperty:%s exited %s: %s\n' "$1" "$status" "$out" >&2; return 1; fi
  printf '%s\n' "$out" | tr -d '\r' | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//'
}

targets=$(property TargetFrameworks) || { echo "ERROR: cannot evaluate $project" >&2; exit 2; }
[ -n "$targets" ] || targets=$(property TargetFramework) || { echo "ERROR: cannot evaluate $project" >&2; exit 2; }
for tfm in "${frameworks[@]}"; do
  case ";$targets;" in *";$tfm;"*) ;; *)
    echo "ERROR: $project does not target $tfm (it targets: ${targets:-nothing}). Add it to <TargetFrameworks>, or choose with --framework." >&2
    exit 2 ;;
  esac
done

# The xunit.console.exe of the xunit.runner.console version the project restored.
find_console() {
  if [ -n "${XUNIT_CONSOLE:-}" ]; then printf '%s\n' "$XUNIT_CONSOLE"; return; fi
  local assets root version
  assets=$(property ProjectAssetsFile "-p:TargetFramework=$1" "-p:Configuration=$config")
  root=$(property NuGetPackageRoot "-p:TargetFramework=$1" "-p:Configuration=$config")
  [ -f "$assets" ] && [ -n "$root" ] || return 1
  version=$(grep -io '"xunit\.runner\.console/[^"]*"' "$assets" | head -n 1 | tr -d '"' | cut -d/ -f2) || return 1
  [ -n "$version" ] || return 1
  version=$(printf '%s' "$version" | tr '[:upper:]' '[:lower:]')
  printf '%s\n' "${root%/}/xunit.runner.console/$version/tools/net48/xunit.console.exe"
}

results=()
reports=()
cleanup() { [ ${#reports[@]} -eq 0 ] || rm -f "${reports[@]}"; }
trap cleanup EXIT
failed=0
missing=0
for tfm in "${frameworks[@]}"; do
  if ! is_netfx "$tfm"; then
    echo "== $tfm (dotnet test)"
    args=(test "$project" -f "$tfm" -c "$config")
    [ "$has_filter" = 0 ] || args+=(--filter "$filter")
    status=0
    dotnet "${args[@]}" ${extra[@]+"${extra[@]}"} || status=$?
    if [ "$status" = 0 ]; then results+=("$tfm: passed")
    else results+=("$tfm: FAILED (tests, exit $status)"); failed=1
    fi
    continue
  fi

  if [ "$on_windows" = 1 ]; then echo "== $tfm (xunit console, .NET Framework)"
  else echo "== $tfm (xunit console, Mono)"
  fi
  status=0
  dotnet build "$project" -f "$tfm" -c "$config" -nologo -v quiet || status=$?
  if [ "$status" != 0 ]; then results+=("$tfm: FAILED (build, exit $status)"); failed=1; continue; fi
  dll=$(property TargetPath "-p:TargetFramework=$tfm" "-p:Configuration=$config") || dll=
  if [ ! -f "$dll" ]; then results+=("$tfm: FAILED (the build reported $dll, which does not exist)"); failed=1; continue; fi
  console=$(find_console "$tfm") || console=
  if [ -z "$console" ] || [ ! -f "$console" ]; then
    echo "ERROR: no xunit console runner${console:+ at $console}. Add <PackageReference Include=\"xunit.runner.console\" Version=\"<your xunit version>\" PrivateAssets=\"all\" /> to the test project, or set XUNIT_CONSOLE." >&2
    results+=("$tfm: not run (no xunit.runner.console)"); missing=1; continue
  fi
  mode=$parallel
  [ -n "$mode" ] || { [ "$on_windows" = 1 ] && mode=default || mode=none; }
  # The XML report counts the tests that ran: a filter or discovery problem
  # that selects none must not pass.
  report=$(mktemp "${TMPDIR:-/tmp}/run-tests-xunit.XXXXXX")
  reports+=("$report")
  run=("$console" "$dll" -xml "$report" -nologo)
  [ "$mode" = default ] || run+=(-parallel "$mode")
  run+=(${xunit_filter[@]+"${xunit_filter[@]}"})
  if [ "$on_windows" = 1 ]; then "${run[@]}" || status=$?
  else "$mono" "${run[@]}" || status=$?
  fi
  if [ "$status" != 0 ]; then results+=("$tfm: FAILED (tests, exit $status)"); failed=1; continue; fi
  total=$(grep -o '<assembly [^>]*' "$report" 2>/dev/null | grep -o ' total="[0-9]*"' | tr -dc '0-9\n' | awk '{ n += $1 } END { print n + 0 }') || true
  if [ "$total" = 0 ]; then
    echo "ERROR: the xunit console runner ran no tests on $tfm. Check the filter, and that the tests are public xunit tests." >&2
    results+=("$tfm: FAILED (no tests ran)"); failed=1
  else results+=("$tfm: passed ($total tests)")
  fi
done

echo "== summary"
printf '%s\n' "${results[@]}"
if [ "$failed" = 1 ]; then exit 1; fi
if [ "$missing" = 1 ]; then exit 3; fi
exit 0
