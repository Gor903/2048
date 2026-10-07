# Contract — 2048 (Unity, Android)

Derived from `PLAN.md`. This is the document code is written from; `PLAN.md`
stays as the originating brief and is not edited by the build.

Anything marked **[GAP]** is not answered by `PLAN.md`. Each has a proposed
default so work is not blocked; defaults are listed together in §9 for one
batched confirmation.

---

## 1. Genre and core loop

Single-player turn-based sliding-tile puzzle. Portrait, offline-playable,
session length 1–5 minutes.

Loop: player swipes → every tile slides as far as it can → equal tiles that
collide merge and score → if the board changed, one new tile spawns → repeat
until no move is possible.

**Win:** reach the target tile (2048 on 4×4). Shows a win screen with
*Keep going* — play continues, and the win fires once per game.

**Lose:** no empty cell **and** no orthogonally adjacent equal pair. Stones
count as occupied and never form a pair.

---

## 2. Entities and rules — the pure logic layer

No engine types in any of this. All of it is testable without a scene.

### 2.1 Grid

- Square, size `N ∈ {3, 4, 5, 6}`, default 4.
- Cell holds: empty, a **numbered tile** (value = power of two ≥ 2), or a
  **stone**.
- Coordinates `(x, y)`, origin bottom-left, x right, y up.

### 2.2 Move

A move has a direction `Left | Right | Up | Down` and resolves as:

1. For each line (row for horizontal, column for vertical), walk cells from
   the **leading edge** (the edge the tiles travel toward) backwards.
2. Each numbered tile slides until it hits the wall, a stone, or an occupied
   cell.
3. If the blocking cell holds a numbered tile of equal value **and neither
   tile has already merged this move**, they merge into one tile of double
   value at the blocking cell's position.
4. Stones never slide, never merge, and block sliding.
5. Score increases by the **value of each tile produced by a merge** (a 4+4
   into an 8 adds 8).

**Merge-once rule.** A tile produced by a merge cannot merge again during the
same move. `4 4 8` swiped left → `8 8`, not `16`.

**Validity.** A move is valid iff at least one tile changed cell or a merge
occurred. An invalid move spawns nothing, scores nothing, and does not consume
a turn or decrement stone timers.

### 2.3 Spawn

After a valid move only: pick uniformly at random from empty cells; value 2
with probability 0.9, else 4. All randomness comes from one seedable
`System.Random`-backed `IRng`, never `UnityEngine.Random`.

### 2.4 Stones (Stones mode)

- Seeded at game start: **[GAP]** count and placement.
- Each stone carries a remaining-moves counter, decremented on every **valid**
  move; at zero the stone is removed, leaving the cell empty.
- Stones never merge, never move, never score.

### 2.5 Power-ups

| Power-up | Effect | Counts as a move (spawns a tile)? |
| --- | --- | --- |
| Undo | Restore the previous snapshot (board, score, stone timers, RNG state) | No |
| Delete | Remove one player-chosen numbered tile | **[GAP]** — default No |
| Shuffle | Redistribute all existing numbered tiles over the grid's cells | **[GAP]** — default No |

Undo restores score as well as board — the score after an undo equals the score
before the undone move. Delete and Shuffle cannot be undone past their own
snapshot (they push a snapshot like any other action).

Shuffle keeps the multiset of tile values and stone positions; only numbered
tiles are relocated. A shuffle that produces an identical layout is retried a
bounded number of times, then accepted.

### 2.6 Snapshots and undo

Snapshot = grid + score + stone timers + RNG state. Captured **before** each
mutating action. Depth **[GAP]** — default 10.

RNG state is part of the snapshot so that undo-then-replay is deterministic;
otherwise the daily challenge is re-rollable by undoing.

### 2.7 Win targets per board size

**[GAP]** — `PLAN.md` says "win at 2048" but 2048 on a 3×3 grid is not
reachable in practice. Proposed:

| Size | Target |
| --- | --- |
| 3×3 | 256 |
| 4×4 | 2048 |
| 5×5 | 4096 |
| 6×6 | 8192 |

### 2.8 Daily challenge

Seed = a pure function of the UTC date (`yyyymmdd` → int). Fixed 4×4 Classic.
One scored attempt per day; best score and a streak counter persist. **[GAP]**
— whether a day can be replayed for fun after the scored attempt; default yes,
replays are unscored.

### 2.9 Scoring and persistence

Best score is stored **per (board size, mode)** pair — 4 sizes × 2 modes = 8
slots, plus the daily challenge's own best and streak.

---

## 3. Screens and transitions

```
Launch ──► [first run only] Consent ──► Home
                                         │
   ┌─────────────┬───────────────┬───────┴──────┬──────────────┐
   ▼             ▼               ▼              ▼              ▼
  Game         Daily          Themes        Settings       Mode/Size
   │             │
   ├─► Pause ────┴──► (Resume | Restart | Home)
   └─► Game over ──► (Retry | Home | Continue-via-rewarded-ad)
```

- **Android back** is handled on every screen: Home → confirm exit; Game →
  Pause; any sub-screen → parent; Pause/Game-over → their own default action.
- Win overlay appears over Game, non-blocking, dismissed by *Keep going*.

---

## 4. Player-visible strings

Every string below lives in one localisation table, one entry per key. Zero
literals in views or controllers.

**Home:** Play · Daily Challenge · Mode · Themes · Settings · Quit

**Game HUD:** Score · Best · Undo · Delete · Shuffle · Moves left (stones)

**Game over:** Game Over · Score {0} · New Best! · Retry · Home · Keep Playing
(rewarded) · No thanks

**Win:** You reached {0}! · Keep going · Back to Home

**Pause:** Paused · Resume · Restart · Home

**Settings:** Settings · Sound · Vibration · Privacy settings · Reset progress
· Reset progress? This deletes every score and unlock. · Cancel · Reset

**Modes:** Classic · Stones · Board size · 3×3 · 4×4 · 5×5 · 6×6

**Themes:** Themes · Classic · Dark · Unlock · Unlocked · Watch ad to unlock

**Power-ups:** Out of charges · Watch an ad for {0} more · Tap a tile to remove
it · Cancel

**System:** Loading… · No connection · Ad not ready · Exit game? · Yes · No

**Daily:** Daily Challenge · Day {0} · Streak: {1} · Come back tomorrow ·
Today's best: {0}

---

## 5. Ads — and where this conflicts with the offline assumption

`PLAN.md` §4 specifies AdMob banner, interstitial and rewarded, plus UMP
consent. This is a deliberate, important departure from the skill's default
offline-game shape, and it changes four things:

1. `android.permission.INTERNET` **must be present** — the pitfall about
   stripping it does **not** apply here. The preflight check is inverted:
   assert the permission *is* there.
2. The Data safety form must declare the advertising ID and the ad SDK's
   collection.
3. **Ads: Yes** in App content; advertising-ID declaration required.
4. The privacy policy must name AdMob and link Google's partner policy.

The game must remain fully playable with no network and with consent denied.
`AdManager` is an interface with a no-op implementation, so the whole game
builds, runs and tests green with no SDK present; the AdMob implementation is a
drop-in behind it.

Frequency rules, from `PLAN.md`, enforced in one place:
- Interstitial: after game over only; ≥ 2 minutes apart; at most 1 per 3 games;
  never in the first 2 games of a session.
- Rewarded: opt-in buttons only — extra charges, continue after game over,
  theme unlock.
- Banner: bottom of game screen, never overlapping board or controls, inside
  the safe area.

---

## 6. What `PLAN.md` does not say — resolved

| # | Gap | Proposed default |
| --- | --- | --- |
| 1 | Backgrounding mid-game | Auto-save on pause; restore exactly on return |
| 2 | Progress on kill | Same save; a game in progress always resumes |
| 3 | Sound/vibration default | Both **on** at first launch |
| 4 | First launch | UMP consent, then Home. No tutorial — rules are shown as a one-line hint on the first game only |
| 5 | Undo stack depth | 10 |
| 6 | Free power-up charges | 3 of each per game; +1 per rewarded ad |
| 7 | Stone count / lifetime | 2 stones on 4×4 (scaled by size), 15 moves each |
| 8 | Win target per size | §2.7 table |
| 9 | Delete/Shuffle spawn a tile | No |
| 10 | Daily replay after scored attempt | Allowed, unscored |
| 11 | Localisation set | **Decision needed** — see §9 |
| 12 | Rotation | Portrait only, locked (from `PLAN.md` §1) |
| 13 | Minimum Android version | **Decision needed** — see §9 |
| 14 | Theme unlock persistence | Permanent once unlocked |
| 15 | Score on undo | Reverts with the board |

---

## 7. Architecture (from `PLAN.md` §2, made concrete)

```
Assets/Scripts/
  Core/          Game2048.Core.asmdef      — no engine reference at all
  Game/          Game2048.Game.asmdef      — presentation, input, services
  Editor/        Game2048.Editor.asmdef    — release tooling, screenshots
  Tests/EditMode Game2048.Tests.EditMode.asmdef
  Tests/PlayMode Game2048.Tests.PlayMode.asmdef
```

`Core` references nothing from Unity — enforced by the asmdef and verified by
the phase-2 gate. Presentation reads Core state and calls Core methods; Core
never holds a reference to a MonoBehaviour.

---

## 8. Non-goals for v1

Explicitly out: backend, accounts, leaderboards, IAP, puzzle levels, time-limited
mode. Listed in `PLAN.md` §7 as post-v1.

---

## 9. Open decisions requiring a human

Batched, because each blocks something irreversible or user-visible:

1. **App name** — see the Phase 1 report.
2. **Application id** — fixed forever by the first upload.
3. **Free vs paid** — free→paid is impossible.
4. **Signing key identity** (CN/O/C for the certificate).
5. **Default store language + locale list.**
6. **Minimum API level.**

Everything in §6 marked *proposed default* proceeds as written unless
corrected — none of it is irreversible.
