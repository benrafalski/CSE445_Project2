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
            Thread theaterThread = new Thread(new ThreadStart(theater.PricingModel));
            theaterThread.Start();

            TicketBroker brokerThread = new TicketBroker(); // subscribe to event
            Theater.promotion += new promotionalEvent(brokerThread.promotionalEvent);
            Thread[] brokers = new Thread[2];
            for(int i = 0; i < 5; i++) // N = 5
            {
                brokers[i] = new Thread(new ThreadStart(brokerThread.brokerFunc));
                //brokers[i].Name = (i + 1).ToString();
                brokers[i].Start();
            }


        }
    }

    public delegate void promotionalEvent(int price, int old_price, int t_count);


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
        public static int getPrice() { return ticketPrice; }
        public static void changePrice(int price)
        {
         
            if (price < ticketPrice)
            {
                if(promotion != null)
                {
                    promotion(price, ticketPrice, counter_t);
                    counter_t = counter_t + 1;
                }
                ticketPrice = price;
            }
        }
       
        // starts thread
        // determine ticket prices
        public void PricingModel()
        {
            //int old_price = getPrice();
            while(counter_t < 20)
            {
                Thread.Sleep(500);
                int price = rng.Next(40, 200);
                changePrice(price);
            }
        }
        // event stuff

        public void orderProcessing()
        {
            // new thread is instantiated
            OrderClass orderObject = new OrderClass();
            //Thread orderThread = new Thread(new ThreadStart())
            // start new thread
            // check valitity of cc number
            // cc number must be between 5000 and 7000

        }

        // 
    }

    public class TicketBroker
    {
        public TicketBroker()
        {
            // generates order object
            // 
        }

        public void brokerFunc()
        {
            //Theater theater = new Theater();
            for(int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                int p = Theater.getPrice();
                Console.WriteLine("TicketBroker{0} has everyday low price: ${1} each", Thread.CurrentThread.Name, p);
            }

            
        }
        public void promotionalEvent(int p, int old_p, int count)
        {
            Console.WriteLine("TicketBroker{0} tickets are on sale: as low as ${1} each", Thread.CurrentThread.Name, p);
        }
    }

    public class MultiCellBuffer
    {
        private OrderClass[] cell;
        // create a semaphore to track buffer availabality
        Semaphore cell_pool = new Semaphore(0, 2);
        // use a lock mechanism here asw
        public MultiCellBuffer()
        {
            // cells, 2 for individual
            cell = new OrderClass[2];
            cell[0] = new OrderClass();
            cell[1] = new OrderClass();
        }

        public void setOneCell()
        {
            cell_pool.WaitOne();   
        }

        public void getOnecell()
        {
            cell_pool.Release();
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
