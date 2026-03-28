param(
    [string]$OutputDir = (Join-Path $PSScriptRoot "..\wwwroot\images\catalog"),
    [int]$ThumbnailWidth = 1600
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$entries = @(
    @{ Slug = "toyota-vios"; Queries = @("Toyota Vios") }
    @{ Slug = "toyota-corolla-cross"; Queries = @("Toyota Corolla Cross") }
    @{ Slug = "toyota-innova-cross"; Queries = @("Toyota Innova", "Toyota Innova Cross") }
    @{ Slug = "toyota-fortuner"; Queries = @("Toyota Fortuner") }
    @{ Slug = "toyota-camry"; Queries = @("Toyota Camry") }

    @{ Slug = "hyundai-accent"; Queries = @("Hyundai Accent") }
    @{ Slug = "hyundai-creta"; Queries = @("Hyundai Creta") }
    @{ Slug = "hyundai-tucson"; Queries = @("Hyundai Tucson") }
    @{ Slug = "hyundai-santa-fe"; Queries = @("Hyundai Santa Fe") }
    @{ Slug = "hyundai-stargazer-x"; Queries = @("Hyundai Stargazer", "Hyundai Stargazer X") }

    @{ Slug = "kia-k3"; Queries = @("Kia Forte", "Kia K3") }
    @{ Slug = "kia-seltos"; Queries = @("Kia Seltos") }
    @{ Slug = "kia-sportage"; Queries = @("Kia Sportage") }
    @{ Slug = "kia-sorento"; Queries = @("Kia Sorento") }
    @{ Slug = "kia-carnival"; Queries = @("Kia Carnival") }

    @{ Slug = "ford-territory"; Queries = @("Ford Territory", "Ford Territory SUV") }
    @{ Slug = "ford-everest"; Queries = @("Ford Everest") }
    @{ Slug = "ford-ranger"; Queries = @("Ford Ranger") }
    @{ Slug = "ford-transit"; Queries = @("Ford Transit") }
    @{ Slug = "ford-explorer"; Queries = @("Ford Explorer") }

    @{ Slug = "mazda-mazda3"; Queries = @("Mazda3") }
    @{ Slug = "mazda-cx-3"; Queries = @("Mazda CX-3") }
    @{ Slug = "mazda-cx-5"; Queries = @("Mazda CX-5") }
    @{ Slug = "mazda-cx-8"; Queries = @("Mazda CX-8") }
    @{ Slug = "mazda-bt-50"; Queries = @("Mazda BT-50") }

    @{ Slug = "honda-city"; Queries = @("Honda City") }
    @{ Slug = "honda-civic"; Queries = @("Honda Civic") }
    @{ Slug = "honda-hr-v"; Queries = @("Honda HR-V") }
    @{ Slug = "honda-cr-v"; Queries = @("Honda CR-V") }
    @{ Slug = "honda-br-v"; Queries = @("Honda BR-V") }

    @{ Slug = "mitsubishi-attrage"; Queries = @("Mitsubishi Mirage G4", "Mitsubishi Attrage") }
    @{ Slug = "mitsubishi-xforce"; Queries = @("Mitsubishi Xforce") }
    @{ Slug = "mitsubishi-xpander"; Queries = @("Mitsubishi Xpander") }
    @{ Slug = "mitsubishi-pajero-sport"; Queries = @("Mitsubishi Pajero Sport") }
    @{ Slug = "mitsubishi-triton"; Queries = @("Mitsubishi Triton", "Mitsubishi L200") }

    @{ Slug = "vinfast-vf-3"; Queries = @("VinFast VF 3") }
    @{ Slug = "vinfast-vf-5"; Queries = @("VinFast VF 5") }
    @{ Slug = "vinfast-vf-6"; Queries = @("VinFast VF 6") }
    @{ Slug = "vinfast-vf-7"; Queries = @("VinFast VF 7") }
    @{ Slug = "vinfast-vf-8"; Queries = @("VinFast VF 8") }

    @{ Slug = "ferrari-296-gtb"; Queries = @("Ferrari 296 GTB") }
    @{ Slug = "ferrari-sf90-spider"; Queries = @("Ferrari SF90 Spider") }
    @{ Slug = "ferrari-12cilindri"; Queries = @("Ferrari 12Cilindri") }
    @{ Slug = "ferrari-purosangue"; Queries = @("Ferrari Purosangue") }
    @{ Slug = "ferrari-f80"; Queries = @("Ferrari F80") }

    @{ Slug = "lamborghini-temerario"; Queries = @("Lamborghini Temerario") }
    @{ Slug = "lamborghini-revuelto"; Queries = @("Lamborghini Revuelto") }
    @{ Slug = "lamborghini-urus-se"; Queries = @("Lamborghini Urus", "Lamborghini Urus SE") }
    @{ Slug = "lamborghini-huracan-sto"; Queries = @("Lamborghini Huracan STO") }
    @{ Slug = "lamborghini-huracan-sterrato"; Queries = @("Lamborghini Huracan Sterrato") }
)

$headers = @{
    "User-Agent" = "AutoCarShowroom seed image downloader/1.0"
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

function Invoke-WithRetry {
    param(
        [scriptblock]$Action,
        [string]$Operation,
        [int]$MaxAttempts = 5
    )

    $delaySeconds = 4

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        try {
            return & $Action
        }
        catch {
            if ($attempt -eq $MaxAttempts) {
                throw
            }

            Write-Warning "$Operation failed on attempt $attempt/$MaxAttempts. Retrying in $delaySeconds seconds. $($_.Exception.Message)"
            Start-Sleep -Seconds $delaySeconds
            $delaySeconds = [Math]::Min($delaySeconds * 2, 45)
        }
    }
}

function Get-ThumbnailCandidate {
    param(
        [string]$Query,
        [int]$Width
    )

    $encodedQuery = [Uri]::EscapeDataString($Query)
    $requestUrl = "https://en.wikipedia.org/w/api.php?action=query&generator=search&gsrsearch=$encodedQuery&prop=pageimages|info&inprop=url&piprop=thumbnail&pithumbsize=$Width&format=json&gsrlimit=5&origin=*"
    $response = Invoke-WithRetry -Operation "Search '$Query'" -Action {
        Invoke-RestMethod -Uri $requestUrl -Headers $headers -TimeoutSec 60
    }

    if (-not $response.query.pages) {
        return $null
    }

    $pages = $response.query.pages.PSObject.Properties.Value | Sort-Object index
    return $pages | Where-Object { $_.thumbnail.source } | Select-Object -First 1
}

function Get-FileExtension {
    param([string]$ImageUrl)

    if ($ImageUrl -match "\.(jpg|jpeg|png|webp)(?:$|\?)") {
        return "." + $matches[1].ToLowerInvariant()
    }

    return ".jpg"
}

function Remove-ExistingSlugFiles {
    param([string]$Slug)

    Get-ChildItem -Path $OutputDir -File -ErrorAction SilentlyContinue |
        Where-Object { $_.BaseName -eq $Slug } |
        Remove-Item -Force
}

$results = New-Object System.Collections.Generic.List[object]
$failures = New-Object System.Collections.Generic.List[string]

foreach ($entry in $entries) {
    $slug = $entry.Slug
    $queries = $entry.Queries
    $resolved = $null
    $resolvedQuery = $null

    Write-Host "Searching image for $slug..."

    foreach ($query in $queries) {
        try {
            $candidate = Get-ThumbnailCandidate -Query $query -Width $ThumbnailWidth

            if ($candidate) {
                $resolved = $candidate
                $resolvedQuery = $query
                break
            }
        }
        catch {
            Write-Warning "Search failed for '$query': $($_.Exception.Message)"
        }
    }

    if (-not $resolved) {
        $failures.Add($slug) | Out-Null
        Write-Warning "No image found for $slug."
        continue
    }

    $imageUrl = $resolved.thumbnail.source
    $extension = Get-FileExtension -ImageUrl $imageUrl
    $targetPath = Join-Path $OutputDir ($slug + $extension)

    Remove-ExistingSlugFiles -Slug $slug
    Invoke-WithRetry -Operation "Download '$slug'" -Action {
        Invoke-WebRequest -Uri $imageUrl -Headers $headers -OutFile $targetPath -TimeoutSec 120
    } | Out-Null

    $results.Add([pscustomobject]@{
        slug = $slug
        query = $resolvedQuery
        pageTitle = $resolved.title
        pageUrl = $resolved.fullurl
        imageUrl = $imageUrl
        localFile = (Resolve-Path $targetPath).Path
    }) | Out-Null

    Write-Host "Saved $slug to $targetPath"
    Start-Sleep -Seconds 2
}

$results |
    ConvertTo-Json -Depth 4 |
    Set-Content -Path (Join-Path $OutputDir "sources.json") -Encoding UTF8

Write-Host ""
Write-Host "Downloaded $($results.Count) of $($entries.Count) images."

if ($failures.Count -gt 0) {
    Write-Warning ("Missing images: " + ($failures -join ", "))
    exit 1
}

Write-Host "All seed images downloaded successfully."
