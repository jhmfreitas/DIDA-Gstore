using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GStoreServer.Services
{
    class RequestInterceptor : Interceptor
    {
        private readonly int minDelay;
        private readonly int maxDelay;
        private readonly ManualResetEventSlim freezeLock;
        private readonly Random r = new Random();

        public RequestInterceptor(ManualResetEventSlim freezeLock, int minDelay, int maxDelay)
        {
            if (minDelay > maxDelay)
            {
                throw new ArgumentException("Minimum delay has to be less or equal than maximum delay.");
            }
            this.freezeLock = freezeLock ?? throw new ArgumentNullException("ReaderWriter lock cannot be null.");
            this.minDelay = minDelay;
            this.maxDelay = maxDelay;
        }
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            freezeLock.Wait();
            TResponse response = await base.UnaryServerHandler(request, context, continuation);
            await Task.Delay(r.Next(minDelay, maxDelay));
            return response;
        }
    }
}
