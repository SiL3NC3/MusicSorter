using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSorter.Classes
{
    internal class Error
    {
        public Exception Exception { get; set; }
        public Error(Exception ex)
        {
            Exception = ex;
        }

    }
}
