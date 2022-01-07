using System;
using System.Collections.Generic;
using System.Linq;

namespace ImagesToPdfConverter
{
    public class Program
    {
        public static void Main()
        {
            ToPDF toPDF = new ToPDF();
            toPDF.StartTimer();
        }
    }
}
