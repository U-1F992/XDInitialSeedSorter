#Requires -Version 7

using assembly PokemonCoRNGLibrary.dll

[UInt32] $target = [UInt32] 0x514fa562L
[UInt32] $seed = 0

[long] $count = 0

do {

    $count++
    Write-Host "--- Attempt: $count ---"
    Write-Host ""

    .\XDInitialSeedSorter.exe | Out-File result.txt

    [string] $result = (Get-Content result.txt)
    [string[]] $seeds = [RegEx]::Matches($result, "Current seed: [0123456789abcdefABCDEF]{0,8}")

    $seed = [System.Convert]::ToUInt32($seeds[$seeds.Length - 1].Split(" ")[2], 16)
    Write-Host "Current seed: $([System.Convert]::ToString($seed, 16))"
    Write-Host ""

} while (
    ![System.Linq.Enumerable]::Take(
        [System.Linq.Enumerable]::Skip(
            [PokemonCoRNGLibrary.SeedEnumerator]::EnumerateSeedAtNamingScreen($seed),
        116 + 607),
    1000000).ToArray().Contains($target)
)
