$badWords = @("çünkü", "dolayı", "bu yüzden", "için", "not:", "şimdilik", "todo", "bunun için", "nedeniyle", "basit olması açısından", "test verilerini", "yaptık", "ettik", "olarak kullanıyoruz", "diye yarım", "hata fırlat", "geçici")
$files = Get-ChildItem -Path "." -Include *.cs,*.xaml -Recurse -File

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Encoding UTF8
    $newContent = @()
    $changed = $false
    
    foreach ($line in $content) {
        $trimmed = $line.Trim()
        if ($trimmed.StartsWith("//") -or $trimmed.StartsWith("<!--")) {
            $lowerLine = $line.ToLower()
            $hasBadWord = $false
            foreach ($word in $badWords) {
                if ($lowerLine.Contains($word)) {
                    $hasBadWord = $true
                    break
                }
            }
            if ($hasBadWord) {
                $changed = $true
                continue
            }
        }
        $newContent += $line
    }
    
    if ($changed) {
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
        Write-Host "Temizlendi: $($file.Name)"
    }
}
