# Store listing

Draft for the console. Every text block below is copied verbatim into the
console — they stay inside fenced blocks so they can be diffed
programmatically rather than retyped.

Character counts in this document are **measured**, not estimated. Re-run
`Publishing/check-listing.py` after any edit.

## Platform limits

| Field | Limit | Required |
| --- | --- | --- |
| App name | 30 characters | yes |
| Short description | 80 characters | yes |
| Full description | 4000 characters | yes |
| App icon | 512×512 PNG, no real transparency | yes |
| Feature graphic | 1024×500 PNG or JPEG | yes |
| Phone screenshots | 2–8, side 320–3840 px, ratio at most 2:1 | yes |
| Video | YouTube link | no |

## Files

- `art/store-icon-512.png` — 512×512
- `art/feature-graphic-1024x500.png` — 1024×500
- `art/screenshots/` — 6 frames, 1080×1920 each

## Default locale — en-US

### App name

```
Tilevault 2048
```

### Short description

```
Slide tiles, merge numbers, beat your best. Offline, no ads, no nonsense.
```

### Full description

```
Tilevault is a clean, quiet take on the sliding-tile number puzzle.

Swipe to move every tile at once. Two tiles with the same number merge into
one worth double. Keep merging, keep the board alive, and push for the
highest tile you can reach.

No account. No network. No advertisements. It opens instantly and plays the
same on a plane as it does at home.

FOUR BOARD SIZES
Play the familiar 4x4, drop to a tight 3x3 where every move matters, or open
up to 5x5 and 6x6 for longer, calmer games. Each size keeps its own best
score.

STONES MODE
Immovable blocks sit on the board and refuse to merge with anything. Tiles
pile up against them and your usual openings stop working. Each stone
crumbles after fifteen moves, so the board you are stuck with is never the
board you are stuck with for long.

THREE WAYS OUT OF TROUBLE
Undo takes back a move you regret. Delete removes a single awkward tile.
Shuffle redistributes everything and gives you a fresh arrangement to work
with. You get three of each per game, so they are worth saving for the
moment that actually needs them.

DAILY CHALLENGE
Everyone gets the same board on the same day, drawn from the date itself.
Build a streak by coming back. Undo cannot re-roll a bad spawn, so the day's
board really is the same board for everyone.

FOUR THEMES
Classic and Dark are there from the start, with Sunset and Forest to unlock.
The whole interface changes with the palette, not just the tiles.

MADE TO STAY OUT OF THE WAY
Portrait only, one hand, no timers and no pressure. Sound and vibration both
switch off. Your game saves itself when you leave and is waiting exactly as
you left it when you come back, even if the phone kills the app.

Nothing is collected, nothing is uploaded, and nothing is sold. The game
holds a single permission, for the small vibration when tiles merge, and you
can turn that off too.
```

## Screenshots

Uploaded in this order:

| File | What is on the frame |
| --- | --- |
| `01-game-4x4.png` | Classic 4×4 mid-game, score and best visible, power-up row with charges |
| `02-game-bigger-tiles.png` | Later 4×4 position showing the higher-value colour ramp |
| `03-stones-mode.png` | Stones mode on 5×5, three stones showing their remaining-move counters |
| `04-dark-theme.png` | The same game under the Dark palette |
| `05-home.png` | Home screen: Play, Daily Challenge, mode selector, best score |
| `06-game-over.png` | Game-over panel with Keep Playing / Retry / Home |

Captured by code through the editor seams in
`Assets/Scripts/Editor/ScreenshotTool.cs`, not played by hand. Each frame is
a seeded game driven by real moves, so every position shown is one the rules
can actually produce. Regenerate the whole set with
**Tilevault → Art → Capture Screenshots**, or headlessly with
`-executeMethod Tilevault.Editor.ScreenshotTool.CI`.

Seeds and move counts are defined at the top of `CaptureAll()` — edit there,
not in the images.

**What the set does not show:** the Themes screen, the Settings screen, the
Mode/size picker, the win overlay, and the Delete power-up's tile-picking
state. Six frames already covers both modes, both shipped themes, the HUD
and the end-of-game panel; the remaining screens are menus, and store
screenshots of menus sell nothing.

Editor captures have no status bar, no cutout and no gesture bar. That does
not violate store rules, and it is not a substitute for the device run in
Phase 13.

## Countries and languages

Default listing language **en-US**, and the only locale at launch. The
interface picks its language from the system locale at startup and falls back
to English for anything not shipped, so releasing to all countries is safe —
a player on any locale gets a working English interface rather than missing
text.

Android does not localise the launcher label. It reads **Tilevault** in every
locale, which is intended, not a bug.
