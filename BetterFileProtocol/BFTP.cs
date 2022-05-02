using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;


namespace BetterFileProtocol
{
    public static class BFTP
    {
        public static void Send(NetworkStream stream, string path, int chunkSize=256)
        {
            Aes aes = new Aes();

            using(FileStream fs = File.OpenRead(path))
            using(BufferedStream bs = new BufferedStream(fs))
            {
                byte[] buffer = new byte[chunkSize];
                string[] pathSplit = path.Split('/');
                string name = pathSplit[pathSplit.Length - 1];
                byte[] nameBytes = Encoding.UTF8.GetBytes(name);

                List<byte> extraInfo = new List<byte>();
                extraInfo.AddRange(BitConverter.GetBytes(nameBytes.Length));
                extraInfo.AddRange(nameBytes);
                extraInfo.AddRange(BitConverter.GetBytes(chunkSize));

                stream.Write(extraInfo.ToArray(), 0, extraInfo.ToArray().Length);

                int bytesRead;
                while ((bytesRead = bs.Read(buffer, 0, chunkSize)) != 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }

            }
        }
        public static void Recieve(NetworkStream stream, int chunkSize=256)
        {
            int bytesRead;
            float totalBytes = 0;
            byte[] buffer = new byte[chunkSize];
            stream.Read(buffer, 0, 4);
            int nameLength = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[nameLength];
            stream.Read(buffer, 0, nameLength);
            string fileName = Encoding.UTF8.GetString(buffer, 0, nameLength);

            stream.Read(buffer, 0, 4);
            chunkSize = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[chunkSize];

            if (!Directory.Exists(@"Recieved/")) Directory.CreateDirectory(@"Recieved/");
            using (FileStream fs = File.Open(@"Recieved/" + fileName, FileMode.OpenOrCreate))
            {

                while ((bytesRead = stream.Read(buffer, 0, chunkSize)) != 0)
                {
                    totalBytes += bytesRead / 1000f;
                    fs.Write(buffer, 0, bytesRead);
                    Console.Write("KB Recieved: " + totalBytes + '\r');

                }
                Console.Write('\n');
                fs.Close();
                
                Console.WriteLine("File Complete!");
            }
        }
        public static async void Server(TcpListener listener, int chunkSize = 256)
        {
            while (true)
            {
                if (listener.Pending())
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected!");
                    NetworkStream stream = client.GetStream();
                    while (client.Client.Connected)
                    {
                        //Recieve(stream, chunkSize);
                        
                        Send(stream, @"Recieved/stairs.png", 2560);
                        client.Close();
                        
                    }
                    Console.WriteLine("Client disconnected!");

                }
            }
        }
    }

}
