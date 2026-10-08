# Publishing

Status of the project and what is left. Keep this file true — it is the
one place anyone looks to answer "can we ship yet".

Write this document in the language the design documents use.

## Automation

| Command | What it does |
| --- | --- |
| `<menu path> → Apply release settings` | Sets every platform setting |
| `<menu path> → Verify release readiness` | Prints what blocks a release build |
| `<menu path> → Bump build number` | `versionCode += 1` before each upload |
| `<menu path> → Build bundle` | Release build into `<output dir>` |
| `<menu path> → Capture store screenshots` | Regenerates the store set |

Command-line equivalents: see `release-build.md`.

## Name

| Where | Value |
| --- | --- |
| In game, `<locale>` system | |
| In game, other | |
| Store listing, `<locale>` | |
| Store listing, default | |
| Launcher label, package, build artifact | |
| Application id | |

The launcher label is one string for every language — Android does not
localise it.

Record what name research was actually done: store search, trademark
registry, or both. They are not the same check.

The application id is fixed permanently by the first upload.

## Done

- [ ] Platform target, bundle output, development build off
- [ ] Scripting backend, architectures, code stripping
- [ ] Application id
- [ ] Target and minimum API
- [ ] Version name and version code
- [ ] App icon
- [ ] Store icon 512×512
- [ ] Feature graphic 1024×500
- [ ] Screenshots, per locale
- [ ] Interface localised, strings in one file
- [ ] Privacy policy written, no placeholders left
- [ ] Artifact signed with the release key, not the debug key
- [ ] Debug symbols exported
- [ ] Only shipping scenes in the build list
- [ ] Orientation and safe-area rendering
- [ ] Permission list reviewed
- [ ] Tests green
- [ ] Artifact builds

Verify these against the artifact, not the build log. Record the
verification output, not a claim.

### Permissions

List every permission in the built artifact and why each is there. If
the engine injected one that the game does not need, record how it was
removed and what would silently bring it back.

## Decisions for the human

### Language

### Signing key

Location, alias, certificate validity. Where the backups are and when
each was last verified by opening it.

### Monetisation

### Everything else in the console

- Developer account
- Privacy policy hosting — URL
- Content rating, data safety, target audience — answers in
  `play-console-answers.md`
- Testing track requirements and timeline

Record which email is the console account and which is the public
support contact. They are easy to confuse and one of them is visible to
players.

## Left before upload

- [ ] Signing key backed up off this machine, verified by opening
- [ ] Real device run — checklist at the end of `release-build.md`
- [ ] Privacy policy live over HTTPS
- [ ] Listing, forms and artifact submitted — steps in
      `play-console-steps.md`

## Worth doing, not blocking

Things that do not block publication but deserve attention: engine
splash screen, template leftovers, unused dependencies, packaging
options, architecture coverage.
