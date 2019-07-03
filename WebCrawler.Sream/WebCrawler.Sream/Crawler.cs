using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Util;
using CsQuery;

namespace WebCrawler.Stream
{
    public class Crawler
    {
        public static IGraph<FlowShape<Uri, Uri>, NotUsed> MyCrawler()
        {
            var index = new ConcurrentSet<Uri>();

            var graph = GraphDsl.Create(b =>
            {
                var merge = b.Add(new MergePreferred<Uri>(1));
                var bcast = b.Add(new Broadcast<Uri>(2));

                // async downlad page from provided uri
                // resolve links from it
                var flow = Flow.Create<Uri>()
                    .Where(uri => index.TryAdd(uri))
                    .SelectAsyncUnordered(4, DownloadPage)
                    .SelectMany(ResolveLinks);

                // feedback loop - take only those elements,
                // which were successfully added to index (unique)
                var flowBack = Flow.Create<Uri>()
                    .Collect(uri => !index.Contains(uri) ? uri : null)
                    .ConflateWithSeed(uri => ImmutableList.Create(uri), (uris, uri) => uris.Add(uri))
                    .SelectMany(uris => uris);

                b.From(merge).Via(flow).To(bcast);
                b.From(bcast).Via(flowBack).To(merge.Preferred);

                return new FlowShape<Uri, Uri>(merge.In(0), bcast.Out(1));
            });

            return graph;
        }

        private static async Task<Tuple<Uri, CQ>> DownloadPage(Uri uri)
        {
            var request = WebRequest.CreateHttp(uri);
            var response = await request.GetResponseAsync();
            using (var stream = response.GetResponseStream())
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var html = await reader.ReadToEndAsync();
                        return Tuple.Create(uri, CQ.CreateDocument(html));
                    }
                }
                else return Tuple.Create(uri, new CQ());
            }
        }

        private static IEnumerable<Uri> ResolveLinks(Tuple<Uri, CQ> t)
        {
            foreach (var link in t.Item2["a[href]"])
            {
                var href = link.GetAttribute("href");
                Uri result;
                if (Uri.TryCreate(href, UriKind.Absolute, out result))
                    yield return result;
                else if (Uri.TryCreate(t.Item1, href, out result))
                    yield return result;
            }
        }
    }
}
