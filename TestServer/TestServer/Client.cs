﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class Client
    {
        public readonly int Id;
        public bool Report { get; set; }
        public bool Log { get; set; }
        public readonly TcpClient TcpClient;
        private readonly RequestProcessor requestProcessor;
        
        public Client(TcpClient cl, int count)
        {
            TcpClient = cl;
            Id = count;
            requestProcessor = new RequestProcessor();
        }

        public bool Handle(string request, out string answer)
        {
            return requestProcessor.HandleRequest(this, request, out answer);
        }

        public void HandleClient()
        {
            // sets two streams
            using (StreamWriter sWriter = new StreamWriter(TcpClient.GetStream(), Encoding.ASCII))
            using (StreamReader sReader = new StreamReader(TcpClient.GetStream(), Encoding.ASCII))
            {
                String data = null;

                while (true)
                {
                    try
                    {
                        Stopwatch watch = new Stopwatch();
                        // reads from stream
                        data = sReader.ReadLine();

                        string answer;
                        watch.Start();
                        bool success = Handle(data, out answer);
                        watch.Stop();
                        if (success)
                        {     
                            sWriter.WriteLine(answer);
                            sWriter.Flush();

                            if (Report)
                            {
                                sWriter.WriteLine("command " + data + " completed in " + watch.Elapsed.TotalMilliseconds +
                                                  " ms");
                                sWriter.Flush();
                            }

                            if (Log)
                            {
                                using (StreamWriter stream =
                                    new StreamWriter($"log of {Id} client.txt", true))
                                {
                                    stream.WriteLine(data);
                                }
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        break;
                    }
                }
            }
        }
    }
}

