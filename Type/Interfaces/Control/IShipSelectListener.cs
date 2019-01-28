using System;
using System.Collections.Generic;
using System.Text;
using Type.UI;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// Interface for objects that listen to <see cref="ShipSelectButton"/>
    /// </summary>
    public interface IShipSelectListener
    {
        /// <summary>
        /// Invoked when a ship select buton is pressed
        /// </summary>
        /// <param name="id"> ID of the selected ship </param>
        void OnButtonPressed(Int32 id);

        /// <summary>
        /// Invoked when a ship select button is released
        /// </summary>
        /// <param name="id"></param>
        void OnButtonReleased(Int32 id);
    }
}
