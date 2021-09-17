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


            TicketBroker brokerTs = new TicketBroker("test_broker1");
            

            Theater.promotion += new promotionalEvent(brokerTs.promotionalEvent); // subscribe to event
            
            Thread[] brokers = new Thread[5];
            string[] broker_names = { "tickets.com", "stubhub.com", "getseats.com", "buytickets.com", "promotional.com"};
            for (int i = 0; i < 5; i++) // N = 5
            {
                Console.WriteLine("broker started");

                brokers[i] = new Thread(new ThreadStart(brokerTs.brokerFunc));
                brokers[i].Name = broker_names[i];
                brokers[i].Start();
            }

            



        }
    }
    //event
    public class PriceCutEvenArgs
    {
        public int price { get; set; }
        public int old_price { get; set; }
    }

    public delegate void promotionalEvent(object source, PriceCutEvenArgs args);

    // server
    public class Theater
    {
        private static int counter_t;
        private static int ticketPrice;
        public static event promotionalEvent promotion;
        static Random rng;
        public static string id;
        static object _locker = new object();
        public static int orders;
        

        public Theater(string ID)
        {
            // constructor
            counter_t = 0;
            ticketPrice = 200;
            rng = new Random();
            id = ID;
            orders = 0;


        }
        public static int getPrice() { return ticketPrice; }


        // price cut event
        public virtual void changePrice(int price)
        {
            // if the new price is lower than the previous price
            // emit an event
            // calls event handler in the ticket brokers
         
            if (price < ticketPrice)
            {
                lock (_locker)
                {
                    if (promotion != null)
                    {
                        promotion(this, new PriceCutEvenArgs() { price = price, old_price = ticketPrice});
                       // promotion(price, ticketPrice, id);
                        //Console.WriteLine($"ticketprice = {ticketPrice}, price = {price}");
                        counter_t++;
                    }
                    ticketPrice = price;
                }
                
            }
        }
       
        // starts thread
        // determine ticket prices
        public void PricingModel()
        {
            //int old_price = getPrice();
            while(counter_t < 20)
            {
                //Console.WriteLine(counter_t);
                Thread.Sleep(500);
                int price;
                if (rng.Next(1,10) % 2 == 0)
                {
                    double discount = rng.Next(0, 100) * 0.1;
                    price = ticketPrice - (int)discount;
                }
                else
                {
                    double increase = rng.Next(0, 100) * 0.1;
                    price = ticketPrice + (int)increase;
                }
                
               // Console.WriteLine(price);
                changePrice(price);
                recieveOrder();
            }
        }
        // event stuff
        public static void orderProcessing(object order)
        {
            // new thread is instantiated
            OrderClass orderObject = (OrderClass)order;
            orders++;
            // check valitity of cc number
            // cc number must be between 5000 and 7000
            if (orderObject.getCardNumber() >= 5000 && orderObject.getCardNumber() <= 7000)
            {
                
                
                double charge_amt = orderObject.getUnitPrice() * orderObject.getQuantity();
                //Console.WriteLine($"chargeamt = {charge_amt}");
                double after_tax = charge_amt * 0.10;
                double grand_total = charge_amt + after_tax + 10;
                Console.WriteLine($"order #{orders};\nfrom {orderObject.getSenderId()};\nwith card ending in *{orderObject.getCardNumber()}*;\nhas been processed for ${orderObject.getUnitPrice()} per ticket with {orderObject.getQuantity()} tickets;\nfor a total : ${grand_total};\n\n");
            }
            else
            {
                Console.WriteLine($"order #{orders};\norder from {orderObject.getSenderId()};\ncredit card rejected;\n\n");
            }
            
        }

        public void recieveOrder()
        {
            //Thread orderThread = new Thread(new ThreadStart())
            // start new thread
            OrderClass order = Program.buf.getOnecell();
            //Console.WriteLine($"order : {order}");
            if(order != null)
            {
                //Console.WriteLine($"processing an order from : {order.getSenderId()}");
                Thread orderThread = new Thread(new ParameterizedThreadStart(orderProcessing));
                orderThread.Name = "{recieved order thread}";
                orderThread.Start(order);
                
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
        public TicketBroker(string ID)
        {
            //id = ID;
            //Console.WriteLine($"id = {id}");
            rng = new Random();

            // generates order object
            // 
        }

        public void brokerFunc()
        {

            
            for (int i = 0; i < 20; i++)
            {
                lock (_locker)
                {
                    id = Thread.CurrentThread.Name;
                }

                Thread.Sleep(1000);
                
               

                //Console.WriteLine("TicketBroker {0} has everyday low price: ${1} each", Thread.CurrentThread.Name, null);
            }
            //Theater theater = new Theater();



        }

        // event handler
        public void promotionalEvent(object source, PriceCutEvenArgs args)
        {

            OrderClass new_order = new OrderClass();
            new_order.setCardNumber(rng.Next(5000, 7500));
            new_order.setQuantity(numberOfTickets(args.price, args.old_price));
            //new_order.setRecieverId(name);
            new_order.setSenderId(id);
            new_order.setUnitPrice(args.price);
            //Console.WriteLine($"Broker : {id} has placed an order to {name}");
            // send to buffer
            Program.buf.setOneCell(new_order);
        }

        public int numberOfTickets(int price, int old_price)
        {
            //Console.WriteLine($"old = {old_price}, new = {Theater.getPrice()}");
            int amount = old_price - price;
            //Console.WriteLine($"amoutn = {amount}");
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
           //Console.WriteLine($"{order.getSenderId()} is waiting to set a cell");
            cell_pool.WaitOne(300);
            lock (_locker)
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
                        //Console.WriteLine("getting a cell");
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
