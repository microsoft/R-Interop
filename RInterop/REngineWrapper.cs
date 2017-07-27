using RDotNet;

namespace RInterop
{
    public static class REngineWrapper
    {
        private static REngine _engine;

        private static readonly object LockObject = new object();

        public static REngine REngine
        {
            get
            {
                if (_engine == null)
                {
                    lock (LockObject)
                    {
                        if (_engine == null)
                        {
                            _engine = REngine.GetInstance();
                        }
                    }
                }

                return _engine;
            }
        }
    }
}