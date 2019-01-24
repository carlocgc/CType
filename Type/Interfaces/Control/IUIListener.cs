using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Control
{
    public interface IUIListener : IAnalogListener, IFireButtonListener, IShieldButtonListener, IProbeButtonListener
    {
    }
}
