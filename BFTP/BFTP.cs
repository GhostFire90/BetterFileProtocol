using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace BetterFileProtocol
{
    public static class BFTP
    {
        /// <summary>
        /// Sends a file over a given NetworkStream
        /// </summary>
        /// <param name="stream">Stream to send the file over</param>
        /// <param name="path">Path to the file</param>
        /// <param name="chunkSize">Size of chunks the file is read in as</param>
        public static void Send(NetworkStream stream, string path, int chunkSize=256)
        {
           

            using(FileStream fs = File.OpenRead(path))
            using(BufferedStream bs = new BufferedStream(fs))
            {
                int fileSize = (int)fs.Length;

                byte[] buffer = new byte[chunkSize];
                string[] pathSplit = path.Split('/');
                string name = pathSplit[pathSplit.Length - 1];
                byte[] nameBytes = Encoding.UTF8.GetBytes(name);

                List<byte> extraInfo = new List<byte>();
                extraInfo.AddRange(BitConverter.GetBytes(fileSize));
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



        /// <summary>
        /// Recieves a file from the given NetworkStream
        /// </summary>
        /// <param name="stream">stream to recieve from</param>
        /// <param name="chunkSize">original size of file chunks [Is overwritten by the client]</param>
        /// <param name="dir">the directory to save the file to</param>
        /// <returns>Returns the path from the program to the file that was recieved</returns>
        public static string Recieve(NetworkStream stream, int chunkSize=256, string dir = @"Recieved/")
        {
            int bytesRead = 0;
            float totalBytes = 0;
            byte[] buffer = new byte[chunkSize];
            
            stream.Read(buffer, 0, 4);
            int fileSize = BitConverter.ToInt32(buffer);

            stream.Read(buffer, 0, 4);
            int nameLength = BitConverter.ToInt32(buffer);

            buffer = new byte[nameLength];
            stream.Read(buffer, 0, nameLength);
            string fileName = Encoding.UTF8.GetString(buffer, 0, nameLength);

            stream.Read(buffer, 0, 4);
            chunkSize = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[chunkSize];

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            using (FileStream fs = File.Open(dir + fileName, FileMode.OpenOrCreate))
            {
                
                while (fileSize > 0 && (bytesRead = stream.Read(buffer, 0, fileSize > chunkSize ? chunkSize : fileSize)) != 0)
                {
                    totalBytes += bytesRead / 1000f;

                    fs.Write(buffer, 0, bytesRead);
                    Console.Write("KB Recieved: " + totalBytes + '\r');
                    fileSize -= bytesRead;
                    


                }
                Console.Write('\n');
                fs.Close();
                
                Console.WriteLine("File Complete!");
            }
            return dir + fileName;
        }

        /// <summary>
        /// USED FOR TESTING PURPOSES OF THE LIBRARY, DO NOT USE
        /// </summary>
        /// <param name="listener">Listener to accept connections from</param>
        /// <param name="chunkSize">Chunk size of recieve</param>
        [Obsolete]
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
                        Recieve(stream, chunkSize);
                        
                        Send(stream, @"Recieved/stairs.png", 2560);
                        
                        break;
                        
                    }
                    Console.WriteLine("Client disconnected!");

                }
            }
        }
    }

}
