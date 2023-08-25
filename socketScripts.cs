using System;
using System.IO.Ports;

namespace EmpaticaE4BluetoothExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to the Empatica E4 device
            SerialPort serialPort = new SerialPort("28000", 115200);
            serialPort.Open();

            // Send the command to start streaming data
            byte[] command = new byte[] { 0x02, 0x02, 0x0D };
            serialPort.Write(command, 0, command.Length);

            // Read the response from the device
            byte[] response = new byte[4];
            serialPort.Read(response, 0, response.Length);

            // Print the response
            Console.WriteLine("Response: " + BitConverter.ToString(response));

            // Close the serial port
            serialPort.Close();
        }
    }
}