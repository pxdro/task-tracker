using MassTransit;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Infrastructure.Consumers
{
    public class TaskDeleteEventConsumer : IConsumer<TaskDeleteEventDto>
    {
        public async Task Consume(ConsumeContext<TaskDeleteEventDto> context)
        {
            var m = context.Message;
            string logMessage = $"[🐰 TaskDeleteEvent Consumer] Event received at {DateTime.UtcNow:O}: " +
                               $"TaskId={m.TaskId}, UserId={m.UserId}";

            string logDirectory = "logs";
            string logFilePath = Path.Combine(logDirectory, "taskcreatedorupdated_events_log.txt");

            try
            {
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);
                await File.AppendAllTextAsync(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering event in log file: {ex.Message}");
                throw;
            }
        }
    }
}
