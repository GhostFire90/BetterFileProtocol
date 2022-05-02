using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BetterFileProtocol;

namespace BFTPServer
{
    class Server
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 90);
            listener.Start();
            Task.Run(() => BFTP.Server(listener));
            Console.ReadKey();
        }
    }
}
