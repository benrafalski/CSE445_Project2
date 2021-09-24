using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CSE445_Project2
{
    public class MultiCellBuffer
    {
        private OrderClass[] cell;
        // creates two semaphore to track buffer availabality
        // uses solution from the producer - consumer problem with semaphores
        Semaphore _buf_size = new Semaphore(0, 2);
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
            // blocks the thread if there open cells semaphore is 0
            // subtracts one from this semaphore otherwise
            if (_open_cells.WaitOne(300) == false)
            {
                // waits 300ms 
                // if there is not room then return false
                return 0;
            }
            lock (cell)
            {
                // updates the cell
                cell[_in] = order;
                // array gets filled in in a circular fashion
                _in = (_in + 1) % 2;
            }
            // adds 1 to the buffer size semaphore once a cell is set
            _buf_size.Release();
            return 1;
        }

        public OrderClass getOnecell()
        {

            OrderClass cell_order = null;
            // blocks thread if the buffer size semaphore is 0
            // subtracts one from the semaphore otherwise
            if (_buf_size.WaitOne(300) == false)
            {
                // waits for 300ms
                // if there is no cells then returns null
                return null;
            }
            lock (_locker)
            {

                cell_order = cell[_out];
                cell[_out] = null;
                _out = (_out + 1) % 2;
            }
            // adds one to the open cells semaphore once an order has been removed
            _open_cells.Release();
            return cell_order;

        }
    }
}
