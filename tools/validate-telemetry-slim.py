"""
Validate slim TelemetryPersistence JSON sizes for 1/2/4 bots (must stay under 4000).
Run: python tools/validate-telemetry-slim.py
"""
from __future__ import annotations

import json
import sys

SUMMARY_FIELDS = {
    "AvgHandSeconds": 22.5,
    "LastHandDeltaSeconds": 21.3,
    "LastTwoHandDeltaSeconds": [22.1, 21.3],
    "RapidL5TriggerActive": False,
    "CurrentStreak": 5,
    "SecurityRiskScore": 2,
    "SecurityFilterActive": False,
    "PauseBot": False,
    "PauseScope": "NONE",
    "PauseComputer": "",
    "PreventedL6": 0,
    "LastShoeHand": 15,
    "Martingala": 1,
    "HasL6Credit": True,
    "LastReason": "score 1/4",
    "L6PlayedCount": 2,
    "AuthorizedL8LostCount": 0,
}


def build_slim(n_bots: int) -> dict:
    bots = {f"PC{i + 1}": dict(SUMMARY_FIELDS) for i in range(n_bots)}
    return {
        "TotalPBHandsPlayed": 221,
        "TotalAuthL6Authorized": 2,
        "TotalL5Played": 8,
        "TotalL5Won": 5,
        "TotalL5Lost": 3,
        "TotalL8Played": 0,
        "TotalL8Won": 0,
        "TotalL8Lost": 0,
        "BotMargins": {k: 84.15 for k in bots},
        "SpotID": 1,
        "SpotPBHandsPlayed": 21,
        "SpotAuthL6Counter": 0,
        "SpotL5Loss": 0,
        "GlobalPauseScalping": False,
        "GlobalPauseScalpingDetails": "Pausa non attiva",
        "GlobalPauseScalpingDuration": "0",
        "INC": 0,
        "EWMA": 0,
        "TotalPauseScalpingSoglieActivated": 0,
        "TotalPauseScalpingEWMAActivated": 0,
        "TotalSecurityFilterActivated": 0,
        "TotalSecurityFilterPreventedL6": 0,
        "LastAvgHandSeconds": 22.0,
        "ActiveSecurityFilterBots": 0,
        "SecurityFilterByBot": bots,
    }


def main() -> int:
    failed = False
    print("=== Slim telemetry size validation (target <= 4000) ===")
    for n in (1, 2, 4, 8):
        payload = build_slim(n)
        raw = json.dumps(payload, separators=(",", ":"))
        sf = json.dumps(payload["SecurityFilterByBot"], separators=(",", ":"))
        ok = len(raw) <= 4000
        status = "OK" if ok else "FAIL"
        print(f"  {n} bot: total={len(raw):4d} SecurityFilterByBot={len(sf):4d} [{status}]")
        try:
            json.loads(raw)
        except json.JSONDecodeError as exc:
            print(f"    JSON invalid: {exc}")
            failed = True
            continue
        if not ok:
            failed = True

    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
