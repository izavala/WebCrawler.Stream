using System;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Microsoft.Owin.Hosting;

namespace WebCrawler.Stream
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://localhost:5000";
            var system = ActorSystem.Create("system");
            var materializer = system.Materializer();
            var graph = RunnableGraph.FromGraph(GraphDsl.Create(b =>
            {
                var source = b.Add(Source.Single(new Uri("http://getakka.net/")));
                var sink = b.Add(Sink.ForEach<Uri>(Console.WriteLine));
                var crawlerFlow = b.Add(Crawler.MyCrawler());

                b.From(source).Via(crawlerFlow).To(sink);

                return ClosedShape.Instance;
            }));
            using (WebApp.Start<Startup>(url))
            {
                App.System = system;
                App.Materializer = materializer;

                Console.WriteLine($"Listening at {url}");
                Console.ReadLine();
            }
            graph.Run(materializer);
            Console.WriteLine("Press Enter to close...");
            Console.ReadLine();

        }
    }
}
