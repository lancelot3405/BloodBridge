namespace BloodBridge.API.Services;

public interface IEmailSender
{
    Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default);
}

public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}

public sealed class ConsoleEmailSender : IEmailSender
{
    public Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Mock Email] To: {recipient} | Subject: {subject} | {message}");
        return Task.CompletedTask;
    }
}

public sealed class ConsoleSmsSender : ISmsSender
{
    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Mock SMS] To: {phoneNumber} | {message}");
        return Task.CompletedTask;
    }
}
