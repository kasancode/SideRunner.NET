using System;
using System.Collections.Generic;
using System.Text;

namespace Sider
{
    public class SeleniumAssertionException : Exception
    {
        public SeleniumAssertionException() :
            base()
        {
        }
        public SeleniumAssertionException(string message) :
            base(message)
        {
        }
    }
}
