using System;
using Burrows.Tests.Messages;

namespace Burrows.Tests.PublishConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("To publish a message, type a message count and hit enter");
            Console.WriteLine();
            Console.WriteLine();

            string input;

            while (!string.IsNullOrEmpty(input = Console.ReadLine()))
            {
                var msg = new SimpleMessage {Id = "testId", Name = "TestName"};
            	
            }
        }
        
    }
}
