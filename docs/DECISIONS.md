# Irreversible decisions — confirmed

Confirmed by Gor Beglaryan, 2026-10-08, before any code was written.
Nothing here changes without a deliberate decision to restart the listing.

| # | Decision | Value | Reversible? |
| --- | --- | --- | --- |
| 1 | Store name | **Tilevault 2048** | Hard after publication, impossible after a rights complaint |
| 2 | Launcher label | **Tilevault** | Changeable, but Android never localises it |
| 3 | Application id | **com.gor903.tilevault2048** | **Never** — fixed by the first upload |
| 4 | Monetisation | **Free, ad-supported** | Free → paid is impossible |
| 5 | Default store language | **en-US**, English only at launch | Default is fixed; more locales can be added later |
| 6 | Minimum API | **24** (Android 7.0) | Raisable later, lowering drops existing users |
| 7 | Target API | Current Play requirement, set in code | Must rise over time |
| 8 | Signing key | `CN=Gor Beglaryan, O=Gor903, C=AM`, RSA 2048, 10000 days | **Never** — losing it ends updates |

Monetisation was not asked as an open question: `PLAN.md` §4 specifies a full
AdMob plan and §7 lists remove-ads IAP as post-v1, so free-with-ads is already
specified by the design.

## Name collision research

Done before any code existed, per `pitfalls.md`.

**What was searched:** Google Play store listings, Apple App Store, Steam and
Microsoft Store titles via web search; USPTO registrations via web search.

**What was *not* done:** a formal trademark clearance search, any non-US
registry, or a legal opinion. Store search finding nothing is not a trademark
search.

### Rejected candidates

| Candidate | Why rejected |
| --- | --- |
| `2048` (bare) | At least 8 live apps including the original (Cirulli / Solebon LLC), Androbaby, Estoty, Chapt Productions. Legally fine, commercially invisible |
| `Stonefall 2048` | **Stonefall Defense: Bolt Puzzle** — live puzzle game on Play. Plus *Stone Fall* (Fate Factory) and *Stonefall* on Microsoft Store |
| `Tilefall 2048` | **Tilefall: Block Puzzle** — direct puzzle-genre collision. Plus *TileFall* (BearlyGames) and a Steam title |
| `Cairn 2048` | *Cairn* — The Game Bakers, PS5/Windows, Jan 2026 |
| anything with *Merge* | **2048 MERGE GAMES** is a live USPTO registration (Guru Network Limited Inc., filed 2024-03-10) covering downloadable mobile game software |

### Selected

**Tilevault 2048** — no game of this name found on Google Play, the App Store,
Steam or the Microsoft Store. A publisher called *Vault Games Studio* exists;
that is a studio name rather than a title, and is not a collision.

### Listing keyword warning

Threes! was pulled from Google Play in 2015 purely for carrying "2048" as a
keyword — automated keyword-stuffing detection. Using 2048 in the title as a
genre descriptor is normal; repeating it through the short and full
descriptions is what triggers takedowns. The listing text in Phase 14 keeps
"2048" to the title and a single natural mention.
