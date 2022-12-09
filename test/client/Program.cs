using NamedPipes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

internal class Program
{
    private const int nbClient = 4;

    private static void Main()
    {
        TestClient<byte[]> testConnection = new TestClient<byte[]>();
        for (int i = 0; i < nbClient; i++)
        {
            testConnection.CreateClient(i);
            Thread.Sleep(10);
        }
        while (testConnection.clientPipes.Any(p => p.IsConnected)) { Thread.Sleep(50); }
        Thread.Sleep(500);

        // ---------------------

        TestClient<CommandLine> testCommand = new TestClient<CommandLine>();
        for (int i = 0; i < nbClient; i++)
        {
            testCommand.CreateClient(i);
            Thread.Sleep(10);
        }
        while (testCommand.clientPipes.Any(p => p.IsConnected)) { Thread.Sleep(50); }
        Thread.Sleep(500);

        // ---------------------

        TestClient<string> testString = new TestClient<string>();
        for (int i = 0; i < nbClient; i++)
        {
            testString.CreateClient(i);
            Thread.Sleep(10);
        }
        while (testString.clientPipes.Any(p => p.IsConnected)) { Thread.Sleep(50); }
        Thread.Sleep(500);

        // ---------------------

        TestClient<byte[]> testBytes = new TestClient<byte[]>();
        for (int i = 0; i < nbClient; i++)
        {
            testBytes.CreateClient(i);
            Thread.Sleep(10);
        }
        while (testBytes.clientPipes.Any(p => p.IsConnected)) { Thread.Sleep(50); }
        Thread.Sleep(500);

        // ---------------------

        Console.ReadKey();
    }

    public class TestClient<T>
    {
        public List<PipeClient<T>> clientPipes = new List<PipeClient<T>>(nbClient);

        public void CreateClient(int id)
        {
            PipeClient<T> client = new PipeClient<T>(".", "Test", p => p.StartReaderAsync());
            clientPipes.Add(client);

            client.Connected += (sndr, args) => Console.WriteLine($"Client {typeof(T)} connected with id: {id}");
            client.DataReceived += (sndr, args) =>
            {
                Console.WriteLine($"{args.Message} received by client {id}");
                client.Close();
            };
            client.Closed += (sndr, args) =>
            {
                Console.WriteLine($"Client {typeof(T)} disconnected with id: {id}");
            };

            client.Connect();
        }
    }
}