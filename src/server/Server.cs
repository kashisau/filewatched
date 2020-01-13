using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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

     List<string> files;

    public Server(string ip, int port, ILogger logger, List<string> files, CancellationToken cancellationToken)
    {
        this._logger = logger;
        this.cancellationToken = cancellationToken;
        this.port = port;
        this.files = files;
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
                _logger.LogInformation($"Client connected. ID: {data}");

                using (MemoryStream fileListMs = new MemoryStream())
                {
                  BinaryFormatter bf = new BinaryFormatter();
                  bf.Serialize(fileListMs, files);
                  using (GZipStream gzFiles = new GZipStream(fileListMs, CompressionLevel.Optimal))
                  {
                    gzFiles.CopyTo(stream);
                    gzFiles.Close();
                  }
                  fileListMs.Close();
                }
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