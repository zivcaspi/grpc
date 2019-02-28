using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grpc.Core;
using Grpc.Core.Interceptors;
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

            // channel.Intercept(new ClientChannelInterceptor());
            var callInvoker = channel.Intercept(ClientChannelInterceptor.SetActivityContextInMetadata);

            var client = new InterNodeCommunication.InterNodeCommunicationClient(callInvoker);

            // Sync call
            Console.WriteLine("Sync call...");
            var reply = client.ExecuteRequestV1(new RequestV1 { Text = "T | count", Database = "NetDefaultDB", Properties = "properties" });
            Console.WriteLine("Reply: " + reply.KustoDataStream.ToStringUtf8());

            // Async call
            Console.WriteLine("Async call...");
            var response = client.ExecuteRequestV1Async(new RequestV1 { Text = "T | where 1 == 2", Database = "NetDefaultDB", Properties = "properties" });
            Console.WriteLine("ResponseAsync: " + response.ResponseAsync.Result.KustoDataStream.ToStringUtf8());
            var responseMetadata = response.GetTrailers();
            var responseActivityId = responseMetadata.Where(e => e.Key == "x-ms-activity-id").FirstOrDefault();
            Console.WriteLine("  x-ms-activity-id: " + responseActivityId);

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Client has completed sending data. Press any key to quit");
            Console.ReadKey();

            // TODO: Add handling of exceptions. In gRPC, any exception thrown by the service code
            //       is translated into an RpcException object. It is better that the service code
            //       will translate all its internal exceptions to RpcException and throw _that_
            //       back at gRPC, which knows how to carry it then to the client.
            //
            // throw new RpcException(new Status(StatusCode.InvalidArgument, "bla bla bla")); // Service-side
            //
            // try {...gRPC-client-call..} catch (RpcException ex) { ... } // Client-side

        }
    }

    public class ClientChannelInterceptor : Interceptor
    {
        public static Metadata SetActivityContextInMetadata(Metadata metadata)
        {
            // We "simulate" the injection of a metadata header into the request
            var key = "x-ms-activity-context";
            var value = Guid.NewGuid().ToString("N");
            metadata.Add(key, value);
            return metadata;
        }


        // TODO: This form of interception currently doesn't work. I still to need to work out how it's intended to be used.
#if FUTURE
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
        }

        private async Task<TResponse> AsyncUnaryCallImpl<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                // Push activity context
                // TODO

                var ret = await base.AsyncUnaryCall(request, context, continuation);
                return ret;
            }
            finally
            {
                // Clear activity context
                // TODO
            }

        }
#endif
    }


}
