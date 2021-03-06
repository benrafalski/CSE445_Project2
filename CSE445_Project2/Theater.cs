using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

// student: Benjamin Rafalski
// ASUID: 1216740421

namespace CSE445_Project2
{
    // server side
    // uses a pricing model to calculate ticket prices
    // emits an event if the ticket price has decreased
    // this calls the subscriber's event handlers
    // recieves orders from the multicell buffer
    // creates an order processing thread to process the order
    public class Theater
    {
        public static event promotionalEvent promotion;
        private static Random rng = new Random();
        public static object _locker = new object();
        private static int demand;
        private static int orders;
        private static int counter_t;
        private static double ticketPrice;

        public Theater()
        {
            // constructor
            counter_t = 0;
            ticketPrice = 120; // initial ticket price
            orders = 0;
            demand = 0;
        }
        
        // price cut event emitter
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
                    promotion(this, new PriceCutEventArgs() { price = price, old_price = ticketPrice }); // calls event handler
                    counter_t++; // increase the number of orders made
                }
                // waits for 1 second for brokers to place their orders if they have any  
                Thread.Sleep(1000);
                checkForOrders();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"This price cut had {demand} total order(s)");
                Console.ForegroundColor = ConsoleColor.White;
                demand = 0;
            }
            // updates price
            ticketPrice = price;
        }

        // starts theater thread
        // determine ticket prices
        public void PricingModel()
        {
            while (counter_t < 20)
            {

                double price;
                // dynamically calculates ticket price for the ticket brokers
                // calculates a random number between 1-10
                // if the number is even then there is a price cut
                // otherwise there is a price increase
                // uses a lock mechanism to ensure print statements do not get interupted
                lock (_locker)
                {
                    if (rng.Next(1, 10) % 2 == 0)
                    {
                        // price can decrease
                        double discount = rng.Next(0, 100) * 0.2;
                        // if there is not discount then discount is 5;
                        if (discount - 1 <= 0)
                        {
                            discount += 5;
                        }
                        price = ticketPrice - discount;

                        Console.WriteLine($"NEW PRICE CHANGE: current price decreased by {String.Format("{0:0.00}", discount)} to {String.Format("{0:0.00}", price)}\n");
                    }
                    else
                    {
                        // price can increase
                        double increase = rng.Next(0, 100) * 0.2;
                        price = ticketPrice + increase;
                        Console.WriteLine($"NEW PRICE CHANGE: current price increased by {String.Format("{0:0.00}", increase)} to {String.Format("{0:0.00}", price)}\n");
                    }
                    // checks if the price is between 200 and 40
                    // if it is out of range the price is reset to 120
                    if (price > 200 || price < 40)
                    {
                        if (price > 200)
                        {
                            Console.WriteLine("PRICE OUT OF RANGE: price decreased to 120\n");
                        }
                        else
                        {
                            Console.WriteLine("PRICE OUT OF RANGE: price increased to 120\n");
                        }

                        price = 120;
                    }
                    changePrice(price);
                    
                }
            }
            // theater terminates after 20 price cuts / orders have been made
            Thread.Sleep(1000); // waits for final orders to be processed
            Console.WriteLine($"Theater terminated with {counter_t} price decreases and {orders} total orders");
        }


        // order processing thread entry point
        public static void orderProcessing(object order)
        {
            // creates order object
            OrderClass orderObject = (OrderClass)order;

            // check valitity of cc number
            // cc number must be between 5000 and 7000
            // and maximum number of tickets allowed for purchase
            // this is locked because the orders counter had 
            // problems if there was an interupt during this portion of code
            lock (_locker)
            {
                orders++;
                if (orderObject.CardNumber >= 5000 && orderObject.CardNumber <= 7000)
                {
                    // order can not have more than 25 tickets
                    if(orderObject.Quantity > 25)
                    {
                        Console.WriteLine($"################################################\n" +
                        $"Order #{orders};\norder from {orderObject.Sender};\n" +
                        $"for {orderObject.Quantity} tickets has been rejected;\n" +
                        $"Order can have at must 25 tickets;\n" +
                        $"Order status: REJECTED\n" +
                        $"################################################\n");
                    }
                    else
                    {
                        // calculates the total amount for a valid the order
                        double charge_amt = orderObject.UnitPrice * orderObject.Quantity; // price * number of tickets
                        double after_tax = charge_amt * 0.10; // 10% tax before location charge
                        double grand_total = charge_amt + after_tax + 10; // theater changes a 10 dollar location fee

                        // prints order info on screen
                        Console.WriteLine($"################################################\n" +
                            $"Order #{orders};\nfrom {orderObject.Sender};\n" +
                            $"with card ending in *{orderObject.CardNumber}*;\n" +
                            $"has been processed for ${String.Format("{0:0.00}", orderObject.UnitPrice)} per ticket with {orderObject.Quantity} tickets;\n" +
                            $"for a total : ${String.Format("{0:0.00}", grand_total)};\n" +
                            $"Order status: COMPLETED\n" +
                            $"################################################\n");
                    }
                }
                else
                {
                    // prints order rejection on screen
                    Console.WriteLine($"################################################\n" +
                        $"Order #{orders};\norder from {orderObject.Sender};\n" +
                        $"credit card ending in *{orderObject.CardNumber}* rejected;\n" +
                        $"Theater does not accept this credit card number;\n" +
                        $"Order status: REJECTED\n" +
                        $"################################################\n");
                }
            }
        }

        // theater checks for orders from the multicell buffer
        // starts an order processing thread for each order in the buffer
        public void checkForOrders()
        {
            //int count = counter_t;

            // checks both cells in the buffer
            for (int i = 0; i < 2; i++)
            {
                // gets one cell
                OrderClass order = Program.buf.getOnecell();
                Thread.Sleep(500); // time spent to process the order
                // if there was an order recieved
                // start a new order processing thread
                if (order != null)
                {
                    demand++;
                    Thread orderThread = new Thread(new ParameterizedThreadStart(orderProcessing));
                    orderThread.Name = "{recieved order thread}";
                    orderThread.Start(order);
                }
                // if there was no order found
                // wait for 500ms before checking again
                else
                {
                    Thread.Sleep(500);
                }   
            }
            // waits to process the orders
            Thread.Sleep(1000);
        }
    }
}
