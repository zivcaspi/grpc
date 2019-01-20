using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grpc.Core;
using Contract;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Which port? ");
            var port = int.Parse(Console.ReadLine());
            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new InterNodeCommunication.InterNodeCommunicationClient(channel);

            var reply = client.ExecuteRequestV1(new RequestV1 { Text = "T | count", Database = "NetDefaultDB", Properties = "properties" });
            Console.WriteLine("Reply: " + reply.KustoDataStream.ToStringUtf8());

            channel.ShutdownAsync().Wait();
        }
    }
}
