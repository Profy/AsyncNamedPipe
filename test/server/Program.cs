using NamedPipes;

using System;
using System.Collections.Generic;
using System.Text;

internal class Program
{
    private static string? message;

    private static void Main()
    {
        Console.WriteLine("Test connection and server disconnection request. Type enter to continue");
        TestServer<byte[]> testConnection = new TestServer<byte[]>();
        testConnection.CreateServer();

        message = Console.ReadLine();
        for (int i = 0; i < testConnection.serverPipes.Count; i++)
        {
            Console.WriteLine($"Disconnect to {i}");
            testConnection.serverPipes[i].Close();
        }

        // ---------------------

        Console.WriteLine("Write a message to send to all clients. Test CommandLine type");
        TestServer<CommandLine> testCommand = new TestServer<CommandLine>();
        testCommand.CreateServer();

        message = Console.ReadLine();
        for (int i = 0; i < testCommand.serverPipes.Count; i++)
        {
            Console.WriteLine($"Send {message!} to {i}");
            testCommand.serverPipes[i].Write(new CommandLine(message!, new Dictionary<string, string>() { { "key", "value" } }));
        }

        // ---------------------

        Console.WriteLine("Write a message to send to all clients. Test string type");
        TestServer<string> testString = new TestServer<string>();
        testString.CreateServer();

        message = Console.ReadLine();
        for (int i = 0; i < testString.serverPipes.Count; i++)
        {
            Console.WriteLine($"Send {message!} to {i}");
            testString.serverPipes[i].Write($"Hello {i}: {message!}");
        }

        // ---------------------

        Console.WriteLine("Write a message to send to all clients. Test byte[] type");
        TestServer<byte[]> testBytes = new TestServer<byte[]>();
        testBytes.CreateServer();

        message = Console.ReadLine();
        for (int i = 0; i < testBytes.serverPipes.Count; i++)
        {
            Console.WriteLine($"Send {message!} to {i}");
            testBytes.serverPipes[i].Write(Encoding.UTF8.GetBytes(message!));
        }

        // ---------------------

        Console.ReadKey();
    }

    public class TestServer<T>
    {
        public List<PipeServer<T>> serverPipes = new List<PipeServer<T>>();

        public void CreateServer()
        {
            int clientID = serverPipes.Count;
            PipeServer<T> server = new PipeServer<T>("Test", p => p.StartReaderAsync());

            server.Connected += (sndr, args) =>
            {
                Console.WriteLine($"Client {typeof(T)} {clientID} is connected");
                serverPipes.Add(server);
                CreateServer(); // Prepare next connection
            };
            server.DataReceived += (sndr, args) => Console.WriteLine($"{clientID} say: {args.Message}");
            server.Closed += (sndr, args) =>
            {
                Console.WriteLine($"Client {typeof(T)} {clientID} have disconnect");
                serverPipes.Remove(server);
            };
        }
    }
}