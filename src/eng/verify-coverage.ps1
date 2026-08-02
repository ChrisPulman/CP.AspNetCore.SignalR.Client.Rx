param(
    [Parameter(Mandatory = $true)]
    [string] $ResultsDirectory
)

$ErrorActionPreference = 'Stop'
$resolvedResultsDirectory = Resolve-Path -LiteralPath $ResultsDirectory
$coverageFiles = @(Get-ChildItem -LiteralPath $resolvedResultsDirectory -Filter '*.cobertura.xml' -File -Recurse)

if ($coverageFiles.Count -eq 0) {
    throw "No Cobertura coverage files were found beneath '$resolvedResultsDirectory'."
}

$coverageDocuments = @(
    foreach ($coverageFile in $coverageFiles) {
        [xml] (Get-Content -LiteralPath $coverageFile.FullName -Raw)
    }
)

$variants = @(
    @{
        Name = 'CP.AspNetCore.SignalR.Client.Rx'
        Match = {
            $_.name.StartsWith('CP.AspNetCore.SignalR.Client.Rx.', [System.StringComparison]::Ordinal) -and
                -not $_.name.StartsWith('CP.AspNetCore.SignalR.Client.Rx.Reactive.', [System.StringComparison]::Ordinal) -and
                -not $_.name.Contains('.Tests', [System.StringComparison]::Ordinal)
        }
    },
    @{
        Name = 'CP.AspNetCore.SignalR.Client.Rx.Reactive'
        Match = {
            $_.name.StartsWith('CP.AspNetCore.SignalR.Client.Rx.Reactive.', [System.StringComparison]::Ordinal) -and
                -not $_.name.Contains('.Tests', [System.StringComparison]::Ordinal)
        }
    }
)

foreach ($variant in $variants) {
    $classes = @(
        foreach ($coverageDocument in $coverageDocuments) {
            $coverageDocument.coverage.packages.package.classes.class | Where-Object $variant.Match
        }
    )

    if ($classes.Count -eq 0) {
        throw "Coverage for '$($variant.Name)' was not found."
    }

    $lines = @($classes.lines.line)
    $missedLines = @($lines | Where-Object { [int] $_.hits -eq 0 })
    $coveredBranches = 0
    $totalBranches = 0

    foreach ($branchLine in @($lines | Where-Object { $_.branch -eq 'true' })) {
        if ($branchLine.'condition-coverage' -notmatch '\((\d+)/(\d+)\)') {
            throw "Unable to parse branch coverage for '$($variant.Name)' at line $($branchLine.number)."
        }

        $coveredBranches += [int] $Matches[1]
        $totalBranches += [int] $Matches[2]
    }

    if ($missedLines.Count -ne 0 -or $coveredBranches -ne $totalBranches) {
        $missedLocations = @(
            $classes |
                ForEach-Object {
                    $class = $_
                    $class.lines.line |
                        Where-Object { [int] $_.hits -eq 0 } |
                        ForEach-Object { "$($class.filename):$($_.number)" }
                }
        )

        throw "Coverage for '$($variant.Name)' is below 100%. Missed lines: $($missedLocations -join ', '); branches: $coveredBranches/$totalBranches."
    }

    Write-Host "$($variant.Name): $($lines.Count)/$($lines.Count) lines and $coveredBranches/$totalBranches branches covered."
}
