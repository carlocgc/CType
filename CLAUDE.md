# CLAUDE.md

Guidance for AI agents working in the C:Type repository.

## What this is

C:Type is a 2D side-scrolling space shooter written in C# on top of **AmosEngine**, a
custom OpenGL/OpenTK engine. It shipped on Android in 2019 and was delisted in 2023.
The current goal is a **Steam release for Windows desktop**.

The game was designed mobile-first (touch controls, virtual analog stick, interstitial
ads, Google Play achievements/leaderboards). Most work now is about turning it into a
desktop-native game.

## Hard rules

1. **Never modify `SupportingFiles/`.** It is a git submodule pointing at
   `https://gitlab.com/amerigo14/AmosEngine.git` and is owned by a third party.
   If something appears to need an engine change, find a game-side workaround and
   record the limitation in [ROADMAP.md](ROADMAP.md) instead.
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

MSBuild (not `dotnet build` — these are legacy .NET Framework 4.8 projects). **Restore
first on a fresh checkout**, then build:

```bash
"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" Type.Desktop/Type.Desktop.csproj /t:Restore /v:minimal
```

```bash
"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" Type.Desktop/Type.Desktop.csproj /p:Configuration=Debug /v:minimal
```

Run: `Type.Desktop/bin/Debug/Type.Desktop.exe`

`Type.Desktop` itself has **no NuGet packages**. The only package dependencies are
`AmosDesktop`'s (`Newtonsoft.Json`, `Plugin.InAppBilling`), declared as `PackageReference`
so `/t:Restore` handles them with no `nuget.exe` installed; they flow transitively into the
game's output. `OpenTK` and `FarseerPhysics` are plain `HintPath` references into the
engine's committed `Libraries/Desktop`, so they need no restore. If you skip restore the
build fails with `CS0246` on `Newtonsoft`/`Plugin` — that means restore, not a code problem.

**The v4.8 retarget is uncommitted and load-bearing.** The committed projects target
.NET Framework **v4.6.1**; the working build depends on local uncommitted retargets to v4.8
in `Type.Desktop/Type.Desktop.csproj`, `Type.Desktop/App.config`, and — critically —
`SupportingFiles/AmosDesktop/AmosDesktop.csproj`, which is inside the submodule and cannot
be committed here. A clean checkout fails with `MSB3644` unless the 4.6.1 targeting pack is
installed. Do not discard these local changes. See ROADMAP item D6.

**Why a command-line build may fail where Visual Studio succeeds.** `Type.Desktop.csproj`
has a spurious `ProjectReference` to `Type.Android.csproj`. In the IDE the Android projects
(`Type.Android`, `AmosAndroid`, `AmosiOS`) are **unloaded**, so VS never walks that reference
and the build succeeds. That unload state lives in `.vs/Type/v17/.suo`, which is gitignored —
it does not survive a fresh clone, and MSBuild on the command line does not honour it. If you
hit `MSB4226` (no Xamarin targets) or `XA5207` (Android SDK platform missing), this is why.
See ROADMAP item D0; the reference is safe to delete and doing so does not affect Android.

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

- `Constants.Global` has `#if DEBUG` overrides for `INVINCIBLE` and `START_LEVEL`.
  These are committed as `INVINCIBLE = true` and `START_LEVEL = 11`. Check them before
  testing anything gameplay-related, and never let a non-default value reach `Release`.
- The engine's world origin is screen centre; `Constants.Global.ScreenTop/Bottom/Left/Right`
  are derived from a fixed 1920x1080 target. Backgrounds are positioned at `(-960, -540)`.
- `new AudioPlayer(...)` is constructed per sound effect, per shot. Enemy classes carry a
  `// TODO FIXME` hack to rate-limit hit sounds because of this. Do not add more of these
  hacks; if you touch audio, fix it properly (ROADMAP item G4).
- `Type/Glide/` is a vendored copy of the Glide tweening library. Leave it alone.

## Testing

There is no test project. Verification is manual: build, run, play. When changing
gameplay, say explicitly what you did and did not verify by playing.

## Where to start

Read [ROADMAP.md](ROADMAP.md). Work items are numbered (D0, I1, ...) and ordered by
dependency; the phase gates matter more than the item order within a phase.
