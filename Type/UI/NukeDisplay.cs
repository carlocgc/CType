using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Base;

namespace Type.UI
{
    /// <summary> Displays the amount of nukes the player has </summary>
    public class NukeDisplay : GameObject
    {
        /// <summary> Text that displays the count of nukes </summary>
        private readonly TextDisplay _Count;

        private Int32 _NukeCount;

        #region Overrides of GameObject

        /// <summary> Position of the object </summary>
        public override Vector2 Position
        {
            get => _Position;
            set
            {
                _Position = value;
                _Count.Position = _Position;
            }
        }

        #endregion

        /// <summary> The number of nukes to display </summary>
        public Int32 NukeCount
        {
            get => _NukeCount;
            set
            {
                _NukeCount = value;
                _Count.Text = $"BOMBS:{NukeCount}";
            }
        }

        public NukeDisplay()
        {
            _Count = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"BOMBS:{NukeCount}",
                Scale = new Vector2(2f, 2f),
                Visible = true,
                Position = new Vector2(-800, 350)
            };
            _Count.Offset = new Vector2(_Count.Size.X * _Count.Scale.X, _Count.Size.Y * _Count.Scale.Y) / 2;
            NukeCount = 0;
        }

        #region Overrides of GameObject

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _Count.Dispose();
        }

        #endregion
    }
}
