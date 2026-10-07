# 2048 (Unity) — Project Plan

Android puzzle game: classic 2048 with extra features (power-ups, daily challenge, stone tiles, themes) and ads (banner, interstitial, rewarded).

## 1. Tech decisions

- **Engine:** Unity current LTS, 2D template
- **Target:** Android, build as AAB, portrait only
- **Ads:** Google AdMob (Google Mobile Ads Unity plugin) + UMP consent SDK
- **Saving:** JSON file in `Application.persistentDataPath` (or `PlayerPrefs` for simple values). No backend in v1.
- **Testing:** Unity Test Framework (EditMode tests) for game logic
- **Ad IDs:** test ad unit IDs only during development

## 2. Architecture rules

- Game logic lives in **plain C# classes with no Unity dependencies** (`Board`, `MoveResult`, `GameState`). This keeps it testable and makes undo and daily seeds simple.
- Presentation (tiles, animation, UI) only reads the logic state and calls its methods. Logic never references MonoBehaviours.
- One script, one responsibility.
- All ads go through a single `AdManager`, so ads can be disabled or swapped from one place.
- All randomness goes through a seedable `System.Random` instance (needed for the daily challenge).
- Suggested folders:

```
Assets/
  Scripts/
    Core/        (Board, Tile, MoveResult, GameState, RngProvider)
    Presentation/(BoardView, TileView, InputHandler, Animations)
    UI/          (Screens, Popups, Buttons)
    Services/    (AdManager, SaveService, AudioService, HapticsService)
    Meta/        (DailyChallenge, Themes, PowerUps, Modes)
  Tests/EditMode/
  Prefabs/  Scenes/  Art/  Audio/
```

## 3. Features

### Core
- [ ] Classic 2048 rules: swipe in 4 directions, merge equal tiles, a tile merges only once per move
- [ ] Spawn after each valid move: 2 (90%) or 4 (10%)
- [ ] Game-over detection (no empty cells and no possible merges)
- [ ] Win at 2048 with "Keep going" option
- [ ] Score and best score, saved **per board size and mode**
- [ ] Board sizes: 3x3, 4x4 (default), 5x5, 6x6

### Differentiators
- [ ] **Undo** (stack of board snapshots)
- [ ] **Delete tile** power-up (tap a tile to remove it)
- [ ] **Shuffle** power-up (rearranges existing tiles randomly)
- [ ] Free power-up charges per game, extra charges via rewarded ad
- [ ] **Daily challenge:** one fixed seed per date, best score and streak tracking
- [ ] **Stones mode:** immovable blocker tiles that disappear after N moves
- [ ] **Themes:** Classic, Dark, plus 1-2 more (some unlocked via rewarded ad)
- [ ] **Juice:** haptics, sound effects, particles at 512 / 1024 / 2048, tile merge and spawn animations

### Menus
- [ ] Home screen: Play, Daily, Mode/Size, Themes, Settings
- [ ] Settings: sound, vibration, privacy/consent options, reset progress
- [ ] Pause and game-over screens
- [ ] Android back button handled on every screen

## 4. Ad plan

| Type | Where | Rules |
|---|---|---|
| Banner (adaptive) | Bottom of game screen | Must never overlap the board or buttons |
| Interstitial | After game over only | Never mid-game. At most once per 3 games, minimum 2 minutes apart, none in the first 2 games of a session |
| Rewarded | Opt-in buttons | Extra undo/power-up, continue after game over, unlock a theme |

- [ ] UMP consent form on first launch, plus a "Privacy settings" button in Settings
- [ ] `AdManager` with: `Init()`, `ShowBanner()`, `HideBanner()`, `TryShowInterstitial()`, `ShowRewarded(onReward)`
- [ ] Ads fail gracefully (no internet or no fill must never block the game)
- [ ] App not directed at children (avoids the stricter Families policy)

## 5. Build phases

Work through one phase per Claude Code session. Run tests and commit after each phase.

### Phase 1 — Project setup
- [ ] Create Unity project, git repo, Unity `.gitignore`
- [ ] Set up folder structure, portrait orientation, Android build settings
- [ ] Write `CLAUDE.md` with the architecture rules above
- [ ] Build and run an empty scene on a real phone

### Phase 2 — Core logic (no visuals)
- [ ] `Board` class: grid, slide, merge, spawn, game-over check
- [ ] Support variable board size
- [ ] Seedable RNG
- [ ] Snapshot/restore for undo
- [ ] EditMode unit tests: slide, merge-once rule, no spawn on invalid move, game-over, seeded determinism

### Phase 3 — Presentation
- [ ] Tile prefab and board view generated from `Board` state
- [ ] Swipe input (with a minimum swipe distance) and keyboard input for editor testing
- [ ] Slide, merge and spawn animations
- [ ] Score UI and game-over screen

### Phase 4 — Variants and power-ups
- [ ] Board size selection
- [ ] Undo, Delete, Shuffle
- [ ] Stones mode

### Phase 5 — Meta
- [ ] Menus and navigation
- [ ] `SaveService` (best scores, settings, unlocked themes, power-up counts, daily streak)
- [ ] Themes
- [ ] Daily challenge (seed from date)
- [ ] Audio and haptics

### Phase 6 — Ads
- [ ] Import AdMob plugin, configure test app ID and ad unit IDs
- [ ] UMP consent flow
- [ ] Banner, interstitial (with frequency rules), rewarded
- [ ] Hook rewarded ads into power-ups, continue and theme unlock

### Phase 7 — Polish and QA
- [ ] Test different aspect ratios and notches (safe area)
- [ ] Pause/resume, app killed mid-game, back button
- [ ] Performance check (no per-frame allocations in the game loop)
- [ ] Offline behavior and ad failure handling

### Phase 8 — Release prep
- [ ] App icon, 2+ phone screenshots, feature graphic
- [ ] Short and full store description
- [ ] Privacy policy page (public URL)
- [ ] Signed AAB with Play App Signing; back up the upload keystore
- [ ] Switch to real ad unit IDs for the release build only

## 6. Google Play checklist

- [ ] Create the developer account early ($25 fee, identity verification)
- [ ] **Personal account:** run a closed test with at least 12 testers opted in for 14 continuous days before applying for production access (recruit 15-20). Organization accounts are exempt.
- [ ] Declare **Ads: Yes** in App content
- [ ] Complete the **Data safety** form (include ad SDK data such as advertising ID)
- [ ] Add the privacy policy URL
- [ ] Complete the advertising ID declaration and the content rating questionnaire
- [ ] Meet the current target API level requirement
- [ ] Never click your own live ads (AdMob ban risk)

Realistic timeline for a new personal account: about 3 weeks from first upload to public release.

## 7. After v1 (ideas)

- Puzzle levels ("reach 64 in 30 moves")
- More themes and skins
- Leaderboards (Google Play Games Services)
- Remove-ads in-app purchase
- Time-limited mode
