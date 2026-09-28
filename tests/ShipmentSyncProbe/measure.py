"""Measure port interactions on an already prepared, leased test client/server."""
import argparse
import json
import shlex
import subprocess
import time
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument("--client", required=True)
parser.add_argument("--server", required=True)
parser.add_argument("--source", required=True)
parser.add_argument("--output", type=Path, required=True)
parser.add_argument("--count", type=int, default=10)
args = parser.parse_args()
if args.count < 1:
    parser.error("--count must be positive")


def remote(host, command):
    return subprocess.check_output(["ssh", "-o", "BatchMode=yes", host, command], text=True)


start = int(time.time() * 1000)
actions = []
for index in range(args.count):
    output = remote(args.client, "~/.local/bin/valheim-cli mwl_probe_open")
    if "ERROR:" in output or "OK:" not in output:
        raise RuntimeError(output)
    actions.append(output.strip())
    remote(args.client, "DISPLAY=:0 XAUTHORITY=/run/user/1000/gdm/Xauthority xdotool key Escape")
    time.sleep(1)
end = int(time.time() * 1000)

# Wait for the collector to receive the end of the measurement window.
for attempt in range(12):
    sessions = json.loads(remote(args.server, "curl -fsS --max-time 10 http://127.0.0.1:8770/api/sessions"))
    if any(session["source"] == args.source and session["max_ts"] >= end for session in sessions["sessions"]):
        break
    time.sleep(5)
else:
    raise RuntimeError("Monitor samples did not reach the end of the measurement window")

from urllib.parse import urlencode
query = urlencode({"from": start, "to": end, "n": 250, "source": args.source,
                   "filter": "RS_PortInitManager", "direction": "outbound"})
data = json.loads(remote(args.server, "curl -fsS --max-time 15 " + shlex.quote("http://127.0.0.1:8770/api/net/top?" + query)))
result = {"from": start, "to": end, "source": args.source, "interactions": actions, "network": data}
args.output.write_text(json.dumps(result, indent=2) + "\n")
print(json.dumps(data["table"], indent=2))
