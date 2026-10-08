# Publishing — Tilevault 2048

The one place to answer "can we ship yet". Keep it true.

**Current answer: not yet.** Three things block upload, all of them human
decisions rather than engineering work. They are listed under
[Left before upload](#left-before-upload).

## Automation

| Menu | What it does |
| --- | --- |
| Tilevault → Release → Apply Settings | Sets every platform setting |
| Tilevault → Release → Verify | Lists what blocks a release build |
| Tilevault → Release → Build Bundle | Release build into `Build/` |
| Tilevault → Art → Generate All | Regenerates icons and store art |
| Tilevault → Art → Capture Screenshots | Regenerates the store screenshot set |
| Tilevault → Rebuild Main Scene | Regenerates the single shipping scene |

Command-line equivalents, including the version-code bump, are in
`release-build.md`. Nothing here is a checkbox someone has to remember
ticking — the build refuses to run if any of it drifts.

## Name

| Where | Value |
| --- | --- |
| In game (title screen) | Tilevault, with "2048" as a subtitle |
| Store listing, en-US | Tilevault 2048 |
| Launcher label, package, artifact | Tilevault |
| Application id | `com.gor903.tilevault2048` |

The launcher label is one string for every language — Android does not
localise it, so seeing the same text across all locales is correct.

**The application id is fixed permanently by the first upload.**

### Name research actually performed

Store search (Google Play, App Store, Steam, Microsoft Store) and a USPTO
search, both via web search, before any code existed. **A formal trademark
clearance search was not done, and no non-US registry was checked.** Store
search finding nothing is not a trademark search.

Three candidates were rejected on collisions, including the first two
proposed: *Stonefall* collides with "Stonefall Defense: Bolt Puzzle", a live
puzzle game on Play, and *Tilefall* with "Tilefall: Block Puzzle". Anything
containing *Merge* was ruled out by the live **2048 MERGE GAMES** registration
(Guru Network Limited Inc.). Full record in `docs/DECISIONS.md`.

## Done

- [x] Platform target, bundle output, development build off
- [x] Scripting backend (IL2CPP), ARMv7 + ARM64, High stripping
- [x] Application id
- [x] Target API 35, minimum API 26
- [x] Version name 1.0.0, version code 1
- [x] App icon (adaptive, two layers)
- [x] Store icon 512×512 — verified fully opaque
- [x] Feature graphic 1024×500 — verified fully opaque
- [x] Screenshots — 6 frames, 1080×1920, captured from the real interface
- [x] Interface localised, every string in one table, zero literals in views
- [x] Only the shipping scene in the build list
- [x] Portrait lock and safe-area rendering
- [x] Tests green — 76 edit-mode, 12 play-mode, from the command line
- [ ] Privacy policy written — **placeholder support address still in it**
- [x] Artifact builds — `Build/tilevault.aab`, 44 MB
- [x] Artifact signed with the release key, not the debug key —
      `CN=Gor Beglaryan, O=Gor903, C=AM`, SHA256withRSA
- [x] Debug symbols exported — `tilevault-1.0.0-v1-IL2CPP.symbols.zip`, 27.7 MB
- [x] Permission list verified against the artifact — `VIBRATE` is the whole
      list, `INTERNET` and `ACCESS_NETWORK_STATE` both count 0
- [x] Download size measured from the artifact — 25.0–26.5 MB per device,
      which is not the 44 MB bundle

Everything above is verified against the artifact or by command output, never
from a build log. A Unity build summary once reported a 35 MB bundle as
"664 MB".

### Permissions

| Permission | Why |
| --- | --- |
| `android.permission.VIBRATE` | The short tick when tiles merge. Switchable off in Settings |

That is the whole list. Unity injects `INTERNET` and `ACCESS_NETWORK_STATE`
by itself even though this build opens no socket; both are stripped by
`Assets/Plugins/Android/AndroidManifest.xml` using `tools:node="remove"`.

**Deleting that manifest silently restores them**, with no warning from
anywhere — which would contradict both the privacy policy and the Data safety
answers. Its existence is therefore a preflight check, and the build refuses
to run without it.

## Decisions for the human

### Language

Default **en-US**, the only locale at launch. The interface reads the system
locale at startup and falls back to English, so shipping to all countries is
safe. Adding a language is a new table in `Strings.cs` and a new listing — no
code changes.

### Signing key

| | |
| --- | --- |
| Keystore | `~/keystores/tilevault.keystore` |
| Backup | `~/keystores/tilevault.keystore.backup1` |
| Credentials | `~/keystores/tilevault-signing.env`, mode 600 |
| Alias | `tilevault` |
| Certificate | `CN=Gor Beglaryan, O=Gor903, C=AM`, RSA 2048 |
| Valid until | 23 February 2054 |
| Last verified | 8 October 2026, both copies opened with `keytool -list` |

Both copies are **on the same machine**, which is not a backup. One copy
belongs somewhere else entirely. Verify any new copy by opening it — a
corrupt keystore is byte-identical in length to a good one.

### Monetisation

Free. No in-app purchases. **No ads in this build** — there is no ad SDK
compiled in, which is why `INTERNET` is stripped and "contains ads" is No.

`PLAN.md` §4 specifies an AdMob integration. It needs an AdMob account that
only you can create, and turning it on moves six things together: the
`RequiresInternet` constant, the manifest entry, the ads declaration, the
advertising-ID declaration, the Data safety form, and the privacy policy.
Shipping without it is the honest option and leaves that door open.

### Everything else in the console

- Developer account — $25, identity verification, yours to create
- Privacy policy hosting — needs a URL that outlives publication
- Content rating, data safety, target audience — answers in
  `play-console-answers.md`
- Testing track requirements — read the current rule in the console

Record which address is the console account and which is the public support
contact. They are easy to confuse, and one of them is visible to every player.

## Left before upload

- [ ] **Choose a public support email.** `privacy/index.html` carries
      `SUPPORT_EMAIL_PLACEHOLDER` in two places;
      `python3 Publishing/check-listing.py` fails while it is there
- [ ] **Host the privacy policy over HTTPS** and put the URL in two console
      fields. A 404 later is grounds for removal, not just rejection
- [ ] **Back the signing key up off this machine**, verified by opening it
- [ ] **Real device run** — install
      `Build/tilevault-universal-debugsigned.apk`, built from this same
      bundle. Editor captures have no status bar, cutout or gesture bar and
      are not a test
- [ ] Submit listing, forms and artifact — steps in `play-console-steps.md`

## Worth doing, not blocking

- **Unused dependencies.** The build currently compiles 2D Animation,
  SpriteShape, PathTracing, UnifiedRayTracing, GPUDriven rendering, Timeline,
  Visual Scripting and both Physics modules. The game uses none of them.
  `Assets/Welcome/` holds three ScriptableObjects owned by
  `com.unity.learn.iet-framework` — package and assets must be removed
  together or broken assets are left behind.
- **Template leftovers.** `Assets/Scenes/SampleScene.unity` and
  `Assets/Welcome/` do not reach the build but drag dependencies with them.
- **Engine splash screen.** Disabling it needs a paid Unity plan; on the free
  tier it stays.
- **Closed testing is the calendar.** A personal account created after
  November 2023 needs testers opted in continuously for a fixed period before
  production opens. Everything else here is an evening of work; that part is
  weeks, so recruit testers before polishing the listing.
