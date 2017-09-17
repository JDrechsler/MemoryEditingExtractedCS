using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace WPM35CS
{
    internal class Options
    {
        [Option('p', "processName", DefaultValue = "Tutorial-x86_64", HelpText = "ProcessName.")]
        public string ProcessName { get; set; }

        [Option('a', "address", Required = true, HelpText = "Address in Hex like this 4355F without 0x.")]
        public string Address { get; set; }

        [Option('b', "newBytes", Required = true, HelpText = "new bytes in hex like this '90 80 70 60'")]
        public string BytesToWrite { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    internal class Program
    {
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr handleToProcess, IntPtr pointerToBaseAddress, byte[] pointerToBufferReceiver, int sizeOfBufferReceiver, int justPutNull);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr handleToProcess, IntPtr pointerToBaseAddress, byte[] pointerToBufferSender, int sizeBytesToWrite, int justPutNull);

        private static void Main(string[] args)
        {
            //string exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            Options options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                
                // Values are available here
                Console.WriteLine($"ARG ProcessName: {options.ProcessName}");
                Console.WriteLine($"ARG Address: {options.Address}");
                Console.WriteLine($"ARG NewBytes: {options.BytesToWrite}");
            }

            string processName = options.ProcessName;
            IntPtr address = (IntPtr)int.Parse(options.Address, NumberStyles.HexNumber);
            string bytesToWrite = options.BytesToWrite;

            Console.WriteLine($"ProcessName: {processName}");
            Console.WriteLine($"Address: {address.ToString("X")}");
            Console.WriteLine($"bytesToWrite: {bytesToWrite}");

            List<byte> byteArray = new List<byte>();
            string[] bytesString = bytesToWrite.Split(' ');

            foreach (string s in bytesString)
            {
                byte newByte = byte.Parse(s, NumberStyles.HexNumber);
                byteArray.Add(newByte);
            }

            byte[] dataBuffer = byteArray.ToArray();
            
            Process[] foundprocesses = Process.GetProcessesByName(processName);
            Process process = foundprocesses.FirstOrDefault();

            if (process == null)
            {
                Console.WriteLine($"Could not find process: {processName}");
                Environment.Exit(1);
            }
            
            
            Console.WriteLine("Process found");
            try
            {
                IntPtr processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
                int readValue = GetReadProcessMemory(processHandle, address);
                Console.WriteLine($"Read value: {readValue}");
 
                
                bool writePmWorked = GetWriteProcessMemory(processHandle, address, dataBuffer);
                if (writePmWorked)
                {
                    Console.WriteLine($"WritePM worked!!");
                }
                else
                {
                    Console.WriteLine($"WritePM didn't work!!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }
            

            Console.Read();
        }

        private static int GetReadProcessMemory(IntPtr processHandle, IntPtr pointerToAddress)
        {
            byte[] buffer = new byte[1];

            try
            {
                bool tryReadingMemory = ReadProcessMemory(processHandle, pointerToAddress, buffer, buffer.Length, 0);
                if (!tryReadingMemory)
                {
                    Console.WriteLine("Could not read memory.");
                }
                else
                {
                    return buffer.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
            return 1;
        }

        private static bool GetWriteProcessMemory(IntPtr processHandle, IntPtr pointerToAddress, byte[] dataBuffer)
        {
            try
            {
                bool tryWritingMemory = WriteProcessMemory(processHandle, pointerToAddress, dataBuffer, dataBuffer.Length, 0);
                if (!tryWritingMemory)
                {
                    Console.WriteLine("Could not write memory.");
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return false;
        }
    }
}
