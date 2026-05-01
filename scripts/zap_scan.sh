#!/usr/bin/env bash
set -euo pipefail

HOST_APP_BASE_URL="${HOST_APP_BASE_URL:-http://localhost:8080}"
ZAP_APP_BASE_URL="${ZAP_APP_BASE_URL:-http://host.docker.internal:8080}"
LOGIN_URL="${LOGIN_URL:-$HOST_APP_BASE_URL/api/auth/login}"
OPENAPI_URL="${OPENAPI_URL:-$ZAP_APP_BASE_URL/swagger/v1/swagger.json}"
REPORT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/zap-report"
ADMIN_USERNAME="${ADMIN_USERNAME:-admin}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-Admin@123}"

mkdir -p "$REPORT_DIR"

TOKEN_RESPONSE="$(curl -sS -X POST "$LOGIN_URL" \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$ADMIN_USERNAME\",\"password\":\"$ADMIN_PASSWORD\"}")"

JWT_TOKEN="$(python3 -c 'import json,sys; print(json.load(sys.stdin)["token"])' <<< "$TOKEN_RESPONSE")"

/usr/local/bin/docker run --rm \
  -v "$REPORT_DIR:/zap/wrk/:rw" \
  -e ZAP_AUTH_HEADER_VALUE="Bearer $JWT_TOKEN" \
  -e ZAP_AUTH_HEADER_SITE="host.docker.internal" \
  zaproxy/zap-stable \
  zap-api-scan.py \
  -t "$OPENAPI_URL" \
  -f openapi \
  -I \
  -r zap-report.html \
  -J zap-report.json \
  -x zap-report.xml
