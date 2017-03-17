using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetMT
{
    public static class PluginTest
    {
        public static void DoTest()
        {
            Core.print("PluginTest.DoTest!");
        }
    }

    public static class StructTest
    {
        public struct TST
        {
            public int x;
            public int y;
        }

        public static TST Test0(string message)
        {
            Core.print(message);
            var t = new TST();
            t.x = 10;
            t.y = 15;
            return (t);
        }

        public static void Test1(ref bool message)
        {
            message = true;
        }

        public static void DoTest()
        {
            Core.print("ReturnStructTest.DoTest!");
            Test0("Test0-message");
            bool bReturn = false;
            Test1(ref bReturn);
            if (bReturn)
                Core.print("bReturn=true!");            
        }
    }
}
