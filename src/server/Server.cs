using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace filewatched
{
  public class Server
  {
     TcpListener server = null;
     int port;
     readonly ILogger _logger;
     CancellationToken cancellationToken;

    public Server(string ip, int port, ILogger logger, CancellationToken cancellationToken)
    {
        this._logger = logger;
        this.cancellationToken = cancellationToken;
        this.port = port;
        IPAddress localAddr = IPAddress.Parse(ip);
        server = new TcpListener(localAddr, port);
        server.Start();
        StartListener();
    }

    public void StartListener()
    {
        try
        {
            while ( ! cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Waiting for a connection on port {port}");
                TcpClient client = server.AcceptTcpClient();
                _logger.LogInformation("Connected to a client.");

                Thread t = new Thread(new ParameterizedThreadStart(HandleDeivce));
                t.Start(client);
            }
        }
        catch (SocketException e)
        {
            _logger.LogInformation($"SocketException: {e}");
            server.Stop();
        }
    }

    public void HandleDeivce(Object obj)
    {
        TcpClient client = (TcpClient)obj;
        var stream = client.GetStream();
        string imei = String.Empty;

        string data = null;
        Byte[] bytes = new Byte[256];
        int i;
        try
        {
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                string hex = BitConverter.ToString(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId); 

                string str = "Hey Device!";
                Byte[] reply = System.Text.Encoding.ASCII.GetBytes(str);   
                stream.Write(reply, 0, reply.Length);
                _logger.LogInformation($"{str}: Sent: {Thread.CurrentThread.ManagedThreadId}");
            }
        }
        catch(Exception e)
        {
            _logger.LogInformation($"Exception: {e.ToString()}");
            client.Close();
        }
    }
  }
}