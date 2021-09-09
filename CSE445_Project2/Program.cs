using System;
using System.Threading;

namespace CSE445_Project2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            Theater theater = new Theater();
            Thread theaterThread = new Thread(new ThreadStart(theater.theaterFunc));
            theaterThread.Start();

            TicketBroker brokerThread = new TicketBroker(); // subscribe to event
            Theater.promotion += new promotionalEvent(brokerThread.promotionalEvent);
            Thread[] brokers = new Thread[2];
            for(int i = 0; i < 2; i++)
            {
                brokers[i] = new Thread(new ThreadStart(brokerThread.brokerFunc));
                brokers[i].Name = (i + 1).ToString();
                brokers[i].Start();
            }


        }
    }

    public delegate void promotionalEvent(int price);


    public class Theater
    {
        private static int counter_t;
        private static int ticketPrice;
        public static event promotionalEvent promotion;
        static Random rng;
        

        public Theater()
        {
            // constructor
            counter_t = 0;
            ticketPrice = 100;
            rng = new Random();
        }
        public int getPrice() { return ticketPrice; }
        public static void changePrice(int price)
        {
            if(price < ticketPrice)
            {
                if(promotion != null)
                {
                    promotion(price);
                    counter_t = counter_t + 1;
                }
                ticketPrice = price;
            }
        }

        public void theaterFunc()
        {
            for(int i = 0; i < 50; i++)
            {
                Thread.Sleep(500);
                int p = rng.Next(50, 100);
                Theater.changePrice(p);
            }
        }
        

        // method to be started as a thread
        



        // determine ticket prices
        public void PricingModel()
        {
            while(counter_t < 20)
            {
                int price = rng.Next(40, 200);
                changePrice(price);
            }
        }
        // event stuff

        public void orderProcessing()
        {

        }

        // 
    }

    public class TicketBroker
    {
        public TicketBroker()
        {
            // generates order object
        }

        public void brokerFunc()
        {
            Theater theater = new Theater();
            for(int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                int p = theater.getPrice();
                Console.WriteLine("TicketBroker{0} has everyday low price: ${1} each", Thread.CurrentThread.Name, p);
            }

            
        }
        public void promotionalEvent(int p)
        {
            Console.WriteLine("TicketBroker{0} tickets are on sale: as low as ${1} each", Thread.CurrentThread.Name, p);
        }
    }

    public class MultiCellBuffer
    {
        private OrderClass[] cell;
        public MultiCellBuffer()
        {
            // cells
            cell = new OrderClass[2];
            cell[0] = new OrderClass();
            cell[1] = new OrderClass();
        }
    }

    public class OrderClass
    {
        private int cardNo, quantity;
        private string senderID;
        private double unit_price;
        public OrderClass()
        {
            cardNo = -1;
            quantity = -1;
            senderID = "";
            unit_price = -1;
        }

        public void setCardNumber(int number)
        {
            this.cardNo = number;
        }
        public void setQuantity(int amount)
        {
            this.quantity = amount;
        }
        public void setSenderId(string id)
        {
            this.senderID = id;
        }
        public void setUnitPrice(double price)
        {
            this.unit_price = price;
        }

        public int getCardNumber()
        {
            return this.cardNo;
        }
        public int getQuantity()
        {
            return this.quantity;
        }
        public string getSenderId()
        {
            return this.senderID;
        }
        public double getUnitPrice()
        {
            return this.unit_price;
        }
    }

}
