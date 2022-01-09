using System;

namespace PROIECTfizica
{
    class Program
    {
        static void Main(string[] args)
        {
            BluetoothModule  module = new BluetoothModule();
            module.Conect().GetAwaiter().GetResult();
        }
    }
}
