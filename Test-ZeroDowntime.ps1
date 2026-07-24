$url = "http://localhost:7233/swagger/index.html"
$successCount = 0
$errorCount = 0

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Đang bắt đầu Test Loop Zero Downtime" -ForegroundColor Cyan
Write-Host "Target URL: $url" -ForegroundColor Cyan
Write-Host "Nhấn Ctrl+C để dừng." -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

while ($true) {
    try {
        # Gửi request với ErrorAction Stop để bắt lỗi rớt mạng
        $response = Invoke-WebRequest -Uri $url -Method Get -UseBasicParsing -ErrorAction Stop
        
        if ($response.StatusCode -eq 200) {
            $successCount++
            Write-Host "[$((Get-Date).ToString('HH:mm:ss.fff'))]  HTTP 200 OK | Thành công: $successCount | Lỗi: $errorCount" -ForegroundColor Green
        }
    } catch {
        $errorCount++
        $errMsg = $_.Exception.Message
        Write-Host "[$((Get-Date).ToString('HH:mm:ss.fff'))]  LỖI RỚT MẠNG: $errMsg | Thành công: $successCount | Lỗi: $errorCount" -ForegroundColor Red
    }
    
    # Nghỉ 100ms tương đương 10 requests/giây
    Start-Sleep -Milliseconds 100
}
