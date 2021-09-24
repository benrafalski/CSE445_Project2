using System;
using System.Collections.Generic;
using System.Text;

namespace CSE445_Project2
{
    // order class object
    // used to send orders in the buffer
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
            set { cardNo = value; }
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
