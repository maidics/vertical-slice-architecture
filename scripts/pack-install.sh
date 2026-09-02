#!/bin/bash
rm -rf ./artifacts/package/release/
rm -rf ./scratch/
dotnet pack nuspec.csproj
dotnet new uninstall Vertical.Slice.Architecture
dotnet new install ./artifacts/package/release/*.nupkg
