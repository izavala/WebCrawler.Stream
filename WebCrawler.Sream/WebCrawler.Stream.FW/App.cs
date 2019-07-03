using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.Streams;

namespace WebCrawler.Stream
{
    public static class App
    {
        public static ActorSystem System;
        public static IMaterializer Materializer;
    }
}
