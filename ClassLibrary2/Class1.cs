using System;

namespace ClassLibrary2
{
    public class Class1
    {
        public bool IsValidBarcode(string barcode)
        {
            return barcode.StartsWith("BB");
        }

        public void DoStuff(string barcode)
        {

        }
    }
}
