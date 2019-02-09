using AmosShared.Competitive;
using AmosShared.State;
using System;
using Type.States;

namespace Type.Android.States
{
    /// <summary>
    /// Makes a call to connect to google play and then ends, used to only make one attempt to connect automatically when the game launches
    /// </summary>
    public class GooglePlayConnectState : State
    {
        private Boolean _IsComplete;

        #region Overrides of State

        /// <summary> Function that is called when the state is entered </summary>
        protected override void OnEnter()
        {
            CompetitiveManager.Instance.Connect();
            _IsComplete = true;
        }

        /// <summary>If true then this state is considered complete and control will be passed over to <see cref="State.NextState"/></summary>
        /// <returns></returns>
        public override Boolean IsComplete()
        {
            if (_IsComplete) ChangeState(new MainMenuState());
            return _IsComplete;
        }

        /// <summary> Function that is called when the state is exited </summary>
        protected override void OnExit()
        {

        }

        #endregion
    }
}