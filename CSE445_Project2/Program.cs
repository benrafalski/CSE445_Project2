using System;
using System.Threading;

namespace CSE445_Project2
{
    class Program
    {

        /*
        is there only one order per price cut?
        can there be orders still after the theater thread terminates?
        need to use mre or are?


         */
        public static MultiCellBuffer buf;
        public static Thread theaterThread;
        public static Thread[] brokers;
        public static Theater theater;

        static void Main(string[] args)
        {
            buf = new MultiCellBuffer();
            theater = new Theater();
            // theater thread started 
            // number of theaters K = 1 (individual)
            theaterThread = new Thread(new ThreadStart(theater.PricingModel));
            theaterThread.Start();

            TicketBroker brokerTs = new TicketBroker();
            Theater.promotion += new promotionalEvent(brokerTs.promotionalEvent);// subscribe to event
            
            brokers = new Thread[5];
            for (int i = 0; i < 5; i++) // N = 5
            {
                brokers[i] = new Thread(new ThreadStart(brokerTs.brokerFunc));
                brokers[i].Name = $"Broker{i + 1}";
                brokers[i].Start();
            }
        }
    }
    //event
    public class PriceCutEvenArgs
    {
        public double price { get; set; }
        public double old_price { get; set; }
    }

    public delegate void promotionalEvent(object source, PriceCutEvenArgs args);

    // server
    public class Theater
    {
        private static int counter_t;
        private static double ticketPrice;
        public static event promotionalEvent promotion;
        static Random rng;

        public static int orders;


        public int Orders
        {
            get { return counter_t; }
        }

        public Theater()
        {
            // constructor
            counter_t = 0;
            ticketPrice = 120;
            rng = new Random();
            orders = 0;


        }
        public static double getPrice() { return ticketPrice; }


        // price cut event
        public virtual void changePrice(double price)
        {
            // if the new price is lower than the previous price
            // emit an event
            // calls event handler in the ticket brokers
         
            if (price < ticketPrice)
            {
                // if there is a subsription
                if (promotion != null)
                {
                    promotion(this, new PriceCutEvenArgs() { price = price, old_price = ticketPrice}); // calls event handler
                    counter_t++; // increase the number of orders made
                }                
            }
            ticketPrice = price;
        }
       
        // starts thread
        // determine ticket prices
        public void PricingModel()
        {
            while(counter_t < 20)
            {
                
                double price;
                // dynamically calculates ticket price for the ticket brokers
                // calculates a random number between 1-10
                // if the number is even then there is a price cut
                // otherwise there is a price increase
                if (rng.Next(1,10) % 2 == 0)
                {
                    // price can decrease
                    double discount = rng.Next(0, 100) * 0.2;
                    price = ticketPrice - discount;
                    
                    Console.WriteLine($"NEW PRICE CHANGE: current price decreased by {String.Format("{0:0.00}", discount)} to {String.Format("{0:0.00}", price)}\n\n");
                }
                else
                {
                    // price can increase
                    double increase = rng.Next(0, 100) * 0.2;
                    price = ticketPrice + increase;
                    Console.WriteLine($"NEW PRICE CHANGE: current price increased by {String.Format("{0:0.00}", increase)} to {String.Format("{0:0.00}", price)}\n\n");
                }
                // checks if the price is between 200 and 40
                // if it is out of range the price is reset to 120
                if(price > 200 || price < 40)
                {
                    Console.WriteLine("PRICE OUT OF RANGE: price reset to 120\n\n");
                    price = 120;
                }
                changePrice(price);
                // changes the price and waits for an order
                Thread.Sleep(700);
                recieveOrder();
            }
            Thread.Sleep(1000);
            // theater terminates after 20 price cuts / orders have been made
            Console.WriteLine("theater therminated");
        }


        // order processing thread entry point
        public static void orderProcessing(object order)
        {
            // creates order object
            OrderClass orderObject = (OrderClass)order;
            orders++;
            // check valitity of cc number
            // cc number must be between 5000 and 7000
            // and maximum number of tickets allowed for purchase
            if (orderObject.CardNumber >= 5000 && orderObject.CardNumber <= 7000)
            {
                // calculates the total amount for a valid the order
                double charge_amt = orderObject.UnitPrice * orderObject.Quantity; // price * number of tickets
                double after_tax = charge_amt * 0.10; // 10% tax before location charge
                double grand_total = charge_amt + after_tax + 10; // theater changes a 10 dollar location fee
                Console.WriteLine("################################################\n");
                Console.WriteLine($"Order #{counter_t};\nfrom {orderObject.Sender};\nwith card ending in *{orderObject.CardNumber}*;\nhas been processed for ${orderObject.UnitPrice} per ticket with {orderObject.Quantity} tickets;\nfor a total : ${grand_total};\n");
                
            }
            else
            {
                Console.WriteLine("################################################\n");
                Console.WriteLine($"Order #{counter_t};\norder from {orderObject.Sender};\ncredit card rejected;\n");
            }
            Console.WriteLine($"Order {counter_t} COMPLETED\n");
            Console.WriteLine("################################################\n");

        }

        // theater recieves orders from the multicell buffer
        public void recieveOrder()
        {
            // gets an order from the buffer
            OrderClass order = Program.buf.getOnecell();
           
            // if there was an order recieved
            // start and order processing thread
            if(order != null)
            {
                //Console.WriteLine($"processing an order from : {order.getSenderId()}");
                Thread orderThread = new Thread(new ParameterizedThreadStart(orderProcessing));
                orderThread.Name = "{recieved order thread}";
                orderThread.Start(order);
                Console.WriteLine($"order from : {order.Sender} has been taken from the buffer by the theater");

            }
            // if there was no order found
            // wait for 500ms then check again
            else
            {
                Thread.Sleep(500);
            }
        }

        

        // 
    }

    // client

    public class TicketBroker
    {

        public static string id;
        public Random rng;
        public int number_of_tickets;
        object _locker = new object();
        public TicketBroker()
        {
            //id = ID;
            //Console.WriteLine($"id = {id}");
            rng = new Random();

            // generates order object
            // 
        }

        public void brokerFunc()
        {
            bool threadAlive = true;
            while(threadAlive)
            {
                if (Program.theater.Orders >= 20)
                {
                    threadAlive = false;
                }
                id = Thread.CurrentThread.Name;
                Thread.Sleep(500);
            }
            Console.WriteLine($"broker : {Thread.CurrentThread.Name} terminated");
        }

        // event handler
        // generates an order object and places it into the buffer
        public void promotionalEvent(object source, PriceCutEvenArgs args)
        {

            OrderClass new_order = new OrderClass();
            new_order.CardNumber = rng.Next(5000, 7500);
            new_order.Quantity = numberOfTickets(args.price, args.old_price);
            new_order.Sender = id;
            new_order.UnitPrice = args.price;
            // send to buffer
            Program.buf.setOneCell(new_order);
        }

        // evaluates the needs based on the new price and other factors
        public int numberOfTickets(double price, double old_price)
        {
            int amount = (int)(old_price - price);
            return amount;
        }


    }

    public class MultiCellBuffer
    {
        private OrderClass[] cell;
        // create a semaphore to track buffer availabality
        Semaphore cell_pool = new Semaphore(0, 2);
        // use a lock mechanism
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
           //Console.WriteLine($"{order.Sender} is waiting to set a cell");
            cell_pool.WaitOne(300);
            lock (cell)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (cell[i] == null)
                    {
                        //Console.WriteLine("setting a cell");
                        cell[i] = order;
                        break;
                        
                       
                       

                    }
                }
            }
            
        }

        public OrderClass getOnecell()
        {
            
            OrderClass cell_order = null;
            lock (_locker)
            {
                for(int i = 0; i < 2; i++)
                {
                    if(cell[i] != null)
                    {
                            cell_order = cell[i];
                            cell[i] = null;
                            cell_pool.Release();
                            break;
                    }
                }
            }
            //Console.WriteLine("getting a cell");
            //cell_pool.Release();
            return cell_order;

        }
    }

    public class OrderClass
    {
        // variables 
        private int cardNo, quantity;
        private string senderID;
        private double unit_price;

        // properties
        public int CardNumber
        {
            get { return cardNo; }
            set { cardNo = value;  }
        }

        public int Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        public string Sender
        {
            get { return senderID; }
            set { senderID = value; }
        }

        public double UnitPrice
        {
            get { return unit_price; }
            set { unit_price = value; }
        }

        
    }

}
