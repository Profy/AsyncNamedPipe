using System;
using System.IO.Pipes;

namespace NamedPipes
{
    /// <summary>
    /// Named pipe server side.
    /// </summary>
    /// <remarks>
    /// <b>Warning</b>
    /// <br>Due to the asynchronicity of the pipe, this object <b>must not be declared or used in a MonoBehaviour</b>.
    /// There is a risk of having unexpected behavior due to how Unity execution order works.</br>
    ///</remarks>
    /// <example>
    /// Server server = new Server("example", p => p.StartStringReaderAsync());
    /// server.Connected += (sndr, args) => Console.WriteLine("Client connected");
    /// </example>
    public sealed class PipeServer<T> : PipeBasic<T>
    {
        /// <summary>
        /// Named pipe server side.
        /// </summary>
        private readonly NamedPipeServerStream _serverPipeStream;

        /// <summary>
        /// Constructs a new server instance.
        /// </summary>
        /// <param name="pipeName">The name of the pipe.</param>
        /// <param name="asyncReader">Asynchronous reader. Use <see cref="PipeBasic{T}.StartReaderAsync"/> for self implementation.</param>
        /// <param name="maxAllowedServerInstances">Represents the maximum number of server instances that the system resources allow.</param>
        public PipeServer(string pipeName, Action<PipeBasic<T>> asyncReader, int maxAllowedServerInstances = -1) : base(pipeName, asyncReader)
        {
            _serverPipeStream = new NamedPipeServerStream($"{pipeName}_{typeof(T)}", PipeDirection.InOut, maxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            pipeStream = _serverPipeStream;
            _serverPipeStream.BeginWaitForConnection(new AsyncCallback(Connect), null);
        }

        /// <summary>
        /// Accept connection from a client.
        /// </summary>
        private void Connect(IAsyncResult ar)
        {
            _serverPipeStream.EndWaitForConnection(ar);
            OnConnected(EventArgs.Empty);
            asyncReaderStart(this);
        }
    }
}