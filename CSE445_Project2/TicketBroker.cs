using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

// student: Benjamin Rafalski
// ASUID: 1216740421

namespace CSE445_Project2
{
    // client
    // evaluates needs based on new price and other factors
    // sends an order object to the multicell buffer
    // if there is a free cell open
    public class TicketBroker
    {
        public Random rng = new Random();
        public OrderClass new_order = new OrderClass();


        // entry point for the brokers
        // each broker has its own function call
        public void brokerFunc()
        {
            // thread loops until the theater terminates
            while (Program.theaterThread.IsAlive)
            {
                // waits for 500ms and updates ID
                new_order.Sender = Thread.CurrentThread.Name;
                Thread.Sleep(500);
            }
            Thread.Sleep(500); // waits for 500ms so the theater can terminate
            Console.WriteLine($"{Thread.CurrentThread.Name} terminated \t");
        }

        // event handler / callback method called by the theater class
        // generates an order object and places it into the buffer
        public void promotionalEvent(object source, PriceCutEventArgs args)
        {
            Theater t = (Theater)source;
            int demand, price_difference;
            // sets the remaining values for the order
            new_order.CardNumber = rng.Next(5000, 7500); // random number, if < 7000 it is rejected
            // number of tickets to order
            // is the difference in the old price and new price
            // plus the demand (some random number)
            price_difference = (int)(args.old_price - args.price);
            demand = rng.Next(0, 10);
            new_order.Quantity =  price_difference + demand;
            new_order.UnitPrice = args.price; // current ticket price
            // 50% chance the broker will choose to place an order
            if (rng.Next(0, 10) % 2 == 0)
            {
                int order_placed = Program.buf.setOneCell(new_order);
                // checks if the order has been sent into the buffer
                // or if the buffer was already full
                if (order_placed == 1)
                {
                    // successfully sent the order
                    Console.WriteLine($"{new_order.Sender} successfully sent an order for this price cut...");
                }
                else
                {
                    // requests the theater to make room in the buffer 
                    t.checkForOrders();
                    // waits for 500 ms for the buffer to be cleared
                    Console.WriteLine($"{new_order.Sender} price cut buffer is full, waiting for cells to open...");
                    Thread.Sleep(500);
                    if(Program.buf.setOneCell(new_order) == 1)
                    {
                        // success
                        Console.WriteLine($"{new_order.Sender} successfully sent an order for this price cut...");
                    }
                    
                }
            }
            else
            {
                // brokers do not have to place an order each time
                Console.WriteLine($"{new_order.Sender} did not place an order for this price cut...");
            }
        }
    }
}
