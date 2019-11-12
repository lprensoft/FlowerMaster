using CefSharp;
using CefSharp.Wpf;
using FlowerMaster.Models;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace FlowerMaster
{
    /// <summary>
    /// NewsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewsWindow
    {
        ChromiumWebBrowser mainWeb;

        [ComImport, Guid("6d5140c1-7436-11ce-8034-00aa006009fa"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false)]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryService(ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
        }

        public NewsWindow()
        {
            InitializeComponent();
            initChromiumWebBrowser();
        }

        private void initChromiumWebBrowser()
        {
            mainWeb = new ChromiumWebBrowser("about:blank");
            BrowserSettings config = new BrowserSettings();

            config.FileAccessFromFileUrls = CefState.Disabled;
            config.UniversalAccessFromFileUrls = CefState.Disabled;
            config.WebSecurity = CefState.Enabled;
            config.WebGl = CefState.Enabled;
            config.ApplicationCache = CefState.Enabled;

            mainWeb.BrowserSettings = config;
            mainWeb.HorizontalAlignment = HorizontalAlignment.Stretch;
            mainWeb.VerticalAlignment = VerticalAlignment.Stretch;
            mainWeb.AllowDrop = false;
            mainWeb.IsBrowserInitializedChanged += MainWeb_IsBrowserInitializedChanged;
            Grid.SetRow(mainWeb,1);
            Grid.SetColumn(mainWeb, 1);

            grid.Children.Add(mainWeb);

            mainWeb.BringIntoView();
        }

        private void MainWeb_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!mainWeb.IsBrowserInitialized) return;
            
            cbShowNews.IsChecked = DataUtil.Config.sysConfig.showLoginNews;
            if (DataUtil.Game.gameNewsUrl == "") return;
            mainWeb.Load(DataUtil.Game.gameNewsUrl);
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cbShowNews.IsChecked.HasValue)
            {
                DataUtil.Config.sysConfig.showLoginNews = (bool)cbShowNews.IsChecked;
            }
            else
            {
                DataUtil.Config.sysConfig.showLoginNews = false;
            }
            DataUtil.Config.SaveConfig();
        }

        //private void newsWeb_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    try
        //    {
        //        var provider = newsWeb.Document as IServiceProvider;
        //        if (provider == null) return;

        //        object ppvObject;
        //        provider.QueryService(typeof(IWebBrowserApp).GUID, typeof(IWebBrowser2).GUID, out ppvObject);
        //        var webBrowser = ppvObject as IWebBrowser2;
        //        if (webBrowser == null) return;

        //        using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
        //        {
        //            if (graphics.DpiX > 96)
        //            {
        //                object pvaIn = (int)(graphics.DpiX / 96 * 100 * 2) - 100;
        //                webBrowser.ExecWB(OLECMDID.OLECMDID_OPTICAL_ZOOM, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref pvaIn);
        //            }
        //        }
        //    }
        //    catch { }
        //}
    }
}
