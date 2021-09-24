using System;
using System.Threading;

// student: Benjamin Rafalski
// ASUID: 1216740421

namespace CSE445_Project2
{
    // event delegate
    public delegate void promotionalEvent(object source, PriceCutEventArgs args);
    class Program
    {
        // public vars used by other classes
        public static MultiCellBuffer buf;
        public static Thread theaterThread;

        static void Main(string[] args)
        {
            buf = new MultiCellBuffer();
            Theater theater = new Theater();
            // theater thread started 
            // number of theaters K = 1 (individual)
            theaterThread = new Thread(new ThreadStart(theater.PricingModel));
            theaterThread.Start();
            theaterThread.Name = "theater";

            TicketBroker[] brokerTs = new TicketBroker[5];
            for (int i = 0; i < 5; i++)
            {
                // each ticket broker object subscribes to the event
                brokerTs[i] = new TicketBroker();
                Theater.promotion += new promotionalEvent(brokerTs[i].promotionalEvent);
            }
           
            Thread[] brokers = new Thread[5];
            for (int i = 0; i < 5; i++) // N = 5
            {
                // each broker is started with the same method in different objects
                brokers[i] = new Thread(new ThreadStart(brokerTs[i].brokerFunc));
                brokers[i].Name = $"Broker{i + 1}";
                brokers[i].Start();
            }
        }
    }
}

