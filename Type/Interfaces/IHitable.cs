using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Type.Interfaces
{
    /// <summary> Interface for objects that can be hit </summary>
    public interface IHitable
    {
        /// <summary>
        /// The hitpoints of the <see cref="IHitable"/>
        /// </summary>
        Int32 HitPoints { get; }

        /// <summary>
        /// Hit the <see cref="IHitable"/>
        /// </summary>
        void Hit();
    }
}
