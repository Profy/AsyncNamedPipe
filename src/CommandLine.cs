using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NamedPipes
{
    /// <summary>
    /// Command line argument specially designed to be sent through a pipe.
    /// </summary>
    public sealed class CommandLine
    {
        /// <summary>
        /// Name of the command to execute.
        /// </summary>
        public string Command { get; private set; } = string.Empty;
        /// <summary>
        /// Command line argument. Key is parameter, Value is value.
        /// </summary>
        public ReadOnlyDictionary<string, string> Args { get; private set; } = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        private CommandLine() { }
        /// <summary>
        /// Constructs a new command to send through a pipe.
        /// </summary>
        /// <param name="command">Name of the command to execute.</param>
        /// <param name="args">Command line argument. Key is parameter, Value is value.</param>
        /// <example>new Command("Execute", new Dictionary<string, string>() { { "debug", "false" } })</example>
        public CommandLine(string command, Dictionary<string, string> args = null)
        {
            Command = command;
            Args = args != null && args.Any() ? new ReadOnlyDictionary<string, string>(args) : null;
        }

        /// <summary>
        /// Convert this object to string.
        /// </summary>
        public override string ToString()
        {
            string utf8 = Command;
            if (Args != null && Args.Any())
            {
                foreach (KeyValuePair<string, string> arg in Args)
                {
                    utf8 += $" --{arg.Key} {arg.Value}";
                }
            }
            return utf8;
        }
        /// <summary>
        /// Convert this object to byte array.
        /// </summary>
        public byte[] ToByte()
        {
            string utf8 = Command;
            if (Args != null && Args.Any())
            {
                foreach (KeyValuePair<string, string> arg in Args)
                {
                    utf8 += $" {arg.Key} {arg.Value}";
                }
            }
            return Encoding.UTF8.GetBytes(utf8);
        }

        /// <summary>
        /// Convert byte array to <see cref="CommandLine"/>.
        /// </summary>
        public static CommandLine Parse(byte[] bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return Parse(Encoding.UTF8.GetString(bytes));
        }
        /// <summary>
        /// Convert byte array to <see cref="CommandLine"/>.
        /// </summary>
        public static CommandLine Parse(byte[] bytes, int index, int count)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return Parse(Encoding.UTF8.GetString(bytes, index, count));
        }

        /// <summary>
        /// Convert string to <see cref="CommandLine"/>.
        /// </summary>
        public static CommandLine Parse(string str)
        {
            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] spl = str.Split(' ');
            if (spl.Length < 2)
            {
                return new CommandLine(spl[0], null);
            }
            else
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                for (int i = 1; i < spl.Length; i += 2)
                {
                    args.Add(spl[i], spl[i + 1]);
                }
                return new CommandLine(spl[0], args);
            }
        }
    }
}
