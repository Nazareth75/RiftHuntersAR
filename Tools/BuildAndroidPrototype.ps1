[CmdletBinding()]
param([string]$EditorRoot = '')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$prototypeRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
if ([string]::IsNullOrWhiteSpace($EditorRoot)) {
    $versionLine = Get-Content -LiteralPath (Join-Path $prototypeRoot 'ProjectSettings/ProjectVersion.txt') | Where-Object { $_ -match '^m_EditorVersion:' }
    $unityVersion = ($versionLine -split ':', 2)[1].Trim()
    $EditorRoot = Join-Path 'C:\Program Files\Unity\Hub\Editor' "$unityVersion/Editor"
}
$androidTools = Join-Path $EditorRoot 'Data/PlaybackEngines/AndroidPlayer'
$java = Join-Path $androidTools 'OpenJDK/bin/java.exe'
$gradleJar = Join-Path $androidTools 'Tools/gradle/lib/gradle-launcher-8.13.jar'
$generated = Join-Path $prototypeRoot 'Library/Bee/Android/Prj/IL2CPP/Gradle'
$launcherGradle = Join-Path $generated 'launcher/build.gradle'
if (!(Test-Path -LiteralPath $launcherGradle)) { throw 'Generate the Android project with Unity first (Rift Hunters > Build Android Prototype).' }
# This script packages Unity-generated data; regenerate it in Unity after changing gameplay or scene assets.
$key = Join-Path $prototypeRoot 'Builds/Toolchain/debug.keystore'
if (!(Test-Path -LiteralPath $key)) {
    $existingDebugKey = Join-Path ([Environment]::GetFolderPath('UserProfile')) '.android/debug.keystore'
    if (!(Test-Path -LiteralPath $existingDebugKey)) { throw 'No existing Android debug keystore. Generate a development build with Unity first.' }
    New-Item -ItemType Directory -Path (Split-Path -Parent $key) -Force | Out-Null
    Copy-Item -LiteralPath $existingDebugKey -Destination $key
}
$groovyKey = $key.Replace('\','/').Replace("'","\'")
$gradleLines = [IO.File]::ReadAllLines($launcherGradle) | Where-Object { !($_.TrimStart().StartsWith('android.signingConfigs.debug.storeFile =')) }
$gradleText = ($gradleLines -join "`n") + "`nandroid.signingConfigs.debug.storeFile = file('$groovyKey')`n"
[IO.File]::WriteAllText($launcherGradle,$gradleText,[Text.UTF8Encoding]::new($false))
$rootGradle = Join-Path $generated 'build.gradle'
$shortRoot = ($prototypeRoot + '/Builds/g/').Replace('\','/').Replace("'","\'")
$rootText = [IO.File]::ReadAllText($rootGradle)
if (!$rootText.Contains('// Rift short build paths')) {
    [IO.File]::WriteAllText($rootGradle,$rootText + "`n// Rift short build paths`nallprojects { layout.buildDirectory.set(file('$shortRoot' + project.name)) }`n",[Text.UTF8Encoding]::new($false))
}
$aapt = Join-Path $prototypeRoot 'Builds/Toolchain/AndroidSDK/build-tools/36.0.0/aapt2.exe'
if (!(Test-Path -LiteralPath $aapt)) { $aapt = Join-Path $androidTools 'SDK/build-tools/36.0.0/aapt2.exe' }
if (!(Test-Path -LiteralPath $java) -or !(Test-Path -LiteralPath $gradleJar) -or !(Test-Path -LiteralPath $aapt)) { throw 'Required Unity/SDK tools are missing; see PROJECT_STATUS.md.' }
$logDir = Join-Path $prototypeRoot 'Logs'
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$arguments = @('-classpath',$gradleJar,'org.gradle.launcher.GradleMain',':launcher:assembleDebug','--no-daemon','--console=plain',"-Pandroid.aapt2FromMavenOverride=$aapt")
# Use the standard short Gradle cache. A project-local cache beneath this long project path exceeds Ninja's 260-character limit.
Push-Location $generated
try {
    & $java @arguments 2>&1 | Tee-Object -FilePath (Join-Path $logDir 'GradlePrototype.log')
    if ($LASTEXITCODE -ne 0) { throw "Gradle failed with exit code $LASTEXITCODE. Inspect Logs/GradlePrototype.log." }
}
finally { Pop-Location }
$sourceApk = Join-Path $prototypeRoot 'Builds/g/launcher/outputs/apk/debug/launcher-debug.apk'
if (!(Test-Path -LiteralPath $sourceApk)) { throw 'Gradle returned success without the expected APK.' }
$destination = Join-Path $prototypeRoot 'Builds/Android/RiftHuntersAR_Prototype.apk'
New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
Copy-Item -LiteralPath $sourceApk -Destination $destination -Force
Get-FileHash -LiteralPath $destination -Algorithm SHA256
Write-Output "APK: $destination"
