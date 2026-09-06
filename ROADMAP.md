# C-Type — Roadmap to Steam

Assessment and plan for taking C-Type from a delisted 2019 Android title to a Steam
release. Written 2026-09-04 against `development` @ `5a7c47d`.

**Constraint:** the AmosEngine submodule (`SupportingFiles/`) is out of scope. Everything
below is achievable in game-side code unless flagged **[ENGINE]**.

---

## 1. Where the project stands

**The good.** The architecture is better than most hobby projects of this age. State/Scene
separation is clean, platform code sits behind interfaces selected by preprocessor symbol,
enemy movement is already a strategy object (`IAccelerationProvider`), and levels are data
files rather than code. Twenty levels, four bosses, four ships, five powerups and a full
menu-to-game-over flow all exist and work. The bones are sound; this is a polish and
extension job, not a rewrite.

**The problem.** Every design decision assumes a phone. Controls, UI layout, session
length, monetisation and the level pacing were all built for touch on a 5-inch screen.
The desktop build is a straight port of the mobile build with a keyboard/gamepad reader
bolted on, and it shows.

**Verified findings** (I built and inspected the code; I did not play through the game):

| Area | Finding |
|---|---|
| Build | `Type.Desktop.csproj` has a spurious `ProjectReference` to `Type.Android.csproj`. It builds today **only because `Type.Android` is unloaded in the local VS solution** — that state lives in `.vs/Type/v17/.suo`, which is gitignored, so a fresh clone loads the project and the desktop build breaks. Command-line builds already fail: VS 18's MSBuild has no Xamarin targets (`MSB4226`), and VS 2022's fails `XA5207` (Android SDK API 28 not installed). Removing the reference plus one stray `using Type.Ads;` in `GameCompleteScene.cs` makes the desktop project build standalone — verified, then reverted. |
| Input | `DesktopInputProvider` is a hardcoded polling chain: fixed bindings, fixed 0.2 deadzone, controller 0 only, no D-pad, no hot-plug, no rebinding. |
| Input | Menus are touch `Button` objects driven by the mouse. There is no keyboard or gamepad menu navigation anywhere. Ship select maps A/B/Y to three fixed ships with no cursor. |
| Input | `DesktopInputProvider.cs:189` tests `Buttons.X == Released` in the release branch of the **Back** button — copy-paste bug. |
| Window | Fixed windowed 1344x756 (`InitialResolution * 0.7f`). No fullscreen, no resolution choice, no options menu of any kind. |
| Data | `LevelLoader.cs` never clears its `delays` list between waves, so from wave 2 onward spawn intervals are read from wave 1's data. Level pacing past the first wave is not what the level files say. |
| Code | `DeregisterListener` calls `_Listeners.Add(...)` instead of `Remove(...)` in 5 places (`EnemyFactory` and all 4 player ships). Listener leak. |
| Code | The 6 enemy classes are byte-identical apart from ~5 values (HP, points, fire rate, sprite). Same story for the 4 player ships (~55 differing lines out of 425). Roughly 2,400 lines of copy-paste. |
| Content | Bosses have no attack patterns beyond fixed cannons; every regular enemy uses the same "rotate toward player, fire on a timer" logic. |
| Content | Levels 5/10/15/20 are single-boss files. The other 16 use 6 enemy types across 4 movement patterns and nothing else — no formations, no scripted set pieces. |
| Perf | Collision is O(projectiles × enemies) with a `.ToList()` allocation per check, per frame. Fine at current density, will not survive bullet-hell counts. |
| Audio | 25 MB of uncompressed WAV. A new `AudioPlayer` is allocated per shot; enemies carry a `TODO FIXME` rate-limiter to work around the resulting sound spam. |
| Ship | Debug constants are committed as `INVINCIBLE = true`, `START_LEVEL = 11`. The AdMob unit ID and Google Play achievement IDs are hardcoded in `Constants.cs`. |

---

## 2. Phases

Each phase is a gate. Later work depends on earlier foundations — level design needs the
level format, enemy behaviour needs the enemy consolidation — so the ordering is not
arbitrary.

Phases 0 to 6 take the game to a Steam release. **Phases 7 and 8 add local and networked
co-op** and were added on 2026-09-06, after the first six were already under way; §3 explains
which parts of them want to move earlier than their numbers imply.

### Phase 0 — Unblock the desktop build

Small, mechanical, and everything else depends on it.

- **D0. Sever the desktop→Android dependency.** Delete the `Type.Android` `ProjectReference`
  from `Type.Desktop.csproj` and the unused `using Type.Ads;` from `GameCompleteScene.cs`.
  All three `Type.Android` usages in shared code already sit inside `#if __ANDROID__`, so the
  reference buys nothing. *Verified: the desktop project then builds cleanly with no Android
  tooling installed.*
  This is not urgent — the game builds and runs today with `Type.Android` unloaded in the
  IDE — but that makes the desktop build depend on gitignored `.vs` state, which will not
  survive a fresh clone, a CI runner, or anyone reloading the Android project. **It also does
  not harm a future Android revival:** `Type.Android` stays a full project in the solution
  and keeps its own reference to the shared `Type` code.
- **D1. Strip ads from the desktop build.** `DesktopAdService` is already a no-op stub, but
  `Game.LoadContent` still calls `AdService.Instance.Initialise` with a live AdMob unit ID.
  Guard the whole ad path behind `#if __ANDROID__` and remove the ID from the desktop build.
  Ads in a paid Steam game are a store-review problem, not just a code smell.
- **D2. Reset the debug constants** to `INVINCIBLE = false`, `START_LEVEL = 1`, and add a
  build-time guard so a non-default value cannot reach a Release build.
- **D3. Fix the five `DeregisterListener` bugs** and the Back-button `Buttons.X` typo.
- **D4. Fix `LevelLoader`'s uncleared `delays` list.** Do this before touching level design —
  otherwise you will be tuning pacing against a parser that ignores your numbers.
- **D5. Add a `.gitattributes`** pinning line endings, and confirm `bin/` and `obj/` are
  fully ignored. *Partially done.* `bin/`/`obj/` were already correctly ignored with nothing
  tracked. `.gitattributes` now declares the binary types (`*.png`, `*.wav`, `*.so`, `*.dll`,
  fonts) so Git can never EOL-convert or line-merge them.
  **Still outstanding:** the global `* text=auto` rule. This repo's history stores text files
  with **CRLF in the index**, so enabling normalisation rewrites the line endings of every
  tracked source file. That should be one dedicated `git add --renormalize .` commit on its
  own branch — folding it into a bug-fix branch would bury the real changes under a
  ~160-file diff, and adding the rule without renormalising just drip-feeds the same churn
  through unrelated commits. Low urgency while this is a solo Windows project; do it before
  a second contributor or a Linux CI runner arrives.

- **D6. Commit the .NET Framework 4.8 retarget — and resolve the submodule half.**
  D0–D5 removed the Android dependency, but a clean checkout still does not build. It now
  fails `MSB3644`: the committed projects target **v4.6.1**, whose targeting pack is not
  installed on the dev machine. The working v4.8 retarget exists only as **uncommitted**
  local changes in three places:
  - `Type.Desktop/Type.Desktop.csproj` (`TargetFrameworkVersion`) — committable
  - `Type.Desktop/App.config` (`supportedRuntime`) — committable
  - `SupportingFiles/AmosDesktop/AmosDesktop.csproj` — **inside the submodule, so it cannot
    be committed to this repository**

  *Verified: retargeting all three to v4.8 makes a pristine export build Release cleanly;
  retargeting only the two parent-repo files does not, because `AmosDesktop` still targets
  4.6.1.* So committing the parent-repo half alone produces a tree that looks fixed but
  still fails on any machine without the 4.6.1 targeting pack.

  Three ways out, in order of preference:
  1. **Ask the engine author to retarget `AmosDesktop` upstream** and bump the submodule
     pointer. Correct fix, needs someone else.
  2. **Install the .NET Framework 4.6.1 Developer Pack** on every dev/CI machine and revert
     the retarget entirely. Cheapest, keeps the submodule pristine, but leaves the game on
     an out-of-support framework target.
  3. **Override the submodule's target framework from the parent build** (an MSBuild
     `Directory.Build.props` at the repo root setting `TargetFrameworkVersion`). Avoids
     editing submodule files, but is action-at-a-distance and may not survive an engine update.

  Until this is settled the local uncommitted retarget is **load-bearing** — do not discard
  it, and note that it will not survive a fresh clone.

- **D7. Make a clean checkout produce a working binary, not just a compiling one.**
  With D0–D6 done, a fresh clone of this repo now **compiles with zero errors and zero
  warnings and then crashes on startup**:
  `FileNotFoundException: Could not load file or assembly 'Newtonsoft.Json'`, thrown from
  `AmosShared.Base.DataLoader.Initialise`.

  Cause: **`Type.Desktop`'s dead `Newtonsoft.Json 12.0.1` reference.** The parent repo's
  `packages/` is gitignored (`**/packages/*`) and has never been tracked, so
  `packages\Newtonsoft.Json.12.0.1\...` is absent from a fresh clone and the reference
  resolves to nothing, silently — packages.config projects are not restored by
  `/t:Restore`, which only handles `PackageReference`. Because
  `AutoGenerateBindingRedirects` writes an 11→12 redirect into `Type.Desktop.exe.config`,
  the engine's working 11.0.1 assembly was then forwarded to a 12.0.0.0 that did not exist.
  Hence the exception's outer frame naming `Version=12.0.0.0`. This is the worst failure
  mode available — the build reports success and hands you a broken executable.

  **Correction to an earlier reading of this item:** the engine half was never broken. The
  AmosEngine repository *commits* its `packages/` folder, so all three of `AmosDesktop`'s
  old `HintPath` targets — `Newtonsoft.Json 11.0.1` and both `Plugin.InAppBilling 1.2.4`
  assemblies — were present in a clean clone and needed no restore. *Verified against a
  fresh `git clone --recurse-submodules`.* Removing the dead `Type.Desktop` reference alone
  would have fixed the crash; the `PackageReference` migration below was **not required** to
  close D7.

  **Resolved by removing the dead references, plus an optional `PackageReference`
  migration.** The first bullet below is the actual fix; the rest is a deliberate
  improvement with one trade-off, noted at the end.

  - **`Type.Desktop` now has zero NuGet packages.** Nothing in `Type/` or `Type.Desktop/`
    referenced `Newtonsoft.Json`, `Plugin.InAppBilling`, or
    `Xamarin.GooglePlayServices.Ads.Lite`. All four `Reference` entries and
    `packages.config` were dead weight — the last of them a MonoAndroid assembly referenced
    from the desktop project. Deleting them also removes the version split at its source.
  - **`AmosDesktop` (engine) migrated to `PackageReference`** for its two genuinely used
    packages, `Newtonsoft.Json` and `Plugin.InAppBilling` (the latter is used by
    `AmosShared/Base/PurchaseManager.cs`). Restore is now automatic via
    `msbuild /t:Restore` with **no `nuget.exe` required**.
  - **`Newtonsoft.Json` bumped 11.0.1 → 13.0.3.** Migrating surfaced `NU1903`: 11.0.1 has a
    known **high severity** vulnerability
    ([GHSA-5crp-9r3c-p9vr](https://github.com/advisories/GHSA-5crp-9r3c-p9vr)) that
    packages.config never reported. Engine usage is only `JsonConvert` and `JObject`
    indexing, so the upgrade surface is trivial. The other engine projects
    (`AmosAndroid`, `AmosiOS`, `TestProject`) still carry 11.0.2 via packages.config and
    should be bumped when Android is next revived — see §4.

  *Verified end to end: with no `packages/` folder anywhere, no `nuget.exe` and no `.vs`,
  `/t:Restore` then `/t:Rebuild` produces a Release binary that launches and reaches the
  main menu, with `Newtonsoft.Json` 13.0.3 flowing transitively into the game's output.*

  **The silent-failure mode is gone and needs no extra guard.** A build without restore now
  fails at compile time with `CS0246` on `Newtonsoft` and `Plugin`, rather than succeeding
  and producing a binary that dies at startup. The remaining `HintPath` references
  (`OpenTK`, `FarseerPhysics`) point at DLLs committed in the engine's `Libraries/Desktop`,
  so they cannot go missing in a clean checkout.

  **Trade-off the migration introduced.** Before it, every assembly the engine needed was
  committed, so a clean checkout built **fully offline**. `AmosDesktop` now resolves
  `Newtonsoft.Json 13.0.3` from NuGet, so a first build needs network access or a warm
  package cache. That was accepted in exchange for patching a high-severity advisory and
  stopping the engine from carrying binaries in source control — but it is a real cost, and
  worth remembering if a CI runner or a machine ever builds air-gapped. Reverting is a
  one-line change back to a `HintPath` on the committed 11.0.1, at the price of restoring
  the vulnerability.

**Gate:** `Type.Desktop.exe` builds *and runs* from a **clean checkout** — no `.vs`
directory, no `packages/`, no Android workload, no `nuget.exe`, MSBuild from the command
line — on a machine with only Visual Studio's .NET desktop workload installed.
**Met**, once the D7 engine merge request lands and the submodule pointer is bumped.
Build with `/t:Restore` then `/t:Rebuild`; see CLAUDE.md.

### Phase 1 — Input (your stated first priority)

The goal: the game is playable start to finish with a gamepad alone and with a keyboard
alone, and Steam's controller-support checkbox is honest.

**I1 to I5 and I7 landed in the first three pull requests but were never marked here.**
Recorded now, briefly, from the code as it stands.

- **I1. Rewrite `DesktopInputProvider` around a binding table.** **Done.** `InputBindings`
  maps each `ButtonData.Type` to a list of `ActionBinding` sources, one entry per device, and
  the provider resolves key names to OpenTK keys once rather than parsing per update.
  `ActionStateTracker` turns the per update "is this down" reading into a single
  `PRESSED` / `HELD` / `RELEASED` edge, replacing the `_XPressed` booleans. `ButtonData.Type`
  gained `CONFIRM`, `CANCEL`, `PAUSE` and the four `MENU_` directions, and lost the members
  that named a craft rather than an input.
- **I2. Proper analog handling.** **Done.** `AnalogProcessor` applies a radial deadzone with a
  configurable inner and outer edge and an exponent response curve, and reports direction and
  strength separately, so a stick is analog and a key reports 1.
- **I3. D-pad, both sticks, triggers, and hot-plug.** **Done.** All four slots are polled every
  update, a pad that is actually producing input wins over one that merely reports connected —
  this machine has a phantom pad whose triggers rest at half pull, which is why the trigger
  threshold is 0.65 — and losing the pad mid-game invokes `OnInputDeviceLost`, which pauses
  play rather than leaving the ship uncontrolled.
- **I4. Full keyboard support.** **Done.** WASD and the arrows are bound together, Escape
  pauses and backs out, Enter and Space confirm, and the digital vector is normalised so a
  diagonal is no longer 1.41 times a cardinal.
- **I5. Menu navigation without a mouse.** **Done.** `FocusRing` holds the order and
  `MenuNavigator` drives it from the D-pad, stick and keyboard with hold-to-repeat. The
  engine's touch `Button`s are untouched, so pointer input still works; `FocusableButton`
  layers focus over one. Ship select is a cursor selection rather than one face button per
  craft.
- **I6. Rebinding UI and persistence.** **Done.** A controls screen, reached from the options
  screen on the main menu and over a paused game alike, lists FIRE, NUKE, PAUSE and the four
  movement directions against the inputs bound to them, and rebinds one by waiting for the
  player to press something.
  While a capture is open the provider reports nothing to listeners, so the press that chooses
  a binding cannot also act on the menu that asked for it. Nothing is taken until every input
  has been released, so the confirm that opened the capture is never what gets bound, and the
  capture stays open until the chosen input is let go, so a key just bound to FIRE does not
  also fire as the screen closes. Escape backs out.
  The screen shows **four cells per action** — two keyboard slots and two gamepad slots — with
  left and right moving between them and confirm rebinding the one under the cursor. Only that
  cell's device is listened for, so pressing a key at a gamepad cell leaves the prompt up rather
  than binding something the cell cannot hold.
  **It navigates as a grid, not as a list of rows.** Moving down from the third cell of one row
  lands on the third cell of the next rather than back at its start, which is what a table wants
  and a list of values does not. `IGridFocusable` carries the column and `MenuNavigator` moves it
  between items, remembering it rather than reading it back, so stepping over an entry with no
  columns — `RESET DEFAULTS` at the foot of the screen — and returning lands on the column that
  was left.
  Only the focused row's **label and selected cell** are lifted; the rest of that row stays as
  dim as any other. Lifting the whole row and then lifting the selected cell a little further
  did not read — the step between the two was smaller than the step the eye was already using
  to find the row, so the cursor was lost among the cells it was meant to stand out from.
  Three rules:
  - **A rebind changes one slot and leaves the rest alone.** This started out replacing the
    whole device, on the reasoning that the screen should not show an input it could not edit.
    That was the wrong trade: the defaults bind two of each, so touching one collapsed it to
    one and the shipped state could not be got back except by resetting everything. Editing
    per slot shows everything and can set everything, which was the actual goal.
  - **An input taken from another rebindable action is swapped, not stolen.** That action
    inherits the input given up, so a rebind can never leave a second action unbound. Taking an
    input the action already holds in its other slot swaps the two rather than duplicating it.
  - **PAUSE and the four directions may not take an input bound to CONFIRM or CANCEL.** This
    once applied to *every* action, which the defaults themselves disprove: A is FIRE and
    CONFIRM both, B is NUKE and CANCEL both, so the rule forbade reproducing what the game
    ships with, and anything rebound off A could never be put back. The collision is only real
    where both are dispatched at once — PAUSE stays live while paused, since it is what
    unpauses, and the directions are live alongside confirm and cancel on every menu. FIRE and
    NUKE are suppressed while paused and no menu listens for them, so they share a face button
    with a menu action by design. A refused input shows `TAKEN` and changes nothing.
  Bindings persist through `StorageService` as one `BIND_<ACTION>` key per action, keys before
  a semicolon and pad buttons after, written by name so reordering either enum cannot silently
  change what a save means. A key is only written once something has changed, matching S5, and
  `RESET DEFAULTS` writes the defaults back explicitly because the store cannot delete a key.
  **The bitmap font has no comma**, so multiple inputs are separated by spaces. `TextDisplay`
  looks every character up in `Constants.Font.Map` without checking and throws on a miss, so
  input names that come from a platform's key enum are filtered to what the font can draw —
  found by running it, not by reading it.
  *Verified by driving the model directly at startup: replace, swap, refusal of a reserved
  input, a pad-only rebind leaving the keys alone, a reload from disk reproducing the mapping
  exactly, and reset restoring the defaults.* *Also verified on screen, on both routes in: from
  the options screen on the main menu and over a paused game.* Looking is what caught the font
  crash, and then a second thing arithmetic could not: the menu art has a pale planet directly
  behind the middle column, and the `KEYBOARD` heading washed out against it at the tint the
  other labels use. The headings are now brighter than an unfocused row despite carrying less
  meaning, which is the wrong hierarchy on paper and the right one on screen.
  **Playing it then found what neither had:** `RESET DEFAULTS` threw
  `InvalidOperationException` every time. It is a menu item, so it runs from inside an input
  dispatch, and the provider activates it from within a loop over the very bindings the reset
  was rebuilding — clearing the dictionary invalidated that loop's enumerator. Fixed twice
  over: the reset now replaces each action's inputs in place rather than restructuring, and the
  provider dispatches from a snapshot rebuilt on change, so no future edit to the mapping can
  invalidate the loop either. Removing a stale `Reset` on the press tracker at the same time
  fixed a second fault hiding behind the first — holding confirm on that item would have re-run
  the reset every frame, and with it seven writes to the save file per frame.
  *Reproduced first, then fixed, then confirmed by driving the same call chain.*
  **Playing it a second time found the three things above** — the collapsed second binding, the
  over-broad reserved rule, and the analog stick. All three were reported from the screen, none
  of them by reading the code, which is now three rounds running.
  **Still open:** a cell cannot be emptied. Every cell can be *set*, and the swap can leave one
  empty, but there is no way to deliberately unbind an input short of `RESET DEFAULTS`. Nobody
  has asked for it yet, and adding it means deciding what the player presses to mean "nothing",
  on a screen where every press is a binding.
  **Also still open:** the four movement rows bind the D-pad only. The left stick is read
  straight off the pad in `DispatchDirection`, outside the binding table entirely, and it takes
  priority over the digital inputs — so rebinding `MOVE UP` changes the D-pad and leaves the
  stick alone. The screen now says so rather than implying otherwise, which was the cheap half
  of the fix. Making the stick selectable or rebindable is the other half, and it is a real
  design question: a stick is an axis pair, not four buttons, so it fits the per-direction model
  badly. An options row naming which pad device moves the ship is the likelier answer.
- **I7. Contextual button prompts.** **Done.** `InputPrompt` names the input for an action and
  follows whichever device the player last actually used, not merely what is plugged in. It now
  also re-reads when the mapping's revision changes, so a prompt does not keep naming a key the
  player has just rebound. Text in the existing bitmap font rather than glyph sprites, which
  would mean new art registered in both platform projects.
- **I8. Rumble.** **Done.** Six named events in `Data/Rumble.cs` rather than durations at the
  call sites, so their weights can be compared with each other in one place: player death and
  nuke as they were, plus a hit that gets through, a shield absorbing one, a shield going down,
  and a boss destroyed. An intensity row on the options screen scales all of them and turns
  them off at zero, applied in `InputService` so one place decides how much of the weight an
  event asks for the player actually wants, and read per call so the slider takes effect
  immediately — including from the pause menu mid-run.
  **The reason none of it had ever worked: `GamePad.SetVibration` does nothing on Windows.**
  OpenTK's Windows gamepad driver leaves the method unimplemented and returns false, which is
  what it did here on a correctly mapped Xbox Series pad — reported by OpenTK as `XInput
  Controller`, `Mapped`, `Connected`. The same pad accepts vibration through XInput directly,
  returning `ERROR_SUCCESS` at full motor speed. So `XInputRumble` in `Type.Desktop` calls
  `XInputSetState` itself. **Not a new dependency:** `xinput` is a Windows system library, the
  one OpenTK would be calling if it implemented this. Three versions ship with different Windows
  releases and the first that loads is used.
  It finds the pad through XInput's slots rather than OpenTK's, because the two do not
  correspond — this machine reports a **phantom second controller** that OpenTK sees and XInput
  does not, the same one whose triggers rest at half pull and forced the 0.65 trigger threshold.
  XInput enumerates only XInput devices, which is exactly the set that can rumble, so the
  phantom cannot be picked by mistake.
  **This was reported from a real pad, not found here.** No amount of reading the game's own
  code would have turned it up: the bug is that a library call succeeds silently at nothing.
  **Two further bugs in the existing implementation, found by reading it:**
  - `Vibrate` started the motors and *then* called `CancelAndComplete` on the previous timer,
    which invokes that timer's callback — and its callback sets the motors to zero. Any rumble
    landing inside another one therefore silenced itself instantly. Overlapping rumbles now take
    the stronger and the longer of the two.
  - The stop was scheduled on a `TimedCallback`, which runs on game time. Pausing sets that
    clock's multiplier to zero, so a rumble started just before a pause ran until the game was
    unpaused, and quitting from the pause menu left the motors on with nothing left to stop
    them. Timed against the wall clock now, and stopped on disconnect and on disposal too.

  Two pieces of the old signature went with them. The controller index was a parameter that
  every caller passed as zero, so a player on any other pad slot got nothing; the provider
  already knows which pad is driving and now uses it. The strong-or-weak flag was set to strong
  by every caller, so the weak branch had never once run; it is a strength from 0 to 1 instead.
  `IBoss` is new: a marker with no members, because the only thing saying which enemies were
  bosses was the folder they sat in. `BossCannon` is deliberately not one — it is a station's
  gun, and marking it would have fired a boss kill for each gun. E1 replaces this with a field
  in the enemy data.
  *Verified: the intensity setting clamps at both ends, persists, and reloads; every one of the
  six events reaches the provider and returns; the options row is on screen.*
  **The six weights have still not been felt.** They were chosen by reasoning about which event
  should outweigh which and are a starting point rather than a tuned set. One is worth a second
  look by someone holding a pad: a shield absorbing hits rumbles per hit, which under sustained
  fire will read as a continuous light buzz, and whether that is right feedback or an annoyance
  is a question for the hand, not the head.

**Gate:** complete a full 20-level run using only a gamepad, then again using only a
keyboard, without touching the mouse.
**Every item in this phase is done; the gate is not met, and cannot be met from here.** It asks
for two full playthroughs, and nothing in Phase 1 has been played — the work was verified by
building, by driving the model directly, and by looking at the screen. The gamepad half is the
weaker of the two: no pad has ever touched this code, so hot-plug, the stick, the triggers,
every pad binding and all six rumble events are written and unfelt. Every defect found in
Phase 1 so far was found by running or playing rather than by reading, which is the argument
for the gate rather than against it.

### Phase 2 — Desktop shell

Not glamorous, but these are store-page and refund-request items.

- **S1. Fullscreen and borderless windowed.** **Done.** `AmosDesktop.GameWindow` derives from
  `OpenTK.GameWindow`, so `WindowState` and `WindowBorder` were already reachable and no engine
  change was needed. `DesktopDisplayProvider` sets them behind `IDisplayProvider`; the mode is
  chosen on the options screen, saved by `Settings`, and reapplied during content loading.
  *Verified by forcing each mode at startup and reading the window rect and style back through
  Win32: windowed keeps `WS_CAPTION`; borderless clears it at 0,0; fullscreen clears it and
  covers the full 1920×1080.*
  Borderless was briefly `WindowState.Maximized`, which respects the desktop work area and so
  left the taskbar drawn over the bottom of the screen at 1920×1032. It now sizes the window
  to the bounds of whichever display its centre falls on, which both covers the screen and
  keeps it on the monitor the player put it on — maximising chose the display for free, and
  sizing explicitly has to choose it explicitly. The windowed position is remembered on the
  way out and restored on the way back, since leaving a maximised window used to restore it
  and leaving a resized one does not.
  *Verified at 1920×1080: borderless and fullscreen both fill the screen with no caption, and
  borderless back to windowed returns to the original size and position.*
- **S2. Resolution and aspect handling.** **Done** — AmosEngine merge request `!26`.
  The game assumed 1920×1080 and positions HUD elements at literal coordinates, and a window
  that was not 16:9 stretched everything: a circular star rendered as an ellipse.
  The engine risk turned out to be real but shallow. `Renderer.Init` already derived the
  target height from the device aspect, but `UpdateSize` never repeated that on resize, and
  on desktop `Init` is always handed the initial 16:9 window size, so the correction never
  engaged at all. The viewport now fits the target dimensions inside the surface without
  changing their shape and centres them, which keeps the scale equal on both axes and leaves
  the target dimensions — and therefore every literal coordinate in the game — untouched.
  The offset mechanism already existed for this: `CalculateExtraOffset` is an abstract hook,
  and the Android and iOS touch mapping already added `ViewOffset`. Only the desktop mouse
  mapping ignored it, and now does not.
  *Verified at 16:9, 16:10, 21:9 and two deliberately wrong aspects: equal scale on both
  axes, bars on the correct edges, no bars at all at 16:9, and clicks inside a bar mapping
  outside the world so they cannot activate anything.*
  **Still open:** the game draws its backgrounds at a fixed `(-960, -540)` sized for 1080,
  so the world is still exactly 1920×1080 whatever the display. Bars rather than more visible
  play area is the right default for a game balanced around a fixed field of view, but if
  ultrawide play area is ever wanted, that is a content change, not a renderer one.
- **S3. An options menu.** **Done.** Reached from the main menu, navigable with the focus
  cursor, with master, music and effect volume and rumble intensity adjustable in ten point
  steps and saved through `StorageService`. Settings load and apply during content loading,
  before anything plays. Display mode joined it with S1 and S2, rumble with I8, and controls
  with I6 — as an entry opening a screen of its own rather than a row, because a binding is a
  list of inputs per device and there is one per action.
  This unblocked **I6**, which is why it was brought forward: the rebinding editor had
  somewhere to live, and `Settings` gave it a load, clamp and save pattern to follow.
- **S4. A real pause menu.** **Done.** Pause froze time and showed a powerup help overlay
  with no way out but unpausing. It now also puts up Resume, Options, Restart and Quit,
  driven by the focus cursor from I5.
  Pausing no longer silences all input, only the ship: the provider suppresses the gameplay
  actions and lets the menu ones through, since a menu the pause put on screen has to be
  navigable. Options opens the settings over the paused game rather than as its own screen,
  by giving `OptionsScene` an overlay mode that omits the menu art. Restart and Quit restore
  the clock before changing state, or whatever came next would start frozen.
  The powerup guide it used to show automatically is now a Help entry rather than something
  permanently on top of the menu, where it collided with both the menu and the settings
  opened from it. Sub screens keep the pause overlay's dark wash instead of adding their
  own, and one that carries no prompt of its own borrows a BACK prompt from the overlay.
- **S5. Settings persistence** through `DataLoader`. **Done.** `Settings` reads and writes
  master, music and effect volume plus the display mode, saving on every change and applying
  them during content loading, before anything plays. Volumes are stored as whole percentages
  rather than the engine's 0–1 floats, so a value cannot come back from JSON as a boxed double
  that every read has to guess the type of.
  *Verified by decoding the live save file: `MASTER_VOLUME`, `MUSIC_VOLUME` and `DISPLAY_MODE`
  are all present and read back across restarts.* Keys are only written once changed, so a
  setting left at its default is simply absent and falls back — `EFFECT_VOLUME` is missing on
  this machine for that reason, not through any failure.
  See **S11** for where that file lives, and why surviving a restart is not the same as
  surviving a reinstall.
- **S6. Replace Google Play achievements** with Steamworks equivalents, behind the existing
  `AchievementController` facade. **Achievements only — leaderboards were dropped, see R1.**
  `LeaderboardController` keeps its Android implementation and gains no desktop one.
  **The Steamworks dependency is approved (2026-09-06).** Which binding is still open, and the
  choice matters more than it looks, because Phase 8 needs the same SDK for matchmaking and
  networking — so this decision is made once for both:
  - **Steamworks.NET** is a thin binding over the C API. It states .NET Framework support
    plainly, which is what this build is.
  - **Facepunch.Steamworks** wraps the same API in a much friendlier C# shape, including
    `SteamNetworkingSockets`, which is what Phase 8 wants.

  **Verify the framework target before committing to either.** Facepunch 2.x targets
  .NET Standard 2.0, which a 4.8 project can consume in principle, but "in principle" and "the
  restore succeeds and the callbacks marshal" are different claims and only one of them is
  checkable. A throwaway project that restores the package and calls `SteamClient.Init` settles
  it in minutes; do that before writing anything against either API.
  Keep the SDK plumbing — init, callback pumping, shutdown, the app id — in one place, because
  R1 and all of Phase 8 build on it.
- **S7. Window title, icon, and app metadata.** **Done.**
  The window was titled `"Game"` and the `BaseGame` argument `"Test Game"`. The window now
  reads `C-Type`, and the assembly carries that as its title and product with a real
  description, so the executable's file properties no longer say `Type.Desktop`.
  **The name is `C-Type`**, settled as a play on R-Type, and the title screen, window, taskbar
  and assembly metadata all now agree with the Android label and the README heading. The
  bitmap font had no hyphen — the atlas was exactly full at 600×15 — so one was drawn in the
  font's own weight and the sheet widened by a cell to 615.
  **Beware `Constants.Font.Map` ordering.** `TextDisplay.GenerateCharacterMap` walks that
  dictionary and hands each entry the next atlas cell in turn; the filenames and coordinates
  in `KenPixel.json` are not consulted on this path. A character's position in the map is
  therefore what decides which glyph it draws. Adding the hyphen after `colon` rather than at
  the end silently shifted every symbol after it — spaces rendered as hyphens, dots as percent
  signs — while letters and digits looked fine. Anything new must be appended to the map and
  drawn in the matching new cell at the end of the sheet.
  **The icon** is generated from the Alpha ship sprite: seven sizes from 16 to 256, the small
  ones as BMP entries and 256 as PNG, since `System.Drawing.Icon` cannot decode PNG entries and
  the window loads its icon through it. It is an `ApplicationIcon` build input rather than a
  content asset, so there is one copy compiled into the executable and the window pulls it back
  out with `ExtractAssociatedIcon` rather than shipping a second.
  *Verified: the title screen reads `C-TYPE`, the window and taskbar show the ship, the
  executable reports `C-Type`, and the version string and input prompts render unchanged.*
  The engine hardcodes `"Game"` as the window title in its own constructor, so the real one is
  assigned from `Program.cs` afterwards rather than passed in — no engine change needed. The
  argument that *is* passed is the engine's `AssemblyName` property, which nothing anywhere
  reads.
  The `BaseGame` name is now `Constants.Global.STORE_NAME`, which is `"CType"` and deliberately
  punctuation-free rather than the display name: the engine hands it straight to
  `IsolatedStorageFile.CreateDirectory`, and the desktop save folder already uses that
  spelling, so changing it would orphan saves that exist. Renaming it was gated on **S11** and
  is now safe —
  *verified that the save still reads back intact after the rename*, since it no longer lives
  in a directory named after the game. Only the engine's achievement and leaderboard store
  moved, which is inert on desktop.
  **Company and copyright are deliberately untouched.** `AssemblyCompany` is empty and
  `AssemblyCopyright` still reads `Copyright ©  2019` with no holder. Those are identity and
  legal fields; they need a decision, not a guess.
- **S8. Drop the mobile in-app billing dependency from the desktop build.**
  **Done** — AmosEngine merge request !24, merged as `0a1204a`.
  `AmosShared/Base/PurchaseManager.cs` used `Plugin.InAppBilling` with no platform guard, so
  a mobile store-billing library was compiled into the desktop engine and both
  `Plugin.InAppBilling.dll` and `Plugin.InAppBilling.Abstractions.dll` were shipped in the
  game's output, despite nothing on desktop calling `PurchaseManager`. The class is now
  wrapped in `#if __ANDROID__ || __IOS__`, matching the convention in `Audio/AudioData.cs`;
  those symbols come from the Xamarin targets rather than any `DefineConstants`. With nothing
  left referencing the package, its `PackageReference` was removed from `AmosDesktop`.
  *Verified: `Newtonsoft.Json` is now the only package assembly in the game's output.*

- **S9. Find the leaked drawable.** **Closed: there is no longer a leak to find.** Quitting
  after visiting the menus used to crash in `Canvas.Dispose`, which AmosEngine `!25` fixed by
  making teardown tolerant of a drawable the game never disposed. This item then assumed the
  leak itself was still there. It is not.
  *Measured rather than reasoned:* a debug tally read `Canvas._Drawables` by reflection at
  every state boundary and either side of every screen that opens over another. Every path
  exercised returns the canvases to exactly the count they held before — splash to main menu,
  the options screen and back, the controls screen and back, ship select and back, a run with
  enemies and pickups, pause, the pickup guide, the settings and the bindings opened over the
  paused game, resuming, game over, and the return to the menu after each. Opening the bindings
  adds 45 drawables and closing them removes 45; leaving a run drops both canvases to zero.
  The counts were logged at each step rather than only at the end, so a probe that had silently
  failed to run could not have been mistaken for a clean result.
  It was most likely fixed in passing by the double-dispose fix in I5 or the pause rework in
  S4, both of which landed after this item was written and both of which touched disposal on
  exactly the menu path it names.
  **Not covered:** finishing all twenty levels to reach the game complete screen, restarting
  from the pause menu, and the level-to-level transitions including the boss levels. Those need
  a real playthrough rather than a scripted one.
  **The method is worth repeating** if `Canvas.Dispose` ever complains again: reflect
  `_Drawables` out of both canvases, log the count and the type of each entry at every state
  boundary, and look for a pair that does not balance. It found nothing here in about an hour,
  which is the cheapest possible answer to "is this still a problem".

- **S10. A `NullReferenceException` on quit.** **Found and fixed.** It was
  `EnemyFactory.Dispose` calling `_LevelData.Clear()` on a list that was still null.
  `_LevelData` is only assigned in `Start`, and `Start` is called from the completion callback
  of the level intro, two seconds after the state is entered. The factory therefore exists with
  a null `_LevelData` for exactly the length of that intro, and disposing inside that window
  threw. The field is now empty rather than null from construction.
  **That is why it looked unreproducible.** The window is two seconds wide in a normal run, so
  hitting it by hand is luck — but pausing during the intro holds the window open indefinitely,
  which is precisely what the original report described. Scripted to pause at one second and
  quit at two, it threw on **six attempts out of six**, and after the fix ran clean on **eight
  out of eight**. The original observation was exact; only the frequency was misleading.
  `PowerupFactory` was checked for the same shape and does not have it, and
  `EnemyFactory.Update` is guarded by `_Spawning`, so `Dispose` was the only unguarded reach.
  The rest of this entry is kept because ruling the first suspect out was itself a finding, and
  because the reasoning about `UpdateManager` remains true. This item originally blamed an
  uncancelled `TimedCallback` from `LevelDisplay.ShowLevel`. **That hypothesis is wrong, and
  the description asserting it was inaccurate rather than merely unproven:**
  - `LevelDisplay.Dispose` cancels both `_ShownCallback` and `_CompleteCallback`, and has done
    since `c87ca46`, long before the exception was seen.
  - That `Dispose` is reached on every teardown path: `PlayingState.Dispose` disposes
    `_UIScene`, and `UIScene.Dispose` disposes `LevelDisplay`.
  - A callback cancelled part way through a frame cannot still fire in that frame.
    `UpdateManager.RemoveUpdatable` only queues the removal, but the update loop skips
    anything in `_UpdatablesToRemove` before calling it, so a disposal that happens *during*
    a frame — which is exactly what a state change does — is honoured immediately.

  So the callbacks are cancelled, and no ordering was found that lets one run against a
  disposed state. That reasoning held up: the cause was on the quit path, as predicted, but in
  `EnemyFactory` rather than anywhere near `LevelDisplay`. The guess that it shared a root cause
  with S9 was wrong — S9 had no leak left to share.
  Two things were noticed while ruling the first suspect out, neither of them the culprit:
  - `TimedCallback` was safe only by leaning on `UpdateManager`'s behaviour: `Update` invoked
    `_Callback` with no null check and `CanUpdate` ignored `IsDisposed`, so it depended on the
    engine never delivering an update after removal. Hardened to refuse updates once disposed,
    so the class holds on its own terms. **This is not a fix for the exception** — nothing
    reaches the old code path.
  - `PlayingState.GameOver` disposes `_LevelDisplay` directly, then `PlayingState.Dispose`
    disposes it again by way of `_UIScene`. Harmless — `Drawable.Dispose` returns early when
    already disposed, and the callback fields are nulled — but the double ownership is untidy
    and worth tidying if that area is touched.

- **S11. Saves are keyed to the executable's path, so a reinstall silently wipes progress.**
  **Done.** The game's own values now live in `%APPDATA%\CType\SavedData.txt`, reached through
  a new `StorageService` and `IStorageProvider` rather than the engine's `DataLoader`, so the
  path no longer moves with the executable. On first run with no save there, the desktop
  provider searches every isolated storage store for the newest one this game wrote and brings
  it across, so an existing player keeps their high score, totals and settings.
  Android keeps `DataLoader` behind the same interface: an installed package's storage is keyed
  to the application, not to a directory, so the problem does not arise there.
  Two deliberate choices. The file is `key=value` lines rather than JSON, because
  `Newtonsoft.Json` reaches the desktop *output* but not its *compilation* — it is a transitive
  runtime dependency of `AmosDesktop` — and adding a package to `Type.Desktop` to read a file
  the migration touches once was the worse trade. Every consumer already read through
  `Convert`, so a text-only store needed no call site changes. The same single-byte
  obfuscation is kept, so moving the file changes nothing about how readable it is; it was
  never security, and S11's original note on that still stands.
  *Verified end to end: migration brought all six existing keys across with their values
  intact, and the game then run **from a different directory entirely** read a display mode
  written by the copy in the install folder — the exact case that used to produce an empty
  store.*
  **Still in isolated storage:** the achievement and leaderboard values the engine writes from
  `Achievement.cs` and `LeaderboardScore.cs`. Those are inert on desktop, since
  `CompetitiveManager` only loads under Android, and they are engine-side rather than
  game-side. R1 replaces them with Steam stats anyway.
  **This unblocks S7.** Renaming the `BaseGame` argument no longer touches where saves live;
  the migration search deliberately looks for the old `"Test Game"` folder by literal, so a
  rename cannot move that needle.

  The problem it fixed, for the record:
  `DataLoader.Initialise` opens the store with `IsolatedStorageFile.GetUserStoreForAssembly()`.
  For an assembly with no strong name that scope resolves through Url evidence — the codebase
  path of the running executable — which is why the store lands under
  `%LOCALAPPDATA%\IsolatedStorage\...\Url.<hash>\AssemFiles\Test Game\SavedData.txt`. Two
  copies of the same build at two paths get two unrelated stores.
  *Verified: this machine holds nine `Test Game` stores, only one of which has a
  `SavedData.txt`. Running the same build from a different directory created a fresh empty
  store, with no error and no migration.*
  On Steam that means a second library folder, a moved install, or any path change resets the
  high score, the all-time totals, the settings, and the Omega unlock on the ship select
  screen. The directory is also named from the `BaseGame` constructor argument, currently
  `"Test Game"`, so S7's rename orphans every save unless it migrates first.
  Two ways out. The durable one belongs with R1: Steam Cloud for the file, and Steam stats as
  the source of truth for anything that must survive. Sooner and game-side, C-Type can stop
  routing its own data through `DataLoader` and keep a file at a stable location such as
  `%APPDATA%\CType\`, migrating from whichever isolated store still has content. Changing
  `GetUserStoreForAssembly` itself would be an engine change and is the weaker option — the
  isolated storage API has no scope that is stable across install paths without a strong name.
  Worth recording while here, since it is the same subsystem: the file is obfuscated with a
  single-byte XOR against `EngineConstants.ENCRYPTION_KEY`, carries no checksum, and on
  desktop nothing corroborates it, because `CompetitiveManager` only loads under
  `__ANDROID__`. Any value in it can be edited in a couple of lines.
  **This stopped mattering when leaderboards were dropped (see R1), and in fact helped decide
  it.** The concern was only ever that `HIGH_SCORE` and the `ALLTIME_*` totals would become
  public and comparable, so an editable save meant an unenforceable board. With nothing
  submitted anywhere, an editable save affects only the player who edits it — which is the
  definition of their own business. No checksum, no hardening, nothing to do here.

### Phase 3 — Graphics

Your second stated priority. Ordered cheapest-impact-first.

- **G1. Audit asset resolution — do this before anything else in the phase.** The art was
  authored for phone screens and is now drawn on a 1080p+ desktop display. Establish whether
  it holds up at 1:1 on a 27" monitor. The answer decides whether Phase 3 is "add effects"
  or "re-art the game", and those differ by an order of magnitude in cost.
- **G2. A particle system.** There isn't one. Thrusters, muzzle flashes, impact sparks,
  debris on death. The single biggest visual return per line of code in the project.
- **G3. Screen shake, hit-stop, and flash.** Enemies already flash white on hit; extend to
  brief time dilation on boss kills and camera shake on nukes and player death.
- **G4. Fix the audio architecture.** Pool `AudioPlayer` instances instead of allocating per
  shot, then delete the `TODO FIXME` rate-limit hacks in all six enemy classes. Convert the
  WAVs to a compressed format if the engine supports it — 25 MB of uncompressed audio is
  most of the download size.
- **G5. Deepen the parallax.** Three scrolling layers exist (stars, clusters, planets). Add
  a foreground layer and tie per-layer speed to player movement for a sense of depth.
- **G6. Better explosions.** One shared 9-frame animation is used for every death from a
  small fighter to a boss. Vary scale, tint and duration by enemy class at minimum.
- **G7. Boss telegraphs.** Wind-up animations and warning indicators before attacks. As much
  a fairness fix as a visual one, and a prerequisite for making bosses harder.
- **G8. Menu and HUD pass.** The HUD is mobile-scaled with touch-sized targets. Rebalance
  for desktop viewing distance.

### Phase 4 — Enemy behaviour

Your third stated priority. **E1 is a prerequisite for the rest** — do not add behaviours on
top of six duplicated classes.

- **E1. Collapse the enemy classes into one data-driven `Enemy` type.** The six variants
  differ only in HP, points, fire rate and sprite. Replace them with a single class plus an
  `EnemyDefinition` loaded from a data file. This deletes ~1,400 lines and turns every
  subsequent item in this phase into a data edit instead of six code edits. Do the same for
  the four player ships (~1,300 lines).
- **E2. Split behaviour from movement.** `IAccelerationProvider` handles motion; add a
  parallel `IWeaponBehaviour` so firing patterns compose with movement patterns. Right now
  every enemy in the game shares one behaviour: rotate toward the player, fire a plasma ball
  on a timer.
- **E3. Build a behaviour library.** Aimed shot, spread, burst, sustained beam, mine-layer,
  kamikaze charge, shielded (must be flanked), and a support type that buffs nearby enemies.
- **E4. Add real movement patterns.** The four existing ones are linear, sine wave, and two
  ellipses. Add strafe-and-retreat, hover-at-x-then-attack, swoop-in, and a pattern that
  reacts to player position. Note `WaveMotion` increments its oscillation by a fixed amount
  per *frame* rather than per second — it is framerate-dependent and should be fixed here.
- **E5. Formations.** The level format spawns ships one at a time on a timer. Add a formation
  concept (V, line, box, escorted) so groups arrive and manoeuvre coherently.
- **E6. Rework the bosses.** Multi-phase fights with distinct attack patterns per phase,
  destructible sub-components (`BossCannon` is already a separate object — build on that),
  and phase transitions telegraphed per G7.
- **E7. Difficulty curve.** HP and fire rate are per-class constants today. Introduce a
  per-level scalar so one enemy definition can be tuned across the campaign.

### Phase 5 — Level design

Depends on D4 (parser fix) and E1–E5 (having things worth placing).

- **L1. Replace the level format.** The current `type=0|ypos=-300|delay=0.8|...`
  pipe-delimited text is unreadable and unvalidated — a typo throws
  `ArgumentOutOfRangeException` at runtime, mid-level. Move to JSON (Newtonsoft is already
  referenced) with a schema, and validate every level at startup in Debug builds.
- **L2. Extend the format** to express what Phase 4 adds: formations, behaviours, scripted
  events, mid-level checkpoints, per-level enemy stat scalars, background and music selection.
- **L3. Build a level editor.** Twenty levels of hand-written pipe-delimited text is *why*
  the levels are repetitive. Even a crude visual timeline tool changes the economics of
  iteration more than any single design decision here. A small separate WinForms or Avalonia
  tool reading and writing the L1 JSON would do.
- **L4. Redesign the campaign.** With the above in place, rebuild all 20 levels around a
  deliberate difficulty curve — introduce one new enemy or mechanic at a time, with pacing
  that breathes between waves. The levels currently have no set pieces and no rhythm.
- **L5. Extend length and variety.** Twenty levels is short for a paid desktop release.
  Consider more levels, an endless/survival mode, and a boss rush. Endless was previously
  argued for as leaderboard-friendly; with no leaderboards it has to earn its place on being
  fun to replay, which is a fair test for it to pass on its own.
  Difficulty settings belong here too.

### Phase 6 — Steam release

- **R1. Steamworks integration**: achievements, Cloud saves, rich presence.
  **No leaderboards. Decided 2026-09-06, and this is a deliberate cut rather than an omission.**
  A single-player score in a game with no server is whatever the client says it is. Steam
  accepts the submission as given, there is nothing to validate it against, and this game's own
  save is a single-byte XOR with a constant — so the number is editable in a couple of minutes
  by anyone who wants to. A board that cannot reject a fabricated entry stops being a ranking
  and becomes a list of who cared enough to cheat, which is worse than no board because it also
  devalues the honest entries next to it.
  The only form that survives that objection is a **friends-only** board, where the population
  is small, self-selected and socially accountable. That is worth remembering if the question
  ever comes back, but it is not worth building now — it is the same submission plumbing for a
  fraction of the reach.
  *What this saves:* the submission path, the co-op board split, and the mode plumbing that
  existed to feed it. See M5, which shrinks to a local-only question.
  Builds on the SDK plumbing S6 sets up, and Phase 8 builds on this in turn, so keep init,
  callback pumping and shutdown in one place rather than one per feature.
  `LeaderboardController` is not part of this. It stays as it is — an Android facade over the
  engine's `CompetitiveManager`, which only loads under `__ANDROID__` and is therefore already
  inert on desktop. Nothing needs deleting; it simply gains no Steam implementation.
- **R2. Store assets**: capsule art, trailer, screenshots, description. Note the existing
  [gameplay trailer](https://www.youtube.com/watch?v=kixFrAAmXPs) is from the 2019 build and
  will misrepresent the release.
- **R3. Build and packaging**: a single-command Release build, no Android artefacts in the
  output (`Type.Desktop/bin/` currently contains `Type.Android.dll.config`), Steam depot
  configuration.
- **R4. Steam Deck verification.** Given Phase 1, this should be close to free, and it is
  worth real sales in this genre.
- **R5. Localisation.** The custom `TextDisplay` bitmap font maps A–Z, 0–9 and four symbols
  only. Anything beyond English needs a font and string-table pass — decide early whether it
  is in scope, because retrofitting it is expensive.
- **R6. Crash reporting and a public beta branch** before launch.

### Phase 7 — Local co-op

Two players, one machine, one screen. Added 2026-09-06.

**The good news first, because it is unusually good.** This is a fixed-field shooter: the world
is exactly 1920x1080, the camera never moves, and the background scrolls rather than the view.
So shared-screen co-op needs **no camera work at all** — no split screen, no zoom-to-fit, no
tether, none of what normally makes local co-op expensive. Two ships simply occupy the same
field. That removes the single largest cost item from this phase before it starts.

**The bad news is the input layer, which is the part just finished.** Phase 1 built input around
one player throughout: `IInputListener.UpdateDirectionData(Vector2, Single)` carries no player
identity, the provider tracks one `_ActivePad`, `GamepadActive` is a single boolean, and rumble
goes to "the" pad. Every action is broadcast to every listener with no way to say who pressed
it. That is not a flaw in Phase 1 — it was the right shape for the game as specified — but co-op
invalidates it, and M1 is a rework of code that is three days old.

- **M1. Give input a player identity.** A player index on every dispatched action and direction,
  a device-to-player assignment table, and a binding set per player rather than one global set.
  `ControlSettings` becomes per player, which the `BIND_<ACTION>` key naming will have to grow a
  player segment for. Everything in this phase hangs off it, so do it first and do it properly —
  the same advice I1 gave, for the same reason.
- **M2. Make the player plural in the model.** `CollisionController` holds one `_Player` and
  tests one hitbox; `GameScene` exposes one `Player`; `PlayingState` holds one. All become
  collections. Watch `CollisionController.Dispose`, which disposes the player it was handed.
- **M3. Drop in and drop out.** Locally this is cheap: an unassigned device pressing confirm
  joins, and a player who leaves has their ship removed. It is worth building even for local
  play alone, because it is the same seam Phase 8 needs and doing it here proves the shape on
  the easy side of the problem.
- **M4. Co-op HUD and ship select.** Two life meters, and a ship select that takes two choices
  before starting. `LifeMeter` is built for one; ship select is one cursor over four cards.
- **M5. The rules, which are design questions rather than engineering ones.** Shared lives or
  one pool each. Whether a dead player waits for the level or revives on a timer. Whether score
  is shared or attributed.
  **Decided (2026-09-06): a co-op run never overwrites the single-player high score.**
  Two ships put out twice the firepower against waves authored for one, so the two numbers are
  not comparable and one list of them would be meaningless.
  **This is now a local question only.** It was a much larger item when there were Steam
  leaderboards to keep honest; dropping those (see R1) took the submission path, the co-op board
  and most of the mode plumbing with them. What is left is the score the main menu shows:
  - **The run still needs to know its own mode.** `GameStats` and `Progress` are singletons
    written to from the play state, and `HIGH_SCORE` is written with no notion of how the run
    was played. A flag set when the run starts and read where the high score is written.
  - **A second key alongside `HIGH_SCORE`** in the save, so each mode keeps its own best and the
    menu can show whichever is relevant.
  - **Drop-in makes the mode a property of the run, not of the moment.** A run that starts solo
    and picks up a friend for the last five levels is neither. **Rule: any run that was ever
    co-op is a co-op run, and it never reverts** — the flag is set when a second player joins
    and stays set if they leave. Deciding by player count at the start would be gameable in the
    most obvious way available: start alone, invite immediately. This matters less without a
    public board to protect, but it is the same one-line rule either way and an inconsistent
    local best is still a bug.
  - **Achievements need the same decision**, and it does not have to be the same answer. Some
    are reasonable to earn co-operatively and some are not; the Omega unlock in particular is a
    progression gate rather than a boast. Settle it per achievement when R1 lands rather than
    blanket-refusing them here.
  - **The all-time totals are a separate question from the high score.** Kills and shots fired
    are a record of what the player did rather than a claim about skill, so counting them in
    co-op is defensible. Decide it deliberately rather than by whichever branch is easier.
- **M6. Balance for two.** Twice the firepower against waves authored for one ship. This is why
  the sequencing note below asks for M1 and M2 before Phase 5 rather than after it.

**Gate:** two players complete a run on one machine, with the second joining and leaving
mid-level and the game continuing correctly either way.

### Phase 8 — Networked co-op over Steam

Drop-in co-op with friends, through Steam matchmaking and invites. Added 2026-09-06.

**This is the largest item on the roadmap by a wide margin** — plausibly larger than Phases 0
to 7 put together. The game has no networking of any kind today, and nothing in it was written
with a second machine in mind. Treat it as its own project with its own schedule, not as a
phase that follows the others by a few weeks.

- **N1. Choose the model. Do this before writing a line of it.**
  **Recommendation: host-authoritative with client input forwarding.** The host runs the only
  real simulation, clients send their inputs and render what the host reports.
  The alternative, deterministic lockstep, is cheaper on bandwidth and much cheaper on state
  replication, and **this codebase cannot support it without an audit of every moving object**.
  ROADMAP already records that several classes do not multiply movement by `timeTilUpdate`,
  which makes their motion frame-rate dependent and therefore machine dependent. Enemy spawning
  is driven off accumulated deltas. Determinism would mean fixing all of that first and
  guaranteeing it stays fixed forever. Host authority tolerates every bit of it.
  It also makes N5 tractable: a joiner needs a state snapshot, and only the host has state.
  Co-op has no competitive integrity to protect, so the usual argument against trusting the host
  does not apply.
- **N2. Steam lobbies, matchmaking and invites.** Create and join lobbies, invite a friend, join
  from the friends list, and accept an invite that launches the game. Rich presence from R1
  feeds this. Built on whichever binding S6 settles on.
- **N3. Transport.** `SteamNetworkingSockets`, not the deprecated `SteamNetworking` P2P API.
  It brings NAT traversal and relay through Steam Datagram Relay, plus authentication and
  encryption, which is a large amount of networking nobody has to write.
- **N4. Replicate the simulation.** Entity ids, a snapshot format, delta encoding, and
  interpolation on the client. **This is where E1 pays for itself**: replicating one
  data-driven enemy type is a different job from replicating eleven bespoke classes, so E1
  should land first even though nothing else forces that order.
- **N5. Drop in mid-run.** A full snapshot on join and a clean handover into the delta stream.
  Straightforward under host authority, which is much of why N1 recommends it.
- **N6. Latency, loss and disconnection.** Input delay or rollback for the client's own ship,
  what happens when the host leaves, and whether that ends the run or migrates. **Recommend
  ending the run** for a first version; host migration is a large feature on its own.
- **N7. Test infrastructure.** Two instances on one machine, with artificial latency and loss.
  Without this, every test needs two people and nothing gets tested. Build it early.

**Gate:** two machines complete a run together, one having joined mid-level from a friend
invite, and a mid-run disconnection ends cleanly rather than hanging or corrupting the save.

---

## 3. Sequencing

```
Phase 0 ──> Phase 1 ──> Phase 2 ──┐
   │                              ├──> Phase 6 ──> Phase 7 ──> Phase 8
   ├──> Phase 3 (G1 gates it) ────┤
   └──> Phase 4 (E1 first) ──> Phase 5

Two crossings that the numbering does not show:
   M1 + M2 (the player becomes plural)  ──>  wanted before Phase 5
   E1      (one data-driven enemy type) ──>  wanted before N4
```

Phases 3 and 4 can run in parallel with 1 and 2 once Phase 0 lands. Phase 5 cannot start
until D4 and E1–E5 are done. Phase 6 needs everything up to it.

**Two crossings are worth more than the phase numbers suggest.**

**M1 and M2 should land before Phase 5, even though co-op ships after it.** Phase 5 redesigns
all twenty levels, and levels tuned for one ship have to be retuned for two. Making the player
plural first means that retune never happens, at the cost of doing the input and model rework
earlier than the feature needs it. The alternative is authoring twenty levels twice.

**E1 should land before N4.** Replicating one data-driven enemy type across a network is a
different job from replicating eleven bespoke classes, each with its own fields and lifetime.
Nothing else forces E1 before Phase 8, but doing Phase 8 first would mean writing the
replication layer twice.

**Phases 7 and 8 sit after release deliberately.** Co-op roughly doubles the QA surface of a
game that has never been playtested end to end even in single player — the Phase 1 gate is
still unmet. Shipping single player first gets the game in front of players and gives the co-op
work a stable base to build on. The exception is the sequencing note above: the *refactor* moves
earlier, the *feature* does not.

## 4. Android: dormant, not dead

**Decision (2026-09-04): Android stays as-is for now, and reviving it later is on the table.**
Desktop and Steam are the priority; nothing in the phases above should be blocked waiting on
Android, but nothing should burn the bridge either.

What that means in practice:

- **Keep the platform abstraction.** `IInputProvider`, `IAdService` and the
  `#if __ANDROID__` / `#elif __DESKTOP__` selection pattern stay. When Phase 1 rewrites
  `DesktopInputProvider`, put the shared logic (binding table, action enum, deadzone maths)
  in the `Type` shared project rather than in `Type.Desktop`, so a revived
  `AndroidInputProvider` can reuse it instead of reimplementing it.
- **Do not delete `Type.Android/`, the touch `Buttons/`, or the virtual analog stick.** They
  cost nothing while unloaded.
- **Do not let Android hold back desktop-first design.** Where mobile and desktop genuinely
  conflict (HUD scale, touch-sized hit targets, session length, ads), optimise for desktop
  and leave the Android path on its current behaviour behind the platform guard.
- **Adding a shared asset still means editing both csprojs** (see CLAUDE.md). Keep the
  Android asset list in sync even while the project is unloaded — it is a one-line copy at
  the time, and a painful audit later.

**Cost of an actual revival, so it is not a surprise later.** This is bigger than reloading
the project:

1. **Xamarin.Android is end-of-life** (support ended May 2024). A revival means migrating
   `Type.Android` to .NET for Android, which also means the `AmosAndroid` engine project
   migrating — and that is submodule territory, i.e. out of your hands.
2. **The API 28 target is far below Google Play's current minimum** for new submissions, so
   the manifest and SDK targets need raising regardless.
3. **The Android SDK platform for the target API is not installed on this machine** — the
   command-line build fails `XA5207` today.
4. Play Services, AdMob and the in-app billing plugin are all on 2018-era versions.

**The NuGet vulnerability banner in Visual Studio is entirely this debt.** The solution-wide
warning aggregates every project, and the dormant mobile ones carry roughly 57 Xamarin
packages from 2018 — Support Library 27/28, Play Services 60.1142.1, Firebase,
`Plugin.CurrentActivity` 2.1.0.4 (marked deprecated), and `Newtonsoft.Json` at 11.0.2 and
12.0.1, both affected by the advisory already patched on desktop. `Consolidate: 2` is the
same story: Newtonsoft is installed at four different versions across the solution.

**None of it ships.** *Verified against NuGet's live advisory database at audit level `low`
in `all` mode, covering transitive dependencies: the desktop path — `Type.Desktop`, which has
no packages, and `AmosDesktop`, which has `Newtonsoft.Json` 13.0.3 and
`Plugin.InAppBilling` 1.2.4 — reports zero advisories.* Patching the mobile packages now
would be wasted effort, because a revival replaces the entire Xamarin stack anyway. Leave
them until then.

**The same debt also surfaces on GitHub, and there it looks worse than it is.** Dependabot
raises `GHSA-5crp-9r3c-p9vr` (high) against `Type.Android/packages.config` on the default
branch, which is `development`, not `master`. Its companion alert against
`Type.Desktop/packages.config` is already marked fixed, because the `PackageReference`
migration deleted that file. **Bumping the pin the open alert names would change nothing:**
- `Type.Android.csproj` carries no `Newtonsoft.Json` reference at all, so the entry in its
  `packages.config` is vestigial. The assembly would reach a built APK transitively from
  `AmosAndroid`, which pins **11.0.2** — older than the 12.0.1 the alert names, and inside the
  submodule where this repository cannot change it.
- Editing the parent repo's pin therefore silences the alert while leaving what a revived
  Android build actually links against exactly as it was.
- `Type.Android/app.config` compounds it by redirecting Newtonsoft to `11.0.0.0` while
  `packages.config` says 12.0.1. The two have disagreed for as long as the project has been
  dormant.

The advisory itself is a stack-overflow denial of service on deeply nested JSON. The game's
only JSON inputs are the local save file and shipped spritesheet assets, with nothing arriving
over a network, so the worst case is a player crashing their own game with a save they wrote
themselves — which **S11** notes they can do more directly in any case.

**So a revival has to bump both halves** — the pin here and the engine's, then reconcile the
`app.config` redirect — rather than the one file Dependabot points at. Until then the alert is
accurate but inert. Dismissing it is defensible provided the reason recorded is this
paragraph, and not "not affected".

Treat a revival as its own project with its own assessment, not as a Phase item.

## 5. Open questions

1. **Is the existing art the shipping art?** G1 answers this, and the answer changes the size
   of Phase 3 by an order of magnitude.
2. **Is .NET Framework 4.8 + OpenTK 1.x acceptable to ship on?** It works, and it is what the
   engine targets. But it is Windows-only in practice, rules out a Linux-native build, and
   complicates Steam Deck. Migrating means engine work, which is out of scope — so this is
   really a question about the submodule's future rather than the game's. It is also the same
   question that gates an Android revival, so it is worth raising with the engine's author
   once rather than twice.
3. **Target price and scope.** Twenty short levels reads as a £3–5 title. If the target is
   higher, L5 stops being optional. Phases 7 and 8 move this: drop-in co-op with friends is a
   headline feature rather than a bullet point, and networked co-op in particular is a large
   enough investment that it should be priced for rather than added for free.
4. **Which achievements may be earned in co-op?** The score question is settled — see M5, a
   co-op run keeps its own high score and never overwrites the single-player one — and
   leaderboards are gone entirely, so achievements are now **the only thing co-op can affect
   that is visible outside the player's own machine**. That makes this the question worth
   spending thought on rather than a footnote to the score one. The answer is probably not the
   same for all of them: the Omega unlock is a progression gate rather than a boast, so earning
   it co-operatively is defensible in a way a score-based achievement is not. Needs deciding
   when R1 lands, per achievement rather than as a blanket rule.
5. **Do the all-time totals count co-op runs?** Kills and shots fired record what the player
   did rather than claim how good they are, so counting them is defensible where counting a
   high score is not. Cheap either way, but decide it rather than inheriting whichever branch
   was easier to write.
