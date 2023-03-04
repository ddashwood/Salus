using RabbitMQ.Client;

namespace SalusExampleParent;

// To set up, run RabbitMQ in Docker with the following command:
//   docker run -d --hostname salus-demo --name salus-demo-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3-management

internal class ExampleParent
{
    public ExampleParent(ExampleDbContext context)
    {
    }

    public async Task Run()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.ExchangeDeclare("salus-demo-data", "direct", true, false, null);



        const int DELAY = 10000;

        while (true)
        {
            Console.WriteLine("Running");

            for (int i = 0; i < DELAY / 100; i++)
            {
                await Task.Delay(100);
                if (Console.KeyAvailable)
                {
                    Console.WriteLine("Key pressed - exiting");
                    return;
                }
            }
        }

    }
}
