$env:PATH = "D:\tools\dotnet-sdk-10.0.400-win-x64;$env:PATH"
$repo = "D:\code\vertical-slice-architecture"

dotnet new uninstall Vertical.Slice.Architecture
Remove-Item "$repo\artifacts\package" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$repo\scratch" -Recurse -Force -ErrorAction SilentlyContinue

dotnet pack "$repo\nuspec.csproj"
dotnet new install "$repo\artifacts\package\release\*.nupkg"