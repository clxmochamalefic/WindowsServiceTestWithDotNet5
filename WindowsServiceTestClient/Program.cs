using Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;

namespace WindowsServiceTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1", 50000, ChannelCredentials.Insecure);
            var client = new Commander.CommanderClient(channel);

            Console.WriteLine("plz input empty or number key");
            var read = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(read))
            {
                var fetchWeatherResponse = client.FetchWeather(new Empty());
                Console.WriteLine(fetchWeatherResponse.ReportedAt);
                Console.WriteLine(fetchWeatherResponse.Weather);
                return;
            }

            int hour = 0;
            if (!int.TryParse(read, out hour))
            {
                Console.WriteLine("invalid string");
                return;
            }

            var findWeatherResponse = client.FindWeather(new Request()
            {
                HourAgo = hour
            });
            Console.WriteLine(findWeatherResponse.ReportedAt);
            Console.WriteLine(findWeatherResponse.Weather);
        }
    }
}
