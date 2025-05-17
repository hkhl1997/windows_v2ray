using ServiceLib.Enums;
using ServiceLib.Handler;
using ServiceLib.ViewModels;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        if (!AppHandler.Instance.InitApp())
        {
            Environment.Exit(0);
            return;
        }

        MainWindowViewModel ViewModel = new MainWindowViewModel(UpdateViewHandler);

        Console.ReadLine();
    }

    private static async Task<bool> UpdateViewHandler(EViewAction action, object? obj)
    {
        return await Task.FromResult(true);
    }
}
