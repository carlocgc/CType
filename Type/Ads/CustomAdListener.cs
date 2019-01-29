using System;
using System.Collections.Generic;
using System.Text;
using Android.Gms.Ads;

namespace Type.Ads
{
    /// <summary>
    /// Listenter object attached to advertisement objects that can react to ad events
    /// </summary>
    public class CustomAdListener : AdListener
    {
        /// <summary>
        /// Code to be executed when when the interstitial ad is closed.
        /// </summary>
        public Action OnAdClosedAction { get; set; }

        /// <summary>
        /// Code to be executed when the ad is clicked.
        /// </summary>
        public Action OnAdClickedAction { get; set; }

        /// <summary>
        /// Code to be executed when the user has left the app.
        /// </summary>
        public Action OnAdLeftApplicationAction { get; set; }

        public CustomAdListener()
        {
        }

        /// <inheritdoc />
        public override void OnAdClosed()
        {
            base.OnAdClosed();
            OnAdClosedAction?.Invoke();
        }

        /// <inheritdoc />
        public override void OnAdClicked()
        {
            base.OnAdClicked();
            OnAdClickedAction?.Invoke();
        }

        /// <inheritdoc />
        public override void OnAdLeftApplication()
        {
            base.OnAdLeftApplication();
            OnAdLeftApplicationAction?.Invoke();
        }

        /// <inheritdoc />
        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);
            OnAdClosedAction = null;
            OnAdClickedAction = null;
            OnAdLeftApplicationAction = null;
        }
    }
}
