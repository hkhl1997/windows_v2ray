using ServiceLib.Enums;
using ServiceLib.Handler;
using ServiceLib.Models;
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

        var node = new ProfileItem();

        node.Address = "53bdde59.flzxjxo53slwj3n.e7mbqhx.com";
        node.ConfigType = EConfigType.Shadowsocks;
        node.ConfigVersion = 2;
        node.DisplayLog = true;
        node.Id = "OwzVzPeKuRZm";
        node.IndexId = "1";
        node.IsSub = true;
        node.Port = 57654;
        node.Remarks = "动态BGP+IPLC|美国04(原生)|2x";
        node.Security = "chacha20-ietf-poly1305";
        node.Subid = "2";



        Console.ReadLine();
    }

    private static async Task<bool> UpdateViewHandler(EViewAction action, object? obj)
    {
        return await Task.FromResult(true);
    }
}
