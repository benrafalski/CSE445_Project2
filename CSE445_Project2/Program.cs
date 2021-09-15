using System;
using System.Threading;

namespace CSE445_Project2
{
    class Program
    {
        public static MultiCellBuffer buf = new MultiCellBuffer();
        static void Main(string[] args)
        {
            
            //Console.WriteLine("Hello World!");
            Theater theater = new Theater("ASU Gammage");
            Thread theaterThread = new Thread(new ThreadStart(theater.PricingModel));
            theaterThread.Start();

            TicketBroker brokerThread = new TicketBroker(); 
            Theater.promotion += new promotionalEvent(brokerThread.promotionalEvent); // subscribe to event
            Thread[] brokers = new Thread[5];
            string[] broker_names = { "tickets.com", "stubhub.com", "getseats.com", "buytickets.com", "promotional.com"};
            for (int i = 0; i < 5; i++) // N = 5
            {
                brokers[i] = new Thread(new ThreadStart(brokerThread.brokerFunc));
                brokers[i].Name = broker_names[i];
                brokers[i].Start();
            }


        }
    }

    public delegate void promotionalEvent(int price, int old_price, string name);


    public class Theater
    {
        private static int counter_t;
        private static int ticketPrice;
        public static event promotionalEvent promotion;
        static Random rng;
        public static string id;
        

        public Theater(string ID)
        {
            // constructor
            counter_t = 0;
            ticketPrice = 100;
            rng = new Random();
            id = ID;


        }
        public static int getPrice() { return ticketPrice; }


        // price cut event
        public static void changePrice(int price)
        {
         
            if (price < ticketPrice)
            {
                if(promotion != null)
                {
                    promotion(price, ticketPrice, id);
                    counter_t++;
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
                recieveOrder();
            }
        }
        // event stuff
        public static void orderProcessing(object order)
        {
            // new thread is instantiated
            OrderClass orderObject = (OrderClass)order;
            
            // check valitity of cc number
            // cc number must be between 5000 and 7000
            if (orderObject.getCardNumber() >= 5000 && orderObject.getCardNumber() <= 7000)
            {
                double charge_amt = orderObject.getUnitPrice() * orderObject.getQuantity();
                double after_tax = charge_amt * 0.10;
                double grand_total = after_tax + 10;
                Console.WriteLine($"order has been processed for {orderObject.getQuantity()} tickets with total : {grand_total}");
            }
            
        }

        public void recieveOrder()
        {
            //Thread orderThread = new Thread(new ThreadStart())
            // start new thread
            OrderClass order = Program.buf.getOnecell(Thread.CurrentThread.Name);
            //Console.WriteLine($"order : {order}");
            if(order != null)
            {
                Console.WriteLine($"processing an order from : {order.getSenderId()}");
                Thread orderThread = new Thread(new ParameterizedThreadStart(orderProcessing));
                orderThread.Name = "{recieved order thread}";
                orderThread.Start(order);
                
            }
        }

        

        // 
    }

    public class TicketBroker
    {

        public static string id;
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
                //Console.WriteLine("TicketBroker {0} has everyday low price: ${1} each", Thread.CurrentThread.Name, p);
            }

            
        }

        // event handler
        public void promotionalEvent(int p, int old_p, string name)
        {

            OrderClass new_order = new OrderClass();
            new_order.setCardNumber(7000);
            new_order.setQuantity(numberOfTickets());
            new_order.setRecieverId(name);
            new_order.setSenderId(Thread.CurrentThread.Name);
            new_order.setUnitPrice(p);
            Console.WriteLine($"Broker : {Thread.CurrentThread.Name} has placed an order and added to a cell");
            Program.buf.setOneCell(new_order);
        }

        public int numberOfTickets()
        {
            int price = Theater.getPrice();
            int amount = 100 / price;
            return amount;
        }


    }

    public class MultiCellBuffer
    {
        private OrderClass[] cell;
        // create a semaphore to track buffer availabality
        Semaphore cell_pool = new Semaphore(0, 2);
        // use a lock mechanism here asw
        object _locker = new object();
        public MultiCellBuffer()
        {
            // cells, 2 for individual
            cell = new OrderClass[2];
            cell[0] = null;
            cell[1] = null;
        }

        public void setOneCell(OrderClass order)
        {
            Console.WriteLine("waiting to set a cell");
            cell_pool.WaitOne(300);
            lock (_locker)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (cell[i] == null)
                    {
                        Console.WriteLine("setting a cell");
                        cell[i] = order;
                        break;

                    }
                }
            }
            
        }

        public OrderClass getOnecell(string id)
        {
            
            OrderClass cell_order = null;
            lock (_locker)
            {
                for(int i = 0; i < 2; i++)
                {
                    if(cell[i] != null && String.Compare(cell[i].getSenderId(), id) == 0)
                    {
                        Console.WriteLine("getting a cell");
                        cell_order = cell[i];
                        cell[i] = null;
                        cell_pool.Release();
                        break;
                    }
                }
            }
            //cell_pool.Release();
            return cell_order;

        }
    }

    public class OrderClass
    {
        private int cardNo, quantity;
        private string senderID, recieverID;
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
        public void setRecieverId(string id)
        {
            this.recieverID = id;
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
        public string getRecieverId()
        {
            return this.recieverID;
        }
        public double getUnitPrice()
        {
            return this.unit_price;
        }
    }

}
