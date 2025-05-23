using System.Collections.Immutable;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LibData;

// SendTo();
class Program
{
    static void Main(string[] args)
    {
        ClientUDP.Start();
    }
}

public class Setting
{
    public int ServerPortNumber { get; set; }
    public string? ServerIPAddress { get; set; }
    public int ClientPortNumber { get; set; }
    public string? ClientIPAddress { get; set; }
}

class ClientUDP
{

    static readonly Random random = new();
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);


    private static int MsgId = 0;
    private static byte[] Buffer = new Byte[4096];

    private static void Log(string action, string details)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.WriteLine($"[{timestamp}] {action,-15} {details}");
    }
    public static void Start()
    {
        var clientIP = IPAddress.Parse(setting.ClientIPAddress);                    // 🔧
        var clientEndPoint = new IPEndPoint(clientIP, setting.ClientPortNumber);   // 🔧

        var serverIP = IPAddress.Parse(setting.ServerIPAddress);                   // 🔧
        var serverEndPoint = new IPEndPoint(serverIP, setting.ServerPortNumber);   // 🔧

        Socket socket = new(
            AddressFamily.InterNetworkV6,
            SocketType.Dgram,
            ProtocolType.Udp
        );

        socket.Bind(clientEndPoint);                                               // 🔧

        Log("CLIENT", $"Started, connecting to {serverEndPoint}");                 // 🔧

        if (CreateHandshake(socket, serverEndPoint))                               // 🔧
        {
            Console.WriteLine("Handshake successful.");
            Log("HANDSHAKE", "Successful");

            DnsLookup(socket, serverEndPoint, new DNSRecord() { Type = "A", Name = "www.sample.com" });   // 🔧
        }

        socket.Close();


    }

    private static DNSRecord? DnsLookup(Socket socket, EndPoint endPoint, DNSRecord dNSRecord)
    {
        Message DnsLookupMsg = new()
        {
            MsgId = random.Next(),
            MsgType = MessageType.DNSLookup,
            Content = dNSRecord
        };

        SendTo(socket, DnsLookupMsg, endPoint);

        Message? result = null;
        while (result is null || result.MsgType != MessageType.DNSLookupReply || result.MsgId != DnsLookupMsg.MsgId)
        {
            result = ReceiveFrom(socket, Buffer, ref endPoint);
        }

        SendTo(socket, new Message()
        {
            MsgId = random.Next(),
            MsgType = MessageType.Ack,
            Content = DnsLookupMsg.MsgId
        }, endPoint);

        var end = ReceiveFrom(socket, Buffer, ref endPoint);
        if (end?.MsgType == MessageType.End)
        {
            Console.WriteLine("End of communication.");
        }
        
        return JsonSerializer.Deserialize<DNSRecord>(result.Content.ToString());
    }

    public static bool CreateHandshake(Socket socket, EndPoint endPoint)
    {
        Message HandshakeMsg = new()
        {
            MsgId = random.Next(),
            MsgType = MessageType.Hello,
            Content = "Hello from client."
        };

        SendTo(socket, HandshakeMsg, endPoint);

        Message? HandshakeReply = ReceiveFrom(socket, Buffer, ref endPoint);
        if (HandshakeReply?.MsgType != MessageType.Welcome)
        {
            return false;
        }

        return true;
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