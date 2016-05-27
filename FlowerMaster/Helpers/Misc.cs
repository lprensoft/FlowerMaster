using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowerMaster;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;

namespace FlowerMaster.Helpers
{
    class Misc
    {
        public static MainWindow main;

        public static void AddLog(string log)
        {
            if (!main.Dispatcher.CheckAccess())
            {
                main.Dispatcher.Invoke(new Action(() =>
                {
                    Paragraph p = new Paragraph();
                    Run timeText = new Run() { Text = DateTime.Now.ToLongTimeString(), Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 160, 160)) };
                    Run logText = new Run() { Text = " " + log };
                    p.Inlines.Add(timeText);
                    p.Inlines.Add(logText);
                    p.LineHeight = 3;
                    main.gameLog.Document.Blocks.Add(p);
                    main.gameLog.ScrollToEnd();
                }), DispatcherPriority.Background);
            }
            else
            {
                Paragraph p = new Paragraph();
                Run timeText = new Run() { Text = DateTime.Now.ToLongTimeString(), Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 160, 160)) };
                Run logText = new Run() { Text = " " + log };
                p.Inlines.Add(timeText);
                p.Inlines.Add(logText);
                p.LineHeight = 3;
                main.gameLog.Document.Blocks.Add(p);
                main.gameLog.ScrollToEnd();
            }
        }

        public static void SuppressScriptErrors(WebBrowser webBrowser, bool hide)
        {
            webBrowser.Navigating += (s, e) =>
            {
                var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fiComWebBrowser == null)
                    return;

                object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
                if (objComWebBrowser == null)
                    return;

                objComWebBrowser.GetType().InvokeMember("Silent", System.Reflection.BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            };
        }

    }
}
