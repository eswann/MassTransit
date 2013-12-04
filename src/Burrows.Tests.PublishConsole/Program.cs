using System;
using System.Collections.Generic;

namespace Burrows.Tests.PublishConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("To publish a message, type <topic>,<payload> and hit <enter>");
            Console.WriteLine();
            Console.WriteLine();

            string input;

            while (!string.IsNullOrEmpty(input = Console.ReadLine()))
            {

            	var fields = input.Split(new char[] {','});
            	if (fields.Length != 2)
            		throw new ArgumentException();

            	var topicStr = fields[0];

            	var itemToDeliver = new TestComplexObject() {Id = 1, Name = "Test"};
            	var serializedObject = JsonConvert.SerializeObject(itemToDeliver);

            	var msg = new EmailNotification(topicStr, new MessagePublishingContext {LocaleCode = "en-US", SiteId = 1782, TenantId = 12478, UserId = "123"}, serializedObject)
            	          	{
            	          		Payload =
            	          			{
										To = new List<NotificationRoutingPair>(new[] {new NotificationRoutingPair("Wayne Hearn", "wayne_hearn@volusion.com"), }),
            	          				Sender = new NotificationRoutingPair("Santa Claus", "santa_man@volusion.com")
            	          			}
            	          	};
            	/*var msg = new EntityNotification()
            	          	{
            	          		EntityId = "1234567898",
            	          		MessagePublishingContext = new MessagePublishingContext
            	          		                           	{
            	          		                           		CreateBy = "foobar",
            	          		                           		CreateDate = DateTime.UtcNow,
																SiteId = 1,
																TenantId = 1,
																UserId = "12345"
            	          		                           	},
            	          		Topic = new Topic("order").SubTopic("Created")
            	          	};*/
				{
					//Payload = { Sender = new NotificationRoutingPair("Santa Claus", "santa_man@volusion.com") }
				};

            	/*var toList = new List<NotificationRoutingPair>();
            	toList.Add(new NotificationRoutingPair("Wayne Hearn", "wayne_hearn@volusion.com"));
            	msg.Payload.To = toList;*/

//				using (var bus = RabbitHutch.CreateBus(ConfigurationManager.ConnectionStrings["messagequeue"].ConnectionString))
//				{
//					var emailPublisher = new EmailPublisher(bus);
//					emailPublisher.Publish(msg);
//
//					/*var publisher = new EntityNotificationPublisher(bus);
//					publisher.Publish(msg);*/
//					
//					Console.WriteLine("Posted message topic = {0}, payload = {1}.", msg.Topic, msg);
//					Console.WriteLine();
//				}
            }
        }


            //using (var pubSub = RabbitHutch.CreateBus(ConfigurationManager.ConnectionStrings["messagequeue"].ConnectionString, new Logger()))
            //{
            //    Console.WriteLine("To publish a message, type <topic>,<payload> and hit <enter>");
            //    Console.WriteLine();
            //    Console.WriteLine();

            //    string input;
            //    while (!string.IsNullOrEmpty(input = Console.ReadLine()))
            //    {

            //        var fields = input.Split(new char[] { ',' });
            //        if (fields.Length != 2)
            //            throw new ArgumentException();

            //        //var topicStr = fields[0];
            //        //var payload = fields[1];
            //        //var msg = new Message() { Topic = topicStr, PayloadVersion = "1.0", Payload = payload };
            //        //pubSub.Publish<Message>(msg.Topic, msg);
            //        ////pubSub.Publish<Message>(msg);

            //        var topicStr = fields[0];
            //        //var payload = fields[1];

            //        var itemToDeliver = new TestComplexObject() { Id = 1, Name = "Test"};
            //        var serializedObject = JsonConvert.SerializeObject(itemToDeliver);

            //        var msg = new EmailNotification("TestComplexObject", new MessagePublishingContext{LocaleCode = "en-US", SiteId = 1, TenantId = 1, UserId = "123"}, serializedObject)
            //                    {
            //                        Payload = {Sender = new NotificationRoutingPair("Santa Claus", "santa_man@volusion.com")}
            //                    };

            //        var toList = new List<NotificationRoutingPair>();
            //        toList.Add(new NotificationRoutingPair("Jonathan Gill", "jonathan_gill@volusion.com"));
            //        msg.Payload.To = toList;

            //        //using (var pubChannel = pubSub.OpenPublishChannel())
            //        //{
            //        //    pubChannel.Publish<string>(topicStr, "Test");
            //        //}

            //        using (var pubChannel = pubSub.OpenPublishChannel())
            //        {
            //            pubChannel.Publish<EmailNotification>(topicStr, msg);
            //            //throw new Exception("Test exception");
            //        }

            //        Console.WriteLine("Posted message topic = {0}, payload = {1}.", topicStr, msg);
            //        Console.WriteLine();
            //    }
            //}

            // Original PubSub Wrapper Code: 
            //using (var pubSub = MessageQueueFactory.Create())
            //{
            //    Console.WriteLine("To publish a message, type <topic>,<payload> and hit <enter>");
            //    Console.WriteLine();
            //    Console.WriteLine();

            //    string input;
            //    while (!string.IsNullOrEmpty(input = Console.ReadLine()))
            //    {

            //        var fields = input.Split(new char[] { ',' });
            //        if (fields.Length != 2)
            //            throw new ArgumentException();

            //        //var topicStr = fields[0];
            //        //var payload = fields[1];
            //        //var msg = new Message() { Topic = topicStr, PayloadVersion = "1.0", Payload = payload };
            //        //pubSub.Publish<Message>(msg.Topic, msg);
            //        ////pubSub.Publish<Message>(msg);

            //        var topicStr = fields[0];
            //        //var payload = fields[1];
            //        var msg = new EmailNotification();
            //        msg.Sender = new NotificationRoutingPair("Santa Claus", "santa_man@volusion.com");
            //        var payload = new EmailData();
            //        payload.Subject = fields[0];
            //        payload.TextBody = fields[1];

            //        var toList = new List<NotificationRoutingPair>();
            //        toList.Add(new NotificationRoutingPair("Jonathan Gill", "jonathan_gill@volusion.com"));
            //        payload.To = toList;

            //        msg.Payload = payload;

            //        pubSub.Publish<EmailNotification>(msg.Payload.Subject, msg);
            //        //pubSub.Publish<Message>(msg);

            //        Console.WriteLine("Posted message topic = {0}, payload = {1}.", topicStr, msg);
            //        Console.WriteLine();

            //    }
            //}
        
    }
}
