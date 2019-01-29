using System;
using System.Collections.Generic;
using System.Text;
using Android.Gms.Ads;

namespace Type.Ads
{
    public class CustomAdListener : AdListener
    {
        public Action OnAdClosedAction { get; set; }

        public Action OnAdStartedAction { get; set; }

        public CustomAdListener()
        {
        }

        /// <inheritdoc />
        public override void OnAdFailedToLoad(Int32 errorCode)
        {
            base.OnAdFailedToLoad(errorCode);
        }

        /// <inheritdoc />
        public override void OnAdLoaded()
        {
            base.OnAdLoaded();
        }

        /// <inheritdoc />
        public override void OnAdOpened()
        {
            base.OnAdOpened();
            OnAdStartedAction?.Invoke();
        }

        /// <inheritdoc />
        public override void OnAdClosed()
        {
            base.OnAdClosed();
            OnAdClosedAction?.Invoke();
        }

        /// <inheritdoc />
        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);
            OnAdStartedAction = null;
            OnAdClosedAction = null;
        }
    }
}
