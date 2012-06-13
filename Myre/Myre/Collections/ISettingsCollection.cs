using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Collections
{
    /// <summary>
    /// A collection of named values
    /// </summary>
    public interface ISettingsCollection
    {
        /// <summary>
        /// Add a new setting to this collection
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="name">The name of this setting</param>
        /// <param name="description">A description of what this setting is</param>
        /// <param name="defaultValue">The default value to give this setting</param>
        /// <returns>A box, which holds the value of this setting</returns>
        Box<T> Add<T>(string name, string description = null, T defaultValue = default(T));
    }
}
