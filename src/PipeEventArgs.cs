using System;
using System.Text;

namespace NamedPipes
{
    /// <summary>
    /// Data received event.
    /// </summary>
    public sealed class PipeMessage<T>
    {
        /// <summary>
        /// Message just received.
        /// </summary>
        public T Message { get; private set; }

        /// <summary>
        /// Data received as a byte array.
        /// </summary>
        internal PipeMessage(byte[] data)
        {
            if (typeof(T) == typeof(byte[]))
            {
                Message = (T)(object)data;
            }
            else if (typeof(T) == typeof(string))
            {
                Message = (T)(object)Encoding.UTF8.GetString(data).TrimEnd('\0');
            }
            else if (typeof(T) == typeof(CommandLine))
            {
                Message = (T)(object)CommandLine.Parse(data);
            }
            else
            {
                throw new NotSupportedException($"{typeof(T)} not supported. Supported type: {typeof(string)} {typeof(byte[])} {typeof(CommandLine)}");
            }
        }
    }
}