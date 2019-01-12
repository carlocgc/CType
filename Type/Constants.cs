using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

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

            public const Int32 BULLETS = 78;

            public const Int32 ENEMIES = 79;

            public const Int32 PLAYER = 90;

            public const Int32 UI = 100;
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
