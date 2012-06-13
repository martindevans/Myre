using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using Myre.Debugging;
using Myre.Extensions;

namespace Myre.Debugging
{
    public struct CommandResult
    {
        public object Result { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// A class which provides a command line interface into an assembly.
    /// </summary>
    public class CommandEngine
    {
        Dictionary<string, OptionInfo> options;
        Dictionary<string, CommandInfo> commands;
        string error;

        Dictionary<string, CommandHelpInfo> help;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandEngine"/> class.
        /// </summary>
        public CommandEngine()
            : this(Assembly.GetCallingAssembly())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandEngine"/> class.
        /// </summary>
        /// <param name="assembly">The assembly from which to find options and commands.</param>
        public CommandEngine(params Assembly[] assemblies)
        {
            options = new Dictionary<string, OptionInfo>();
            commands = new Dictionary<string, CommandInfo>();
            help = new Dictionary<string, CommandHelpInfo>();

            foreach (var assembly in assemblies)
                ScanAssembly(assembly);
        }

        private void ScanAssembly(Assembly assembly)
        {
            // search through all types in the assembly
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                //// find static properties with the CommandAttribute, add them to the options dictionary
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (var property in properties)
                {
                    var optionAttributes = property.GetCustomAttributes(typeof(CommandAttribute), false);
                    if (optionAttributes.Length > 0)
                    {
                        var optionAtt = optionAttributes[0] as CommandAttribute;
                        if (string.IsNullOrEmpty(optionAtt.Name))
                            optionAtt.Name = property.Name;
                        if (options.ContainsKey(optionAtt.Name))
                            throw new Exception(string.Format("Multiple options with the name {0} found. Each option must have a unique name.", optionAtt.Name));
                        options.Add(optionAtt.Name, new OptionInfo() { Attribute = optionAtt, Property = property });
                        help.Add(optionAtt.Name, new CommandHelpInfo()
                        {
                            Name = optionAtt.Name,
                            Description = optionAtt.Description,
                            Definition = string.Format("{0} {1} ({2})",
                                property.PropertyType,
                                optionAtt.Name,
                                GetOrSet(property))
                        });
                    }
                }

                // find static methods with the CommandAttribute, add them to the commands dictionary
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var commmandAttributes = method.GetCustomAttributes(typeof(CommandAttribute), false);
                    if (commmandAttributes.Length > 0)
                    {
                        var commandAtt = commmandAttributes[0] as CommandAttribute;
                        if (string.IsNullOrEmpty(commandAtt.Name))
                            commandAtt.Name = method.Name;
                        if (commands.ContainsKey(commandAtt.Name))
                            throw new Exception(string.Format("Multiple commands with the name {0} found. Each command must have a unique name.", commandAtt.Name));
                        commands.Add(commandAtt.Name, new CommandInfo() { Attribute = commandAtt, Command = method });
                        help.Add(commandAtt.Name, new CommandHelpInfo()
                        {
                            Name = commandAtt.Name,
                            Description = commandAtt.Description,
                            Definition = string.Format("{0} {1}({2})",
                                method.ReturnType,
                                commandAtt.Name,
                                ParametersDescription(method))
                        });
                    }
                }
            }
        }

        private object ParametersDescription(MethodInfo method)
        {
            string value = "";
            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                    value += ", ";
                value += string.Format("{0} {1}",
                    parameters[i].ParameterType,
                    parameters[i].Name);
            }
            return value;
        }

        private object GetOrSet(PropertyInfo property)
        {
            if (property.CanRead && property.CanWrite)
                return "read write";
            else if (property.CanRead)
                return "read";
            else if (property.CanWrite)
                return "write";
            else
                return "";
        }

        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <param name="target">The object containing the method.</param>
        /// <param name="methodName">Name of the method.</param>
        public void AddCommand(object target, string methodName)
        {
            AddCommand(target, methodName, methodName);
        }

        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <param name="target">The object containing the method.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="commandName">Name of the command.</param>
        public void AddCommand(object target, string methodName, string commandName, string description = null)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentException("methodName cannot be null or empty.");
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("commandName cannot be null or empty.");

            if (commands.ContainsKey(commandName) || options.ContainsKey(commandName))
                throw new InvalidOperationException(string.Format("A command or option of the name \"{0}\" has already been added.", commandName));

            var method = target.GetType().GetMethod(methodName);
            if (method == null)
                throw new ArgumentException(string.Format("{0} could not be found.", methodName));

            commands.Add(commandName, new CommandInfo()
            {
                Command = method,
                Attribute = new CommandAttribute() { Name = commandName },
                Target = target
            });

            help.Add(commandName, new CommandHelpInfo()
            {
                Name = commandName,
                Description = description,
                Definition = string.Format("{0} {1}({2})",
                    method.ReturnType,
                    commandName,
                    ParametersDescription(method))
            });
        }

        /// <summary>
        /// Removes the command.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        public void RemoveCommand(string commandName)
        {
            commands.Remove(commandName);
            help.Remove(commandName);
        }

        /// <summary>
        /// Adds the option.
        /// </summary>
        /// <param name="target">The object containing the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        public void AddOption(object target, string propertyName)
        {
            AddOption(target, propertyName, propertyName);
        }

        /// <summary>
        /// Adds the option.
        /// </summary>
        /// <param name="target">The object containing the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="optionName">Name of the option.</param>
        public void AddOption(object target, string propertyName, string optionName, string description = null, Action onSet = null)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("methodName cannot be null or empty.");
            if (string.IsNullOrEmpty(optionName))
                throw new ArgumentException("commandName cannot be null or empty.");

            if (commands.ContainsKey(optionName) || options.ContainsKey(optionName))
                throw new InvalidOperationException(string.Format("A command or option of the name \"{0}\" has already been added.", optionName));

            var property = target.GetType().GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException(string.Format("{0} could not be found.", propertyName));

            options.Add(optionName, new OptionInfo()
            {
                Property = property,
                Attribute = new CommandAttribute() { Name = optionName },
                Target = target,
                SetEvent = onSet
            });

            help.Add(optionName, new CommandHelpInfo()
            {
                Name = optionName,
                Description = description,
                Definition = string.Format("{0} {1} ({2})",
                    property.PropertyType,
                    optionName,
                    GetOrSet(property))
            });
        }

        public void RemoveOption(string optionName)
        {
            options.Remove(optionName);
            help.Remove(optionName);
        }

        #region Help
        /// <summary>
        /// Gets help information for a command or option.
        /// </summary>
        /// <param name="commandOrOption">The command or option.</param>
        /// <returns></returns>
        public CommandHelpInfo? GetIdentifierHelp(string commandOrOption)
        {
            if (help.ContainsKey(commandOrOption))
                return help[commandOrOption];
            else
                return null;
        }

        /// <summary>
        /// Gets the command and option help.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CommandHelpInfo> GetHelp()
        {
            return help.Values;
        }

        /// <summary>
        /// Gets intellisense-like help for a command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public CommandHelp GetHelp(string command)
        {
            CommandHelp help = new CommandHelp();
            help.Command = command;
            help.Definitions = "";
            help.Description = "";
            help.PossibleCommands = new string[0];

            string firstIdentifier;
            int equals = command.IndexOf('=');
            if (equals > 0)
            {
                firstIdentifier = command.Substring(0, equals).Trim();
                AppendDefinition(firstIdentifier, ref help);
                if (options.ContainsKey(firstIdentifier))
                    help.ValidType = options[firstIdentifier].Property.PropertyType.Name;
                command = command.Substring(equals + 1);
            }

            //if (command.Length > 0)
                GetExpressionHelp(command.Trim(), ref help);

            return help;
        }

        private void GetExpressionHelp(string command, ref CommandHelp help)
        {
            int paramStart = command.IndexOf('(');
            int paramEnd = command.IndexOf(')');

            if (paramStart == -1)
            {
                if (!AppendDefinition(command, ref help))
                {
                    string validType = help.ValidType;
                    help.PossibleCommands = this.help.Keys.Where(
                        n => n.StartsWith(command, StringComparison.InvariantCultureIgnoreCase)
                            && (validType == null || validType == GetReturnType(n))
                    ).ToArray();
                    help.TabStart = help.Command.LastIndexOf(command);
                    return;
                }
            }
            else
            {
                var def = command.Substring(0, paramStart);
                AppendDefinition(def, ref help);
                FindParameterHelp(def, command.Substring(paramStart), ref help);
            }
        }

        private string GetReturnType(string item)
        {
            if (options.ContainsKey(item))
                return options[item].Property.PropertyType.Name;
            else if (commands.ContainsKey(item))
                return commands[item].Command.ReturnType.Name;
            return null;
        }

        private void FindParameterHelp(string command, string parameters, ref CommandHelp help)
        {
            int brackets = 0;
            int paramStart = 1;
            int paramIndex = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                char c = parameters[i];
                if (c == '(')
                    brackets++;
                else if (c==')')
                    brackets--;
                else if (c == ',' && brackets == 1)
                {
                    paramStart = i + 1;
                    paramIndex++;
                }
            }

            if (brackets > 0)
            {
                if (commands.ContainsKey(command))
                {
                    var p = commands[command].Command.GetParameters();
                    if (p.Length > paramIndex)
                        help.ValidType = p[paramIndex].ParameterType.Name;
                    else
                        help.ValidType = "";
                }
                GetExpressionHelp(parameters.Substring(paramStart).Trim(), ref help);
            }
        }

        private bool AppendDefinition(string item, ref CommandHelp commandHelp)
        {
            if (help.ContainsKey(item))
            {
                var i = help[item];
                commandHelp.Definitions += i.Definition + "\n";
                commandHelp.Description = i.Description;
                if (options.ContainsKey(item))
                {
                    commandHelp.Description += string.Format("\n\"{0}\"", options[item].Property.GetValue(options[item].Target, null).ToString());
                    commandHelp.ValidType = "";
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Executing
        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public CommandResult Execute(string command)
        {
            error = null;
            object result = null;

            int equals = command.IndexOf('=');
            if (equals == -1)
            {
                // there is no '=', so we must (should be) dealing with a command.
                result = RunExpression(command);
            }
            else
            {
                // there is an '=', so we should be dealing with an option assignment.
                var optionName = command.Substring(0, equals).Trim();
                if (options.ContainsKey(optionName))
                {
                    var option = options[optionName];
                    if (option.Property.CanWrite)
                    {
                        var value = RunExpression(command.Substring(equals + 1));
                        try
                        {
                            option.Property.SetValue(options[optionName].Target, value, null);
                            if (option.SetEvent != null)
                                option.SetEvent();
                        }
                        catch (Exception e)
                        {
                            error = e.Message;
                        }
                    }
                    else
                        error = string.Format("Option \"{0}\" cannot be written to.", optionName);
                }
                else
                    error = string.Format("Option \"{0}\" cannot be found.", optionName);
            }

            if (error == null)
                return new CommandResult() { Result = result };
            else
                return new CommandResult() { Error = error };
        }

        private object RunExpression(string commandString)
        {
            // first check this isnt a string literal
            if (commandString.Length > 1 && commandString.StartsWith("\"") && commandString.EndsWith("\""))
                return commandString.Substring(1, commandString.Length - 2);

            // check this isnt a number or boolean
            int n;
            if (commandString.TryToInt(out n))
                return n;
            float f;
            if (commandString.TryToFloat(out f))
                return f;
            bool b;
            if (commandString.TryToBool(out b))
                return b;

            // look for some brackets
            int paramsStart = commandString.IndexOf('(');
            int paramsEnd = commandString.LastIndexOf(')');

            // break out of some error cases
            if ((paramsStart == -1 && paramsEnd != -1) || (paramsStart != -1 && paramsEnd == -1))
            {
                error = "Bracket pair invalid.";
                return null;
            }
            if (paramsStart > paramsEnd )
            {
                error = "')' found before '('.";
                return null;
            }

            // there are no brackets, so this isnt a command. Try to lookup an option value.
            if (paramsStart == -1)
                return GetOption(commandString.Trim());

            // get command name
            string commandName = commandString.Substring(0, paramsStart).Trim();
            if (!commands.ContainsKey(commandName))
            {
                error = string.Format("Command \"{0}\" not found.", commandName);
                return null;
            }

            // get command parameters
            var command = commands[commandName].Command;
            var numParameters = command.GetParameters().Length;
            string[] parameterStrings = SplitParameters(commandString.Substring(paramsStart + 1, paramsEnd - paramsStart - 1));
            if (numParameters != parameterStrings.Length)
            {
                error = string.Format("Command \"{0}\" has {1} parameters, but {2} were given.", commandName, numParameters, parameterStrings.Length);
                return null;
            }

            // create parameter values
            var parameterValues = new object[numParameters];
            for (int i = 0; i < numParameters; i++)
                parameterValues[i] = RunExpression(parameterStrings[i]);

            if (error != null)
                return null;

            try
            {
                return command.Invoke(commands[commandName].Target, parameterValues);
            }
            catch (Exception e)
            {
                error = string.Format("{0} error:\n{1}", commandName, e.Message);
                return null;
            }
        }

        private object GetOption(string optionName)
        {
            if (options.ContainsKey(optionName))
            {
                var option = options[optionName].Property;
                if (option.CanRead)
                    return options[optionName].Property.GetValue(null, null);
                else
                    error = string.Format("Option \"{0}\" cannot be read.", optionName);
            }
            else
                error = string.Format("Option \"{0}\" not found.", optionName);

            return null;
        }

        private string[] SplitParameters(string parametersString)
        {
            List<string> parameters = new List<string>();
            int start = 0;
            int bracketCount = 0;
            for (int i = 0; i < parametersString.Length; i++)
            {
                var character = parametersString[i];
                if (character == '(')
                    bracketCount++;
                else if (character == ')')
                    bracketCount--;
                else if (bracketCount == 0 && character == ',' && i - start > 0)
                {
                    parameters.Add(parametersString.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }

            if (start < parametersString.Length - 1)
                parameters.Add(parametersString.Substring(start));

            return parameters.ToArray();
        }
        #endregion
    }
}
