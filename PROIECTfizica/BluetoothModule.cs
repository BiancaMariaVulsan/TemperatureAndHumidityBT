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
        private readonly int _packageEnd = 48;
        internal void Conect()
        {
            BluetoothClient client = new BluetoothClient();
            Console.WriteLine("Searching devices...");
            IReadOnlyCollection<BluetoothDeviceInfo> devices = client.DiscoverDevices();
            Console.WriteLine("Done.");

            foreach (BluetoothDeviceInfo d in devices)
            {
                if (d.DeviceName.Equals(_arduinoBoardName))
                {
                    EstablishConnection(d, client);
                    break;
                }
            }
        }

        private void EstablishConnection(BluetoothDeviceInfo device, BluetoothClient client)
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
                    ReadData(client);
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                Console.WriteLine("Connection failed!");
                client.Close();
                return;
            }
        }

        private void ReadData(BluetoothClient client)
        {
            while(!client.GetStream().DataAvailable)
            {
            }

            while (client.GetStream().DataAvailable)
            {
                while(client.GetStream().ReadByte() != _packageStart)
                {
                }

                int temperature = client.GetStream().ReadByte();
                int humidity = client.GetStream().ReadByte();

                Console.WriteLine($"Temperature: {temperature}");
                Console.WriteLine($"Humidity: {humidity}");

                if (client.GetStream().ReadByte() != _packageEnd)
                {
                    Console.WriteLine("Error while reading...");
                }
                Task.Delay(2000);
            }      
        }
    }
}
