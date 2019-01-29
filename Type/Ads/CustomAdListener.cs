using System;
using System.Collections.Generic;
using System.Text;
using Android.Gms.Ads;

namespace Type.Ads
{
    public class CustomAdListener : AdListener
    {
        /// <inheritdoc />
        public override void OnAdFailedToLoad(Int32 errorCode)
        {
            base.OnAdFailedToLoad(errorCode);
            int i = 0;
            i++;
        }

        /// <inheritdoc />
        public override void OnAdLoaded()
        {
            base.OnAdLoaded();
            Int32 i = 0;
            i++;
        }

        /// <inheritdoc />
        public override void OnAdClosed()
        {
            base.OnAdClosed();
            Int32 i = 0;
            i++;
        }
    }
}
