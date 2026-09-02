$baseUrl = "http://localhost:5260/api"

# 12. Verify existing Authentication APIs still work
Write-Host "--- TEST AUTHENTICATION ---"
$loginBody = @{ Username = "admin"; Password = "password123" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.token
Write-Host "Login successful. Token acquired."

$headers = @{ Authorization = "Bearer $token" }

# 7. Test unauthorized requests
Write-Host "--- TEST UNAUTHORIZED REQUESTS ---"
try {
    Invoke-RestMethod -Uri "$baseUrl/customers" -Method Get
    Write-Host "FAIL: Unauthorized request succeeded."
} catch {
    Write-Host "PASS: Unauthorized request failed with $($_.Exception.Response.StatusCode)."
}

# 8. Test authorized requests (GET Customers)
Write-Host "--- TEST AUTHORIZED REQUESTS ---"
$customers = Invoke-RestMethod -Uri "$baseUrl/customers" -Method Get -Headers $headers
Write-Host "PASS: Authorized request succeeded."

# 3. Verify Customer APIs
Write-Host "--- TEST CUSTOMER APIS ---"
$customerBody = @{ Name = "John Doe"; MobileNumber = "1234567890"; Address = "123 Main St"; OpeningBalance = 100 } | ConvertTo-Json
$newCustomer = Invoke-RestMethod -Uri "$baseUrl/customers" -Method Post -Body $customerBody -ContentType "application/json" -Headers $headers
Write-Host "Created Customer: $($newCustomer.id)"

# 6. Test duplicate records (Customer)
Write-Host "--- TEST DUPLICATE CUSTOMER ---"
try {
    Invoke-RestMethod -Uri "$baseUrl/customers" -Method Post -Body $customerBody -ContentType "application/json" -Headers $headers
    Write-Host "FAIL: Duplicate customer created."
} catch {
    Write-Host "PASS: Duplicate customer rejected."
}

# 5. Test validation errors (Customer - missing name)
Write-Host "--- TEST VALIDATION ERRORS ---"
try {
    $invalidCustomerBody = @{ MobileNumber = "0987654321"; OpeningBalance = -50 } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/customers" -Method Post -Body $invalidCustomerBody -ContentType "application/json" -Headers $headers
    Write-Host "FAIL: Invalid customer created."
} catch {
    Write-Host "PASS: Invalid customer rejected."
}

# 9. Test invalid IDs (Customer)
Write-Host "--- TEST INVALID IDS ---"
try {
    Invoke-RestMethod -Uri "$baseUrl/customers/999" -Method Get -Headers $headers
    Write-Host "FAIL: Found invalid customer."
} catch {
    Write-Host "PASS: Invalid customer ID returned 404."
}

# 10. Test inactive customers
Write-Host "--- TEST INACTIVE CUSTOMERS ---"
Invoke-RestMethod -Uri "$baseUrl/customers/$($newCustomer.id)/status" -Method Patch -Body "false" -ContentType "application/json" -Headers $headers
$updatedCustomer = Invoke-RestMethod -Uri "$baseUrl/customers/$($newCustomer.id)" -Method Get -Headers $headers
Write-Host "Customer IsActive: $($updatedCustomer.isActive)"

# 4. Verify Product APIs
Write-Host "--- TEST PRODUCT APIS ---"
$productBody = @{ BrandName = "Ponni Rice"; ProductName = "25 KG"; BagSize = 25; SellingPrice = 1200; MinimumStockLevel = 10 } | ConvertTo-Json
$newProduct = Invoke-RestMethod -Uri "$baseUrl/products" -Method Post -Body $productBody -ContentType "application/json" -Headers $headers
Write-Host "Created Product: $($newProduct.id)"

# 6. Test duplicate records (Product)
Write-Host "--- TEST DUPLICATE PRODUCT ---"
try {
    Invoke-RestMethod -Uri "$baseUrl/products" -Method Post -Body $productBody -ContentType "application/json" -Headers $headers
    Write-Host "FAIL: Duplicate product created."
} catch {
    Write-Host "PASS: Duplicate product rejected."
}

# Search tests
Write-Host "--- TEST SEARCH ---"
$searchCustomer = Invoke-RestMethod -Uri "$baseUrl/customers/search?q=John" -Method Get -Headers $headers
Write-Host "Customer Search Results: $($searchCustomer.Count)"
$searchProduct = Invoke-RestMethod -Uri "$baseUrl/products/search?q=Ponni" -Method Get -Headers $headers
Write-Host "Product Search Results: $($searchProduct.Count)"

Write-Host "ALL TESTS COMPLETED"
