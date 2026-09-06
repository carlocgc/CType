using AmosShared.Audio;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Linq;
using AmosShared.Base;
using AmosShared.State;
using Type.Controllers;
using Type.Data;
using Type.Factories;
using Type.Interfaces.Control;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;
using Type.Scenes;
using Type.Services;
using Type.UI;
using Type.UI.Navigation;

namespace Type.States
{
    /// <summary>
    /// Game play state
    /// </summary>
    public class PlayingState : State, IPlayerListener, IEnemyListener, IEnemyFactoryListener, IPowerupListener, IPowerupFactoryListener, IUpdatable, IInputListener
    {
        /// <summary> Max level of the game </summary>
        private readonly Int32 _MaxLevel = 20;
        /// <summary> Maximum amount of nukes the player can hold </summary>
        private readonly Int32 _MaxNukes = 3;
        /// <summary> THe type of player craft </summary>
        private readonly Int32 _PlayerType;

        /// <summary> Factory that will create enemies </summary>
        private EnemyFactory _EnemyFactory;
        /// <summary> Factory that creates power ups </summary>
        private PowerupFactory _PowerupFactory;
        /// <summary> Scene for game objects </summary>
        private GameScene _GameScene;
        /// <summary> Scene for UI objects </summary>
        private UIScene _UIScene;
        /// <summary> Displays the current level as text on the screen </summary>
        private LevelDisplay _LevelDisplay;
        /// <summary> Text to display the score </summary>
        private TextDisplay _ScoreDisplay;
        /// <summary> Displays the players current lives </summary>
        private LifeMeter _LifeMeter;
        /// <summary> The player </summary>
        private IPlayer _Player;
        /// <summary> Whether the game is paused </summary>
        private Boolean _Paused;
        /// <summary> Whether the level has started </summary>
        private Boolean _LevelStarted;
        /// <summary> Whether the game is over </summary>
        private Boolean _GameOver;
        /// <summary> Whether the game is complete </summary>
        private Boolean _GameComplete;
        /// <summary> Whether the nuke button was pressed last update, used to prevent multiple triggers of nukes </summary>
        private Boolean _NukePressed;
        /// <summary> The current level </summary>
        private Int32 _CurrentLevel;
        /// <summary> Total enemies in this level </summary>
        private Int32 _EnemiesInLevel;
        /// <summary> Total enemies destroyed this level </summary>
        private Int32 _EnemiesDestroyedThisLevel;
        /// <summary> amount of nukes the player has </summary>
        private Int32 _CurrentNukes;

        /// <summary> The pause overlay, present only while paused </summary>
        private PauseScene _PauseScene;
        /// <summary> Moves focus between the pause commands </summary>
        private MenuNavigator _PauseNavigator;
        /// <summary> The settings shown over the paused game, present only while open </summary>
        private OptionsScene _PauseOptionsScene;
        /// <summary> Moves focus between the settings shown over the paused game </summary>
        private MenuNavigator _PauseOptionsNavigator;
        /// <summary> The controls screen shown over the settings, present only while open </summary>
        private ControlsScene _PauseControlsScene;
        /// <summary> Moves focus between the bindings shown over the paused game </summary>
        private MenuNavigator _PauseControlsNavigator;
        /// <summary> Receives the cancel that closes the pickup guide </summary>
        private MenuNavigator _PauseHelpNavigator;
        /// <summary> Whether the pickup guide is open over the pause menu </summary>
        private Boolean _HelpOpen;
        /// <summary> Whether the player asked to start the run again </summary>
        private Boolean _Restarting;
        /// <summary> Whether the player asked to abandon the run </summary>
        private Boolean _Quitting;

        /// <summary> Whether or not the updatable is disposed </summary>
        public Boolean IsDisposed { get; set; }

        public PlayingState(Int32 type)
        {
            _PlayerType = type;
        }

        protected override void OnEnter()
        {
            _CurrentLevel = Constants.Global.START_LEVEL;

            // Before anything can ask for particles, and after the canvas exists, since every
            // pooled particle registers a sprite with it.
            ParticleController.Instance.Initialise();

            _EnemyFactory = new EnemyFactory();
            _EnemyFactory.RegisterListener(this);
            _EnemyFactory.ParentState = this;

            _PowerupFactory = new PowerupFactory();
            _PowerupFactory.RegisterListener(this);

            _GameScene = new GameScene(_PlayerType) { Visible = true };

            _Player = _GameScene.Player;
            _Player.RegisterListener(this);
            CollisionController.Instance.RegisterPlayer(_Player);

            _UIScene = new UIScene(_PlayerType);
            _ScoreDisplay = _UIScene.ScoreDisplay;
            _LifeMeter = _UIScene.LifeMeter;
            _LevelDisplay = _UIScene.LevelDisplay;
            _UIScene.ShowOnScreenControls(true);
            _UIScene.Visible = true;

            _GameScene.StartBackgroundScroll();

            _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
            {
                _EnemyFactory.Start(LevelLoader.GetWaveData(_CurrentLevel));
                CollisionController.Instance.IsActive = true;
            });

            _Player.Spawn();
            GameStats.Instance.GameStart();
            InputService.Instance.RegisterListener(this);
            InputService.Instance.OnInputDeviceLost = () => SetPaused(true);
            UpdateManager.Instance.AddUpdatable(this);

        }

        /// <summary>If true then this state is considered complete and control will be passed over to <see cref="State.NextState"/></summary>
        /// <returns></returns>
        public override Boolean IsComplete()
        {
            if (_GameOver) ChangeState(new GameOverState());
            else if (_GameComplete) ChangeState(new GameCompleteState(_PlayerType));
            else if (_Restarting) ChangeState(new PlayingState(_PlayerType));
            else if (_Quitting) ChangeState(new MainMenuState());

            return _GameOver || _GameComplete || _Restarting || _Quitting;
        }

        #region Player

        /// <summary>
        /// Invoked when a life is added
        /// </summary>
        /// <param name="player"></param>
        /// <param name="points"></param>
        public void OnLifeAdded(IPlayer player, Int32 points)
        {
            if (_LifeMeter.PlayerLives == 5)
            {
                UpdateScore(points);
                new AudioPlayer("Content/Audio/points_instead.wav", false, AudioManager.Category.EFFECT, 1);
                return;
            }
            _LifeMeter.AddLife();
        }

        /// <inheritdoc />
        public void OnPointPickup(Int32 value)
        {
            UpdateScore(value);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Only reached when the hit got through: a shield that absorbs one returns before
        /// notifying, and rumbles for itself.
        /// </remarks>
        public void OnPlayerHit(IPlayer player)
        {
            Rumble.PlayerHit();
        }

        /// <inheritdoc />
        public void OnPlayerDeath(IPlayer player, Int32 probeCount, Vector2 position)
        {
            _LifeMeter.LoseLife();
            _GameScene.RemovePowerUps();
            Rumble.PlayerDeath();
            Particles.PlayerDestroyed(position);

            if (_LifeMeter.PlayerLives > 0)
            {
                _Player.Spawn();
                if (probeCount > 0)
                {
                    _PowerupFactory.Create(1, position, _CurrentLevel);
                }
            }
            else
            {
                GameOver();
            }
        }

        /// <summary>
        /// Invoked when a nuke is collected by the player
        /// </summary>
        public void OnNukeAdded(Int32 points)
        {
            if (_CurrentNukes >= _MaxNukes)
            {
                UpdateScore(points);
                new AudioPlayer("Content/Audio/points_instead.wav", false, AudioManager.Category.EFFECT, 1);
                return;
            }
            _CurrentNukes++;
            _UIScene.NukeDisplay.NukeCount = _CurrentNukes;
            new AudioPlayer("Content/Audio/nuke_pickup.wav", false, AudioManager.Category.EFFECT, 1);
        }

        #endregion

        #region Enemy

        /// <summary>
        /// Invoked when the factory has started a new level
        /// </summary>
        /// <param name="levelTotal"></param>
        public void OnLevelStarted(Int32 levelTotal)
        {
            _EnemiesDestroyedThisLevel = 0;
            _EnemiesInLevel = levelTotal;
            _LevelStarted = true;
        }

        /// <inheritdoc />
        public void EnemyCreated(IEnemy enemy)
        {
            _GameScene.Enemies.Add(enemy);
        }

        /// <summary>
        /// Invoked when the factory has finished spawning the levels enemies
        /// </summary>
        public void OnLevelFinishedSpawning()
        {
        }

        /// <inheritdoc />
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            _EnemiesDestroyedThisLevel++;
            UpdateScore(enemy.Points);
            _PowerupFactory.Create(0, enemy.Position, _CurrentLevel);

            // Only a whole boss, not a station's individual cannons, which is why BossCannon is
            // deliberately not an IBoss.
            if (enemy is IBoss)
            {
                Rumble.BossDestroyed();
                Particles.BossDestroyed(enemy.Position);
            }
            else
            {
                Particles.EnemyDestroyed(enemy.Position);
            }
        }

        /// <inheritdoc />
        public void OnEnemyOffscreen(IEnemy enemy)
        {
            _EnemiesDestroyedThisLevel++;
            enemy.Dispose();
        }

        #endregion

        #region  Powerups

        /// <inheritdoc />
        public void OnPowerupCreated(IPowerup powerup)
        {
            _GameScene.Powerups.Add(powerup);
            CollisionController.Instance.RegisterPowerup(powerup);
        }

        /// <inheritdoc />
        public void OnPowerupApplied(IPowerup powerup)
        {
            _GameScene.Powerups.Remove(powerup);
        }

        /// <inheritdoc />
        public void OnPowerupExpired(IPowerup powerup)
        {
            _GameScene.Powerups.Remove(powerup);
        }

        #endregion

        #region Game_Logic

        /// <summary>
        /// Pauses or resumes play
        /// </summary>
        /// <param name="paused"> Whether play should be paused </param>
        private void SetPaused(Boolean paused)
        {
            if (_Paused == paused) return;

            _Paused = paused;
            Game.GameTime.Multiplier = paused ? 0 : 1;
            _UIScene.SetPaused(paused);
            InputService.Instance.SetPaused(paused);

            if (paused) ShowPauseMenu();
            else ClosePauseMenu();
        }

        /// <summary>
        /// Builds the pause overlay and gives it focus
        /// </summary>
        private void ShowPauseMenu()
        {
            _PauseScene = new PauseScene(
                onResume: () => SetPaused(false),
                onHelp: ShowPauseHelp,
                onOptions: ShowPauseOptions,
                onRestart: () => LeaveRun(() => _Restarting = true),
                onQuit: () => LeaveRun(() => _Quitting = true))
            {
                Visible = true,
            };

            FocusPauseMenu();
        }

        /// <summary>
        /// Gives the pause menu the focus cursor
        /// </summary>
        private void FocusPauseMenu()
        {
            _PauseScene.SetMenuVisible(true);

            _PauseNavigator = new MenuNavigator { OnCancel = () => SetPaused(false) };
            foreach (MenuTextItem item in _PauseScene.Items) _PauseNavigator.Add(item);
            _PauseNavigator.FocusFirst();
        }

        /// <summary>
        /// Hides the pause menu so something can be shown over it, keeping the dark wash
        /// </summary>
        private void HidePauseMenu()
        {
            _PauseScene.SetMenuVisible(false);
            _PauseNavigator?.Dispose();
            _PauseNavigator = null;
        }

        /// <summary>
        /// Tears down the pause overlay and anything opened from it
        /// </summary>
        private void ClosePauseMenu()
        {
            ClosePauseOptions();
            ClosePauseHelp();

            _PauseNavigator?.Dispose();
            _PauseNavigator = null;
            _PauseScene?.Dispose();
            _PauseScene = null;
        }

        /// <summary>
        /// Returns to the pause menu after a sub screen closes, unless the run is ending
        /// </summary>
        /// <remarks>
        /// Quitting or restarting from inside a sub screen would otherwise put a menu back on
        /// top of a state that is being torn down.
        /// </remarks>
        private void ReturnToPauseMenu()
        {
            if (!_Paused || _PauseScene == null) return;
            FocusPauseMenu();
        }

        /// <summary>
        /// Shows what the pickups do. This used to appear automatically on pause, where it
        /// collided with the menu and with anything opened from it.
        /// </summary>
        private void ShowPauseHelp()
        {
            if (_HelpOpen) return;

            HidePauseMenu();
            _PauseScene.SetBackPromptVisible(true);
            _HelpOpen = true;
            _UIScene.Help.Show();

            // No items: the screen is a page to read, so only backing out of it does anything.
            _PauseHelpNavigator = new MenuNavigator { OnCancel = ClosePauseHelp };
        }

        /// <summary>
        /// Hides the pickup guide and returns focus to the pause menu
        /// </summary>
        private void ClosePauseHelp()
        {
            if (!_HelpOpen) return;

            _PauseHelpNavigator?.Dispose();
            _PauseHelpNavigator = null;
            _UIScene.Help.Hide();
            _PauseScene?.SetBackPromptVisible(false);
            _HelpOpen = false;

            ReturnToPauseMenu();
        }

        /// <summary>
        /// Shows the settings over the paused game, hiding the pause menu behind it
        /// </summary>
        private void ShowPauseOptions()
        {
            if (_PauseOptionsScene != null) return;

            HidePauseMenu();

            _PauseOptionsScene = new OptionsScene(ShowPauseControls, overlay: true) { Visible = true };

            FocusPauseOptions();
        }

        /// <summary>
        /// Gives the settings shown over the paused game the focus cursor
        /// </summary>
        private void FocusPauseOptions()
        {
            _PauseOptionsScene.SetSettingsVisible(true);

            _PauseOptionsNavigator = new MenuNavigator { OnCancel = ClosePauseOptions };
            foreach (OptionRow row in _PauseOptionsScene.Rows) _PauseOptionsNavigator.Add(row);
            _PauseOptionsNavigator.Add(_PauseOptionsScene.ControlsItem);
            _PauseOptionsNavigator.FocusFirst();
        }

        /// <summary>
        /// Closes the settings and returns focus to the pause menu
        /// </summary>
        private void ClosePauseOptions()
        {
            if (_PauseOptionsScene == null) return;

            DisposePauseControls();

            _PauseOptionsNavigator?.Dispose();
            _PauseOptionsNavigator = null;
            _PauseOptionsScene.Dispose();
            _PauseOptionsScene = null;

            ReturnToPauseMenu();
        }

        /// <summary>
        /// Shows the bindings over the settings, hiding the settings behind them
        /// </summary>
        private void ShowPauseControls()
        {
            if (_PauseControlsScene != null) return;

            _PauseOptionsNavigator?.Dispose();
            _PauseOptionsNavigator = null;
            _PauseOptionsScene.SetSettingsVisible(false);

            _PauseControlsScene = new ControlsScene(overlay: true) { Visible = true };

            _PauseControlsNavigator = new MenuNavigator { OnCancel = ClosePauseControls };
            foreach (BindingRow row in _PauseControlsScene.Rows) _PauseControlsNavigator.Add(row);
            _PauseControlsNavigator.Add(_PauseControlsScene.ResetItem);
            _PauseControlsNavigator.FocusFirst();
        }

        /// <summary>
        /// Closes the bindings and returns focus to the settings
        /// </summary>
        private void ClosePauseControls()
        {
            if (_PauseControlsScene == null) return;

            DisposePauseControls();
            FocusPauseOptions();
        }

        /// <summary>
        /// Tears the bindings down without giving focus back, for the paths that are closing the
        /// settings behind them too
        /// </summary>
        private void DisposePauseControls()
        {
            _PauseControlsNavigator?.Dispose();
            _PauseControlsNavigator = null;
            _PauseControlsScene?.Dispose();
            _PauseControlsScene = null;
        }

        /// <summary>
        /// Abandons the run, restoring the clock first so whatever comes next is not frozen
        /// </summary>
        /// <param name="markIntent"> Sets the flag that decides where to go </param>
        private void LeaveRun(Action markIntent)
        {
            ClosePauseMenu();

            _Paused = false;
            Game.GameTime.Multiplier = 1;
            InputService.Instance.SetPaused(false);

            markIntent();
        }

        /// <summary>
        /// Adds the value to the players current score
        /// </summary>
        /// <param name="amount"></param>
        private void UpdateScore(Int32 amount)
        {
            GameStats.Instance.Score += amount;
            _ScoreDisplay.Text = GameStats.Instance.Score.ToString();
        }

        /// <summary>
        /// Sets the next level data and displays the current level, ends the game if complete
        /// </summary>
        private void LevelComplete()
        {
            if (_GameOver) return;

            AchievementController.Instance.LevelCompleted(_CurrentLevel);

            if (_CurrentLevel >= _MaxLevel) GameCompleted();
            else
            {
                _CurrentLevel++;
                _EnemiesDestroyedThisLevel = 0;
                _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    _EnemyFactory.Start(LevelLoader.GetWaveData(_CurrentLevel));
                    _LevelStarted = true;
                });
            }
        }

        /// <summary>
        /// Ends the playing state and set the next state to be <see cref="GameCompleteState"/>
        /// </summary>
        private void GameCompleted()
        {
            GameStats.Instance.GameEnd();
            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();
            _EnemyFactory.Stop();
            _UIScene.ShowOnScreenControls(false);
            _UIScene.Visible = false;
            _GameComplete = true;
        }

        /// <summary>
        /// Ends the game and sets the next state to Game over state
        /// </summary>
        private void GameOver()
        {
            GameStats.Instance.GameEnd();
            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();
            _LevelDisplay.Dispose();
            _EnemyFactory.Dispose();
            _UIScene.ShowOnScreenControls(false);
            _UIScene.Visible = false;
            _GameOver = true;
        }

        #endregion

        protected override void OnExit()
        {

        }

        /// <summary> Updates the state </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeSinceUpdate)
        {
            base.Update(timeSinceUpdate);
            if (!_LevelStarted) return;
            if (_EnemiesDestroyedThisLevel >= _EnemiesInLevel)
            {
                LevelComplete();
            }
        }

        /// <summary> Whether or not the object can be updated </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }


        #region Implementation of IInputListener

        /// <summary>
        /// Update data from the analog stic
        /// </summary>
        /// <param name="direction"> The direction the stick is pushed </param>
        /// <param name="strength"> The distance the stick is pushed </param>
        public void UpdateDirectionData(Vector2 direction, Single strength)
        {
        }

        /// <summary> Informs the listener of input events </summary>
        /// <param name="data"> Data packet from the <see cref="InputService"/> </param>
        public void UpdateInputData(ButtonEventData data)
        {
            switch (data.ID)
            {
                case ButtonData.Type.NUKE:
                    {
                        if (data.State == ButtonData.State.RELEASED) _NukePressed = false;

                        if (data.State != ButtonData.State.PRESSED || _CurrentNukes <= 0) return;

                        if (_NukePressed) return;

                        _CurrentNukes--;
                        _UIScene.NukeDisplay.NukeCount = _CurrentNukes;
                        CollisionController.Instance.ClearProjectiles();

                        foreach (IEnemy enemy in _GameScene.Enemies.Where(e => e.CanBeRoadKilled))
                        {
                            if (!enemy.IsDisposed && !enemy.IsDestroyed) enemy.Destroy();
                        }

                        _GameScene.ShowNukeEffect();
                        new AudioPlayer("Content/Audio/nuke.wav", false, AudioManager.Category.EFFECT, 1);
                        Rumble.Nuke();

                        _NukePressed = true;

                        break;
                    }
                case ButtonData.Type.PAUSE:
                    {
                        if (data.State != ButtonData.State.PRESSED) return;
                        SetPaused(!_Paused);
                        break;
                    }
            }
        }

        #endregion

        /// <inheritdoc />
        public override void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            base.Dispose();

            InputService.Instance.DeregisterListener(this);
            InputService.Instance.OnInputDeviceLost = null;
            ClosePauseMenu();
            UpdateManager.Instance.RemoveUpdatable(this);
            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();

            // Every pooled particle holds a sprite registered with the canvas, so leaving the
            // pool behind is exactly the leak S9 went looking for.
            ParticleController.Instance.Dispose();

            _LevelDisplay = null;
            _ScoreDisplay = null;
            _LifeMeter = null;
            _Player = null;

            _PowerupFactory.Dispose();
            _PowerupFactory = null;
            _EnemyFactory.Dispose();
            _EnemyFactory = null;
            _GameScene.Dispose();
            _GameScene = null;
            _UIScene.Dispose();
            _UIScene = null;
        }

    }
}
