using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Data;

namespace Type.UI
{
    public class StatsDisplay : IDisposable
    {
        private readonly List<TextDisplay> _Text;

        private readonly TextDisplay _ShotsFired;

        private readonly TextDisplay _ProbesCreated;

        private readonly TextDisplay _ShieldsCreated;

        private readonly TextDisplay _Deaths;

        private readonly TextDisplay _EnemiesKilled;

        private readonly TextDisplay _TimePlayed;

        public StatsDisplay()
        {
            _Text = new List<TextDisplay>();

            _Text.Add(_ShotsFired = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"SHOTS FIRED: {GameStats.Instance.BulletsFired}",
                Position = new Vector2(-660, -180),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            });
            _Text.Add(_ShotsFired = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"PROBES CREATED: {GameStats.Instance.ProbesCreated}",
                Position = new Vector2(-660, -230),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            });
            _Text.Add(_ShotsFired = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"SHIELDS CREATED: {GameStats.Instance.ShieldsCreated}",
                Position = new Vector2(-660, -280),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            });
            _Text.Add(_ShotsFired = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"DEATHS: {GameStats.Instance.Deaths}",
                Position = new Vector2(200, -180),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            });
            _Text.Add(_ShotsFired = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"ENEMIES KILLED: {GameStats.Instance.EnemiesKilled}",
                Position = new Vector2(200, -230),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            });
            _Text.Add(_ShotsFired = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"GAME TIME: {GameStats.Instance.PlayTime:hh\\:mm\\:ss}",
                Position = new Vector2(200, -280),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (TextDisplay display in _Text)
            {
                display.Dispose();
            }
        }
    }
}
