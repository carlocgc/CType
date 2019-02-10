using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// Interface for objects that listen to the nuke button
    /// </summary>
    public interface INukeButtonListener
    {
        /// <summary> Invoked when the nuke button is pressed </summary>
        void OnNukeButtonPressed();
    }
}
