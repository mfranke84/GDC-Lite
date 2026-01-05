#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:8080}"

echo "== GDC-Lite smoke test =="
echo "BASE_URL=$BASE_URL"
echo

# helper: wait until ready returns 200 (max 30s)
echo "Waiting for /ready ..."
for i in {1..30}; do
  code="$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/ready" || true)"
  echo "  attempt $i: /ready -> $code"
  if [[ "$code" == "200" ]]; then
    break
  fi
  sleep 1
done

code="$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/ready" || true)"
if [[ "$code" != "200" ]]; then
  echo "ERROR: Service not ready after 30s."
  echo "Try: curl -i $BASE_URL/ready"
  exit 1
fi

echo
echo "Checking /health ..."
curl -fsS "$BASE_URL/health" >/dev/null
echo "  OK"

echo
echo "Creating an order ..."
VIN="WVWZZZ1JZXW000001"
payload="$(cat <<'JSON'
{
  "vin": "WVWZZZ1JZXW000001",
  "dataScope": "telemetry-basic"
}
JSON
)"

create_resp="$(curl -fsS -H "Content-Type: application/json" -d "$payload" "$BASE_URL/orders")"
echo "  Response: $create_resp"

# extract orderId without jq (portable)
order_id="$(python3 - <<'PY'
import json,sys
obj=json.loads(sys.argv[1])
print(obj.get("orderId",""))
PY
"$create_resp")"

if [[ -z "$order_id" ]]; then
  echo "ERROR: orderId missing in response."
  exit 1
fi
echo "  orderId=$order_id"

echo
echo "Fetching orders by VIN ..."
curl -fsS "$BASE_URL/orders/$VIN" >/dev/null
echo "  OK"

echo
echo "Validating order ..."
curl -fsS -X POST "$BASE_URL/orders/$order_id/validate" >/dev/null
echo "  OK"

echo
echo "✅ Smoke test passed."
