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
            
            var ipAddress = IPAddress.Parse(setting.ServerIPAddress);
            IPEndPoint ipEndPoint = new(ipAddress, setting.ServerPortNumber);

            using Socket listener = new (
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Udp
            );

            listener.Bind(ipEndPoint);
            listener.Listen(100);

            // TODO:[Receive and print a received Message from the client]
            var handler = await listener.AcceptAsync();

            while (true)
            {
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1)
                {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");

                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    break;

            }



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