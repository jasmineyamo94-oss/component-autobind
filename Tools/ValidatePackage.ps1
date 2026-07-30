[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$packageRoot = Join-Path $repositoryRoot 'Packages/com.jasmineyamo.component-autobind'
$integrationPackageRoot = Join-Path $repositoryRoot 'Packages/com.jasmineyamo.simple-ui-vcontainer'
$manifestPath = Join-Path $packageRoot 'package.json'
$integrationManifestPath = Join-Path $integrationPackageRoot 'package.json'

if (-not (Test-Path -LiteralPath $manifestPath)) {
    throw "Package manifest not found: $manifestPath"
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
if ($manifest.name -ne 'com.jasmineyamo.component-autobind') {
    throw "Unexpected package name: $($manifest.name)"
}

if ($manifest.version -ne '0.2.0') {
    throw "Core package validation expects version 0.2.0, found $($manifest.version)"
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
    'Runtime/IAutoBindHost.cs',
    'Runtime/IUiViewComponent.cs',
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

$forbiddenCoreReferences = @(
    'Third_Party',
    'VContainer',
    'CCDebug',
    'AAGame',
    'GenericDictionary',
    'ViewCore',
    'TMPro',
    'DateTime.Now',
    'IAutoBindTarget',
    'IAutoBindComponentSet',
    'Assets/Third Party',
    'Assets/ComponentAutoBind'
)

$packageFiles = Get-ChildItem -LiteralPath $packageRoot -Recurse -File
foreach ($file in $packageFiles) {
    $content = Get-Content -Raw -LiteralPath $file.FullName
    foreach ($forbiddenReference in $forbiddenCoreReferences) {
        if ($content.IndexOf($forbiddenReference, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $relativeFile = $file.FullName.Substring($repositoryRoot.Length + 1)
            throw "Forbidden reference '$forbiddenReference' found in $relativeFile"
        }
    }
}

if (-not (Test-Path -LiteralPath $integrationManifestPath)) {
    throw "Integration package manifest not found: $integrationManifestPath"
}

$integrationManifest =
    Get-Content -Raw -LiteralPath $integrationManifestPath | ConvertFrom-Json
if ($integrationManifest.name -ne 'com.jasmineyamo.simple-ui-vcontainer') {
    throw "Unexpected integration package name: $($integrationManifest.name)"
}

if ($integrationManifest.version -ne '0.1.0') {
    throw "Integration package validation expects version 0.1.0, found $($integrationManifest.version)"
}

if ($integrationManifest.displayName -ne 'Simple UI - VContainer') {
    throw "Unexpected Simple UI display name: $($integrationManifest.displayName)"
}

if ($integrationManifest.unity -ne '2022.3') {
    throw "Integration Unity baseline must be 2022.3, found $($integrationManifest.unity)"
}

if ($integrationManifest.dependencies.'com.jasmineyamo.component-autobind' -ne '0.2.0') {
    throw 'The integration package must depend on core package version 0.2.0.'
}

if ($integrationManifest.dependencies.PSObject.Properties.Name -contains 'jp.hadashikick.vcontainer') {
    throw 'The integration package must not pin a jp.hadashikick.vcontainer version.'
}

$resourcesDemo = $integrationManifest.samples |
    Where-Object {
        $_.displayName -eq 'Resources Demo' `
            -and $_.path -eq 'Samples~/ResourcesDemo'
    }
if (-not $resourcesDemo) {
    throw 'The Simple UI package must declare the Resources Demo Sample.'
}

$integrationRequiredPaths = @(
    'Runtime/JasmineYamo.SimpleUI.VContainer.asmdef',
    'Runtime/ViewLifetimeScope.cs',
    'Runtime/ViewBundle.cs',
    'Runtime/ViewManager.cs',
    'Runtime/ViewManagerContracts.cs',
    'Runtime/SimpleUIVContainerBuilderExtensions.cs',
    'Editor/JasmineYamo.SimpleUI.VContainer.Editor.asmdef',
    'Editor/VContainerDependencyNotifier.cs',
    'Editor/VContainerViewTemplateCreator.cs',
    'Editor/Templates/VContainerView.cs.txt',
    'Editor/Templates/VContainerViewPresenter.cs.txt',
    'Tests/Editor/JasmineYamo.SimpleUI.VContainer.Tests.Editor.asmdef',
    'Tests/Editor/ViewLifetimeScopeTests.cs',
    'Tests/Editor/VContainerViewTemplateCreatorTests.cs',
    'Tests/Editor/ViewManagerTests.cs',
    'Samples~/ResourcesDemo/Runtime/ResourcesViewPrefabHelper.cs',
    'Samples~/ResourcesDemo/Demo/ViewManagerSample.unity',
    'README.md'
)

foreach ($relativePath in $integrationRequiredPaths) {
    $requiredPath = Join-Path $integrationPackageRoot $relativePath
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required integration package file is missing: $relativePath"
    }
}

$integrationRuntimeAsmdefPath = Join-Path $integrationPackageRoot `
    'Runtime/JasmineYamo.SimpleUI.VContainer.asmdef'
$integrationRuntimeAsmdef =
    Get-Content -Raw -LiteralPath $integrationRuntimeAsmdefPath | ConvertFrom-Json
if ($integrationRuntimeAsmdef.name -ne 'JasmineYamo.SimpleUI.VContainer') {
    throw "Unexpected Simple UI runtime assembly name: $($integrationRuntimeAsmdef.name)"
}

if ($integrationRuntimeAsmdef.defineConstraints -notcontains 'JASMINEYAMO_SIMPLE_UI_VCONTAINER') {
    throw 'The Simple UI runtime assembly must be conditional on JASMINEYAMO_SIMPLE_UI_VCONTAINER.'
}

$vcontainerVersionDefine = $integrationRuntimeAsmdef.versionDefines |
    Where-Object {
        $_.name -eq 'jp.hadashikick.vcontainer' `
            -and $_.expression -eq '' `
            -and $_.define -eq 'JASMINEYAMO_SIMPLE_UI_VCONTAINER'
    }
if (-not $vcontainerVersionDefine) {
    throw 'The integration runtime assembly must detect any VContainer package version.'
}

$forbiddenIntegrationReferences = @(
    'com.jasmineyamo.component-autobind.vcontainer-viewcore',
    'JasmineYamo.ComponentAutoBind.VContainerViewCore',
    'JasmineYamo.ComponentAutoBind.ViewCore',
    'JASMINEYAMO_VCONTAINER',
    'ManagedViewLifetimeScope',
    'IViewManagerInjectable',
    'IViewRootHelper',
    'IViewEvent'
)
$integrationFiles = Get-ChildItem -LiteralPath $integrationPackageRoot -Recurse -File
foreach ($integrationFile in $integrationFiles) {
    $integrationContent = Get-Content -Raw -LiteralPath $integrationFile.FullName
    foreach ($forbiddenReference in $forbiddenIntegrationReferences) {
        if ($integrationContent.IndexOf(
                $forbiddenReference,
                [StringComparison]::Ordinal) -ge 0) {
            $relativeFile =
                $integrationFile.FullName.Substring($repositoryRoot.Length + 1)
            throw "Forbidden legacy reference '$forbiddenReference' found in $relativeFile"
        }
    }
}

if (Test-Path -LiteralPath (Join-Path $integrationPackageRoot `
        'Samples~/ResourcesDemo/Editor')) {
    $sampleEditorFiles = Get-ChildItem -LiteralPath (Join-Path `
        $integrationPackageRoot 'Samples~/ResourcesDemo/Editor') -File -Recurse
    if ($sampleEditorFiles.Count -gt 0) {
        throw 'Resources Demo must not ship a duplicate View creation Editor menu.'
    }
}

$integrationEditorAsmdefPath = Join-Path $integrationPackageRoot `
    'Editor/JasmineYamo.SimpleUI.VContainer.Editor.asmdef'
$integrationEditorAsmdef =
    Get-Content -Raw -LiteralPath $integrationEditorAsmdefPath | ConvertFrom-Json
if ($integrationEditorAsmdef.references -notcontains 'JasmineYamo.ComponentAutoBind.Editor') {
    throw 'The integration Editor assembly must reference the core Editor settings assembly.'
}

$viewTemplateCreatorPath = Join-Path $integrationPackageRoot `
    'Editor/VContainerViewTemplateCreator.cs'
$viewTemplateCreatorContent = Get-Content -Raw -LiteralPath $viewTemplateCreatorPath
if ($viewTemplateCreatorContent.IndexOf(
        'Assets/Create/C# Scripts VContainer View',
        [StringComparison]::Ordinal) -lt 0) {
    throw 'The VContainer View creation menu is missing.'
}

if ($viewTemplateCreatorContent.IndexOf(
        'Assets/Create/C# Scripts/VContainer View',
        [StringComparison]::Ordinal) -ge 0) {
    throw 'The legacy nested VContainer View creation menu must be removed.'
}

if ($viewTemplateCreatorContent.IndexOf(
        'ShowModalUtility',
        [StringComparison]::Ordinal) -lt 0) {
    throw 'The VContainer View command must use the modal naming window.'
}

if ($viewTemplateCreatorContent.IndexOf(
        'MenuPriority = 80',
        [StringComparison]::Ordinal) -lt 0) {
    throw 'The VContainer View menu must appear above the built-in C# Script item.'
}

if ($viewTemplateCreatorContent.IndexOf(
        'StartNameEditingIfProjectWindowExists',
        [StringComparison]::Ordinal) -ge 0 `
        -or $viewTemplateCreatorContent.IndexOf(
            'EndNameEditAction',
            [StringComparison]::Ordinal) -ge 0) {
    throw 'The legacy Project rename placeholder flow must be removed.'
}

if ($viewTemplateCreatorContent.IndexOf(
        'FileMode.CreateNew',
        [StringComparison]::Ordinal) -lt 0) {
    throw 'The VContainer View scripts must be written directly without overwrite.'
}

$inspectorPath = Join-Path $packageRoot 'Editor/ComponentAutoBindToolInspector.cs'
$inspectorContent = Get-Content -Raw -LiteralPath $inspectorPath
if ($inspectorContent.IndexOf('DisplayDialog', [StringComparison]::Ordinal) -ge 0) {
    throw 'Auto Bind and Generate Code actions must not display confirmation dialogs.'
}

$integrationEditorFiles =
    Get-ChildItem -LiteralPath (Join-Path $integrationPackageRoot 'Editor') `
        -Recurse -File -Filter '*.cs'
foreach ($integrationEditorFile in $integrationEditorFiles) {
    $integrationEditorContent = Get-Content -Raw -LiteralPath $integrationEditorFile.FullName
    if ($integrationEditorContent.IndexOf('DisplayDialog', [StringComparison]::Ordinal) -ge 0) {
        throw "Integration Editor actions must not display dialogs: $($integrationEditorFile.Name)"
    }
}

$sourceFiles = @(
    Get-ChildItem -LiteralPath $packageRoot -Recurse -File -Filter '*.cs'
    Get-ChildItem -LiteralPath $integrationPackageRoot -Recurse -File -Filter '*.cs'
)
foreach ($sourceFile in $sourceFiles) {
    $sourceText = Get-Content -Raw -LiteralPath $sourceFile.FullName
    if ($sourceText -match '[^\x00-\x7F]') {
        $relativeFile = $sourceFile.FullName.Substring($repositoryRoot.Length + 1)
        throw "C# source must remain ASCII for package portability: $relativeFile"
    }
}

$asmdefFiles = @(
    Get-ChildItem -LiteralPath $packageRoot -Recurse -File -Filter '*.asmdef'
    Get-ChildItem -LiteralPath $integrationPackageRoot -Recurse -File -Filter '*.asmdef'
)
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

Write-Host "Validated core package $($manifest.name) version $($manifest.version)."
Write-Host "Validated Simple UI package $($integrationManifest.name) version $($integrationManifest.version)."
