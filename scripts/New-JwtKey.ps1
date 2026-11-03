# Kør fra RESTmusic-mappen: .\scripts\New-JwtKey.ps1
# Kopiér derefter nøglen ind i Azure-indstillingen Jwt__Key.
# Nøglen vises ikke i terminalen og gemmes ikke i en fil.
$keyBytes = New-Object byte[] 32
$generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $generator.GetBytes($keyBytes)
    [Convert]::ToBase64String($keyBytes) | Set-Clipboard
    Write-Output 'En ny JWT-nøgle er kopieret til udklipsholderen. Indsæt den som Jwt__Key i Azure.'
}
finally {
    $generator.Dispose()
}
