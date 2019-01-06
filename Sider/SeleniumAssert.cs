using System;
using System.Collections.Generic;
using System.Text;

namespace Sider
{
    public static class SeleniumAssert
    {
        public static void AreSame<T>(T expected, T actual)
        {
            if (!((expected == null && actual == null) || (expected?.Equals(actual) ?? false)))
            {
                throw new SeleniumAssertionException($"Excected '{expected}' but '{actual}'.");
            }
        }
    }
}
