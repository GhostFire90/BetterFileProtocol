using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterFileProtocol;
using System.Net.Sockets;
using System.Net;

namespace BFTPClient
{
    class Client
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 90));
            BFTP.Send(client.GetStream(), "stairs.png");

            BFTP.Recieve(client.GetStream());
            


        }
    }
}
