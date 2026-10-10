$repo = "D:\code\vertical-slice-architecture"

dotnet new uninstall Vertical.Slice.Architecture
Remove-Item "$repo\artifacts\package" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$repo\scratch" -Recurse -Force -ErrorAction SilentlyContinue

dotnet pack "$repo\nuspec.csproj"
dotnet new install "$repo\artifacts\package\release\*.nupkg"