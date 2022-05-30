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

    public class ControlFlowSyntaxException : Exception
    {
        public int Index { get; }

        public ControlFlowSyntaxException() : base() { }

        public ControlFlowSyntaxException(string message, int index) :
        base(message)
        {
            this.Index = index;
        }
    }
}
