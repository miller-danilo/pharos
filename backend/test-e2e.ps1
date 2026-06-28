# E2E Manual Test Simulation Script for Pharos AI
# Run this script to test backend API endpoints locally.

$baseUrl = "http://localhost:5088" # Default local .NET Web API URL (modify if needed)
$testUserId = "test-user-e2e-123"

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Pharos AI E2E Test Suite" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

# 1. Test Public Scanner Endpoint (/api/shield/analyze)
Write-Host "`n[Test 1] Testing Public Scan Endpoint..." -ForegroundColor Yellow
$scanBody = @{
    text = "Busco desarrollador para tareas sencillas en Telegram. Pago inmediato en cripto. Registrarse en link sospechoso."
}
try {
    $scanResponse = Invoke-RestMethod -Uri "$baseUrl/api/shield/analyze" -Method Post -Form $scanBody
    Write-Host "✓ Scan success! Risk Level: $($scanResponse.riskLevel)" -ForegroundColor Green
    Write-Host "✓ Summary: $($scanResponse.summary)" -ForegroundColor Green
} catch {
    Write-Host "✗ Public Scan failed. Make sure the backend is running at $baseUrl" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

# 2. Test Lemon Squeezy Webhook Endpoint (/api/payment/webhook)
Write-Host "`n[Test 2] Simulating Purchase via Webhook (Adding 10 Credits)..." -ForegroundColor Yellow
$webhookPayload = @{
    meta = @{
        event_name = "order_created"
        custom_data = @{
            user_id = $testUserId
        }
    }
    data = @{
        id = "order-e2e-999"
        attributes = @{
            first_order_item = @{
                quantity = 10
            }
        }
    }
} | ConvertTo-Json -Depth 5

try {
    $headers = @{
        "Content-Type" = "application/json"
    }
    # Note: X-Signature security header is omitted to test fallback when WebhookSecret is empty
    $webhookResponse = Invoke-RestMethod -Uri "$baseUrl/api/payment/webhook" -Method Post -Headers $headers -Body $webhookPayload
    Write-Host "✓ Webhook success!" -ForegroundColor Green
    Write-Host "✓ Response: $($webhookResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "✗ Webhook simulation failed." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n===============================================" -ForegroundColor Cyan
Write-Host "E2E Testing completed." -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
