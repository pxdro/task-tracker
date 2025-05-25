using MassTransit;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Infrastructure.Consumers
{
    public class TaskCreatedOrUpdatedConsumer : IConsumer<TaskCreatedOrUpdatedEventDto>
    {
        public async Task Consume(ConsumeContext<TaskCreatedOrUpdatedEventDto> context)
        {
            var m = context.Message;
            string logMessage = $"[🐰 TaskCreatedOrUpdated Consumer] Event received at {DateTime.UtcNow:O}: " +
                               $"TaskId={m.TaskId}, UserId={m.UserId}, Status={m.Status}, Title={m.Title}";

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
                Console.WriteLine($"Erro ao escrever no arquivo de log: {ex.Message}");
                throw;
            }
        }
    }
}
