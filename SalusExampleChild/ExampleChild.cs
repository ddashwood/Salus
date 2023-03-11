using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace SalusExampleChild;

public class ExampleChild
{
    private IModel? channel;
    private readonly ChildContext _context;

    public ExampleChild(ChildContext context)
    {
        _context = context;
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
        channel = connection.CreateModel();

        // Create the queue if it doesn't already exist
        channel.QueueDeclare("salus-demo-queue", true, false, false, null);

        ShowData();

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += OnReceived;

        channel.BasicConsume("salus-demo-queue", false, consumer);

        await Task.Delay(-1);
    }

    private void ShowData()
    {
        Console.Clear();
        var data = _context.ExampleData.ToList();

        Console.WriteLine($"Got {data.Count} rows of data!");
        foreach (var row in data)
        {
            Console.WriteLine("Id: " + row.Id);
            Console.WriteLine(row.Data1);
            Console.WriteLine();
        }
    }

    private void OnReceived(object? sender, BasicDeliverEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Body.ToArray());
        _context.Apply(message);
        ShowData();

        channel?.BasicAck(e.DeliveryTag, false);
    }
}
