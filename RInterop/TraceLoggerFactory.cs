namespace RInterop
{
    public class TraceLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string prefix)
        {
            return Create("%temp%", prefix);
        }

        public ILogger Create(string rootFolder, string prefix)
        {
            return new TraceLogger(rootFolder, prefix);
        }
    }
}
