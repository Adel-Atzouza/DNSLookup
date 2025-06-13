using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LibData;

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

        static readonly string dnsRecordsFile = @"./DNSrecords.json";
        static readonly string dnsRecordsContent = File.ReadAllText(dnsRecordsFile);
        static readonly List<DNSRecord>? dnsRecords = JsonSerializer.Deserialize<List<DNSRecord>>(dnsRecordsContent);


        private static byte[] Buffer = new byte[4096];

        private static int MsgId = 0;

        private static void Log(string action, string details)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"[{timestamp}] {action,-15} {details}");
        }

        public static void Start()
        {
            var ipAddress = IPAddress.Parse(setting.ServerIPAddress);
            var ipEndPoint = new IPEndPoint(ipAddress, setting.ServerPortNumber);

            using var socket = new Socket
            (
                AddressFamily.InterNetworkV6,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            var sender = new IPEndPoint(IPAddress.IPv6Any, 0);
            var senderRemote = (EndPoint) sender;

            socket.Bind(ipEndPoint);

            Console.WriteLine($"Server started at {ipEndPoint} and waiting for messages...");

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
                    case MessageType.Ack:
                        HandleAck(socket, senderRemote, msg);
                        break;
                }
            }

            socket.Close();

        }
        private static void HandleDNSLookup(Socket socket, EndPoint senderRemote, Message msg)
        {
            try
            {
                Log("RECEIVED", $"DNSLookup (ID:{msg.MsgId}) for {msg.Content}");
                DNSRecord? dnsRecord = null;
                
                if (msg.Content is JsonElement jsonElement)
                {
                    dnsRecord = JsonSerializer.Deserialize<DNSRecord>(jsonElement.GetRawText());
                }

                if (dnsRecord == null || string.IsNullOrEmpty(dnsRecord.Name))
                {
                    throw new ArgumentException("DNS lookup must contain Name and Type");
                }

                DNSRecord? foundRecord = dnsRecords?.FirstOrDefault(record => 
                    record.Name.Equals(dnsRecord.Name, StringComparison.OrdinalIgnoreCase) && 
                    record.Type.Equals(dnsRecord.Type, StringComparison.OrdinalIgnoreCase));

                Message replyMsg = new()
                {
                    MsgId = foundRecord is not null ? msg.MsgId : random.Next(),
                    MsgType = foundRecord is not null ? MessageType.DNSLookupReply : MessageType.Error,
                    Content = foundRecord is not null ? foundRecord : $"Domain not found: {dnsRecord.Name}"
                };
                
                Log("SENDING", $"{replyMsg.MsgType} (ID:{replyMsg.MsgId}) for {dnsRecord.Name}");
                SendTo(socket, replyMsg, senderRemote);
            }
            catch (Exception ex)
            {

                Message errorMsg = new()
                {
                    MsgId = random.Next(),
                    MsgType = MessageType.Error,
                    Content = $"Error: {ex.Message}"
                };

                SendTo(socket, errorMsg, senderRemote);
            }
        }

        private static void HandleAck(Socket socket, EndPoint senderRemote, Message ackMessage)
        {
            if(ackMessage.MsgType == MessageType.Ack)
            {
                Message endMsg = new()
                {
                    MsgId = random.Next(),
                    MsgType = MessageType.End,
                    Content = "End of DNSLookup"
                };
                Log("SENDING", $"End (ID:{endMsg.MsgId}) to {senderRemote}");
                SendTo(socket, endMsg, senderRemote);

            }
        }

        private static void HandleHandshake(Socket socket, EndPoint endPoint)
        {
            Message HandshakeMsg = new()
            {
                MsgId = random.Next(),
                MsgType = MessageType.Welcome,
                Content = "Welcome from server."
            };
            Log("SENDING", $"Welcome (ID:{HandshakeMsg.MsgId}) to {endPoint}");
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