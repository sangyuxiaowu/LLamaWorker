$versionInfo = nbgv get-version
#Get NuGetPackageVersion
$version = ($versionInfo | Select-String -Pattern "NuGetPackageVersion" | ForEach-Object { $_.Line.Split(":")[1].Trim() })

if (-not $version) {
    Write-Error "err"
    exit 1
}

$tagName = "v$version"
git tag $tagName

git push origin $tagName

Write-Output "Success push $tagName"