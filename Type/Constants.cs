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
            /// <summary> Show FPS on screen </summary>
            public const Boolean SHOW_FPS = true;
            /// <summary> Draws white pixels over the game objects </summary>
            public const Boolean SHOW_SPRITE_AREAS = false;

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

            public const Int32 BOSS_BASE = 59;

            public const Int32 BOSS_LOWER = 60;

            public const Int32 BULLETS = 78;

            public const Int32 POWERUPS = 79;

            public const Int32 BOSS_UPPER = 61;

            public const Int32 ENEMIES = 85;

            public const Int32 ENEMIES_OVERLAY = 86;

            public const Int32 PLAYER = 90;

            public const Int32 SHIELD = 95;

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
