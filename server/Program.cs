using System;
using System.Data;
using System.Data.SqlTypes;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        static readonly string configFile = @"../Setting.json";
        static readonly string configContent = File.ReadAllText(configFile);
        static readonly Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);

        // [Done] TODO: [Read the JSON file and return the list of DNSRecords]
        static readonly string dnsRecordsFile = @"../DNSrecords.json";
        static readonly string dnsRecordsContent = File.ReadAllText(dnsRecordsFile);
        static readonly List<DNSRecord>? dnsRecords = JsonSerializer.Deserialize<List<DNSRecord>>(dnsRecordsContent);




        public static async void Start()
        {


            // TODO: [Create a socket and endpoints and bind it to the server IP address and port number]
            

            // var ipAddress = IPAddress.Parse(setting.ServerIPAddress);
            IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint ipEndPoint = new(iPHostEntry.AddressList[0], 11000);//setting.ServerPortNumber);

            using Socket socket = new (
                ipEndPoint.Address.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            // Creates an IPEndPoint to capture the identity of the sending host.
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;

            // Binding is required with ReceiveFrom calls.
            socket.Bind(ipEndPoint);

            byte[] msg = new Byte[256];
            Console.WriteLine("Waiting to receive datagrams from client...");
            // This call blocks.
            socket.ReceiveFrom(msg, msg.Length, SocketFlags.None, ref senderRemote);
            socket.Close();
            Console.WriteLine($"Message received from {senderRemote.ToString()}");
            Console.WriteLine(Encoding.ASCII.GetString(msg));

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


    }
}