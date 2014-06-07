properties { 
  $zipFileName = "Dson10r3.zip"
  $majorVersion = "1.0"
  $majorWithReleaseVersion = "1.0.3"
  $version = GetVersion $majorWithReleaseVersion
  $signAssemblies = $false
  $signKeyPath = "D:\Development\Releases\newtonsoft.snk"
  $buildDocumentation = $false
  $buildNuGet = $true
  $treatWarningsAsErrors = $false
  
  $baseDir  = resolve-path ..
  $buildDir = "$baseDir\Build"
  $sourceDir = "$baseDir\Src"
  $toolsDir = "$baseDir\Tools"
  $docDir = "$baseDir\Doc"
  $releaseDir = "$baseDir\Release"
  $workingDir = "$baseDir\Working"
  $builds = @(
    @{Name = "Newtonsoft.Dson"; TestsName = "Newtonsoft.Dson.Tests"; Constants="PORTABLE"; FinalDir="Portable"; NuGetDir = "portable-net45+wp80+win8+wpa81"; Framework="net-4.0"; Sign=$true}
  )
}

$framework = '4.0x86'

task default -depends Test

# Ensure a clean working directory
task Clean {
  Set-Location $baseDir
  
  if (Test-Path -path $workingDir)
  {
    Write-Output "Deleting Working Directory"
    
    del $workingDir -Recurse -Force
  }
  
  New-Item -Path $workingDir -ItemType Directory
}

# Build each solution, optionally signed
task Build -depends Clean { 
  Write-Host -ForegroundColor Green "Updating assembly version"
  Write-Host
  Update-AssemblyInfoFiles $sourceDir ($majorVersion + '.0.0') $version

  foreach ($build in $builds)
  {
    $name = $build.Name
    $finalDir = $build.FinalDir
    $sign = ($build.Sign -and $signAssemblies)

    Write-Host -ForegroundColor Green "Building " $name
    Write-Host -ForegroundColor Green "Signed " $sign
    Write-Host
    exec { msbuild "/t:Clean;Rebuild" /p:Configuration=Release "/p:Platform=Any CPU" /p:OutputPath=bin\Release\$finalDir\ /p:AssemblyOriginatorKeyFile=$signKeyPath "/p:SignAssembly=$sign" "/p:TreatWarningsAsErrors=$treatWarningsAsErrors" (GetConstants $build.Constants $sign) ".\Src\$name.sln" | Out-Default } "Error building $name"
  }
}

# Optional build documentation, add files to final zip
task Package -depends Build {
  foreach ($build in $builds)
  {
    $name = $build.TestsName
    $finalDir = $build.FinalDir
    
    robocopy "$sourceDir\Newtonsoft.Dson\bin\Release\$finalDir" $workingDir\Package\Bin\$finalDir Newtonsoft.Dson.dll Newtonsoft.Dson.pdb Newtonsoft.Dson.xml /NP /XO /XF *.CodeAnalysisLog.xml | Out-Default
  }
  
  if ($buildNuGet)
  {
    New-Item -Path $workingDir\NuGet -ItemType Directory
    Copy-Item -Path "$buildDir\Newtonsoft.Dson.nuspec" -Destination $workingDir\NuGet\Newtonsoft.Dson.nuspec -recurse
    
    foreach ($build in $builds)
    {
      if ($build.NuGetDir -ne $null)
      {
        $name = $build.Name
        $finalDir = $build.FinalDir
        $frameworkDirs = $build.NuGetDir.Split(",")
        
        foreach ($frameworkDir in $frameworkDirs)
        {
          robocopy "$sourceDir\Newtonsoft.Dson\bin\Release\$finalDir" $workingDir\NuGet\lib\$frameworkDir Newtonsoft.Dson.dll Newtonsoft.Dson.pdb Newtonsoft.Dson.xml /NP /XO /XF *.CodeAnalysisLog.xml | Out-Default
        }
      }
    }
  
    exec { .\Tools\NuGet\NuGet.exe pack $workingDir\NuGet\Newtonsoft.Dson.nuspec -Symbols }
    move -Path .\*.nupkg -Destination $workingDir\NuGet
  }
  
  #Copy-Item -Path $docDir\readme.txt -Destination $workingDir\Package\
  #Copy-Item -Path $docDir\license.txt -Destination $workingDir\Package\

  robocopy $sourceDir $workingDir\Package\Source\Src /MIR /NP /XD .svn bin obj TestResults AppPackages /XF *.suo *.user | Out-Default
  robocopy $buildDir $workingDir\Package\Source\Build /MIR /NP /XD .svn /XF runbuild.txt | Out-Default
  robocopy $toolsDir $workingDir\Package\Source\Tools /MIR /NP /XD .svn | Out-Default
  
  exec { .\Tools\7-zip\7za.exe a -tzip $workingDir\$zipFileName $workingDir\Package\* | Out-Default } "Error zipping"
}

# Unzip package to a location
task Deploy -depends Package {
  exec { .\Tools\7-zip\7za.exe x -y "-o$workingDir\Deployed" $workingDir\$zipFileName | Out-Default } "Error unzipping"
}

# Run tests on deployed files
task Test -depends Deploy {
  foreach ($build in $builds)
  {
    $name = $build.TestsName
    if ($name -ne $null)
    {
        $finalDir = $build.FinalDir
        $framework = $build.Framework
        
        Write-Host -ForegroundColor Green "Copying test assembly $name to deployed directory"
        Write-Host
        robocopy ".\Src\Newtonsoft.Dson.Tests\bin\Release\$finalDir" $workingDir\Deployed\Bin\$finalDir /MIR /NP /XO /XF LinqBridge.dll | Out-Default
        
        Copy-Item -Path ".\Src\Newtonsoft.Dson.Tests\bin\Release\$finalDir\Newtonsoft.Dson.Tests.dll" -Destination $workingDir\Deployed\Bin\$finalDir\

        Write-Host -ForegroundColor Green "Running tests " $name
        Write-Host
        exec { .\Tools\NUnit\nunit-console.exe "$workingDir\Deployed\Bin\$finalDir\Newtonsoft.Dson.Tests.dll" /framework=$framework /xml:$workingDir\$name.xml | Out-Default } "Error running $name tests"
    }
  }
}

function GetConstants($constants, $includeSigned)
{
  $signed = switch($includeSigned) { $true { ";SIGNED" } default { "" } }

  return "/p:DefineConstants=`"CODE_ANALYSIS;TRACE;$constants$signed`""
}

function GetVersion($majorVersion)
{
    $now = [DateTime]::Now
    
    $year = $now.Year - 2000
    $month = $now.Month
    $totalMonthsSince2000 = ($year * 12) + $month
    $day = $now.Day
    $minor = "{0}{1:00}" -f $totalMonthsSince2000, $day
    
    $hour = $now.Hour
    $minute = $now.Minute
    $revision = "{0:00}{1:00}" -f $hour, $minute
    
    return $majorVersion + "." + $minor
}

function Update-AssemblyInfoFiles ([string] $sourceDir, [string] $assemblyVersionNumber, [string] $fileVersionNumber)
{
    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $assemblyVersion = 'AssemblyVersion("' + $assemblyVersionNumber + '")';
    $fileVersion = 'AssemblyFileVersion("' + $fileVersionNumber + '")';
    
    Get-ChildItem -Path $sourceDir -r -filter AssemblyInfo.cs | ForEach-Object {
        
        $filename = $_.Directory.ToString() + '\' + $_.Name
        Write-Host $filename
        $filename + ' -> ' + $version
    
        (Get-Content $filename) | ForEach-Object {
            % {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
            % {$_ -replace $fileVersionPattern, $fileVersion }
        } | Set-Content $filename
    }
}