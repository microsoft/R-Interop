namespace Logging
{
    public interface ILogger
    {
        void LogInformation(string message);
        void LogError(string message);
        void Close();
    }
}
