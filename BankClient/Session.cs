using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BankClient
{
    enum ServerMessageEnum { Response, Text};
    class Session
    {
        NetworkStream tcpStream;
        byte[] bRead = new byte[8192];
        protected int key = 4;
        Queue<string> responses = new Queue<string>();

        public Session(NetworkStream tcpStream)
        {
            this.tcpStream = tcpStream;
        }

        public void Run()
        {
            while (true)
            {
                Tuple<ServerMessageEnum, string> serverReply = Receive();
                if (serverReply.Item2 == "$$DISCONNECT")
                {
                    throw new DisconnectExceptions();
                }

                if (serverReply.Item1 == ServerMessageEnum.Response)
                {
                    Console.WriteLine(serverReply.Item2);
                    Send(Console.ReadLine());
                }
                else
                {
                    Console.WriteLine(serverReply.Item2);
                }
            }
        }

        //Reads respons from server, returns a tuple type with message type and content
        public Tuple<ServerMessageEnum, String> Receive()
        {
            string s;

            //Checks if any messages are in queue
            if (responses.Count > 0)
            {
                //If so, use first message in queue
                s = responses.Dequeue();
            }
            else
            {
                //Else, get new message from server
                int bReadSize = tcpStream.Read(bRead, 0, bRead.Length);
                s = System.Text.Encoding.UTF8.GetString(bRead, 0, bReadSize);
            }

            //Splits server respone into messages by splitting on the EOM-character (¤)
            string[] parts = s.Split('¤');

            //Checks if more than one message in server message
            if (parts.Length > 1)
            {
                s = parts[0];

                //Saves rest of messages to queue
                for (int i = 1; i < parts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parts[i]))
                    {
                        responses.Enqueue(parts[i]);
                    }
                }
            }

            //Checks Enum
            string command = s.Substring(0, 1);

            ServerMessageEnum type;
            if (command == "R")
            {
                type = ServerMessageEnum.Response;
            }
            else
            {
                type = ServerMessageEnum.Text;
            }
            return new Tuple<ServerMessageEnum, string>(type, s.Substring(1));
        }
        
        public void Send(string message)
        {
            Byte[] bSend = System.Text.Encoding.UTF8.GetBytes(message);
            tcpStream.Write(bSend, 0, bSend.Length);
        }
    }
}
