using System;
using Burrows.Configuration;
using Burrows.Log4NetIntegration;
using Burrows.PublisherConfirms;
using Burrows.Tests.Messages;

namespace Burrows.Tests.PublishConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            PublishSettings publishSettings = null;

            var serviceBus = ServiceBusFactory.New(sbc =>
                {
                    sbc.ReceiveFrom(@"rabbitmq://localhost/PublishConsole");
                    publishSettings = sbc.UsePublisherConfirms().WithFileBackingStore();
                    sbc.UseControlBus();
                    sbc.UseLog4Net();
                });

            var publisher = new Publisher(serviceBus, publishSettings);

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
