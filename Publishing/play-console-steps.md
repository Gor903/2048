# Console submission, step by step

The phase-14 deliverable. Everything the human pastes, in the order the
console asks for it, so they invent nothing and go nowhere else.

Menu labels change without notice. If wording has drifted, match by
meaning — the order of steps does not depend on it.

## Keep at hand

| What | Where |
| --- | --- |
| Bundle | `Build/tilevault.aab` — 43 290 904 bytes |
| Debug symbols | `Build/tilevault-1.0.0-v1-IL2CPP.symbols.zip` |
| Store icon | `Publishing/art/store-icon-512.png` — 512×512 |
| Feature graphic | `Publishing/art/feature-graphic-1024x500.png` — 1024×500 |
| Screenshots, en-US | `Publishing/art/screenshots/` — six, 1080×1920 |
| Privacy policy | https://tilevault-privacy.gor-beglaryan-rw.workers.dev/ |
| Support email | `gor.beglaryan.rw@gmail.com` |

Build facts the console may ask for: application id, version code,
version name, minimum and target API, architectures, permission list.
All taken from the artifact, not the log.

Signing fingerprint to compare after upload:

```
SHA-256: <fingerprint>
```

## Step 1 — Create app

| Field | Value |
| --- | --- |
| App name | |
| Default language | |
| App or game | |
| Free or paid | |

Flag the irreversible choices right here: free → paid is one-way, the
default language sets the primary listing locale, and the application id
is fixed by the first upload.

## Step 2 — App content

One section per form. For each: the answer, and the reason it follows
from the build. Cross-reference `play-console-answers.md` rather than
restating the basis.

- Privacy policy — URL, and which field it belongs in
- App access
- Ads
- Content rating questionnaire
- Target audience and content
- Data safety
- Remaining declarations

## Step 3 — Store listing

One subsection per locale. Each text in a fenced block with its measured
character count against the limit, so it can be diffed against
`store-listing.md`.

Then store settings: category, tags, support email.

## Step 4 — Internal testing

Upload the bundle and the symbols. Accept store-managed signing: the
developer key becomes an upload key, and losing it stops being fatal.

After processing, compare the upload key fingerprint against the one
above.

Install from the store rather than sideloading — real signature, real
per-device delivery.

## Step 5 — Closed testing

Record the account's actual requirement from the console, not a
remembered number. If a tester-count-and-duration rule applies, this is
the critical path and starts before the listing is polished.

## Step 6 — Production

## If the console objects

Common rejections and the fix for each — version code already used,
debuggable build uploaded, package name taken, privacy policy
unreachable. Write the project's own as they come up.
