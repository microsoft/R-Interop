namespace RInterop
{
    public interface ILogger
    {
        void LogInformation(string message, params object[] parameters);

        void Close();
    }
}
