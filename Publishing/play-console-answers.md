# Console form answers

Every answer below follows from what the build actually does. If that stops
being true — an ad network appears, analytics go in, a leaderboard is added —
these answers change and Data safety has to be resubmitted.

## Basis

Verified against the artifact, not from memory. See
`Publishing/release-build.md` for the commands and their output.

| Fact | How it was established |
| --- | --- |
| No network code | `NullAdService` is the only `IAdService` implementation compiled in; no ad SDK is present in the project |
| No `INTERNET` permission | Stripped by `Assets/Plugins/Android/AndroidManifest.xml`; asserted absent by the Phase 11 bundle dump |
| Only `VIBRATE` is requested | Full permission list read from the bundle manifest |
| No analytics, no crash reporting | No such package in `Packages/manifest.json`; no SDK in `Assets/` |
| Storage is local and private | One JSON file in `Application.persistentDataPath`, written by `SaveService` |
| No purchases | No billing library, no IAP code |

The save file holds exactly: best score per board size and mode, daily
challenge best and streak, the game in progress, and settings (sound,
vibration, chosen theme, unlocked themes). No identifier of any kind is
generated or stored.

## Store listing

- **Category:** Games → Puzzle
- **Tags:** Puzzle, Casual, Brain games, Single player, Offline
- **Contains ads:** **No** — there is no ad SDK in the build
- **In-app purchases:** **No**
- **Free or paid:** **Free** — free → paid is not reversible

## App access

**All functionality is available without special access.** No login, no code,
no region lock, no purchase gate. Every mode, board size and power-up is
reachable from a clean install. Two of the four themes start locked, but they
are cosmetic and unlock without payment.

Nothing needs to be supplied to the reviewer.

## Ads

**This app does not contain ads.**

Do not tick the ads declaration. `PLAN.md` describes an AdMob integration for
a later version; it is not in this build, and answering "yes" for code that
is not present would be a false declaration.

When ads are added, the following all change together: this answer, the ads
declaration, the advertising-ID declaration, the Data safety form, the
`INTERNET` permission, and the privacy policy.

## Data safety

- **Does the app collect user data?** **No**
- **Does it share data with third parties?** **No**
- **Encryption in transit:** Not applicable — the app has no network access
- **Deletion on request:** Not applicable — nothing is collected to delete

Data that never leaves the device and goes away with the app is not
"collection" in this form's terms. The keys involved are listed under Basis
above, so the answer can be rechecked rather than taken on trust.

Expected result: a Data safety section reading "No data collected" and
"No data shared".

## Content rating questionnaire

Questionnaire email: the support address (see Developer contacts).
Category: **Game**.

| Question | Answer |
| --- | --- |
| Violence, blood, fear | No |
| Sexual content, nudity | No |
| Profanity | No |
| Drugs, alcohol, tobacco | No |
| Gambling and simulated gambling | No |
| Real-money purchases | No |
| Data sharing, location | No |
| User interaction, user-generated content | No |

Expected result: **Everyone / PEGI 3 / USK 0** or each region's equivalent.

## Target audience and content

- **Age groups:** 13–15, 16–17, 18+
- **Appeals to children by design:** No

The game is a number puzzle with a restrained palette and no characters,
mascots, bright-reward loops or child-directed language. Selecting any group
under 13 brings the Families policy, a stricter ads and SDK regime, and a
separate content review — all of it unnecessary here.

Answer **No** to "Is your app designed primarily for children?"

## Declarations

| Declaration | Answer |
| --- | --- |
| Contains ads | No |
| Advertising ID | **No** — `com.google.android.gms.permission.AD_ID` is not in the manifest |
| Restricted permissions | None. The verified permission list is `VIBRATE` only |
| Background location | No |
| Health, finance, government, news | No |
| Data safety resubmission needed | No |

For restricted permissions, point at the Phase 11 permission dump rather than
asserting it from memory.

## Privacy policy

- **URL:** https://tilevault-privacy.gor-beglaryan-rw.workers.dev/ — goes in
  both **Store listing → Privacy policy** and **App content → Privacy policy**
- **Source:** `Publishing/privacy/index.html` plus `style.css`, self-contained,
  no scripts and no external resources, so any static host will serve it
- **Host:** Cloudflare, project `tilevault-privacy`, redeployed with
  `wrangler pages project create` from inside `Publishing/privacy`. The domain
  is `workers.dev` rather than the `pages.dev` the QuietBlocks policy uses,
  because Cloudflare now folds Pages into Workers; for two static files the
  two are the same thing, and the surviving product is the safer home for a
  URL that must never 404

The page must stay reachable after publication. A 404 on the privacy policy
is grounds for removal, not merely rejection — so a host that hands out a URL
without an account does not qualify.

## Developer contacts

- **Support email:** `gor.beglaryan.rw@gmail.com`, the same address the
  QuietBlocks listing uses. It is in `Publishing/privacy/index.html` in both
  places, and `Publishing/check-listing.py` passes.

It is publicly visible on the listing, must match the contact in the privacy
policy, and is best kept distinct from the Console account address.
