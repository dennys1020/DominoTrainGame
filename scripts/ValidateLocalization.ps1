param(
    [string] $ProjectDirectory = (Join-Path $PSScriptRoot '..\scr')
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath($ProjectDirectory)
$resourceRoot = Join-Path $projectRoot 'Shared\Resources\Localization'
$issues = New-Object 'System.Collections.Generic.List[string]'

function Read-ResourceValues([string] $Path)
{
    [xml] $document = [IO.File]::ReadAllText($Path)
    $values = [Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)

    foreach ($entry in $document.root.data)
    {
        if ($values.ContainsKey($entry.name))
        {
            $issues.Add(("Duplicate resource key in {0}: {1}" -f $Path, $entry.name))
        }
        else
        {
            $values.Add($entry.name, [string] $entry.value)
        }
    }

    return ,$values
}

$spanish = Read-ResourceValues (Join-Path $resourceRoot 'UiStrings.resx')
$english = Read-ResourceValues (Join-Path $resourceRoot 'UiStrings.en.resx')
$placeholderPattern = '(?<!\{)\{(?<index>\d+)(?:,[+-]?\d+)?(?::[^{}]*)?\}(?!\})'
$translations = @{}

foreach ($key in $spanish.Keys)
{
    if (-not $english.ContainsKey($key))
    {
        $issues.Add("Missing English translation: $key")
        continue
    }

    $placeholderSets = foreach ($value in @($spanish[$key], $english[$key]))
    {
        if ([string]::IsNullOrWhiteSpace($value))
        {
            $issues.Add("Empty translation: $key")
        }
        elseif ($value -cne $value.Trim())
        {
            $issues.Add("Leading or trailing whitespace: $key")
        }

        $indexes = @([regex]::Matches($value, $placeholderPattern) |
            ForEach-Object { [int] $_.Groups['index'].Value } | Sort-Object -Unique)
        $indexes -join ','

        if ($indexes.Count -gt 0)
        {
            $arguments = [object[]]::new(($indexes | Measure-Object -Maximum).Maximum + 1)
            for ($index = 0; $index -lt $arguments.Length; $index++)
            {
                $arguments[$index] = 0
            }
            try
            {
                [void] [string]::Format([Globalization.CultureInfo]::InvariantCulture, $value, $arguments)
            }
            catch
            {
                $issues.Add("Invalid format string: $key")
            }
        }
    }

    if ($placeholderSets[0] -cne $placeholderSets[1])
    {
        $issues.Add("Different format placeholders between languages: $key")
    }

    $normalizedSpanish = [regex]::Replace($spanish[$key].Trim(), '\s+', ' ').ToUpperInvariant()
    $normalizedEnglish = [regex]::Replace($english[$key].Trim(), '\s+', ' ').ToUpperInvariant()
    $translationPair = $normalizedSpanish + [char]0 + $normalizedEnglish
    if ($translations.ContainsKey($translationPair))
    {
        $issues.Add("Redundant translations: $($translations[$translationPair]), $key")
    }
    else
    {
        $translations[$translationPair] = $key
    }
}

foreach ($key in $english.Keys)
{
    if (-not $spanish.ContainsKey($key))
    {
        $issues.Add("Missing Spanish translation: $key")
    }
}

$usedKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$sourceFiles = Get-ChildItem -LiteralPath $projectRoot -Recurse -File | Where-Object {
    $_.Extension -in @('.cs', '.xaml') -and $_.Name -ne 'UiStrings.Designer.cs' -and
    $_.FullName -notmatch '\\(bin|obj|\.vs)\\'
}

foreach ($sourceFile in $sourceFiles)
{
    $source = [IO.File]::ReadAllText($sourceFile.FullName)
    $referencePattern = 'UiStrings\.(?<key>[A-Za-z_]\w*)'
    if ($sourceFile.Extension -eq '.xaml')
    {
        $referencePattern += '|\[(?<key>[A-Za-z_]\w*)\]'
    }

    foreach ($reference in [regex]::Matches($source, $referencePattern))
    {
        $key = $reference.Groups['key'].Value
        if ($key -in @('Culture', 'ResourceManager'))
        {
            continue
        }
        [void] $usedKeys.Add($key)
        if (-not $spanish.ContainsKey($key))
        {
            $issues.Add("Unknown resource in $($sourceFile.FullName): $key")
        }
    }
}

$designer = [IO.File]::ReadAllText((Join-Path $resourceRoot 'UiStrings.Designer.cs'))
$designerKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($property in [regex]::Matches($designer, 'public static string\s+(?<key>\w+)'))
{
    [void] $designerKeys.Add($property.Groups['key'].Value)
}

foreach ($key in $spanish.Keys)
{
    if (-not $usedKeys.Contains($key))
    {
        $issues.Add("Unused resource: $key")
    }
    if (-not $designerKeys.Contains($key))
    {
        $issues.Add("Missing generated property: $key")
    }
}

foreach ($key in $designerKeys)
{
    if (-not $spanish.ContainsKey($key))
    {
        $issues.Add("Obsolete generated property: $key")
    }
}

if ($issues.Count -gt 0)
{
    throw ($issues -join [Environment]::NewLine)
}

Write-Output ("Localization validated: {0} keys per language; no missing, unused or redundant translations." -f
    $spanish.Count)
