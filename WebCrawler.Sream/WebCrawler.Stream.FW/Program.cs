using System;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Microsoft.Owin.Hosting;

namespace WebCrawler.Stream.FW
{
    class Program
    {
        static void Main(string[] args)
        {

            var url = "http://localhost:5000";
            var system = ActorSystem.Create("system");
            var materializer = system.Materializer();
           
            using (WebApp.Start<Startup>(url))
            {
                App.System = system;
                App.Materializer = materializer;

                Console.WriteLine($"Listening at {url}");
                Console.ReadLine();
            }
            Console.WriteLine("Press Enter to close...");
            Console.ReadLine();

        }
    }
}
