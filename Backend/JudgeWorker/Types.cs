namespace JudgeWorker;

public class AppSettings
{
    public int WorkerCount { get; set; } = 5;
}

public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public string QueueName { get; set; } = "judge-queue";
}

public interface ISubmissionQueueConsumer
{
    // public Task StartAsync(Func<long>);
}
