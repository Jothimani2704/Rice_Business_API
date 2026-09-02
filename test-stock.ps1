$BaseUrl = "http://localhost:5260/api"

Write-Host "==========================================" -ForegroundColor Cyan
# Auto-register admin user (ignore if exists)
$registerData = @{ username = "admin_stock"; password = "Password123!" }
try {
    Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method Post -Body ($registerData | ConvertTo-Json) -ContentType "application/json" -ErrorAction Stop
} catch {}

# First Login to get Token
$loginData = @{ username = "admin_stock"; password = "Password123!" }
$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method Post -Body ($loginData | ConvertTo-Json) -ContentType "application/json"
$token = $loginResponse.token
$headers = @{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }

Write-Host "Logged in. Token retrieved." -ForegroundColor Green

# 1. Create a dummy product for testing
$productData = @{
    brandName = "Test Brand"
    productName = "Test Rice"
    bagSize = 25
    sellingPrice = 1000
    minimumStockLevel = 10
}
$productJson = $productData | ConvertTo-Json
$productResponse = Invoke-RestMethod -Uri "$BaseUrl/products" -Method Post -Body $productJson -Headers $headers
$productId = $productResponse.id
Write-Host "Created Product ID: $productId" -ForegroundColor Green

# 2. Add Stock (In)
Write-Host "`nTest: Add stock (20 bags)" -ForegroundColor Yellow
$stockIn = @{ productId = $productId; quantity = 20; notes = "Initial Stock" }
$stockInResponse = Invoke-RestMethod -Uri "$BaseUrl/stock/in" -Method Post -Body ($stockIn | ConvertTo-Json) -Headers $headers
Write-Host "Stock In Response: $($stockInResponse.quantity) bags added. New Stock: $($stockInResponse.newStock)" -ForegroundColor Green

# 3. View Current Stock
Write-Host "`nTest: View current stock (should be 20)" -ForegroundColor Yellow
$stockResponse = Invoke-RestMethod -Uri "$BaseUrl/stock" -Headers $headers
$stockItem = $stockResponse | Where-Object { $_.id -eq $productId }
Write-Host "Current Stock: $($stockItem.currentStock)" -ForegroundColor Green

# 4. Add multiple stock entries (Adjustment)
Write-Host "`nTest: Adjust stock (Add 5)" -ForegroundColor Yellow
$stockAdj = @{ productId = $productId; transactionType = 1; quantity = 5; notes = "Adjustment Add" }
$stockAdjResp = Invoke-RestMethod -Uri "$BaseUrl/stock/adjustment" -Method Post -Body ($stockAdj | ConvertTo-Json) -Headers $headers
Write-Host "Adjustment Response: New Stock: $($stockAdjResp.newStock)" -ForegroundColor Green

# 5. Stock-out validation (Subtract 10)
Write-Host "`nTest: Adjust stock (Subtract 10)" -ForegroundColor Yellow
$stockAdjOut = @{ productId = $productId; transactionType = 2; quantity = 10; notes = "Adjustment Subtract" }
$stockAdjOutResp = Invoke-RestMethod -Uri "$BaseUrl/stock/adjustment" -Method Post -Body ($stockAdjOut | ConvertTo-Json) -Headers $headers
Write-Host "Adjustment Response: New Stock: $($stockAdjOutResp.newStock)" -ForegroundColor Green

# 6. Negative quantity (Should fail)
Write-Host "`nTest: Negative quantity" -ForegroundColor Yellow
try {
    $negStock = @{ productId = $productId; quantity = -5; notes = "Negative" }
    Invoke-RestMethod -Uri "$BaseUrl/stock/in" -Method Post -Body ($negStock | ConvertTo-Json) -Headers $headers -ErrorAction Stop
} catch {
    Write-Host "Expected Failure: $($_.Exception.Message)" -ForegroundColor Green
}

# 7. Zero quantity (Should fail)
Write-Host "`nTest: Zero quantity" -ForegroundColor Yellow
try {
    $zeroStock = @{ productId = $productId; quantity = 0; notes = "Zero" }
    Invoke-RestMethod -Uri "$BaseUrl/stock/in" -Method Post -Body ($zeroStock | ConvertTo-Json) -Headers $headers -ErrorAction Stop
} catch {
    Write-Host "Expected Failure: $($_.Exception.Message)" -ForegroundColor Green
}

# 8. Stock-out greater than available quantity (Should fail, available is 15, request 20)
Write-Host "`nTest: Over-subtract quantity" -ForegroundColor Yellow
try {
    $overStock = @{ productId = $productId; transactionType = 2; quantity = 20; notes = "Over subtract" }
    Invoke-RestMethod -Uri "$BaseUrl/stock/adjustment" -Method Post -Body ($overStock | ConvertTo-Json) -Headers $headers -ErrorAction Stop
} catch {
    Write-Host "Expected Failure: $($_.Exception.Message)" -ForegroundColor Green
}

# 9. Invalid product (Should fail)
Write-Host "`nTest: Invalid product" -ForegroundColor Yellow
try {
    $invalidProd = @{ productId = 999999; quantity = 5; notes = "Invalid" }
    Invoke-RestMethod -Uri "$BaseUrl/stock/in" -Method Post -Body ($invalidProd | ConvertTo-Json) -Headers $headers -ErrorAction Stop
} catch {
    Write-Host "Expected Failure: $($_.Exception.Message)" -ForegroundColor Green
}

# 10. Inactive product (Should fail)
Write-Host "`nTest: Inactive product" -ForegroundColor Yellow
# First deactivate
Invoke-RestMethod -Uri "$BaseUrl/products/$productId/status" -Method Patch -Body "false" -Headers $headers
try {
    $inactiveProd = @{ productId = $productId; quantity = 5; notes = "Inactive" }
    Invoke-RestMethod -Uri "$BaseUrl/stock/in" -Method Post -Body ($inactiveProd | ConvertTo-Json) -Headers $headers -ErrorAction Stop
} catch {
    Write-Host "Expected Failure: $($_.Exception.Message)" -ForegroundColor Green
}
# Reactivate
Invoke-RestMethod -Uri "$BaseUrl/products/$productId/status" -Method Patch -Body "true" -Headers $headers

# 11. Low-stock calculation
Write-Host "`nTest: Low stock calculation" -ForegroundColor Yellow
# Reduce stock to below minimum level (10)
$lowStockOut = @{ productId = $productId; transactionType = 2; quantity = 10; notes = "Reduce to low stock" }
Invoke-RestMethod -Uri "$BaseUrl/stock/adjustment" -Method Post -Body ($lowStockOut | ConvertTo-Json) -Headers $headers
$lowStockResp = Invoke-RestMethod -Uri "$BaseUrl/stock/low-stock" -Headers $headers
$isLowStock = $lowStockResp | Where-Object { $_.productId -eq $productId }
if ($isLowStock) {
    Write-Host "Product successfully identified as low stock! (Current: $($isLowStock.currentStock))" -ForegroundColor Green
} else {
    Write-Host "Failed to identify low stock" -ForegroundColor Red
}

# 12. Stock History
Write-Host "`nTest: View stock history" -ForegroundColor Yellow
$historyResp = Invoke-RestMethod -Uri "$BaseUrl/stock/$productId/history" -Headers $headers
Write-Host "Found $($historyResp.Length) transaction(s) for product" -ForegroundColor Green
foreach ($txn in $historyResp) {
    Write-Host "- Type: $($txn.transactionType), Qty: $($txn.quantity), Old: $($txn.previousStock), New: $($txn.newStock), Notes: $($txn.notes)" -ForegroundColor Gray
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "ALL TESTS EXECUTED" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
