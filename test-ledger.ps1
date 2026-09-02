$BaseUrl = "http://localhost:5260/api"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "TESTING CUSTOMER LEDGER / TRANSACTIONS API" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Login to get Token
$loginData = @{ username = "admin_sales"; password = "Password123!" }
$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method Post -Body ($loginData | ConvertTo-Json) -ContentType "application/json"
$token = $loginResponse.token
$headers = @{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }
Write-Host "Logged in. Token retrieved."

# 1. Create a Customer
$randomStr = Get-Random
$customerData = @{
    name = "Ledger Test Customer $randomStr"
    mobileNumber = "$randomStr"
    openingBalance = 2000
    isActive = $true
}
$customerResponse = Invoke-RestMethod -Uri "$BaseUrl/customers" -Method Post -Body ($customerData | ConvertTo-Json) -Headers $headers
$customerId = $customerResponse.id
Write-Host "Created Customer ID: $customerId with Opening Balance: 2000"

# 2. Create a Product
$productData = @{
    productName = "Ledger Rice $randomStr"
    brandName = "Brand L"
    bagSize = 25
    sellingPrice = 1200
    minimumStockLevel = 10
    currentStock = 0
    isActive = $true
}
$productResponse = Invoke-RestMethod -Uri "$BaseUrl/products" -Method Post -Body ($productData | ConvertTo-Json) -Headers $headers
$productId = $productResponse.id
Write-Host "Created Product ID: $productId"

# 3. Add Stock (50 bags)
$stockData = @{
    productId = $productId
    quantity = 50
    notes = "Initial Stock for Ledger"
}
Invoke-RestMethod -Uri "$BaseUrl/stock/in" -Method Post -Body ($stockData | ConvertTo-Json) -Headers $headers

# 4. Perform Transactions
Write-Host "`nTest: Sale 1 (10 bags * 1200 = 12000. Paid: 5000)" -ForegroundColor Yellow
$saleData = @{
    customerId = $customerId
    paidAmount = 5000
    paymentMode = "Cash"
    saleItems = @( @{ productId = $productId; quantity = 10; rate = 1200 } )
}
$saleResp1 = Invoke-RestMethod -Uri "$BaseUrl/sales" -Method Post -Body ($saleData | ConvertTo-Json -Depth 5) -Headers $headers
Write-Host "Sale 1 created. Total: $($saleResp1.totalAmount), Balance: $($saleResp1.balanceAmount)"
Write-Host "Expected Balance: 9000 (2000 + 12000 - 5000 = 9000)"

Write-Host "`nTest: Payment 1 (3000)" -ForegroundColor Yellow
$paymentData = @{ customerId = $customerId; amount = 3000; paymentMode = "Cash" }
Invoke-RestMethod -Uri "$BaseUrl/payments" -Method Post -Body ($paymentData | ConvertTo-Json) -Headers $headers
Write-Host "Expected Balance: 6000"

Write-Host "`nTest: Payment 2 (2000)" -ForegroundColor Yellow
$paymentData2 = @{ customerId = $customerId; amount = 2000; paymentMode = "UPI" }
Invoke-RestMethod -Uri "$BaseUrl/payments" -Method Post -Body ($paymentData2 | ConvertTo-Json) -Headers $headers
Write-Host "Expected Balance: 4000"

Write-Host "`nTest: Sale 2 (5 bags * 1000 = 5000. Paid: 0)" -ForegroundColor Yellow
$saleData2 = @{
    customerId = $customerId
    paidAmount = 0
    paymentMode = "Cash"
    saleItems = @( @{ productId = $productId; quantity = 5; rate = 1000 } )
}
$saleResp2 = Invoke-RestMethod -Uri "$BaseUrl/sales" -Method Post -Body ($saleData2 | ConvertTo-Json -Depth 5) -Headers $headers
Write-Host "Sale 2 created. Total: $($saleResp2.totalAmount), Balance: $($saleResp2.balanceAmount)"
Write-Host "Expected Balance: 9000"

# 5. Check Account Summary
Write-Host "`nFetching Account Summary..." -ForegroundColor Cyan
$summary = Invoke-RestMethod -Uri "$BaseUrl/customers/$customerId/account-summary" -Headers $headers
Write-Host "Customer: $($summary.customerName)"
Write-Host "Opening Balance: $($summary.openingBalance)"
Write-Host "Total Sales (Debt added to balance): $($summary.totalSales)"
Write-Host "Total Payments: $($summary.totalPayments)"
Write-Host "Current Outstanding: $($summary.currentOutstandingBalance)"
if ($summary.currentOutstandingBalance -eq 9000) { Write-Host "Balance matches expectation (9000)." -ForegroundColor Green } else { Write-Host "Balance mismatch!" -ForegroundColor Red }

# 6. Check Transaction Ledger
Write-Host "`nFetching Ledger Transactions..." -ForegroundColor Cyan
$transactions = Invoke-RestMethod -Uri "$BaseUrl/customers/$customerId/transactions" -Headers $headers

Write-Host "ID | Date                 | Type    | RefType | Debit   | Credit  | Running Balance" -ForegroundColor Yellow
foreach ($tx in $transactions) {
    # Print list in original chronological order since the API returns them ordered by date descending
    # Wait, they are returned latest first, so let's reverse to print chronologically for the test output
}
[array]::Reverse($transactions)

foreach ($tx in $transactions) {
    $dateStr = [DateTime]::Parse($tx.transactionDate).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host "$($tx.id.ToString().PadRight(2)) | $dateStr | $($tx.transactionType.PadRight(7)) | $($tx.referenceType.PadRight(7)) | $($tx.debit.ToString().PadRight(7)) | $($tx.credit.ToString().PadRight(7)) | $($tx.runningBalance)"
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "ALL LEDGER TESTS EXECUTED" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
