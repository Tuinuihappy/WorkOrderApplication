$ErrorActionPreference = "Continue"

Write-Host "--- 1. Testing Unprotected Access (401 Expected) ---"
try {
    $res = Invoke-RestMethod -Uri "http://127.0.0.1:5034/api/workorders" -Method Get
    Write-Host "FAILED: Expected 401 but got success"
} catch {
    Write-Host "SUCCESS: Protected endpoint returned $($_)"
}

Write-Host "`n--- 2. Testing User Creation (Register) ---"
$userJson = @{
    UserName = "TestUser99"
    EmployeeId = "TU99"
    Position = "Tester"
    Department = "QA"
    Shift = "Day"
    Email = "test99@test.com"
    Role = "Admin"
    Password = "Password123"
} | ConvertTo-Json

try {
    $resCreate = Invoke-RestMethod -Uri "http://127.0.0.1:5034/api/users" -Method Post -Body $userJson -ContentType "application/json"
    Write-Host "SUCCESS: User created. ID: $($resCreate.id)"
} catch {
    Write-Host "Note: User might already exist or error $($_)"
}

Write-Host "`n--- 3. Testing Login ---"
$loginJson = @{
    EmployeeId = "TU99"
    Password = "Password123"
} | ConvertTo-Json

$token = $null
try {
    $resLogin = Invoke-RestMethod -Uri "http://127.0.0.1:5034/api/auth/login" -Method Post -Body $loginJson -ContentType "application/json"
    $token = $resLogin.token
    Write-Host "SUCCESS: Logged in! Token: $($token.Substring(0, 30))..."
} catch {
    Write-Host "FAILED: Could not login. $($_)"
}

Write-Host "`n--- 4. Testing Protected Access with Token ---"
if ($token) {
    try {
        $headers = @{ "Authorization" = "Bearer $token" }
        $resAuth = Invoke-RestMethod -Uri "http://127.0.0.1:5034/api/workorders" -Method Get -Headers $headers
        Write-Host "SUCCESS: Can access /api/workorders. Count: $($resAuth.Count)"
    } catch {
        Write-Host "FAILED: Access denied with token. $($_)"
    }
} else {
    Write-Host "FAILED: Skipped because no token."
}
