using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NamedPipes
{
    /// <summary>
    /// Named pipe base class. <see cref="PipeServer{T}"/> and <see cref="PipeClient{T}"/> inherit from this class.
    /// </summary>
    public abstract class PipeBasic<T>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected PipeStream pipeStream;
        /// <summary>
        /// Start listening the pipe.
        /// </summary>
        protected Action<PipeBasic<T>> asyncReaderStart;

        /// <summary>
        /// Use to cancel asynchronous reader on Close.
        /// </summary>
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        /// <summary>
        /// Event trigger when data is received.
        /// </summary>
        public event EventHandler<PipeMessage<T>> DataReceived;

        /// <summary>
        /// Event trigger when a pipe is connected on any side.
        /// </summary>
        public event EventHandler<EventArgs> Connected;
        /// <summary>
        /// Invoke <see cref="Connected"/>
        /// </summary>
        protected virtual void OnConnected(EventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        /// <summary>
        /// Event trigger when a pipe is close on any side.
        /// </summary>
        public event EventHandler<EventArgs> Closed;
        /// <summary>
        /// Invoke <see cref="Closed"/>
        /// </summary>
        protected virtual void OnClosed(EventArgs e)
        {
            Closed?.Invoke(this, e);
        }

        /// <summary>
        /// The name of the pipe.
        /// </summary>
        public string PipeName { get; protected set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether a PipeStream object is connected.
        /// </summary>
        public bool IsConnected => pipeStream.IsConnected;

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="pipeName">The name of the pipe.</param>
        /// <param name="asyncReader">Asynchronous reader. Use <see cref="PipeBasic{T}.StartReaderAsync"/> for self implementation.</param>
        protected PipeBasic(string pipeName, Action<PipeBasic<T>> asyncReader)
        {
            PipeName = pipeName;
            asyncReaderStart = asyncReader;

            Closed += (sndr, args) => OnClose();
        }

        /// <summary>
        /// Reads an array of bytes, where the first [n] bytes (based on the server's intsize) indicates the number of bytes to read to complete the packet.
        /// </summary>
        public void StartReaderAsync()
        {
            StartReaderAsync((b) => DataReceived?.Invoke(this, new PipeMessage<T>(b)), _cancellationToken.Token);
        }
        /// <summary>
        /// Reads an array of bytes, where the first [n] bytes (based on the server's intsize) indicates the number of bytes to read to complete the packet.
        /// </summary>
        protected void StartReaderAsync(Action<byte[]> packet, CancellationToken ct)
        {
            int intSize = sizeof(int);
            byte[] bDataLength = new byte[intSize];

            try
            {
                // Message length
                _ = pipeStream.ReadAsync(bDataLength, 0, intSize, ct).ContinueWith(t =>
                {
                    int len = t.Result;

                    if (len == 0)
                    {
                        Closed?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        int dataLength = BitConverter.ToInt32(bDataLength, 0);
                        byte[] data = new byte[dataLength];

                        // Read message
                        _ = pipeStream.ReadAsync(data, 0, dataLength, ct).ContinueWith(t2 =>
                        {
                            len = t2.Result;

                            if (len == 0)
                            {
                                Closed?.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                packet(data);
                                StartReaderAsync(packet, ct);
                            }
                        }, ct);
                    }
                }, ct);
            }
            catch
            {
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Writes a data stream to the pipe.
        /// </summary>
        public Task Write(T data)
        {
            if (typeof(T) == typeof(byte[]))
            {
                return !(data is byte[] d) ? throw new ArgumentNullException(nameof(data)) : WriteBytes(d);
            }
            else if (typeof(T) == typeof(string))
            {
                return !(data is string d) ? throw new ArgumentNullException(nameof(data)) : WriteBytes(Encoding.UTF8.GetBytes(d));
            }
            else if (typeof(T) == typeof(CommandLine))
            {
                return !(data is CommandLine d) ? throw new ArgumentNullException(nameof(data)) : WriteBytes(d.ToByte());
            }
            else
            {
                throw new NotSupportedException($"{typeof(T)} not supported. Supported type: {typeof(string)} {typeof(byte[])} {typeof(CommandLine)}");
            }
        }
        /// <summary>
        /// Writes a bytes stream to the pipe.
        /// </summary>
        public Task WriteBytes(byte[] bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            // First [n] bytes (based on the server's intsize) indicates the number of bytes to read to complete the packet.
            byte[] size = BitConverter.GetBytes(bytes.Length);
            byte[] stream = new byte[size.Length + bytes.Length];

            Buffer.BlockCopy(size, 0, stream, 0, size.Length);
            Buffer.BlockCopy(bytes, 0, stream, size.Length, bytes.Length);

            return pipeStream.WriteAsync(stream, 0, stream.Length);
        }

        /// <summary>
        /// Flush this pipe stream.
        /// </summary>
        public void Flush()
        {
            pipeStream.Flush();
        }
        /// <summary>
        /// Close this pipe stream.
        /// </summary>
        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Close this pipe stream.
        /// </summary>
        protected void OnClose()
        {
            _cancellationToken.Cancel();

            pipeStream.Close();
            pipeStream.Dispose();
        }
    }
}