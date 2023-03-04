using RabbitMQ.Client;
using Salus.Messaging;
using System.Text;

namespace SalusExampleParent.Messaging;

internal class RabbitMqMessageSender : IMessageSender
{
    public void Send(string message)
    {
        // Make a connection
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        // Create the exchange, queue and binding if they don't already exist
        channel.ExchangeDeclare("salus-demo-data", "direct", true, false, null);
        channel.QueueDeclare("salus-demo-queue", true, false, false, null);
        channel.QueueBind("salus-demo-queue", "salus-demo-data", "salus", null);

        // Publish the message
        var bytes = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish("salus-demo-data", "salus", null, bytes);
    }
}
