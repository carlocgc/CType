using System;
using System.Collections.Generic;
using System.Text;

namespace Type
{
    /// <summary>
    /// Constant values
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// ZOrders for sprites
        /// </summary>
        public static class ZOrders
        {
            public const Int32 BACKGROUND = 0;

            public const Int32 BULLETS = 1;

            public const Int32 ENEMIES = 2;

            public const Int32 PLAYER = 3;

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
                    for (Int32 i = 0; i < alphaBet.Length; i++)
                    {
                        var c = alphaBet[i];
                        map.Add(c.ToString().ToUpperInvariant()[0], c.ToString().ToUpperInvariant());
                        if (i > 9) continue;
                        map.Add(i.ToString()[0], i.ToString());
                    }
                    map.Add('.', "dot");
                    map.Add(':', "colon");
                    map.Add(' ', "space");
                    map.Add('%', "percentage");
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
