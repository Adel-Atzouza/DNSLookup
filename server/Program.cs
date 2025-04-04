using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LibData;

// ReceiveFrom();
namespace server 
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerUDP.Start();
        }
    }

    public class Setting
    {
        public int ServerPortNumber { get; set; }
        public string? ServerIPAddress { get; set; }
        public int ClientPortNumber { get; set; }
        public string? ClientIPAddress { get; set; }
    }


    class ServerUDP
    {
        static readonly Random random = new();

        static readonly string configFile = @"../Setting.json";
        static readonly string configContent = File.ReadAllText(configFile);
        static readonly Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);

        // [Done] TODO: [Read the JSON file and return the list of DNSRecords]
        static readonly string dnsRecordsFile = @"./DNSrecords.json";
        static readonly string dnsRecordsContent = File.ReadAllText(dnsRecordsFile);
        static readonly List<DNSRecord>? dnsRecords = JsonSerializer.Deserialize<List<DNSRecord>>(dnsRecordsContent);


        private static byte[] Buffer = new byte[4096];

        private static int MsgId = 0;

        public static void Start()
        {
            // var ipAddress = IPAddress.Parse(setting.ServerIPAddress);
            var iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ipEndPoint = new IPEndPoint(iPHostEntry.AddressList[0], 11000);

            using var socket = new Socket
            (
                ipEndPoint.Address.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            var sender = new IPEndPoint(IPAddress.Any, 0);
            var senderRemote = (EndPoint) sender;

            socket.Bind(ipEndPoint);

            while (true)
            {
                Message? msg = ReceiveFrom(socket, Buffer, ref senderRemote);

                switch (msg.MsgType)
                {
                    case MessageType.Hello:
                        HandleHandshake(socket, senderRemote);
                        break;
                    case MessageType.DNSLookup:
                        HandleDNSLookup(socket, senderRemote, msg);
                        break;
                }
            }

            socket.Close();
            // TODO:[Receive and print a received Message from the client]





            // TODO:[Receive and print Hello]



            // TODO:[Send Welcome to the client]


            // TODO:[Receive and print DNSLookup]


            // TODO:[Query the DNSRecord in Json file]

            // TODO:[If found Send DNSLookupReply containing the DNSRecord]



            // TODO:[If not found Send Error]


            // TODO:[Receive Ack about correct DNSLookupReply from the client]


            // TODO:[If no further requests receieved send End to the client]

        }

        private static void HandleDNSLookup(Socket socket, EndPoint senderRemote, Message msg)
        {
            DNSRecord? dnsRecord = JsonSerializer.Deserialize<DNSRecord>(msg.Content.ToString());
            DNSRecord? foundRecord = dnsRecords?.FirstOrDefault(record => record.Name == dnsRecord.Name && record.Type == dnsRecord.Type);

            Message replyMsg = new()
            {
                MsgId = foundRecord is not null ? msg.MsgId : random.Next(),
                MsgType = foundRecord is not null ? MessageType.DNSLookupReply : MessageType.Error,
                Content = foundRecord is not null ? foundRecord : "Domain not found"
            };

            SendTo(socket, replyMsg, senderRemote);
        }

        private static void HandleHandshake(Socket socket, EndPoint endPoint)
        {
            Message HandshakeMsg = new()
            {
                MsgId = random.Next(),
                MsgType = MessageType.Welcome,
                Content = "Welcome from server."
            };

            SendTo(socket, HandshakeMsg, endPoint);
        }

        private static int SendTo(Socket socket, object message, EndPoint endPoint)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] msg = Encoding.ASCII.GetBytes(json);
            return socket.SendTo(msg, 0, msg.Length, SocketFlags.None, endPoint);
        }

        private static Message? ReceiveFrom(Socket socket, byte[] buffer, ref EndPoint endPoint)
        {
            int bytesReceived = socket.ReceiveFrom(buffer, ref endPoint);
            string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            return JsonSerializer.Deserialize<Message>(receivedData);
        }
    }
}