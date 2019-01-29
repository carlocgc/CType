using AmosShared.State;
using System;
using Type.Scenes;
using Android.Gms.Ads;
using Java.Nio.Channels;

namespace Type.States
{
    public class GameOverState : State
    {
        private GameOverScene _Scene;

        protected override void OnEnter()
        {
            _Scene = new GameOverScene { Visible = true };

            Game.MInterstitialAd.RewardedVideoAdClosed += MInterstitialAdOnRewardedVideoAdClosed;

            if (Game.MInterstitialAd.IsLoaded)
            {
                Game.MInterstitialAd.Show();
            }
        }

        private void MInterstitialAdOnRewardedVideoAdClosed(Object sender, EventArgs eventArgs)
        {
            _Scene.Start();
        }

        public override Boolean IsComplete()
        {
            if (_Scene.IsComplete) ChangeState(new MainMenuState());
            return _Scene.IsComplete;
        }

        protected override void OnExit()
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _Scene.Dispose();
        }
    }
}
