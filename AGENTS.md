# Vertical Slice Architecture Template Agent Guide

Use the specified SDK version in [`global.json`](global.json). Solution uses central package management: [`Directory.Packages.props`](Directory.Packages.props).

---

## Validation

- Run tests relevant to the change before pushing code.
- Instantiating the template is only required if the user asks for it or before opening a PR.
- Instatiate template with relevant options to your change.
- Prior instantiation:
  - Delete `./artifacts/package/release` folder recursively.
  - Delete `./scratch` folder recursively.
  - `dotnet pack nuspec.csproj`
  - `dotnet new uninstall Vertical.Slice.Architecture`
  - `dotnet new install ./artifacts/package/release/*.nupkg`
- Use the following base command for instantiating the template: `dotnet new vsa-sln -n Scratch -o ./scratch` - extend with options as required.

## Git workflow

- Use conventional naming when it comes to commits and branches.
- Never use the Claude Code Web default branch: `claude/`, create a new branch for your changes.
- Use a feature branch and a ready-for-review PR by default. Create drafts only when requested.
- Trivial documentation and typo changes may go directly into main.