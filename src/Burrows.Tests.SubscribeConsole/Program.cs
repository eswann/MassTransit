namespace Burrows.Tests.SubscribeConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
        //    using (var pubSub = RabbitHutch.CreateBus(ConfigurationManager.ConnectionStrings["messagequeue"].ConnectionString, new RabbitLogger()))
        //    {
        //        var fields = GatherInput();
        //        var subscriptionId = fields[0];
        //        var topic = fields[1];
        //        Console.WriteLine();

        //        //Thread thread = null;
        //        //var ts = new ThreadStart(Execute);
        //        //thread = new Thread(ts);
        //        //thread.Start();

        //        //pubSub.Subscribe<EmailNotification>(subscriptionId, new Topic(EmailNotification.PrimaryTopic).SubTopic(topic).ToString(), msg =>
        //        //{
        //        //    Console.WriteLine(String.Format("Message = {0}", msg));
        //        //    Console.WriteLine();
        //        //});

        //        //pubSub.SubscribeAsync<EmailNotification>(subscriptionId, topic, msg =>
        //        //{
        //        //    return Task.Factory.StartNew(() =>
        //        //    {
        //        //        Console.WriteLine(String.Format("Message = {0}, Payload = {1}", msg, msg.Payload.InnerPayload));
        //        //        Console.WriteLine();
        //        //    });
        //        //});

        //        //pubSub.Subscribe<EmailNotification>(subscriptionId, topic, msg =>
        //        //{
        //        //    Console.WriteLine(String.Format("Payload = {0}", msg.Payload));
        //        //    Console.WriteLine();
        //        //});

        //        Console.WriteLine("Press Enter to stop");
        //        Console.WriteLine();
        //        Console.ReadLine();
        //    }

        //    Console.WriteLine("Press Enter to stop");
        //    Console.WriteLine();
        //    Console.ReadLine();

        //    // Original Code: 
        //    //using (var pubSub = MessageQueueFactory.Create())
        //    //{
        //    //    var fields = GatherInput();
        //    //    var subscriptionId = fields[0];
        //    //    var topic = fields[1];
        //    //    Console.WriteLine();

        //    //    //pubSub.Subscribe<Message>(subscriptionId, topic, msg =>
        //    //    //{
        //    //    //    Console.WriteLine(String.Format("Topic = {0}, Payload = {1}", msg.Topic, msg.Payload));
        //    //    //    Console.WriteLine();
        //    //    //});

        //    //    pubSub.Subscribe<EmailNotification>(subscriptionId, topic, msg =>
        //    //    {
        //    //        Console.WriteLine(String.Format("Topic = {0}, Payload = {1}", msg.Payload.Subject, msg.Payload));
        //    //        Console.WriteLine();
        //    //    });

        //    //    //pubSub.Subscribe<Message>(subscriptionId, msg =>
        //    //    //{
        //    //    //    //throw new ApplicationException("test when exception happens");
        //    //    //    Console.WriteLine(String.Format("Topic = {0}, Payload = {1}", msg.Topic, msg.Payload));
        //    //    //    Console.WriteLine();
        //    //    //});

        //    //    Console.WriteLine("Press Enter to stop");
        //    //    Console.WriteLine();
        //    //    Console.ReadLine();
        //    //}
        //}

        //static string[] GatherInput()
        //{
        //    Console.WriteLine("Subscriber");
        //    Console.WriteLine("Enter <subscription id>,<topic>");
        //    var tmpStr = Console.ReadLine();
        //    if (tmpStr == null || !tmpStr.Contains(","))
        //    {
        //        Console.WriteLine("Enter a comma (,) in your input statement.");
        //        GatherInput();
        //    }

        //    var fields = tmpStr.Split(new char[] { ',' });
        //    if (fields.Length != 2)
        //    {
        //        Console.WriteLine("Enter a two values delmited by a comma (item1,item2) in your input statement.");
        //        GatherInput();
        //    }
        //        var subscriptionId = fields[0];
        //        var topic = fields[1];

        //   return fields;
        }
    }
}
