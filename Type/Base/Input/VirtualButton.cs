using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using System;
using Type.Data;
using Type.Interfaces.Control;

namespace Type.Base.Input
{
    /// <summary>
    /// Button extension that adds public facing data about its state
    /// </summary>
    public class VirtualButton : Button, IVirtualButton, IDisposable
    {
        #region Implementation of IVirtualButton

        /// <summary> Unique id of the button </summary>
        public ButtonData.Type ID { get; }

        /// <summary> The current press state of the button </summary>
        public ButtonData.State State { get; set; }

        #endregion

        public VirtualButton(Int32 touchOrder, Sprite sprite, ButtonData.Type id) : base(touchOrder, sprite)
        {
            ID = id;
            OnButtonPress = OnPress;
            OnButtonRelease = OnRelease;
        }

        private void OnPress(Button obj)
        {
            if (State == ButtonData.State.PRESSED || State == ButtonData.State.HELD) State = ButtonData.State.HELD;
            else State = ButtonData.State.PRESSED;
        }

        private void OnRelease(Button obj)
        {
            State = ButtonData.State.RELEASED;
        }
    }
}
