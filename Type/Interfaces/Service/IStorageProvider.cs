using System;

namespace Type.Interfaces.Service
{
    /// <summary>
    /// Interface for a platform specific store of values that outlive a session, so shared code
    /// can save progress and settings without knowing where the platform keeps them.
    /// </summary>
    /// <remarks>
    /// Values are held as objects for the caller's convenience, but a provider is only required
    /// to round trip them as text: every consumer reads through <see cref="Convert"/> or
    /// <see cref="Object.ToString"/>, so a provider that stores "100" for the integer 100 is
    /// behaving correctly.
    /// </remarks>
    public interface IStorageProvider
    {
        /// <summary>
        /// Reads the stored values into memory. Called once during content loading, before
        /// anything asks for one.
        /// </summary>
        void Load();

        /// <summary>
        /// Reads one stored value
        /// </summary>
        /// <param name="key"> The key to read </param>
        /// <returns> The stored value, or null when nothing has been saved under that key </returns>
        Object GetValue(String key);

        /// <summary>
        /// Writes one value and saves the store
        /// </summary>
        /// <param name="key"> The key to write </param>
        /// <param name="value"> The value to store </param>
        void SetValue(String key, Object value);
    }
}
