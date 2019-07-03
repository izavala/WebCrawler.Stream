using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.SignalR;
using WebCrawler.Stream.FW;


namespace WebCrawler.Stream
{
    public class EchoConnection : StreamConnection
    {
        public EchoConnection()
        {


            var graph = RunnableGraph.FromGraph(GraphDsl.Create(b =>
            {
                var source = Source.Collect(x => x as Received).Select(x => Signals.Broadcast(x.Data));
                var f1 = Flow.Create<ISignalRResult>().Select(x => x.Data.ToString());
                var f2 = Flow.Create<string>().Select(x => new Uri(x));
                var f3 = Flow.Create<Uri>().Select(x => Signals.Broadcast(x.ToString()));
                var merge = b.Add(new Merge<ISignalRResult>(2));

                var sink = b.Add(Akka.Streams.Dsl.Sink.ForEach<Uri>(Console.WriteLine));
                var crawlerFlow = b.Add(Crawler.MyCrawler());
                var broadcast = b.Add(new Broadcast<ISignalRResult>(2));
                b.From(source).Via(broadcast).To(this.Sink);
                b.From(broadcast).Via(f1).Via(f2).Via(crawlerFlow).Via(f3).To(this.Sink);

                return ClosedShape.Instance;
            }));
            graph.Run(App.Materializer);
        }
    }

    
}

