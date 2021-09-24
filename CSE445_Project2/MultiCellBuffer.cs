using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

// student: Benjamin Rafalski
// ASUID: 1216740421

namespace CSE445_Project2
{
    public class MultiCellBuffer
    {
        private OrderClass[] cell;
        // creates two semaphore to track buffer availabality
        // uses solution from the producer - consumer problem with semaphores
        Semaphore _open_cells = new Semaphore(2, 2);
        // use a lock mechanism
        object _locker = new object();
        public int _in = 0, _out = 0;
        public MultiCellBuffer()
        {
            // cells, 2 for individual
            cell = new OrderClass[2];
            cell[0] = null;
            cell[1] = null;
        }

        public int setOneCell(OrderClass order)
        {
            // blocks the thread if the semaphore is 0
            // subtracts one from this semaphore otherwise
            if (_open_cells.WaitOne(100) == false)
            {
                // waits 300ms 
                // if there is not room then return false
                return 0;
            }
            lock (cell)
            {
                // updates first available cell (starts at index 0)
                cell[_in] = order;
                // index moves to the next index or resets
                _in = (_in + 1) % 2;
            }
            // returns true if the cell was set
            return 1;
        }

        public OrderClass getOnecell()
        {

            OrderClass cell_order = null;
            
            // checks if there is no current orders
            if(cell[0] == null && cell[1] == null)
            {
                return null;
            }
            lock (_locker)
            {
                // gets the first availabale cell (starts at 0)
                cell_order = cell[_out];
                cell[_out] = null;
                // index moves to the next index or resets
                _out = (_out + 1) % 2;
            }
            // adds one to the semaphore once an order has been removed
            _open_cells.Release();
            return cell_order;

        }
    }
}
