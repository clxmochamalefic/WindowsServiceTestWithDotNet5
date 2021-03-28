using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Common
{
    public delegate Response FetchWeather(int hourAgo);

    public class CommanderService : Commander.CommanderBase
    {
        public FetchWeather commanderServiceDelegate;

        public override Task<Response> FetchWeather(Empty request, ServerCallContext context)
        {
            return Task.Run(() => commanderServiceDelegate.Invoke(0));
        }

        public override Task<Response> FindWeather(Request request, ServerCallContext context)
        {
            return Task.Run(() => commanderServiceDelegate.Invoke(request.HourAgo));
        }
    }
}
