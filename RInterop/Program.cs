namespace RInterop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DependencyFactory.Initialize();

            var bootstrapper = new Bootstrapper();
            bootstrapper.Start(args);
        }
    }
}