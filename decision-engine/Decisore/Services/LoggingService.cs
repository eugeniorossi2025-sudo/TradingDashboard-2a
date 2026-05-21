namespace Decisore.Services
{
    public class LoggingService
    {
        public readonly List<string> _logs = new();

        public string Category { get; set; }
        public int Action { get; set; }

        public void Log(string message)
        {
            _logs.Add(message);
        }
        
        public bool HasLogs => _logs.Count > 0;

        public string GetConcatenatedLogs()
        {
            return string.Join("\n\n", _logs);
        }
    }
}