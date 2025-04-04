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


    public static void Start()
    {

        var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        var endPoint = new IPEndPoint(hostEntry.AddressList[0], 11000);

        Socket socket = new
        (
            endPoint.Address.AddressFamily,
            SocketType.Dgram,
            ProtocolType.Udp
        );

        var Server = (EndPoint) endPoint;

        if (CreateHandshake(socket, endPoint))
        {
            Console.WriteLine("Handshake successful.");

            DnsLookup(socket, endPoint, new DNSRecord() {Type="A", Name= "www.sample.com"});
        }

        
        
        socket.Close();


        //TODO: [Create endpoints and socket]


        //TODO: [Create and send HELLO]

        //TODO: [Receive and print Welcome from server]

        // TODO: [Create and send DNSLookup Message]


        //TODO: [Receive and print DNSLookupReply from server]


        //TODO: [Send Acknowledgment to Server]

        // TODO: [Send next DNSLookup to server]
        // repeat the process until all DNSLoopkups (correct and incorrect onces) are sent to server and the replies with DNSLookupReply

        //TODO: [Receive and print End from server]





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