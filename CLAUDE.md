# CLAUDE.md

Guidance for AI agents working in the C-Type repository.

## What this is

C-Type is a 2D side-scrolling space shooter written in C# on top of **AmosEngine**, a
custom OpenGL/OpenTK engine. It shipped on Android in 2019 and was delisted in 2023.
The current goal is a **Steam release for Windows desktop**.

The game was designed mobile-first (touch controls, virtual analog stick, interstitial
ads, Google Play achievements/leaderboards). Most work now is about turning it into a
desktop-native game.

## Hard rules

1. **Never commit directly to `SupportingFiles/`.** It is a git submodule pointing at
   `https://gitlab.com/amerigo14/AmosEngine.git`, a separate project with its own author.
   **Prefer a game-side solution**, and record any engine limitation you work around in
   [ROADMAP.md](ROADMAP.md).
   When an engine change is genuinely the better fix, it goes through a merge request
   against the **`ctype_development`** branch — the C-Type-specific branch this repo's
   submodule pointer tracks, which the maintainer can merge. Two have gone this route
   (`!22` and the 4.8 retarget). The workflow:
   - branch inside `SupportingFiles/` off `ctype_development`
   - keep the commit **small, self-contained, and about one thing**, so it can be
     cherry-picked onto the engine's mainline independently of C-Type history
   - push with `-o merge_request.create -o merge_request.target=ctype_development`
   - **wait for the merge** before bumping the parent's submodule pointer — source branches
     are deleted on merge, so a pointer aimed at an unmerged branch commit gets orphaned
   - then `git checkout ctype_development && git pull` in the submodule and commit the bump
2. **Do not commit to `development`, `master`, or `firebase`.** Branch off
   `development` for every change.
3. **Do not add third-party dependencies** without asking first.
4. **Preserve the existing code style** (see below). This codebase is stylistically
   very consistent; new code that looks different is a defect.

## Repository layout

```
Type/                  Shared game code (SHARED PROJECT - see "Adding files")
  Base/                GameObject, TimedCallback
  Buttons/             Virtual on-screen controls (touch)
  Controllers/         Singletons: Collision, Achievement, Leaderboard, Probe, PositionRelayer
  Data/                ButtonData, GameStats, LevelLoader, WaveData
  Factories/           EnemyFactory, PowerupFactory
  Glide/               Vendored tweening library - treat as third-party, do not restyle
  Interfaces/          All interfaces, grouped by area
  Objects/             Enemies, Bosses, Player ships, Projectiles, Probes, World
  Powerups/            Powerup implementations
  Scenes/              Visual composition (what is on screen)
  Services/            InputService, AdService - thin facades over platform providers
  States/              Game flow (menu -> ship select -> playing -> game over)
  UI/                  HUD elements
Type.Desktop/          Windows entry point + DesktopInputProvider + DesktopAdService
Type.Android/          Android entry point + AndroidInputProvider + PlayStoreAdService
Assets/                Source art, audio, level files (copied to Content/ at build)
SupportingFiles/       AmosEngine submodule - DO NOT EDIT
```

### States vs Scenes

`State` owns game logic and lifetime; `Scene` owns the drawables. A state creates its
scene in `OnEnter()`, drives it, and disposes it in `Dispose()`. State transitions
happen through `IsComplete()` calling `ChangeState(...)`. Keep logic in states and
presentation in scenes.

### Platform abstraction

Platform-specific behaviour is reached through an interface implemented per platform
and selected by preprocessor symbol:

```csharp
#if __ANDROID__
    _InputProvider = new AndroidInputProvider();
#elif __DESKTOP__
    _InputProvider = new DesktopInputProvider();
#endif
```

`__DESKTOP__` is defined in `Type.Desktop.csproj`; `__ANDROID__` in `Type.Android.csproj`.
**Any `using` of a platform namespace must sit inside the matching guard**, or the other
platform will fail to compile. (This exact mistake currently exists — see ROADMAP item D0.)

## Building

**Open `Type.Desktop.slnf`, not `Type.sln`.** It is a solution filter over the same
`Type.sln` that loads only the four desktop projects — `Type.Desktop`, `AmosDesktop`, and
the `Type.Shared` and `AmosShared` shared projects that hold most of the code.
`Type.sln` itself does **not** build: it pulls in `Type.Android`, `AmosAndroid` and
`AmosiOS`, which need the Xamarin workload and an Android SDK platform that a desktop
machine will not have. If you hit `MSB4226` (no Xamarin targets) or `XA5207` (Android SDK
platform missing), you are building the solution instead of the filter.

A filter rather than a second `.sln` deliberately: it references the real solution, so it
cannot drift out of sync when a project is added. It also replaces the machine-local project
unload state that used to live in the gitignored `.vs/Type/v17/.suo`.

MSBuild, not `dotnet build` — these are legacy .NET Framework 4.8 projects.
**Restore first on a fresh checkout**, then build:

```bash
"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" Type.Desktop.slnf /t:Restore /v:minimal
```

```bash
"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" Type.Desktop.slnf /p:Configuration=Debug /v:minimal
```

Run: `Type.Desktop/bin/Debug/Type.Desktop.exe`

`Type.Desktop` itself has **no NuGet packages**. The only package dependency in the whole
desktop build is `AmosDesktop`'s `Newtonsoft.Json`, declared as a `PackageReference` so
`/t:Restore` handles it with no `nuget.exe` installed; it flows transitively into the game's
output and is the only third-party assembly shipped. `OpenTK` and `FarseerPhysics` are plain
`HintPath` references into the engine's committed `Libraries/Desktop`, so they need no
restore. If you skip restore the build fails with `CS0246` on `Newtonsoft` — that means
restore, not a code problem.

Android is currently **dormant** — not being built, but intended to stay revivable. Do not
delete `Type.Android/`, `Type/Buttons/`, or the touch input paths, and keep new shared logic
in the `Type` shared project rather than in `Type.Desktop` so a revived Android provider can
reuse it. See ROADMAP §4 for the full policy and the cost of an actual revival.

## Adding files and assets

The `Type` project is a **shared project** (`Type.Shared.shproj` + `Type.projitems`),
not a normal csproj. Two consequences:

- **A new `.cs` file must be added to `Type/Type.projitems`** as
  `<Compile Include="$(MSBuildThisFileDirectory)Path\To\File.cs" />` or it will not compile.
  Visual Studio does this automatically; editing files directly does not.
- **After editing `Type.projitems` outside the IDE, close and reopen the solution filter.**
  A shared project's item list is read at project-evaluation time, and Visual Studio does not
  reliably re-evaluate it when the file changes on disk behind its back. Until it is reloaded
  VS compiles the old list, and the new files appear to not exist:
  `CS0234 The type or namespace name 'X' does not exist in the namespace 'Type.Y'`, or
  `CS0246 ... could not be found`, on code that builds cleanly from the command line.
  A build that fails in under a second is the tell. Shared projects usually cannot be
  reloaded individually from Solution Explorer, so reopen the filter.
  Cross-check with
  `MSBuild.exe Type.Desktop.slnf /t:Rebuild /p:Configuration=Debug` before assuming the
  code is at fault — if that is clean, it is the IDE's cache.
- **A new asset must be added to BOTH `Type.Desktop.csproj` and `Type.Android.csproj`**
  as a `<Content Include="..\Assets\...">` with a `<Link>Content\...</Link>` and
  `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`. All 122 assets are
  listed by hand today.

Asset paths in code are always the *output* path (`Content/...`), never `Assets/...`.

## Code style

Match the surrounding code exactly. The conventions in use:

- **BCL type names, not C# keywords**: `Int32`, `Single`, `Boolean`, `String`, `Vector2`.
  Never `int`, `float`, `bool`, `string`, and never `var` for these.
- **Private fields**: `_PascalCase` with a leading underscore. `readonly` where possible.
- **XML doc comments on every member**, including private ones. Use `/// <inheritdoc />`
  when implementing an interface member.
- **Allman braces**, including braced `switch` cases:
  ```csharp
  switch (id)
  {
      case 0:
          {
              DoThing();
              break;
          }
  }
  ```
- **`#region` blocks** to group interface implementations (`#region Implementation of IInputListener`).
- **Listener pattern over C# events.** Objects implement `INotifier<TListener>` and expose
  `RegisterListener` / `DeregisterListener`; consumers implement the listener interface.
- **Singletons**: `public static X Instance => _Instance ?? (_Instance = new X());`
- Frame updates come from `IUpdatable.Update(TimeSpan timeTilUpdate)`, registered via
  `UpdateManager.Instance.AddUpdatable(this)`. **Always multiply movement by
  `timeTilUpdate.TotalSeconds`** — several existing classes do not, and that is a bug,
  not a pattern to copy.
- Every `IDisposable` must deregister itself from every manager/controller it registered with.

## Gotchas

- **Cheats are opt-in.** `Constants.Global.INVINCIBLE` and `START_LEVEL` default to `false`
  and `1` in every configuration. To enable them while testing, add `CTYPE_CHEATS` to
  `DefineConstants` for the Debug configuration, or build with
  `/p:DefineConstants="TRACE;DEBUG;__DESKTOP__;CTYPE_CHEATS"`. Never commit that symbol to a
  checked-in configuration — an `#error` guard fails the build if it reaches Release, but
  nothing stops it reaching a Debug build you then forget about.
- The engine's world origin is screen centre; `Constants.Global.ScreenTop/Bottom/Left/Right`
  are derived from a fixed 1920x1080 target. Backgrounds are positioned at `(-960, -540)`.
- `new AudioPlayer(...)` is constructed per sound effect, per shot. Enemy classes carry a
  `// TODO FIXME` hack to rate-limit hit sounds because of this. Do not add more of these
  hacks; if you touch audio, fix it properly (ROADMAP item G4).
- `Type/Glide/` is a vendored copy of the Glide tweening library. Leave it alone.

## Testing

There is no test project. Verification is manual: build, run, play. When changing
gameplay, say explicitly what you did and did not verify by playing.

**Check the screen, not just the exit code.** A build that succeeds and a process that exits
0 says nothing about whether anything is legible, on screen, or the right size. Capture the
window and look at it:

```powershell
PrintWindow(hwnd, hdc, 2)   # flag 2 = PW_RENDERFULLCONTENT
```

That captures the window's own pixels, works on this OpenGL window, and does not care what
is in front of it. **Do not use `Graphics.CopyFromScreen`** — it grabs whatever occupies
those screen coordinates, so if anything overlaps the game you silently capture someone
else's window instead of the game.

**It only works while the desktop session is actually presenting.** If the session is locked
or disconnected, `PrintWindow` succeeds and hands back a blank bitmap — white in windowed
mode, where the frame is drawn but the GL surface is not, and black in borderless, where
there is no frame either. The tell is `GetForegroundWindow()` returning `0`: check that
before trusting a capture, and if it is zero, say the screen could not be checked rather than
reading anything into a blank image. `CopyFromScreen` is no help here either — it captures
the same nothing, and without the foreground check it does it silently.

To inspect a screen that needs input to reach, temporarily start the game on that state
(`StateManager.Instance.StartState(new OptionsState(null))` in `Game.LoadContent`), capture,
then put the boot path back. Input cannot be driven from outside: OpenTK reads raw device
state, so `SendKeys` is invisible to it, and Windows blocks `SetForegroundWindow` from a
background process.

## Where to start

Read [ROADMAP.md](ROADMAP.md). Work items are numbered (D0, I1, ...) and ordered by
dependency; the phase gates matter more than the item order within a phase.
