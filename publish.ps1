#.SYNOPSIS
# Simple 1-step publish script, but has many assumptions.  See description.

#.DESCRIPTION
# This publish is a simple local convenience measure for publishing SnowSite
# from localhost to your gitrepo, assuming you've customized SnowSite to become
# your site.
#
# It also assumes you've already compiled Sandra.snow in "debug" mode.
#
# It also assumes that your "config" is set to output directly into your target
# static site repo.
#
# If any of these assumptions are false, you may need to use a different publish
# script, or customize this one.

param (
    # git commit message for both Sandra.Snow repo and output repo.
    [Parameter(mandatory)]
    [string] $commitMessage,

    # path to the config file.  Must be in the site directory.  Defaults to SnowSite\Snow\Snow.config
    [Parameter()]
    [IO.FileInfo] $configPath = $null
)

Push-Location $PSScriptRoot

# defaulting.
if (-not $configPath) {
    $configPath = [IO.FileInfo](Resolve-Path ".\SnowSite\Snow\Snow.config").Path
}
$configDirPath = Split-Path $configPath -Parent
$outputPath = Resolve-Path (Join-Path $configDirPath $config.postsOutput)

& src\Snow\bin\Debug\Snow config=$configPath

#region commits
git add .
git commit -m $message
git push

Push-Location $outputPath
    git add .
    git commit -m $message
    git push
Pop-Location
#endregion commits

Pop-Location