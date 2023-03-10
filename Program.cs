using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ConsoleApp
{
    internal class Program
    {
        static System.IO.Ports.SerialPort serialPort;
        string message;

        

        static void Main(string[] args)
        {
            string fileNameSerialConfig = "serial.conf";
            string serialPortName = "";

            //Introduksjon /TCP-port
            Console.WriteLine("instrumentBE has started...");
            Console.WriteLine("Please enter TCP Port number:");
            string serverPort = Console.ReadLine();

            try
            {
                int Portnumber = Convert.ToInt32(serverPort);
            }
            catch (FormatException)
            {
                Console.WriteLine("Portnumber not a number!");
                return;
            }

            // serial configuration load from file
            StreamReader serialConfReader = new StreamReader(fileNameSerialConfig);
            serialPortName = serialConfReader.ReadLine();
            Console.WriteLine("Serial Port Confiqured: " + serialPortName);
            serialConfReader.Close();

            string commandResponse = SerialCommand("COM3", "readscaled");
            Console.WriteLine(commandResponse);
            //commandResponse = SerialCommand("COM3", "readconf");
            //Console.WriteLine(commandResponse);
            //Console.ReadKey();
            /*
            serialPort = new System.IO.Ports.SerialPort();
            serialPort.PortName = "COM3";
            serialPort.BaudRate = 9600;
            serialPort.Parity = System.IO.Ports.Parity.None;
            serialPort.DataBits = 8;
            serialPort.Handshake = System.IO.Ports.Handshake.None;
            serialPort.DataReceived += dataReceived;
            */
            

            //TCP Server start
            //make an endpoint for communication:
            string serverIP = "127.0.0.1";
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 5000);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //bind to endpoint and start server
            server.Bind(endpoint);
            server.Listen(10);
            Console.WriteLine("Server started. Waiting for clients...");

            while (true) //Keep listening for clients connecting
            {
                //accept connecting clients
                Socket client = server.Accept();
                Console.WriteLine("Client connected.");
                //if (log) WriteToLogFile("Client connected.");

                //recieved data
                byte[] buffer = new byte[1024];
                int bytesRecieved = client.Receive(buffer);
                string commandReceived = Encoding.ASCII.GetString(buffer, 0, bytesRecieved);
                Console.WriteLine("Recieved message: " + commandReceived);

                if (commandReceived.Substring(0,7) == "comport") //comport:COM__
                {
                    serialPortName = commandReceived.Substring(8, commandReceived.Length - 8);
                    Console.WriteLine("Serial port configured: " + serialPortName);
                    StreamWriter serialConfWrite = new StreamWriter(fileNameSerialConfig);
                    serialConfWrite.WriteLine(serialPortName);
                    serialConfWrite.Close();

                    client.Send(Encoding.ASCII.GetBytes("Serial Port Configured: " + serialPortName));
                }
                else
                {
                    //legg mesteparten av resten av koden hit
                }

                //if (log) WriteToLogFile("Recieved message: " + commandRecieved);
                string oppbevaring = "null";

                //send to Arduino
                if (commandReceived != "")
                {
                    //serialPort.Open();

                    //serialPort.WriteLine("readconf");
                    //Console.WriteLine("value:" + serialPort.ReadLine());
                    commandResponse = SerialCommand("COM3", commandReceived);
                    Console.WriteLine("This came: " + commandResponse);

                    //oppbevaring = serialPort.ReadLine();

                    //client.Send(Encoding.ASCII.GetBytes("Value back: " + oppbevaring));

                    //client.Send(Encoding.ASCII.GetBytes("Value back!"));

                    //Console.WriteLine("value1: " + oppbevaring);

                    //serialPort.Close();
                }
                else
                {
                    Console.WriteLine("nothing came thrue");
                }

                //Console.WriteLine("Value2: " + oppbevaring);
                
                //return recieved data to server
                client.Send(Encoding.ASCII.GetBytes("Command recieved was: " + commandResponse));
                client.Close();
                Console.WriteLine("Client disconnected.");
                //if (log) WriteToLogFile("Client disconnected.");
            }
        }

        static string SerialCommand(string portName, string command)
        {
            int baudRate = 9600;
            string serialResponse = "";
            SerialPort serialport = new SerialPort(portName, baudRate);
            try
            {
                serialport.Open();
                serialport.WriteLine(command);
                serialResponse= serialport.ReadLine();
                serialport.Close();
            }
            catch(System.IO.IOException)
            {
                serialResponse = "SerialPort failed...";
            }
            return serialResponse;
            //string message = serialPort.ReadLine();
            //Console.WriteLine(message);
            //client.Send(Encoding.ASCII.GetBytes("Value back: " + message));
        }

    }
}