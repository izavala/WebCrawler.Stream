using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.SignalR;
using Microsoft.Owin.Security.Provider;
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
                var f1 = Flow.Create<ISignalRResult>().Select(x => x.Data);
                var f2 = Flow.Create<string>().Select(x => new Uri(x)).Recover(exception =>
                {
                    if(exception is System.UriFormatException)
                        return new Uri(null);
                    throw new Exception("error");
                });
                var f3 = Flow.Create<Uri>().Select(x => Signals.Broadcast(x.ToString()));
                var f4 = Flow.Create<string>().Select(x => Signals.Broadcast(x));
                var merge = b.Add(new Merge<ISignalRResult>(2));
                var crawlerFlow = b.Add(Crawler.MyCrawler());
                var broadcast = b.Add(new Broadcast<string>(2));
                b.From(source).Via(f1).Via(broadcast).Via(f4).Via(merge).To(this.Sink);
                b.From(broadcast).Via(f2).Via(crawlerFlow).Via(f3).Via(merge);
                return ClosedShape.Instance;
            }));
            graph.Run(App.Materializer);
        }
    }

    
}

