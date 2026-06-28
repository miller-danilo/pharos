#!/bin/bash
# E2E Manual Test Simulation Script for Pharos AI using curl

BASE_URL="http://localhost:5088"
TEST_USER_ID="test-user-e2e-123"

echo "==============================================="
echo "Pharos AI E2E Test Suite (curl version)"
echo "==============================================="

# 1. Test Public Scanner Endpoint (/api/shield/analyze)
echo -e "\n[Test 1] Testing Public Scan Endpoint..."
scan_response=$(curl -s -X POST "$BASE_URL/api/shield/analyze" \
  -F "text=Busco desarrollador para tareas sencillas en Telegram. Pago inmediato en cripto. Registrarse en link sospechoso.")

if [ $? -eq 0 ] && [ ! -z "$scan_response" ]; then
  echo "✓ Scan response received:"
  echo "$scan_response"
else
  echo "✗ Public Scan failed. Make sure the backend is running at $BASE_URL"
fi

# 2. Test Lemon Squeezy Webhook Endpoint (/api/payment/webhook)
echo -e "\n[Test 2] Simulating Purchase via Webhook (Adding 10 Credits)..."
webhook_payload=$(cat <<EOF
{
  "meta": {
    "event_name": "order_created",
    "custom_data": {
      "user_id": "$TEST_USER_ID"
    }
  },
  "data": {
    "id": "order-e2e-999",
    "attributes": {
      "first_order_item": {
        "quantity": 10
      }
    }
  }
}
EOF
)

webhook_response=$(curl -s -X POST "$BASE_URL/api/payment/webhook" \
  -H "Content-Type: application/json" \
  -d "$webhook_payload")

if [ $? -eq 0 ] && [ ! -z "$webhook_response" ]; then
  echo "✓ Webhook response received:"
  echo "$webhook_response"
else
  echo "✗ Webhook simulation failed."
fi

echo -e "\n==============================================="
echo "E2E Testing completed."
echo "==============================================="
