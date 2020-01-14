using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace filewatched
{
  public class Server
  {
     TcpListener server;
     IPAddress localAddr;
     int port;
     readonly ILogger _logger;
     CancellationToken cancellationToken;
     
     BackgroundWorker listeningServerWorker;

     List<string> files;

    public Server(string ip, int port, ILogger logger, List<string> files)
    {
        this._logger = logger;
        this.port = port;
        this.files = files;
        this.localAddr = IPAddress.Parse(ip);
        this.server = new TcpListener(localAddr, port);
    }

    public async Task StartListener(CancellationToken cancellationToken)
    {
        server.Start();
        this.cancellationToken = cancellationToken;
        listeningServerWorker = new BackgroundWorker();
        listeningServerWorker.WorkerReportsProgress = false;
        listeningServerWorker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => files = (List<string>) e.Result;
        listeningServerWorker.DoWork += Scan;
        listeningServerWorker.RunWorkerAsync();

        while ( ! cancellationToken.IsCancellationRequested) {
            await Task.Delay(100);
        }
        listeningServerWorker.CancelAsync();
        listeningServerWorker.Dispose();
    }

    private void Scan(object sender, DoWorkEventArgs eventArgs)
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

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, files);
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