using System;
using System.Threading;

namespace CSE445_Project2
{
    public delegate void promotionalEvent(object source, PriceCutEventArgs args);
    class Program
    {
        public static MultiCellBuffer buf;
        public static Theater theater;

        static void Main(string[] args)
        {
            buf = new MultiCellBuffer();
            theater = new Theater();
            // theater thread started 
            // number of theaters K = 1 (individual)
            Thread theaterThread = new Thread(new ThreadStart(theater.PricingModel));
            theaterThread.Start();
            theaterThread.Name = "theater";
            //theaterThread.Join();

            TicketBroker[] brokerTs = new TicketBroker[5];
            for (int i = 0; i < 5; i++)
            {
                brokerTs[i] = new TicketBroker();
                Theater.promotion += new promotionalEvent(brokerTs[i].promotionalEvent);
            }
            //Theater.promotion += new promotionalEvent(brokerTs.promotionalEvent);// subscribe to event

            Thread[] brokers = new Thread[5];
            for (int i = 0; i < 5; i++) // N = 5
            {
                brokers[i] = new Thread(new ThreadStart(brokerTs[i].brokerFunc));
                brokers[i].Name = $"Broker{i + 1}";
                brokers[i].Start();
                // brokers[i].Join();
            }
        }
    }
}

