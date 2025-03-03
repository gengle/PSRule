# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

[CmdletBinding()]
param (
    [Parameter(Mandatory = $False)]
    [String]$Build = '0.0.1',

    [Parameter(Mandatory = $False)]
    [String]$Configuration = 'Debug',

    [Parameter(Mandatory = $False)]
    [String]$ApiKey,

    [Parameter(Mandatory = $False)]
    [Switch]$CodeCoverage = $False,

    [Parameter(Mandatory = $False)]
    [Switch]$Benchmark = $False,

    [Parameter(Mandatory = $False)]
    [String]$ArtifactPath = (Join-Path -Path $PWD -ChildPath out/modules),

    [Parameter(Mandatory = $False)]
    [String]$AssertStyle = 'AzurePipelines'
)

Write-Host -Object "[Pipeline] -- PWD: $PWD" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- ArtifactPath: $ArtifactPath" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- BuildNumber: $($Env:BUILD_BUILDNUMBER)" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- SourceBranch: $($Env:BUILD_SOURCEBRANCH)" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- SourceBranchName: $($Env:BUILD_SOURCEBRANCHNAME)" -ForegroundColor Green;

if ($Env:SYSTEM_DEBUG -eq 'true') {
    $VerbosePreference = 'Continue';
}

if ($Env:BUILD_SOURCEBRANCH -like '*/tags/*' -and $Env:BUILD_SOURCEBRANCHNAME -like 'v0.*') {
    $Build = $Env:BUILD_SOURCEBRANCHNAME.Substring(1);
}

$version = $Build;
$versionSuffix = [String]::Empty;

if ($version -like '*-*') {
    [String[]]$versionParts = $version.Split('-', [System.StringSplitOptions]::RemoveEmptyEntries);
    $version = $versionParts[0];

    if ($versionParts.Length -eq 2) {
        $versionSuffix = $versionParts[1];
    }
}

Write-Host -Object "[Pipeline] -- Using version: $version" -ForegroundColor Green;
Write-Host -Object "[Pipeline] -- Using versionSuffix: $versionSuffix" -ForegroundColor Green;

if ($Env:coverage -eq 'true') {
    $CodeCoverage = $True;
}

# Copy the PowerShell modules files to the destination path
function CopyModuleFiles {

    param (
        [Parameter(Mandatory = $True)]
        [String]$Path,

        [Parameter(Mandatory = $True)]
        [String]$DestinationPath
    )

    process {
        $sourcePath = Resolve-Path -Path $Path;

        Get-ChildItem -Path $sourcePath -Recurse -File -Include *.ps1,*.psm1,*.psd1,*.ps1xml | Where-Object -FilterScript {
            ($_.FullName -notmatch '(\.(cs|csproj)|(\\|\/)(obj|bin))')
        } | ForEach-Object -Process {
            $filePath = $_.FullName.Replace($sourcePath, $destinationPath);

            $parentPath = Split-Path -Path $filePath -Parent;

            if (!(Test-Path -Path $parentPath)) {
                $Null = New-Item -Path $parentPath -ItemType Directory -Force;
            }

            Copy-Item -Path $_.FullName -Destination $filePath -Force;
        };
    }
}

function Get-RepoRuleData {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $False)]
        [String]$Path = $PWD
    )
    process {
        GetPathInfo -Path $Path -Verbose:$VerbosePreference;
    }
}

function GetPathInfo {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True)]
        [String]$Path
    )
    begin {
        $items = New-Object -TypeName System.Collections.ArrayList;
    }
    process {
        $Null = $items.Add((Get-Item -Path $Path));
        $files = @(Get-ChildItem -Path $Path -File -Recurse -Include *.ps1,*.psm1,*.psd1,*.cs | Where-Object {
            !($_.FullName -like "*.Designer.cs") -and
            !($_.FullName -like "*/bin/*") -and
            !($_.FullName -like "*/obj/*") -and
            !($_.FullName -like "*\obj\*") -and
            !($_.FullName -like "*\bin\*") -and
            !($_.FullName -like "*\out\*") -and
            !($_.FullName -like "*/out/*")
        });
        $Null = $items.AddRange($files);
    }
    end {
        $items;
    }
}

task BuildDotNet {
    exec {
        # Build library
        # Add build version -p:versionPrefix=$ModuleVersion
        dotnet publish src/PSRule -c $Configuration -f netstandard2.0 -o $(Join-Path -Path $PWD -ChildPath out/modules/PSRule)
    }
}

task TestDotNet {
    if ($CodeCoverage) {
        exec {
            # Test library
            dotnet test --collect:"Code Coverage" --logger trx -r (Join-Path $PWD -ChildPath reports/) tests/PSRule.Tests
        }
    }
    else {
        exec {
            # Test library
            dotnet test --logger trx -r (Join-Path $PWD -ChildPath reports/) tests/PSRule.Tests
        }
    }
}

task CopyModule {
    CopyModuleFiles -Path src/PSRule -DestinationPath out/modules/PSRule;

    # Copy third party notices
    Copy-Item -Path ThirdPartyNotices.txt -Destination out/modules/PSRule;

    # Copy schemas
    Copy-Item -Path schemas/* -Destination out/modules/PSRule;
}

# Synopsis: Build modules only
task BuildModule BuildDotNet, CopyModule

# Synopsis: Build help
task BuildHelp BuildModule, PlatyPS, {

    # Generate MAML and about topics
    $Null = New-ExternalHelp -OutputPath out/docs/PSRule -Path '.\docs\commands\PSRule\en-US','.\docs\keywords\PSRule\en-US', '.\docs\concepts\PSRule\en-US' -Force;

    # Copy generated help into module out path
    $Null = Copy-Item -Path out/docs/PSRule/* -Destination out/modules/PSRule/en-US;
    $Null = Copy-Item -Path out/docs/PSRule/* -Destination out/modules/PSRule/en-AU;
    $Null = Copy-Item -Path out/docs/PSRule/* -Destination out/modules/PSRule/en-GB;
}

task ScaffoldHelp Build, {
    Import-Module (Join-Path -Path $PWD -ChildPath out/modules/PSRule) -Force;
    Update-MarkdownHelp -Path '.\docs\commands\PSRule\en-US';
}

# Synopsis: Remove temp files.
task Clean {
    Remove-Item -Path out,reports -Recurse -Force -ErrorAction SilentlyContinue;
}

task VersionModule {
    $modulePath = Join-Path -Path $ArtifactPath -ChildPath 'PSRule';
    $manifestPath = Join-Path -Path $modulePath -ChildPath 'PSRule.psd1';
    Write-Verbose -Message "[VersionModule] -- Checking module path: $modulePath";

    if (![String]::IsNullOrEmpty($Build)) {
        # Update module version
        if (![String]::IsNullOrEmpty($version)) {
            Write-Verbose -Message "[VersionModule] -- Updating module manifest ModuleVersion";
            Update-ModuleManifest -Path $manifestPath -ModuleVersion $version;
        }

        # Update pre-release version
        if (![String]::IsNullOrEmpty($versionSuffix)) {
            Write-Verbose -Message "[VersionModule] -- Updating module manifest Prerelease";
            Update-ModuleManifest -Path $manifestPath -Prerelease $versionSuffix;
        }
    }
}

task PackageModule {
    if ($Env:PUBLISH) {
        $modulePath = Join-Path -Path $ArtifactPath -ChildPath 'PSRule';
        $nugetPath = Join-Path -Path $PWD -ChildPath 'out/nuget/';
        if (!(Test-Path -Path $nugetPath)) {
            $Null = New-Item -Path $nugetPath -ItemType Directory -Force;
        }
        Write-Verbose -Message "[PackageModule] -- Checking module path: $modulePath";
        try {
            Register-PSRepository -Name 'OutPath' -SourceLocation $nugetPath -PublishLocation $nugetPath;
            Publish-Module -Path $modulePath -NuGetApiKey na -Repository 'OutPath';
            Out-Host -InputObject "`#`#vso[artifact.upload containerfolder=PSRule.nupkg;artifactname=PSRule.nupkg;]$nugetPath"
        }
        finally {
            Unregister-PSRepository -Name 'OutPath';
        }
    }
}

task ReleaseModule VersionModule, {
    $modulePath = (Join-Path -Path $ArtifactPath -ChildPath 'PSRule');
    Write-Verbose -Message "[ReleaseModule] -- Checking module path: $modulePath";

    if (!(Test-Path -Path $modulePath)) {
        Write-Error -Message "[ReleaseModule] -- Module path does not exist";
    }
    elseif (![String]::IsNullOrEmpty($ApiKey)) {
        Publish-Module -Path $modulePath -NuGetApiKey $ApiKey;
    }
}

# Synopsis: Install NuGet provider
task NuGet {
    if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction Ignore)) {
        Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
    }
}

# Synopsis: Install Pester module
task Pester NuGet, {
    if ($Null -eq (Get-InstalledModule -Name Pester -MinimumVersion 4.0.0 -ErrorAction Ignore)) {
        Install-Module -Name Pester -MinimumVersion 4.0.0 -Scope CurrentUser -Force -SkipPublisherCheck;
    }
    Import-Module -Name Pester -Verbose:$False;
}

# Synopsis: Install PSScriptAnalyzer module
task PSScriptAnalyzer NuGet, {
    if ($Null -eq (Get-InstalledModule -Name PSScriptAnalyzer -MinimumVersion 1.18.3 -ErrorAction Ignore)) {
        Install-Module -Name PSScriptAnalyzer -MinimumVersion 1.18.3 -Scope CurrentUser -Force;
    }
    Import-Module -Name PSScriptAnalyzer -Verbose:$False;
}

# Synopsis: Install PlatyPS module
task platyPS {
    if ($Null -eq (Get-InstalledModule -Name PlatyPS -MinimumVersion 0.14.0 -ErrorAction Ignore)) {
        Install-Module -Name PlatyPS -Scope CurrentUser -MinimumVersion 0.14.0 -Force;
    }
    Import-Module -Name PlatyPS -Verbose:$False;
}

task TestModule TestDotNet, Pester, PSScriptAnalyzer, {
    # Run Pester tests
    $pesterParams = @{ Path = $PWD; OutputFile = 'reports/pester-unit.xml'; OutputFormat = 'NUnitXml'; PesterOption = @{ IncludeVSCodeMarker = $True }; PassThru = $True; };

    if ($CodeCoverage) {
        $pesterParams.Add('CodeCoverage', (Join-Path -Path $PWD -ChildPath 'out/modules/**/*.psm1'));
        $pesterParams.Add('CodeCoverageOutputFile', (Join-Path -Path $PWD -ChildPath reports/pester-coverage.xml));
    }

    if (!(Test-Path -Path reports)) {
        $Null = New-Item -Path reports -ItemType Directory -Force;
    }

    $results = Invoke-Pester @pesterParams;

    # Throw an error if pester tests failed
    if ($Null -eq $results) {
        throw 'Failed to get Pester test results.';
    }
    elseif ($results.FailedCount -gt 0) {
        throw "$($results.FailedCount) tests failed.";
    }
}

# Synopsis: Run validation
task Rules {
    $assertParams = @{
        Path = './.ps-rule/'
        Style = $AssertStyle
        OutputFormat = 'NUnit3';
    }
    Import-Module (Join-Path -Path $PWD -ChildPath out/modules/PSRule) -Force;
    Get-RepoRuleData -Path $PWD |
        Assert-PSRule @assertParams -OutputPath reports/ps-rule-file.xml -Option @{ 'Binding.TargetName' = 'FullName' };
}

task Benchmark {
    if ($Benchmark -or $BuildTask -eq 'Benchmark') {
        dotnet run -p src/PSRule.Benchmark -f netcoreapp2.1 -c Release -- benchmark --output $PWD;
    }
}

# Synopsis: Add shipit build tag
task TagBuild {
    if ($Null -ne $Env:BUILD_DEFINITIONNAME) {
        Write-Host "`#`#vso[build.addbuildtag]shipit";
    }
}

# Synopsis: Run script analyzer
task Analyze Build, PSScriptAnalyzer, {
    Invoke-ScriptAnalyzer -Path out/modules/PSRule;
}

task CleanSite {
    git branch -D gh-pages;
    Remove-Item -Path out/site -Recurse -Force -ErrorAction Ignore;
}

# Synopsis: Build project site
task BuildSite CleanSite, {
    docfx build --force docs/docfx.json;
}

# Synopsis: Publish project site to gh-pages
task PublishSite CleanSite, {
    git worktree add -b gh-pages -f out/site origin/gh-pages;
    docfx build --force docs/docfx.json;

    try {
        Push-Location -Path out/site;
        git add *;
        git commit -m 'Update documentation';
        git push;
    }
    finally {
        Pop-Location;
    }

    git worktree remove out/site
    git worktree prune
}

# Synopsis: Build and clean.
task . Build, Test, Benchmark

# Synopsis: Build the project
task Build Clean, BuildModule, BuildHelp, VersionModule, PackageModule

task Test Build, Rules, TestModule

task Release ReleaseModule, TagBuild
