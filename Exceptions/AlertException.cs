using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPrint.Exceptions
{
    public class AlertException : Exception
    {
        public AlertException(string message) : base(message) { 

        }
    }
}
