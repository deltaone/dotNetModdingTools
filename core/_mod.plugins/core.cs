using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace dotNetMT
{
    public static class CORE
    {
        public static void Print(string message)
        {
            Console.WriteLine(message);
        }

        public static bool IsCanUseHotKeys()
        {
            //if (!Main.blockInput && !Main.drawingPlayerChat && !Main.editSign && !Main.editChest) return(true);
            return (true);
        }
    }
}
