using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CSE445_Project2
{
    // client

    public class TicketBroker
    {

        public static string id;
        public Random rng = new Random();
        public OrderClass new_order = new OrderClass();


        // entry point for the brokers
        // each broker has its own function call
        public void brokerFunc()
        {
            // thread loops until there has been 20 price cuts
            while (Program.theater.Orders < 20)
            {
                // waits for 500 ms and updates ID
                new_order.Sender = Thread.CurrentThread.Name;
                Thread.Sleep(500);
            }
            Console.WriteLine($"{Thread.CurrentThread.Name} terminated \t");
        }

        // event handler
        // generates an order object and places it into the buffer
        public void promotionalEvent(object source, PriceCutEventArgs args)
        {
            // sets the remaining values for the order
            new_order.CardNumber = rng.Next(5000, 7500);
            new_order.Quantity = (int)(args.old_price - args.price) + rng.Next(0, 10);
            new_order.UnitPrice = args.price;
            // 50% chance the broker will choose to place an order
            if (rng.Next(0, 10) % 2 == 0)
            {
                int order_placed = Program.buf.setOneCell(new_order);
                // checks if the order has been sent into the buffer
                // or if the buffer was already full
                if (order_placed == 1)
                {
                    Console.WriteLine($"{new_order.Sender} successfully sent an order for this price cut...");
                }
                else
                {
                    Console.WriteLine($"{new_order.Sender} price cut buffer is full, order unable to be processed, sorry...");
                }
            }
            else
            {
                Console.WriteLine($"{new_order.Sender} did not place an order for this price cut...");
            }
        }
    }
}
