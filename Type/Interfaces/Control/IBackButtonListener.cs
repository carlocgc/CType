using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Control
{
    /// <summary> Interfaces for objects listening to back button presses </summary>
    public interface IBackButtonListener
    {
        /// <summary> Invoked when the back button is pressed </summary>
        void OnBackPressed();
    }
}
