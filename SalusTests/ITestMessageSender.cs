namespace SalusTests;

public interface ITestMessageSender
{
    void Send(string message);
    Task SendAsync(string message);
}
