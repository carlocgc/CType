using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using AmosShared.Base;
using AmosShared.Competitive;

namespace Type
{
    /// <summary>
    /// Constant values
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Global values
        /// </summary>
        public static class Global
        {                      
            public const String VERSION = "1.2.2";

            /// <summary> The game's name, as shown to the player </summary>
            public const String TITLE = "C-Type";

            /// <summary> The game's name for the bitmap font, which only carries capitals </summary>
            public const String TITLE_UPPERCASE = "C-TYPE";

            /// <summary>
            /// The game's name where it has to be a directory name
            /// </summary>
            /// <remarks>
            /// Deliberately punctuation-free rather than <see cref="TITLE"/>. The engine passes
            /// this straight to <c>IsolatedStorageFile.CreateDirectory</c>, and the desktop save
            /// folder already uses this spelling, so changing it would orphan saves that exist.
            /// The game's own save does not live here — see ROADMAP item S11 — but the engine's
            /// achievement and leaderboard store still does.
            /// </remarks>
            public const String STORE_NAME = "CType";

            /// <summary>
            /// Steam application id, zero while the game has none of its own.
            /// </summary>
            /// <remarks>
            /// **Zero disables Steam entirely** rather than standing in for an id. The provider
            /// reports itself unavailable and never starts the SDK, which is a state everything
            /// downstream already handles, because Steam not being installed lands there too.
            /// <para>
            /// It was 480 — Spacewar, Valve's public test application — so that the SDK could be
            /// started and exercised before the game had an id of its own, and that is what
            /// proved the lifecycle in S6. **Borrowing an id turned out to carry a cost that only
            /// shows up with a pad in your hands:** Steam applies whatever controller
            /// configuration the borrowed id is set up for, and 480 is the SDK's own Steam Input
            /// sample, so the game loses the gamepad the moment it initialises. That is not worth
            /// paying every day for plumbing that is already verified. See ROADMAP S6.
            /// </para>
            /// <para>
            /// **Put the real id here when it exists** and everything turns on with it; there is
            /// no second switch. Achievements need it regardless — Steam resolves them against
            /// the id's own configured list, which is why S6 stops at the SDK lifecycle rather
            /// than going on to unlock anything.
            /// </para>
            /// </remarks>
            public const UInt32 STEAM_APP_ID = 0;

#if __ANDROID__
            /// <summary> AdMob application id, Android builds only </summary>
            public const String ADMOB_APP_ID = "ca-app-pub-4204969324853965~4341189590";
#endif // #if __ANDROID__

#if !DEBUG && CTYPE_CHEATS
#error CTYPE_CHEATS must never be defined in a Release build.
#endif

#if DEBUG
            /// <summary> Show FPS on screen </summary>
            public const Boolean SHOW_FPS = true;
            /// <summary> Draws white pixels over the game objects </summary>
            public const Boolean SHOW_SPRITE_AREAS = false;
#else // #if DEBUG
            /// <summary> Show FPS on screen </summary>
            public const Boolean SHOW_FPS = false;
            /// <summary> Draws white pixels over the game objects </summary>
            public const Boolean SHOW_SPRITE_AREAS = false;
#endif // #if DEBUG

            /// <summary> How many levels the game has </summary>
            public const Int32 MAX_LEVEL = 20;

            /// <summary> The top of the screen </summary>
            public static readonly Single ScreenTop = Renderer.Instance.TargetDimensions.Y / 2;
            /// <summary> The right of the screen </summary>
            public static readonly Single ScreenRight = Renderer.Instance.TargetDimensions.X / 2;
            /// <summary> The left of the screen </summary>
            public static readonly Single ScreenLeft = -Renderer.Instance.TargetDimensions.X / 2;
            /// <summary> The bottom of the screen </summary>
            public static readonly Single ScreenBottom = -Renderer.Instance.TargetDimensions.Y / 2;
        }

        /// <summary>
        /// Game related leaderboards
        /// </summary>
        public static class Leaderboards
        {
            public static String[] GetAll()
            {
                return new String[]
                {
                    SINGLERUN_HIGHSCORE,
                    SINGLERUN_KILLS,
                    ALLTIME_SCORE,
                    ALLTIME_KILLS,
                };
            }

            private static readonly String SINGLERUN_HIGHSCORE = "CgkIqLOFpcMIEAIQCg";
            private static readonly String SINGLERUN_KILLS = "CgkIqLOFpcMIEAIQDQ";
            private static readonly String ALLTIME_SCORE = "CgkIqLOFpcMIEAIQDA";
            private static readonly String ALLTIME_KILLS = "CgkIqLOFpcMIEAIQCw";
        }

        /// <summary>
        /// Game achievements
        /// </summary>
        public static class GameAchievements
        {
            public static String[] GetAll()
            {
                return new String[]
                {
                    COMPLETE_LEVEL_3,
                    COMPLETE_LEVEL_5,
                    COMPLETE_LEVEL_7,
                    COMPLETE_LEVEL_10,
                    COMPLETE_LEVEL_13,
                    COMPLETE_LEVEL_15,
                    COMPLETE_LEVEL_17,
                    ALPHA_VICTOR,
                    BETA_VICTOR,
                    GAMMA_VICTOR,
                    PROTOTYPE,
                    REFLEXES,
                    SCORE_ONE_MILLION
                };
            }

            private static readonly String COMPLETE_LEVEL_3 = "CgkIqLOFpcMIEAIQAg";
            private static readonly String COMPLETE_LEVEL_5 = "CgkIqLOFpcMIEAIQCQ";
            private static readonly String COMPLETE_LEVEL_7 = "CgkIqLOFpcMIEAIQBA";
            private static readonly String COMPLETE_LEVEL_10 = "CgkIqLOFpcMIEAIQDw";
            private static readonly String COMPLETE_LEVEL_13 = "CgkIqLOFpcMIEAIQEA";
            private static readonly String COMPLETE_LEVEL_15 = "CgkIqLOFpcMIEAIQEg";
            private static readonly String COMPLETE_LEVEL_17 = "CgkIqLOFpcMIEAIQEQ";
            private static readonly String ALPHA_VICTOR = "CgkIqLOFpcMIEAIQBQ";
            private static readonly String BETA_VICTOR = "CgkIqLOFpcMIEAIQBg";
            private static readonly String GAMMA_VICTOR = "CgkIqLOFpcMIEAIQBw";
            private static readonly String PROTOTYPE = "CgkIqLOFpcMIEAIQCA";
            private static readonly String REFLEXES = "CgkIqLOFpcMIEAIQDg";
            private static readonly String SCORE_ONE_MILLION = "CgkIqLOFpcMIEAIQAw";
        }

        /// <summary>
        /// ZOrders for sprites
        /// </summary>
        public static class ZOrders
        {
            public const Int32 BACKGROUND = 50;

            public const Int32 CLUSTERS = 54;

            public const Int32 PLANETS_FAR = 55;

            public const Int32 PLANETS_NEAR = 56;

            public const Int32 NUKE_EFFECT = 57;

            /// <summary> Darkens the background behind menu text so it stays legible </summary>
            public const Int32 MENU_SCRIM = 58;

            public const Int32 BOSS_BASE = 59;

            public const Int32 BOSS_LOWER = 60;

            public const Int32 BULLETS = 78;

            public const Int32 POWERUPS = 79;

            public const Int32 BOSS_UPPER = 61;

            public const Int32 ENEMIES = 85;

            public const Int32 ENEMIES_OVERLAY = 86;

            /// <summary> Debris and sparks, above the enemies they came from and below the player </summary>
            public const Int32 PARTICLES = 87;

            /// <summary> Blasts running across a dying boss, above its hull and its debris </summary>
            public const Int32 BOSS_DEATH_BLAST = 88;

            public const Int32 PLAYER = 90;

            public const Int32 SHIELD = 95;

            /// <summary> Whites the field out. On the UI canvas so the shake cannot slide it
            /// off its own edges, and under <see cref="UI"/> so the HUD stays readable </summary>
            public const Int32 SCREEN_FLASH = 999;

            public const Int32 UI = 1000;

            public const Int32 UI_OVERLAY = 1001;

            public const Int32 ENGINE_HEAD = 1499;

            public const Int32 ENGINE_LOGO = 1500;

            public const Int32 INFO_SCREENS = 9999;

            public const Int32 ABOVE_GAME = 10000;
        }

        /// <summary>
        /// Fobt related functions
        /// </summary>
        public static class Font
        {
            /// <summary>
            /// Returns the dictionary for a textdisplay
            /// </summary>
            public static Dictionary<Char, String> Map
            {
                get
                {
                    var map = new Dictionary<Char, String>();
                    String alphaBet = "abcdefghijklmnopqrstuvwxyz";

                    for (Int32 i = 0; i < 10; i++)
                    {
                        map.Add(i.ToString()[0], i.ToString());
                    }

                    for (Int32 i = 0; i < alphaBet.Length; i++)
                    {
                        var c = alphaBet[i];
                        map.Add(c.ToString().ToUpperInvariant()[0], c.ToString().ToUpperInvariant());
                    }
                    map.Add(':', "colon");
                    map.Add('.', "dot");
                    map.Add('%', "percentage");
                    map.Add(' ', "space");

                    // Order is layout, not taste. TextDisplay.GenerateCharacterMap walks this
                    // dictionary and hands each entry the next cell of the atlas in turn, so a
                    // character's position here is what decides which glyph it draws — the names
                    // and the coordinates in KenPixel.json are not consulted. Anything new must
                    // therefore be appended, and its glyph drawn in the matching new cell at the
                    // end of the sheet. Inserting mid-list silently shifts every glyph after it.
                    map.Add('-', "hyphen");
                    return map;
                }
            }

        }


        //SpritesheetLoader.LoadSheet("Content/Graphics/", "NinjaData.png", "NinjaData.json");


        //AnimatedSprite test2 = new AnimatedSprite(canvas, 1, new[]
        //{
        //    Texture.GetTexture("Content/Graphics/floor_right.png"),
        //    Texture.GetTexture("Content/Graphics/floor_mid.png"),
        //    Texture.GetTexture("Content/Graphics/floor_left.png")
        //}, 6)
        //{
        //    Visible = true,
        //    Position = new Vector2(500, 0),
        //    Playing = true
        //};
    }
}
