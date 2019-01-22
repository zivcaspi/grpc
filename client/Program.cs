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

            var reply = client.ExecuteRequestV1(new RequestV1 { Text = "T | count", Database = "NetDefaultDB", Properties = "properties" });
            Console.WriteLine("Reply: " + reply.KustoDataStream.ToStringUtf8());

            channel.ShutdownAsync().Wait();
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
