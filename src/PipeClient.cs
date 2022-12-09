using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace NamedPipes
{
    /// <summary>
    /// Named pipe client side.
    /// </summary>
    /// <remarks>
    /// <b>Warning</b>
    /// <br>Due to the asynchronicity of the pipe, this object <b>must not be declared or used in a MonoBehaviour</b>.
    /// There is a risk of having unexpected behavior due to how Unity execution order works.</br>
    ///</remarks>
    /// <example>
    /// Client client = new Client(".", "example", p => p.StartStringReaderAsync());
    /// client.DataReceived += (sndr, args) => Console.WriteLine(args.String);
    /// </example>
    public sealed class PipeClient<T> : PipeBasic<T>
    {
        /// <summary>
        /// Named pipe client side.
        /// </summary>
        private readonly NamedPipeClientStream _clientPipeStream;

        /// <summary>
        /// Constructs a new client instance.
        /// </summary>
        /// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
        /// <param name="pipeName">The name of the pipe.</param>
        /// <param name="asyncReader">Asynchronous reader. Use <see cref="PipeBasic{T}.StartReaderAsync"/> for self implementation.</param>
        public PipeClient(string serverName, string pipeName, Action<PipeBasic<T>> asyncReader) : base(pipeName, asyncReader)
        {
            _clientPipeStream = new NamedPipeClientStream(serverName, $"{pipeName}_{typeof(T)}", PipeDirection.InOut, PipeOptions.Asynchronous);
            pipeStream = _clientPipeStream;
        }

        /// <summary>
        /// Connects to a waiting server.
        /// </summary>
        public void Connect()
        {
            _clientPipeStream.Connect();

            OnConnected(EventArgs.Empty);
            asyncReaderStart(this);
        }
        /// <summary>
        /// Connects to a waiting server.
        /// </summary>
        /// <param name="timeout">The amount of time to wait for the server to respond before the connection times out.</param>
        public void Connect(int timeout)
        {
            _clientPipeStream.Connect(timeout);

            OnConnected(EventArgs.Empty);
            asyncReaderStart(this);
        }

        /// <summary>
        /// Connects to a waiting server.
        /// </summary>
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            await _clientPipeStream.ConnectAsync(ct);

            OnConnected(EventArgs.Empty);
            asyncReaderStart(this);
        }
        /// <summary>
        /// Connects to a waiting server.
        /// </summary>
        /// <param name="timeout">The amount of time to wait for the server to respond before the connection times out.</param>
        public async Task ConnectAsync(int timeout, CancellationToken ct = default)
        {
            await _clientPipeStream.ConnectAsync(timeout, ct);

            OnConnected(EventArgs.Empty);
            asyncReaderStart(this);
        }
    }
}
