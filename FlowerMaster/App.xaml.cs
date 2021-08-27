using CefSharp;
using CefSharp.WinForms;
using FlowerMaster.Models;
using System.Windows;

namespace FlowerMaster
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs startupEventArgs)
        {
            base.OnStartup(startupEventArgs);


            DataUtil.Config = new SysConfig();
            DataUtil.Game = new GameInfo();
            DataUtil.Cards = new CardInfo();
            DataUtil.Config.LoadConfig();
            DataUtil.Game.gameServer = DataUtil.Config.sysConfig.gameServer;
            
            if (!Cef.IsInitialized)
            {
                CefSettings settings = new CefSettings();

                settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.1.142.0 Safari/537.36";
                settings.IgnoreCertificateErrors = true;
                settings.LogSeverity = LogSeverity.Disable;
                settings.CachePath = @"C:\ProgramData\FlowerMaster\Cache";
                settings.CefCommandLineArgs.Add("ppapi-flash-path", "pepflashplayer32_32_0_0_270.dll");
                settings.CefCommandLineArgs.Add("ppapi-flash-version", "32.0.0.270");
                CefSharpSettings.Proxy = new ProxyOptions("127.0.0.1", DataUtil.Config.localProxyPort.ToString());

                Cef.Initialize(settings);
            }

        }
    }
}
