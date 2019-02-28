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
            Console.WriteLine("Service is now open for business. Press any key to stop listening and quit.");
            Console.ReadKey();
            server.ShutdownAsync().Wait();
        }
    }

    internal class InterNodeCommunicationService : InterNodeCommunication.InterNodeCommunicationBase
    {
        public override Task<ReplyV1> ExecuteRequestV1(RequestV1 request, ServerCallContext context)
        {
            Console.WriteLine("Incoming request");

            // Read the metadata sent by the client
            var metadata = context.RequestHeaders;
            foreach (var entry in metadata)
            {
                Console.WriteLine($"Metadata[{entry.Key}] = {entry.Value}");
            }

            // Set some metadata to return back to the client
            // Notes:
            //   1. The client must use the async call API to consume this header
            //   2. Metadata headers are always lowercase
            context.ResponseTrailers.Add(new Metadata.Entry("x-ms-activity-id", Guid.NewGuid().ToString("N")));

            // throw new RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
            var kds = "Hello, Mister!";
            return Task.FromResult(new ReplyV1 { KustoDataStream = Google.Protobuf.ByteString.CopyFromUtf8(kds) });
        }
    }
}
