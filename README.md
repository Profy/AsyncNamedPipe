# Async Named Pipe

A .NET standard 2.1 library for an easy implementation of asynchronous named pipes (Interprocess Communications) with generic type.

Any help or suggestion is welcome.

## Features
- Create a Pipe server
- Create a Pipe client
- Asynchronous read/write
- Support pipe event (connected, received, closed)

## Usage
#### Create a pipe
    PipeServer<string> server = new PipeServer<string>("Test", p => p.StartReaderAsync());
#### Pipe events
    server.Connected += (sndr, args) =>
    {
    	Console.WriteLine("Client is connected");
    };
    server.DataReceived += (sndr, args) =>
    {
        Console.WriteLine("Client say: {args.Message}");
    };
    server.Closed += (sndr, args) =>
    {
        Console.WriteLine("The pipe has been closed");
    };

## Note
Although it is in .net standard 2.1, <b>this project is not compatible with Unity compiled in IL2CPP</b>. The managed method for creating pipes isn't supported and it is not possible to use MonoPInvokeCallback on it.

This project is based on the code of [Marc Clifton](https://github.com/cliftonm).
