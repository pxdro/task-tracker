using MassTransit;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Infrastructure.Consumers
{
    public class TaskCreateEventConsumer : IConsumer<TaskCreateEventDto>
    {
        public async Task Consume(ConsumeContext<TaskCreateEventDto> context)
        {
            var m = context.Message;
            string logMessage = $"[🐰 TaskCreateEvent Consumer] Event received at {DateTime.UtcNow:O}: " +
                               $"TaskId={m.TaskId}, UserId={m.UserId}, Title={m.Title}, Description={m.Description}";

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
