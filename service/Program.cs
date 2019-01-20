using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grpc.Core;
using Contract;

namespace service
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server
            {
                Services = { InterNodeCommunication.BindService(new InterNodeCommunicationService()) },
                Ports = { new ServerPort ("localhost", 0, ServerCredentials.Insecure ) }
            };

            server.Start();
            Console.WriteLine($"Bound port={server.Ports.First().BoundPort}");
            Console.WriteLine("Waiting for you...");
            Console.ReadKey();
            server.ShutdownAsync().Wait();
        }
    }

    internal class InterNodeCommunicationService : InterNodeCommunication.InterNodeCommunicationBase
    {
        public override Task<ReplyV1> ExecuteRequestV1(RequestV1 request, ServerCallContext context)
        {
            Console.WriteLine("Incoming request");
            // throw new RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
            var kds = "Hello, Mister!";
            return Task.FromResult(new ReplyV1 { KustoDataStream = Google.Protobuf.ByteString.CopyFromUtf8(kds) });
        }
    }
}
