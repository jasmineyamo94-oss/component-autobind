[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$packageRoot = Join-Path $repositoryRoot 'Packages/com.jasmineyamo.component-autobind'
$manifestPath = Join-Path $packageRoot 'package.json'

if (-not (Test-Path -LiteralPath $manifestPath)) {
    throw "Package manifest not found: $manifestPath"
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
if ($manifest.name -ne 'com.jasmineyamo.component-autobind') {
    throw "Unexpected package name: $($manifest.name)"
}

if ($manifest.version -ne '0.1.0') {
    throw "Initial package validation expects version 0.1.0, found $($manifest.version)"
}

if ($manifest.unity -ne '2022.3') {
    throw "Unity baseline must be 2022.3, found $($manifest.unity)"
}

if (-not $manifest.dependencies.'com.unity.ugui') {
    throw 'The package must declare com.unity.ugui.'
}

if (-not $manifest.dependencies.'com.unity.test-framework') {
    throw 'The package must declare com.unity.test-framework because it ships test assemblies.'
}

$requiredPaths = @(
    'Runtime/JasmineYamo.ComponentAutoBind.asmdef',
    'Runtime/ComponentAutoBindTool.cs',
    'Runtime/IAutoBindTarget.cs',
    'Runtime/IAutoBindComponentSet.cs',
    'Editor/JasmineYamo.ComponentAutoBind.Editor.asmdef',
    'Tests/Runtime/JasmineYamo.ComponentAutoBind.Tests.Runtime.asmdef',
    'Tests/Editor/JasmineYamo.ComponentAutoBind.Tests.Editor.asmdef',
    'Samples~/BasicUGUI/README.md'
)

foreach ($relativePath in $requiredPaths) {
    $requiredPath = Join-Path $packageRoot $relativePath
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required package file is missing: $relativePath"
    }
}

$forbiddenReferences = @(
    'Third_Party',
    'VContainer',
    'CCDebug',
    'AAGame',
    'GenericDictionary',
    'IUiViewComponent',
    'IAutoBindHost',
    'ViewCore',
    'TMPro',
    'DateTime.Now',
    'UIView',
    'Assets/Third Party',
    'Assets/ComponentAutoBind'
)

$packageFiles = Get-ChildItem -LiteralPath $packageRoot -Recurse -File
foreach ($file in $packageFiles) {
    $content = Get-Content -Raw -LiteralPath $file.FullName
    foreach ($forbiddenReference in $forbiddenReferences) {
        if ($content.IndexOf($forbiddenReference, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $relativeFile = $file.FullName.Substring($repositoryRoot.Length + 1)
            throw "Forbidden reference '$forbiddenReference' found in $relativeFile"
        }
    }
}

$sourceFiles = Get-ChildItem -LiteralPath $packageRoot -Recurse -File -Filter '*.cs'
foreach ($sourceFile in $sourceFiles) {
    $sourceText = Get-Content -Raw -LiteralPath $sourceFile.FullName
    if ($sourceText -match '[^\x00-\x7F]') {
        $relativeFile = $sourceFile.FullName.Substring($repositoryRoot.Length + 1)
        throw "C# source must remain ASCII for package portability: $relativeFile"
    }
}

$asmdefFiles = Get-ChildItem -LiteralPath $packageRoot -Recurse -File -Filter '*.asmdef'
$assemblyNames = @{}
foreach ($asmdefFile in $asmdefFiles) {
    $asmdef = Get-Content -Raw -LiteralPath $asmdefFile.FullName | ConvertFrom-Json
    if ([string]::IsNullOrWhiteSpace($asmdef.name)) {
        throw "Assembly definition has no name: $($asmdefFile.FullName)"
    }

    if ($assemblyNames.ContainsKey($asmdef.name)) {
        throw "Duplicate assembly definition name: $($asmdef.name)"
    }

    $assemblyNames[$asmdef.name] = $true
}

Write-Host "Validated package $($manifest.name) version $($manifest.version)."
