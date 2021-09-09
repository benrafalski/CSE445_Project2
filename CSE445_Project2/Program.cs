using System;
using System.Threading;

namespace CSE445_Project2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //Theater show = new Theater();
            //Thread broker = new Thread(new ThreadStart(show.brokerFunc));
            
           //broker.Start();
            //broker.Name = "broker";
            

            TicketBroker broker new TicketBroker();
            Thread priceModel = new Thread(new ThreadStart(broker.pricingmodel));
            priceModel.Start();

            Thread order = new Thread(new ThreadStart(broker.orderProcessor));
            order.Start();



            Theater.promotion += new promotionalEvent(broker.pro);


        }
    }

    public delegate void promotionalEvent();


    public class Theater
    {
        int counter_t = 0;
        public static event promotionalEvent promotion;
        static Random rng = new Random();
        public Theater()
        {
            // constructor
            counter_t = 0;
        }

        // method to be started as a thread
        public void startTheater()
        {
            for(; counter_t < 20; counter_t++)
            {
                Thread.Sleep(500);

            }
        }



        // determine ticket prices

        // event stuff

        // 
    }

    public class TicketBroker
    {
        public TicketBroker()
        {
            // generates order object
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
