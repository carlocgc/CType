using System;

namespace Type.Interfaces.GameData
{
    /// <summary> Interface for objects that have a points value </summary>
    public interface IPoints
    {
        /// <summary> Amount of points this object is worth </summary>
        Int32 Points { get; }
    }
}
