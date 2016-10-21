namespace RInterop
{
    public interface ILoggerFactory
    {
        ILogger Create(string prefix);

        ILogger Create(string rootFolder, string prefix);
    }
}
