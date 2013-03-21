
namespace Myre.Debugging
{
    public struct CommandHelp
    {
        public string Command { get; internal set; }
        public string[] PossibleCommands { get; internal set; }
        public string Definitions { get; internal set; }
        public string Description { get; internal set; }
        public int TabStart { get; internal set; }
        internal string ValidType { get; set; }
    }

    /// <summary>
    /// A struct containing help information about a command or option.
    /// </summary>
    public struct CommandHelpInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        /// <value>The definition.</value>
        public string Definition { get; set; }
    }
}
