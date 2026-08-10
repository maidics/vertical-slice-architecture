## Summary

<!-- What changed and why -->

## Template integrity

- [ ] `dotnet pack nuspec.csproj` succeeds and produces **exactly one** `.nupkg` in `./artifacts/package/release/`
- [ ] Package **version bumped if the changes are not inside the ``.github/`` or ``docs/`` folders exclusively** (NuGet rejects an already-published version)
- [ ] `dotnet new install ./artifacts/package/release/*.nupkg` installs without error
- [ ] `dotnet new vsa-sln -n Scratch -o ./scratch` instantiates cleanly
- [ ] `dotnet build ./scratch/Scratch.slnx` builds green
- [ ] `dotnet test ./scratch/Scratch.slnx` passes
- [ ] App runs via Aspire

## Packaging hygiene

- [ ] No artifacts leaked into the package: no `bin/`, `obj/`, `.vs/`, `.idea/`, `.git/`, `*.db*`, `*.user`, stray `*.nupkg`
- [ ] `.template.config/template.json` is valid and reflects any new/renamed/removed parameters
- [ ] Ran `dotnet new uninstall` / reinstalled locally if the template identity or `shortName` changed

## Meta

- [ ] README updated if required changed
- [ ] CI is green on this branch