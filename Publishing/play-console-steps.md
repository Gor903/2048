# Console submission, step by step

Everything the human pastes, in the order the console asks for it.

Menu labels change without notice. If wording has drifted, match by
meaning — the order of steps does not depend on it.

Every number here was read from the artifact on 18 October 2026, never from
a build log. Re-read them with the commands in `release-build.md` after any
rebuild; a rebuild changes sizes and invalidates the sideload APK.

## Keep at hand

| What | Where |
| --- | --- |
| Bundle | `Build/tilevault.aab` — 43 293 752 bytes |
| Debug symbols | `Build/tilevault-1.0.0-v1-IL2CPP.symbols.zip` — 26 292 526 bytes |
| Sideload APK | `Build/tilevault-universal-debugsigned.apk` — 43 425 369 bytes |
| Store icon | `Publishing/art/store-icon-512.png` — 512×512 |
| Feature graphic | `Publishing/art/feature-graphic-1024x500.png` — 1024×500 |
| Screenshots, en-US | `Publishing/art/screenshots/` — six, 1080×1920 |
| Privacy policy | https://tilevault-privacy.gor-beglaryan-rw.workers.dev/ |
| Support email | `gor.beglaryan.rw@gmail.com` |

Build facts the console may ask for, all taken from the artifact:

| Fact | Value |
| --- | --- |
| Application id | `com.gor903.tilevault2048` |
| Version name | 1.0.0 |
| Version code | 1 |
| Minimum API | 26 |
| Target API | 36 |
| Architectures | `armeabi-v7a`, `arm64-v8a` |
| Permissions | `android.permission.VIBRATE` — the whole list |
| Download size | 23.6–25.1 MB per device, **not** the 43 MB bundle |

Signing fingerprint to compare after upload:

```
SHA-256: 84:94:C4:56:C7:31:80:1D:3C:05:68:EB:E7:98:74:A2:EC:0A:13:79:26:30:43:DC:AC:24:B6:8B:4C:21:3A:97
SHA-1:   18:AB:20:BE:55:ED:71:54:47:C1:ED:5B:92:E4:87:C8:F8:55:50:FB
```

Read back with `keytool -printcert -jarfile Build/tilevault.aab`. The owner
must be `CN=Gor Beglaryan, O=Gor903, C=AM` — anything reading
`CN=Android Debug` is the wrong artifact.

## Before you open the console

- [ ] Install the sideload APK on a real phone and play it. Editor captures
      have no status bar, cutout or gesture bar and are not a test
- [ ] Confirm the signing key copy on the separate disk still opens —
      `keytool -list -v -keystore <copy>`, never a file-size comparison

## Step 0 — Developer account

$25, once, non-refundable, plus identity verification that can take days.
Nothing below can start until it clears.

Use an address that is **not** the public support email. The support address
is visible on the listing; the account address is not. Record which is which
here once chosen:

| Role | Address |
| --- | --- |
| Console account | |
| Public support contact | `gor.beglaryan.rw@gmail.com` |

## Step 1 — Create app

| Field | Value |
| --- | --- |
| App name | `Tilevault 2048` |
| Default language | English (United States) — en-US |
| App or game | Game |
| Free or paid | Free |

Irreversible here, all three:

- **Free → paid is one-way.** A free app can never become paid.
- **The default language** sets the primary listing locale permanently.
- **The application id** `com.gor903.tilevault2048` is fixed by the first
  upload and can never be changed or reused.

Tick the declarations about developer-programme policies and US export law.

## Step 2 — App content

One section per form. The basis for every answer is in
`play-console-answers.md`; it is not restated here.

### Privacy policy

Paste the URL into **App content → Privacy policy**. It also goes into
**Store listing → Privacy policy** at Step 3 — two separate fields, same URL,
and the listing one is easy to miss.

```
https://tilevault-privacy.gor-beglaryan-rw.workers.dev/
```

The page must stay reachable after publication. A 404 here is grounds for
removal, not merely rejection.

### App access

**All functionality is available without special access.** No login, no
access code, no region lock, no purchase gate. Nothing to supply to the
reviewer.

### Ads

**No, my app does not contain ads.**

There is no ad SDK compiled into this build. Answering yes for code that is
not present would be a false declaration.

### Content rating

Start the questionnaire. Email: the support address. Category: **Game**.

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
Submit the questionnaire — the rating is not applied until you do.

### Target audience and content

| Field | Answer |
| --- | --- |
| Age groups | 13–15, 16–17, 18+ |
| Appeals to children by design | No |

Selecting any group under 13 brings the Families policy, a stricter ads and
SDK regime, and a separate content review — none of it needed here.

### Data safety

| Question | Answer |
| --- | --- |
| Does the app collect or share user data? | **No** |
| Encryption in transit | Not applicable — no network access |
| Deletion on request | Not applicable — nothing is collected |

Expected result: a Data safety section reading **No data collected** and
**No data shared**.

### Remaining declarations

| Declaration | Answer |
| --- | --- |
| Advertising ID | **No** — `AD_ID` is not in the manifest |
| Restricted permissions | None — the verified list is `VIBRATE` only |
| Background location | No |
| Health, finance, government, news | No |
| Financial features | None |
| Government app | No |

## Step 3 — Store listing

**Main store listing → en-US.**

App name — 14 of 30 characters:

```
Tilevault 2048
```

Short description — 73 of 80 characters:

```
Slide tiles, merge numbers, beat your best. Offline, no ads, no nonsense.
```

Full description — 1868 of 4000 characters. Copy it from the fenced block in
`store-listing.md`. It lives in exactly one file on purpose: two copies of a
1868-character block drift, and `check-listing.py` measures only that one.

Graphics:

| Asset | File |
| --- | --- |
| App icon | `art/store-icon-512.png` |
| Feature graphic | `art/feature-graphic-1024x500.png` |
| Phone screenshots | `art/screenshots/`, uploaded 01 → 06 in filename order |

Screenshot order and what each frame shows is the table in
`store-listing.md`. No video.

Store settings:

| Field | Value |
| --- | --- |
| App category | Games → Puzzle |
| Tags | Puzzle, Casual, Brain games, Single player, Offline |
| Support email | `gor.beglaryan.rw@gmail.com` |
| Privacy policy | the URL from Step 2 — this field is separate |

Run `python3 Publishing/check-listing.py` before pasting. It measures every
limit above rather than trusting the counts written here.

## Step 4 — Internal testing

Internal testing has no tester-count or duration requirement, so it is the
fastest way to prove the upload pipeline and the real signed install.

1. **Testing → Internal testing → Create new release.**
2. Accept **Play App Signing**. This is the point where the developer key
   becomes an *upload* key and losing it stops being fatal — Google holds
   the distribution key from here on. Keep the upload key anyway: it is how
   every future release is authenticated.
3. Upload `Build/tilevault.aab`.
4. Upload `Build/tilevault-1.0.0-v1-IL2CPP.symbols.zip` so crash traces
   deobfuscate.
5. Release name: `1.0.0 (1)`. Release notes: first release.
6. After processing, open **App integrity** and compare the **upload key**
   certificate against the SHA-256 above. They must match. The **app signing
   key** will be a different fingerprint — that is Google's, and is correct.
7. Add yourself as a tester, then **install from the store link**, not by
   sideloading. That exercises the real signature and per-device delivery.

Expect the download to be roughly 24 MB, not 43 MB.

## Step 5 — Closed testing

**This is the critical path, and it is measured in weeks.** A personal
developer account created after 13 November 2023 must run a closed test with
a minimum number of testers opted in continuously for a minimum period before
production access is granted.

Read the current requirement off the console rather than a remembered number
— Google has changed both the count and the duration more than once.

Start recruiting before polishing anything else. Every other item in this
document is an evening of work; this one is the calendar.

Testers must stay opted in for the whole period. Someone who opts out resets
their own contribution, so recruit more than the minimum.

## Step 6 — Production

Only available once Step 5's requirement is satisfied and the console says so.

1. **Production → Create new release.**
2. Promote the tested bundle rather than uploading a fresh one, so the
   artifact that ships is the artifact that was tested.
3. Countries: all. The interface falls back to English for any unshipped
   locale, so there is no country that gets a broken build.
4. Rollout percentage: start at 20% if the option is offered. There is no
   telemetry in this build, so the only failure signal is the console's crash
   rate and the reviews — both lag.
5. Submit for review.

If the bundle is ever rebuilt before this step, the version code must be
higher. Bump it with `ReleaseTool.CIBumpVersionCode`, never by hand.

## If the console objects

| Objection | Cause | Fix |
| --- | --- | --- |
| Version code already used | A build with that code was uploaded before | `CIBumpVersionCode`, rebuild, re-upload |
| Debuggable / development build | `Development Build` left on | Preflight catches this; re-run Apply Settings and rebuild |
| Package name already in use | The application id is taken globally | Nothing to do but choose another — and it is fixed forever at first upload |
| Privacy policy unreachable | The worker is down or the URL moved | Re-deploy from `Publishing/privacy`; verify 200 before resubmitting |
| Target API too low | Google raised the floor | Raise `ReleaseSettings.TargetSdk`, install the SDK platform, rebuild |
| Data safety mismatch | A permission appeared that the form does not declare | Re-dump the manifest; the form follows the artifact, not the other way round |

Write the project's own here as they come up.
