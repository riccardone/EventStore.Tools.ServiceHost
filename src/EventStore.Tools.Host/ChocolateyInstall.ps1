$packageName = 'EventStore-ServiceHost' 
$serviceFileName = "EventStore.Tools.ServiceHost.exe"

try { 
  
  $installDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)" 
  $fileToInstall = Join-Path $installDir $serviceFileName
  . $fileToInstall install
  . $fileToInstall start

  Write-ChocolateySuccess "$packageName"
} catch {
  Write-ChocolateyFailure "$packageName" "$($_.Exception.Message)"
  throw 
}