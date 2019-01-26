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
        /// Invoked when a ship is selected
        /// </summary>
        /// <param name="id"> ID of the selected ship </param>
        void OnShipSelected(Int32 id);
    }
}
