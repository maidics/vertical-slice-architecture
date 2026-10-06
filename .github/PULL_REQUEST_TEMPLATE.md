## Summary

<!-- What changed and why. -->

## Related Issue

<!-- Link to the issue this PR addresses (use "Fixes #123" or "Closes #123" to auto-close). Delete this section if not relevant. -->

## Checklist

### Testing

- [ ] All existing tests pass (`dotnet test`)
- [ ] I have added tests that cover my changes

### Template requirements

- [ ] `README.md` reflects changes
- [ ] `.template.config/template.json` is valid and reflects any new/renamed/removed parameters
- [ ] `dotnet pack nuspec.csproj` succeeds and produces **exactly one** `.nupkg` in `./artifacts/package/release/`
- [ ] No artifacts or repository files leaked into the package: `bin/`, `obj/`, `.vs/`, `.idea/`, `.git/`, `*.db*`, `*.user`, `*.nupkg`, `.gitattributes`, `AGENTS.md` etc.
- [ ] Instatiated the template with the options relevant to changes: <specify-used-command-here>
- [ ] Instantiated template builds, passes tests and runs via Aspire

## Notes

<!-- Any additional notes that reviewers should know. Delete this section if not relevant -->