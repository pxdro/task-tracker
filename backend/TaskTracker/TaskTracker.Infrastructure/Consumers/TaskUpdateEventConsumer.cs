using MassTransit;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Infrastructure.Consumers
{
    public class TaskUpdateEventConsumer : IConsumer<TaskUpdateEventDto>
    {
        public async Task Consume(ConsumeContext<TaskUpdateEventDto> context)
        {
            var m = context.Message;
            string logMessage = $"[🐰 TaskUpdateEvent Consumer] Event received at {DateTime.UtcNow:O}: " +
                               $"TaskId={m.TaskId}, UserId={m.UserId}, Title={m.Title}, Description={m.Description}, Status={m.Status}";

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
