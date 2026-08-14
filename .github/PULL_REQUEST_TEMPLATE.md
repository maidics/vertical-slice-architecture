## Summary

<!-- What changed and why -->

## Template integrity

- [ ] `.template.config/template.json` is valid and reflects any new/renamed/removed parameters
- [ ] README updated if required to change
- [ ] Package **version bumped if required for new release (NuGet rejects an already-published version)
- [ ] `dotnet pack nuspec.csproj` succeeds and produces **exactly one** `.nupkg` in `./artifacts/package/release/`
- [ ] No artifacts leaked into the package: no `bin/`, `obj/`, `.vs/`, `.idea/`, `.git/`, `*.db*`, `*.user`, `*.nupkg`
- [ ] `dotnet new uninstall Vertical.Slice.Architecture`
- [ ] `dotnet new install ./artifacts/package/release/*.nupkg` installs without error
- [ ] `dotnet new vsa-sln -n Scratch -o ./scratch` instantiates cleanly
- [ ] `dotnet build ./scratch/Scratch.slnx` builds
- [ ] `dotnet test ./scratch/Scratch.slnx` passes
- [ ] App runs via Aspire