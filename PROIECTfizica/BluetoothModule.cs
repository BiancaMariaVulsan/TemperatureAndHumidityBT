using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;
using System.Timers;
using System.Threading.Tasks;

namespace PROIECTfizica
{
    class BluetoothModule
    {
        private readonly string _arduinoBoardName = "HC-05";
        private readonly int _packageStart = 36;
        private readonly int _packageEnd = 70;
        internal async Task Conect()
        {
            BluetoothClient client = new BluetoothClient();
            Console.WriteLine("Searching devices...");
            IReadOnlyCollection<BluetoothDeviceInfo> devices = client.DiscoverDevices();
            Console.WriteLine("Done.");

            foreach (BluetoothDeviceInfo d in devices)
            {
                if (d.DeviceName.Equals(_arduinoBoardName))
                {
                    await EstablishConnection (d, client);
                    break;
                }
            }
        }

        private async Task EstablishConnection(BluetoothDeviceInfo device, BluetoothClient client)
        {
            var serviceClass = BluetoothService.SerialPort;

            if (device == null)
            {
                return;
            }

            try
            {
                if (!device.Connected)
                {
                    Console.WriteLine("Connecting...");
                    client.Connect(device.DeviceAddress, serviceClass);
                    Console.WriteLine("Connected");

                    /*Timer dispatcherTimer = new Timer();
                    dispatcherTimer.Elapsed += (sender, e) => dispatcherTimer_Tick(sender, e, client);
                    dispatcherTimer.Interval = 250;
                    dispatcherTimer.Enabled = true;
                    dispatcherTimer.Start();*/
                    await ReadData(client);
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                Console.WriteLine("Connection failed!");
                client.Close();
                return;
            }
        }

        private async Task ReadData(BluetoothClient client)
        {
            while(!client.GetStream().DataAvailable)
            {
            }

            while (true)
            {
                while(client.GetStream().ReadByte() != _packageStart)
                {
                }

                int temperature = client.GetStream().ReadByte();
                int humidity = client.GetStream().ReadByte();

                // build the number (each bype a digit)
                int digit;
                int luminosityNrDigits = client.GetStream().ReadByte();
                int luminosity = 0;

                while(luminosityNrDigits > 0)
                {
                    digit = client.GetStream().ReadByte();
                    luminosity = digit * (int)Math.Pow(10, luminosityNrDigits-1) + luminosity;
                    luminosityNrDigits--;
                }

                int flameNrDigits = client.GetStream().ReadByte();
                int dg = flameNrDigits;
                int flame = 0;

                while (flameNrDigits > 0)
                {
                    digit = client.GetStream().ReadByte();
                    flame = digit * (int)Math.Pow(10, flameNrDigits-1) + flame;
                    flameNrDigits--;
                }


                Console.WriteLine($"Temperature: {temperature}");
                Console.WriteLine($"Humidity: {humidity}");
                Console.WriteLine($"Luminosity: {luminosity}");
                Console.WriteLine($"Flame: {flame}");

                if (client.GetStream().ReadByte() != _packageEnd)
                {
                    Console.WriteLine("Error while reading...");
                    Console.WriteLine($"flame digits:{dg}");
                    Console.WriteLine($"flame:{flame}");
                }
                await Task.Delay(2000);
            }      
        }
    }
}
