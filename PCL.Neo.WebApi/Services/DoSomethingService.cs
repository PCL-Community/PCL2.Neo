namespace PCL.Neo.WebApi.Services
{
    public interface IDoSomethingService
    {
        void DoSomething(string module, string message);
    }

    public class DoSomethingService : IDoSomethingService
    {
        public void DoSomething(string module, string message)
        {
            Console.WriteLine($"收到数据：{module} {message}");
        }
    }
}
