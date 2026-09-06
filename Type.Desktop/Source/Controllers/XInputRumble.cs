using System;
using System.Runtime.InteropServices;

namespace Type.Desktop.Source.Controllers
{
    /// <summary>
    /// Drives a controller's rumble motors through XInput.
    /// </summary>
    /// <remarks>
    /// **OpenTK's <c>GamePad.SetVibration</c> does nothing on Windows.** Its Windows gamepad
    /// driver leaves the method unimplemented and returns false, which is what it did here on a
    /// properly mapped Xbox pad — so rumble had never worked, before or after being wired up to
    /// death and nuke. The same pad accepts vibration through XInput directly.
    /// <para>
    /// Not a new dependency: <c>xinput</c> is a Windows system library, the same one OpenTK
    /// would be calling if it implemented this. Three versions ship with different Windows
    /// releases, so the first one that loads is used and remembered.
    /// </para>
    /// <para>
    /// The pad is found through XInput's own slots rather than OpenTK's, because the two do not
    /// correspond: this machine reports a second, phantom controller that OpenTK sees and XInput
    /// does not, so a rumble sent to OpenTK's index could be sent to a device that cannot rumble.
    /// XInput only enumerates XInput devices, which is exactly the set that can.
    /// </para>
    /// </remarks>
    internal sealed class XInputRumble
    {
        /// <summary> What XInput returns when the call was accepted </summary>
        private const Int32 Success = 0;

        /// <summary> How many controller slots XInput exposes </summary>
        private const Int32 Users = 4;

        /// <summary> Motor speed at full strength </summary>
        private const Single MaxSpeed = 65535f;

        /// <summary> The motor speeds, as XInput takes them </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Vibration
        {
            /// <summary> Speed of the heavy, low frequency motor </summary>
            public UInt16 LeftMotorSpeed;
            /// <summary> Speed of the light, high frequency motor </summary>
            public UInt16 RightMotorSpeed;
        }

        /// <summary> One version of the XInput entry point </summary>
        private delegate Int32 SetStateCall(Int32 user, ref Vibration vibration);

        [DllImport("xinput1_4.dll", EntryPoint = "XInputSetState")]
        private static extern Int32 SetState14(Int32 user, ref Vibration vibration);

        [DllImport("xinput1_3.dll", EntryPoint = "XInputSetState")]
        private static extern Int32 SetState13(Int32 user, ref Vibration vibration);

        [DllImport("xinput9_1_0.dll", EntryPoint = "XInputSetState")]
        private static extern Int32 SetState910(Int32 user, ref Vibration vibration);

        /// <summary> The version of the entry point this machine has, null when it has none </summary>
        private SetStateCall _SetState;

        /// <summary> Whether the versions have been tried yet </summary>
        private Boolean _Probed;

        /// <summary> The slot the controller answered on last, negative when none has </summary>
        private Int32 _User = -1;

        /// <summary>
        /// Sets both motors of the connected controller
        /// </summary>
        /// <param name="strength"> How hard, from 0 to 1, where 0 stops them </param>
        /// <remarks>
        /// Both motors are driven equally. They differ in weight, so the same speed does not feel
        /// the same on each, but the previous implementation drove them equally at full strength
        /// too and nothing has been felt yet that would justify inventing a ratio.
        /// </remarks>
        public void Set(Single strength)
        {
            SetStateCall setState = Resolve();
            if (setState == null) return;

            UInt16 speed = Speed(strength);
            Vibration vibration = new Vibration { LeftMotorSpeed = speed, RightMotorSpeed = speed };

            // The slot that answered last is tried first. This runs from the update loop, and a
            // slot with nothing in it costs a device lookup to say so.
            if (_User >= 0 && setState(_User, ref vibration) == Success) return;

            for (Int32 user = 0; user < Users; user++)
            {
                if (setState(user, ref vibration) != Success) continue;

                _User = user;
                return;
            }

            _User = -1;
        }

        /// <summary>
        /// Finds the version of XInput this machine has, once
        /// </summary>
        /// <returns> The entry point to call, or null if none of them loaded </returns>
        private SetStateCall Resolve()
        {
            if (_Probed) return _SetState;
            _Probed = true;

            Vibration silent = new Vibration();

            foreach (SetStateCall candidate in new SetStateCall[] { SetState14, SetState13, SetState910 })
            {
                try
                {
                    // The return value does not matter here. What is being established is that
                    // the library is present and the entry point resolves; an empty slot answers
                    // perfectly well, it just answers that there is nothing in it.
                    candidate(0, ref silent);

                    _SetState = candidate;
                    break;
                }
                catch (DllNotFoundException)
                {
                    // An older Windows without this version of the library. Try the next.
                }
                catch (EntryPointNotFoundException)
                {
                    // A library of that name that is not the one meant. Try the next.
                }
            }

            return _SetState;
        }

        /// <summary>
        /// Converts a strength to a motor speed
        /// </summary>
        private static UInt16 Speed(Single strength)
        {
            if (strength <= 0) return 0;

            return strength >= 1 ? (UInt16)MaxSpeed : (UInt16)(strength * MaxSpeed);
        }
    }
}
