param(
    [string]$OutputDir = (Join-Path $PSScriptRoot "..\wwwroot\images\catalog"),
    [int]$ThumbnailWidth = 1600,
    [int]$InteriorWidth = 1600
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
    "User-Agent" = "AutoCarShowroom seed image downloader/2.0"
}

$interiorDir = Join-Path $OutputDir "interior"
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
New-Item -ItemType Directory -Path $interiorDir -Force | Out-Null

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

function Get-FileExtension {
    param([string]$ImageUrl)

    if ($ImageUrl -match "\.(jpg|jpeg|png|webp)(?:$|\?)") {
        return "." + $matches[1].ToLowerInvariant()
    }

    return ".jpg"
}

function Resolve-ExistingSlugFile {
    param(
        [string]$Directory,
        [string]$Slug
    )

    return Get-ChildItem -Path $Directory -File -ErrorAction SilentlyContinue |
        Where-Object { $_.BaseName -eq $Slug } |
        Select-Object -First 1
}

function Remove-ExistingSlugFiles {
    param(
        [string]$Directory,
        [string]$Slug
    )

    Get-ChildItem -Path $Directory -File -ErrorAction SilentlyContinue |
        Where-Object { $_.BaseName -eq $Slug } |
        Remove-Item -Force
}

function Get-OverviewCandidate {
    param(
        [string[]]$Queries,
        [int]$Width
    )

    foreach ($query in $Queries) {
        $encodedQuery = [Uri]::EscapeDataString($query)
        $requestUrl = "https://en.wikipedia.org/w/api.php?action=query&generator=search&gsrsearch=$encodedQuery&prop=pageimages|info&inprop=url&piprop=thumbnail&pithumbsize=$Width&format=json&gsrlimit=5&origin=*"
        $response = Invoke-WithRetry -Operation "Search overview '$query'" -Action {
            Invoke-RestMethod -Uri $requestUrl -Headers $headers -TimeoutSec 60
        }

        if (-not $response.query -or -not $response.query.pages) {
            continue
        }

        $pages = $response.query.pages.PSObject.Properties.Value | Sort-Object index
        $candidate = $pages | Where-Object { $_.thumbnail.source } | Select-Object -First 1
        if ($candidate) {
            return [pscustomobject]@{
                query = $query
                title = $candidate.title
                pageUrl = $candidate.fullurl
                imageUrl = $candidate.thumbnail.source
            }
        }
    }

    return $null
}

function Get-InteriorCandidate {
    param(
        [string[]]$Queries,
        [int]$Width
    )

    $keywords = @("interior", "cockpit", "dashboard", "cabin")

    foreach ($query in $Queries) {
        foreach ($keyword in $keywords) {
            $searchTerm = "$query $keyword"
            $encodedTerm = [Uri]::EscapeDataString($searchTerm)
            $requestUrl = "https://commons.wikimedia.org/w/api.php?action=query&generator=search&gsrnamespace=6&gsrsearch=$encodedTerm&prop=imageinfo&iiprop=url&iiurlwidth=$Width&format=json&gsrlimit=10&origin=*"
            $response = Invoke-WithRetry -Operation "Search interior '$searchTerm'" -Action {
                Invoke-RestMethod -Uri $requestUrl -Headers $headers -TimeoutSec 60
            }

            if (-not $response.query -or -not $response.query.pages) {
                continue
            }

            $pages = $response.query.pages.PSObject.Properties.Value | Sort-Object index
            $candidate = $pages |
                Where-Object { $_.imageinfo -and ($_.title -match 'interior|cockpit|dashboard|cabin') } |
                Select-Object -First 1

            if (-not $candidate) {
                $candidate = $pages |
                    Where-Object { $_.imageinfo } |
                    Select-Object -First 1
            }

            if ($candidate) {
                $imageInfo = $candidate.imageinfo[0]
                $imageUrl = if ($imageInfo.thumburl) { $imageInfo.thumburl } else { $imageInfo.url }
                return [pscustomobject]@{
                    query = $searchTerm
                    title = $candidate.title
                    pageUrl = $imageInfo.descriptionurl
                    imageUrl = $imageUrl
                }
            }
        }
    }

    return $null
}

$overviewResults = New-Object System.Collections.Generic.List[object]
$interiorResults = New-Object System.Collections.Generic.List[object]
$overviewFailures = New-Object System.Collections.Generic.List[string]
$interiorFailures = New-Object System.Collections.Generic.List[string]

foreach ($entry in $entries) {
    $slug = $entry.Slug
    $queries = $entry.Queries

    Write-Host "Processing $slug..."

    $existingOverview = Resolve-ExistingSlugFile -Directory $OutputDir -Slug $slug
    if (-not $existingOverview) {
        $overviewCandidate = $null
        try {
            $overviewCandidate = Get-OverviewCandidate -Queries $queries -Width $ThumbnailWidth
        }
        catch {
            Write-Warning "Overview search failed for ${slug}: $($_.Exception.Message)"
        }

        if ($overviewCandidate) {
            $extension = Get-FileExtension -ImageUrl $overviewCandidate.imageUrl
            $targetPath = Join-Path $OutputDir ($slug + $extension)
            Remove-ExistingSlugFiles -Directory $OutputDir -Slug $slug
            Invoke-WithRetry -Operation "Download overview '$slug'" -Action {
                Invoke-WebRequest -Uri $overviewCandidate.imageUrl -Headers $headers -OutFile $targetPath -TimeoutSec 120
            } | Out-Null

            $overviewResults.Add([pscustomobject]@{
                slug = $slug
                query = $overviewCandidate.query
                pageTitle = $overviewCandidate.title
                pageUrl = $overviewCandidate.pageUrl
                imageUrl = $overviewCandidate.imageUrl
                localFile = (Resolve-Path $targetPath).Path
            }) | Out-Null
        }
        else {
            $overviewFailures.Add($slug) | Out-Null
            Write-Warning "No overview image found for $slug."
        }
    }
    else {
        Write-Host "  Overview already exists, skipping download."
    }

    $existingInterior = Resolve-ExistingSlugFile -Directory $interiorDir -Slug $slug
    if (-not $existingInterior) {
        $interiorCandidate = $null
        try {
            $interiorCandidate = Get-InteriorCandidate -Queries $queries -Width $InteriorWidth
        }
        catch {
            Write-Warning "Interior search failed for ${slug}: $($_.Exception.Message)"
        }

        if ($interiorCandidate) {
            $extension = Get-FileExtension -ImageUrl $interiorCandidate.imageUrl
            $targetPath = Join-Path $interiorDir ($slug + $extension)
            Remove-ExistingSlugFiles -Directory $interiorDir -Slug $slug
            Invoke-WithRetry -Operation "Download interior '$slug'" -Action {
                Invoke-WebRequest -Uri $interiorCandidate.imageUrl -Headers $headers -OutFile $targetPath -TimeoutSec 120
            } | Out-Null

            $interiorResults.Add([pscustomobject]@{
                slug = $slug
                query = $interiorCandidate.query
                pageTitle = $interiorCandidate.title
                pageUrl = $interiorCandidate.pageUrl
                imageUrl = $interiorCandidate.imageUrl
                localFile = (Resolve-Path $targetPath).Path
            }) | Out-Null
        }
        else {
            $interiorFailures.Add($slug) | Out-Null
            Write-Warning "No interior image found for $slug."
        }
    }
    else {
        Write-Host "  Interior already exists, skipping download."
    }

    Start-Sleep -Seconds 1
}

if ($overviewResults.Count -gt 0) {
    $overviewResults |
        ConvertTo-Json -Depth 4 |
        Set-Content -Path (Join-Path $OutputDir "sources.json") -Encoding UTF8
}

$interiorResults |
    ConvertTo-Json -Depth 4 |
    Set-Content -Path (Join-Path $interiorDir "sources.json") -Encoding UTF8

Write-Host ""
Write-Host "Overview images downloaded this run: $($overviewResults.Count)"
Write-Host "Interior images downloaded this run: $($interiorResults.Count)"

if ($overviewFailures.Count -gt 0) {
    Write-Warning ("Missing overview images: " + ($overviewFailures -join ", "))
}

if ($interiorFailures.Count -gt 0) {
    Write-Warning ("Missing interior images: " + ($interiorFailures -join ", "))
}
