using FlowerMaster.Helpers;
using FlowerMaster.Models;
using SHDocVw;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace FlowerMaster
{
    /// <summary>
    /// NewsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewsWindow
    {
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

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            cbShowNews.IsChecked = DataUtil.Config.sysConfig.showLoginNews;
            if (DataUtil.Game.lastNewsUrl.Trim() != "" && DataUtil.Game.lastNewsUrl.IndexOf("/news/news_") != -1)
            {
                newsWeb.Navigate(DataUtil.Game.lastNewsUrl);
            }
        }

        private void newsWeb_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                var provider = newsWeb.Document as IServiceProvider;
                if (provider == null) return;

                object ppvObject;
                provider.QueryService(typeof(IWebBrowserApp).GUID, typeof(IWebBrowser2).GUID, out ppvObject);
                var webBrowser = ppvObject as IWebBrowser2;
                if (webBrowser == null) return;

                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    if (graphics.DpiX > 96)
                    {
                        object pvaIn = (int)(graphics.DpiX / 96 * 100 * 2) - 50;
                        webBrowser.ExecWB(OLECMDID.OLECMDID_OPTICAL_ZOOM, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref pvaIn);
                    }
                }
            }
            catch { }
        }
    }
}
