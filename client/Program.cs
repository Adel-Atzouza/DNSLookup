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
        ClientUDP.start();
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

    //TODO: [Deserialize Setting.json]
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);


    public static void start()
    {

        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPEndPoint endPoint = new IPEndPoint(hostEntry.AddressList[0], 11000);

        Socket s = new Socket(endPoint.Address.AddressFamily,
            SocketType.Dgram,
            ProtocolType.Udp);

        byte[] msg = Encoding.ASCII.GetBytes("This is a test");
        Console.WriteLine("Sending data.");
        // This call blocks.
        s.SendTo(msg, 0, msg.Length, SocketFlags.None, endPoint);
        s.Close();


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
}