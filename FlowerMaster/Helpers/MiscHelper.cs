using System;
using System.Windows.Documents;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using mshtml;
using SHDocVw;
using System.Drawing;
using System.Drawing.Imaging;
using FlowerMaster.Models;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Forms;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;
using System.Linq;
using Size = System.Drawing.Size;
using CefSharp;

namespace FlowerMaster.Helpers
{
    /// <summary>
    /// 通用静态函数库
    /// </summary>
    class MiscHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        internal class DVTARGETDEVICE
        {
            public ushort tdSize;
            public uint tdDeviceNameOffset;
            public ushort tdDriverNameOffset;
            public ushort tdExtDevmodeOffset;
            public ushort tdPortNameOffset;
            public byte tdData;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal class RECT
        {
            public int left;
            public int top;
            public int width;
            public int height;
        }

        [ComImport, Guid("6d5140c1-7436-11ce-8034-00aa006009fa"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false)]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryService(ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
        }

        [ComImport, Guid("0000010d-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IViewObject
        {
            [PreserveSig]
            int Draw(
                [In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect,
                int lindex,
                IntPtr pvAspect,
                [In] DVTARGETDEVICE ptd,
                IntPtr hdcTargetDev,
                IntPtr hdcDraw,
                [In] RECT lprcBounds,
                [In] RECT lprcWBounds,
                IntPtr pfnContinue,
                [In] IntPtr dwContinue);
        }

        /// <summary>
        /// 日志记录类型
        /// </summary>
        public enum LogType
        {
            Default, //默认
            System, //系统
            Search, //探索
            Stage, //副本
            Boss, //BOSS战
            Gacha, //扭蛋
            Sell, //贩卖
            Mailbox, //礼品箱
            Levelup, //升级
            Debug = 99, //调试
        }

        public static MainWindow main;

        /// <summary>
        /// 添加游戏日志
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="type">日志类型</param>
        public static void AddLog(string log, LogType type=LogType.Default)
        {
            System.Windows.Media.Color typeColor = Colors.White;
            switch (type)
            {
                case LogType.System:
                    typeColor = Colors.Bisque;
                    break;
                case LogType.Search:
                    typeColor = Colors.Violet;
                    break;
                case LogType.Stage:
                    typeColor = Colors.LightGreen;
                    break;
                case LogType.Boss:
                    typeColor = Colors.LightCoral;
                    break;
                case LogType.Gacha:
                case LogType.Levelup:
                    typeColor = Colors.Yellow;
                    break;
                case LogType.Sell:
                    typeColor = Colors.DarkOrange;
                    break;
                case LogType.Mailbox:
                    typeColor = Colors.DeepSkyBlue;
                    break;
                case LogType.Default:
                default:
                    typeColor = Colors.White;
                    break;
            }
            if (!main.Dispatcher.CheckAccess())
            {
                main.Dispatcher.Invoke(new Action(() =>
                {
                    Paragraph p = new Paragraph();
                    Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                    Run logText = new Run() { Text = log, Foreground = new SolidColorBrush(typeColor) };
                    p.Inlines.Add(timeText);
                    p.Inlines.Add(logText);
                    p.LineHeight = 3;
                    main.gameLog.Document.Blocks.Add(p);
                    main.gameLog.ScrollToEnd();
                    if (type != LogType.System && type != LogType.Debug) main.stLog.Text = log;
                }));
            }
            else
            {
                Paragraph p = new Paragraph();
                Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                Run logText = new Run() { Text = log, Foreground = new SolidColorBrush(typeColor) };
                p.Inlines.Add(timeText);
                p.Inlines.Add(logText);
                p.LineHeight = 3;
                main.gameLog.Document.Blocks.Add(p);
                main.gameLog.ScrollToEnd();
                if (type != LogType.System && type != LogType.Debug) main.stLog.Text = log;
            }
            if (type != LogType.System && type != LogType.Debug) LogsHelper.LogGame(log);
        }

        /// <summary>
        /// 添加扭蛋日志
        /// </summary>
        /// <param name="cards">角色信息</param>
        public static void AddGachaLog(JArray cards)
        {
            if (!main.gameLog.Dispatcher.CheckAccess())
            {
                main.gameLog.Dispatcher.Invoke(new Action(() =>
                {
                    string logs = "進行了一次抽卡，獲得：";
                    Paragraph p = new Paragraph();
                    Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                    Run log = new Run() { Text = "進行了一次抽卡，獲得：", Foreground = new SolidColorBrush(Colors.Yellow) };
                    p.Inlines.Add(timeText);
                    p.Inlines.Add(log);
                    p.LineHeight = 3;
                    int cnt = 0;
                    foreach (JObject card in cards)
                    {
                        string cardStr = DataUtil.Cards.GetName(int.Parse(card["characterId"].ToString()));
                        System.Windows.Media.Color color = Colors.White;
                        if (cards.Count == 10 && cardStr == "★3茉莉")
                        {
                            color = Colors.Red;
                        }
                        else if (cardStr.IndexOf("★1") != -1)
                        {
                            color = Colors.LightSteelBlue;
                        }
                        else if (cardStr.IndexOf("★2") != -1)
                        {
                            color = Colors.Aquamarine;
                        }
                        else if (cardStr.IndexOf("★3") != -1)
                        {
                            color = Colors.Chocolate;
                        }
                        else if (cardStr.IndexOf("★4") != -1)
                        {
                            color = Colors.Silver;
                        }
                        else if (cardStr.IndexOf("★5") != -1)
                        {
                            color = Colors.Gold;
                        }
                        else if (cardStr.IndexOf("★6") != -1)
                        {
                            color = Colors.Violet;
                        }
                        if (cnt > 0)
                        {
                            Run cardTextEnd = new Run() { Text = "、", Foreground = new SolidColorBrush(Colors.Yellow) };
                            p.Inlines.Add(cardTextEnd);
                        }
                        Run cardText = new Run() { Text = card["bookStatus"].ToString() == "2" ? cardStr + "（新）" : cardStr, Foreground = new SolidColorBrush(color) };
                        p.Inlines.Add(cardText);
                        logs += card["bookStatus"].ToString() == "2" ? cardStr + "（新）、" : cardStr + "、";
                        cnt++;
                    }
                    main.gameLog.Document.Blocks.Add(p);
                    main.gameLog.ScrollToEnd();
                    LogsHelper.LogGame(logs.Substring(0, logs.Length - 1));
                }));
            }
            else
            {
                string logs = "進行了一次抽卡，獲得：";
                Paragraph p = new Paragraph();
                Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                Run log = new Run() { Text = "進行了一次抽卡，獲得：", Foreground = new SolidColorBrush(Colors.Yellow) };
                p.Inlines.Add(timeText);
                p.Inlines.Add(log);
                p.LineHeight = 3;
                int cnt = 0;
                foreach (JObject card in cards)
                {
                    string cardStr = DataUtil.Cards.GetName(int.Parse(card["characterId"].ToString()));
                    System.Windows.Media.Color color = Colors.White;
                    if (cards.Count == 10 && cardStr == "★3茉莉")
                    {
                        color = Colors.Red;
                    }
                    else if (cardStr.IndexOf("★1") != -1)
                    {
                        color = Colors.LightSteelBlue;
                    }
                    else if (cardStr.IndexOf("★2") != -1)
                    {
                        color = Colors.Aquamarine;
                    }
                    else if (cardStr.IndexOf("★3") != -1)
                    {
                        color = Colors.Chocolate;
                    }
                    else if (cardStr.IndexOf("★4") != -1)
                    {
                        color = Colors.Silver;
                    }
                    else if (cardStr.IndexOf("★5") != -1)
                    {
                        color = Colors.Gold;
                    }
                    else if (cardStr.IndexOf("★6") != -1)
                    {
                        color = Colors.Violet;
                    }
                    if (cnt > 0)
                    {
                        Run cardTextEnd = new Run() { Text = "，", Foreground = new SolidColorBrush(Colors.Yellow) };
                        p.Inlines.Add(cardTextEnd);
                    }
                    Run cardText = new Run() { Text = card["bookStatus"].ToString() == "2" ? cardStr + "（新）" : cardStr, Foreground = new SolidColorBrush(color) };
                    p.Inlines.Add(cardText);
                    logs += card["bookStatus"].ToString() == "2" ? cardStr + "（新）、" : cardStr + "、";
                    cnt++;
                }
                main.gameLog.Document.Blocks.Add(p);
                main.gameLog.ScrollToEnd();
                LogsHelper.LogGame(logs.Substring(0, logs.Length - 1));
            }
        }

        /// <summary>
        /// 解析处理用户点数物品日志信息
        /// </summary>
        /// <param name="item">单个物品JSON数据</param>
        /// <returns>返回解析日志文本</returns>
        public static string ProcessUserPointItem(JObject item)
        {
            if (item["itemId"].ToString() == "10")
            {
                return "中級裝備種子" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "11")
            {
                return "上級裝備種子" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "89")
            {
                return "50%體力藥" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "101")
            {
                return "生命結晶" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "144")
            {
                return "绊水晶" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "171")
            {
                return "團長幣" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "205")
            {
                return "特務勳章" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "206")
            {
                return "★2水影秘石碎片" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "207")
            {
                return "★3水影秘石碎片" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "208")
            {
                return "★4水影秘石碎片" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "209")
            {
                return "★5水影秘石碎片" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "210")
            {
                return "★6水影秘石碎片" + item["point"].ToString() + "，";
            }
            else if (item["itemId"].ToString() == "274")
            {
                return "庭院幣" + item["point"].ToString() + "，";
            }
            else
            {
                return "未知物品[" + item["itemId"].ToString() + "]" + item["point"].ToString() + "，";
            }
        }

        /// <summary>
        /// 鼠标左键点击（坐标）
        /// </summary>
        /// <param name="x">x-横向坐标</param>
        /// <param name="y">y-竖向坐标</param>
        public static void MouseLeftClick(int x, int y)
        {
            main.mainWeb.GetBrowser().GetHost().SendMouseClickEvent(x, y, MouseButtonType.Left, false, 1, CefEventFlags.None);
            main.mainWeb.GetBrowser().GetHost().SendMouseClickEvent(x, y, MouseButtonType.Left, true, 1, CefEventFlags.None);
        }

        /// <summary>
        /// 截图游戏并保存
        /// </summary>
        public static void ScreenShot()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\Screenshot_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "." + DataUtil.Config.sysConfig.capFormat.ToString().ToLower();
            CaptureScreen(main.mainWeb, path);
            AddLog("截圖已經保存到" + path, LogType.System);
        }

        /// <summary>
        /// 擷取Control的畫面到指定目錄
        /// </summary>
        /// <param name="source">Control</param>
        /// <param name="path">目錄</param>
        public static void CaptureScreen(Control source, string path)
        {
            Bitmap bm = new Bitmap(source.Width, source.Height);

            using (Graphics g = Graphics.FromImage(bm))
            {
                g.CopyFromScreen(source.PointToScreen(new Point(0, 0)), new Point(0, 0), bm.Size);
            }

            bm.Save(path, DataUtil.Config.sysConfig.capFormat == SysConfig.ScreenShotFormat.PNG ? ImageFormat.Png : ImageFormat.Jpeg);
        }

        /// <summary>
        /// 擷取UIElement的畫面到指定目錄
        /// </summary>
        /// <param name="source">UIElement</param>
        /// <param name="path">目錄</param>
        public static void CaptureScreen(UIElement source, string path)
        {
            try
            {
                double Height, renderHeight, Width, renderWidth;

                Height = renderHeight = source.RenderSize.Height;
                Width = renderWidth = source.RenderSize.Width;

                //Specification for target bitmap like width/height pixel etc.
                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
                //creates Visual Brush of UIElement
                VisualBrush visualBrush = new VisualBrush(source);

                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    //draws image of element
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(new System.Windows.Point(0, 0), new System.Windows.Point(Width, Height)));
                }
                //renders image
                renderTarget.Render(drawingVisual);

                //PNG encoder for creating PNG file
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(stream);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// 控制显示主窗口地图信息按钮
        /// </summary>
        /// <param name="show">是否显示，默认显示</param>
        public static void ShowMapInfoButton(bool show=true)
        {
            if (!main.Dispatcher.CheckAccess())
            {
                main.Dispatcher.Invoke(new Action(() =>
                {
                    main.btnMapInfo.Visibility = show ? Visibility.Visible : Visibility.Hidden;
                }));
            }
            else
            {
                main.btnMapInfo.Visibility = show ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// 控制显示主窗口停止推兔按鈕
        /// </summary>
        /// <param name="show">是否显示，默认显示</param>
        public static void ShowPushSetButton(bool show = true)
        {
            if (!main.Dispatcher.CheckAccess())
            {
                main.Dispatcher.Invoke(new Action(() =>
                {
                    main.btnPushSet.Visibility = show ? Visibility.Visible : Visibility.Hidden;
                }));
            }
            else
            {
                main.btnPushSet.Visibility = show ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// 设置自动推兔模式
        /// </summary>
        /// <param name="modeSwitch">开关</param>
        public static void SetAutoGo(bool modeSwitch)
        {
            if (modeSwitch && !DataUtil.Game.isAuto && DataUtil.Game.canAuto)
            {
                DataUtil.Game.isAuto = true;
                main.autoGoLastConf = 1 + 2000 / DataUtil.Config.sysConfig.autoGoTimeout;
                main.timerAuto.Change(0, DataUtil.Config.sysConfig.autoGoTimeout);
                AddLog("開始自動推兔...", LogType.System);
                if (!main.Dispatcher.CheckAccess())
                {
                    main.Dispatcher.Invoke(new Action(() =>
                    {
                        main.btnAuto.Background = System.Windows.Media.Brushes.Green;
                    }));
                }
                else
                {
                    main.btnAuto.Background = System.Windows.Media.Brushes.Green;
                }
            }
            else if (!modeSwitch && DataUtil.Game.isAuto)
            {
                DataUtil.Game.isAuto = false;
                main.timerAuto.Change(Timeout.Infinite, DataUtil.Config.sysConfig.autoGoTimeout);
                AddLog("自動推兔已停止！", LogType.System);
                if (!main.Dispatcher.CheckAccess())
                {
                    main.Dispatcher.Invoke(new Action(() =>
                    {
                        main.btnAuto.Background = System.Windows.Media.Brushes.Black;
                    }));
                }
                else
                {
                    main.btnAuto.Background = System.Windows.Media.Brushes.Black;
                }
            }
        }

        /// <summary>
        /// 显示提醒信息
        /// </summary>
        /// <param name="timeout">提醒超时时间</param>
        /// <param name="title">提醒的标题</param>
        /// <param name="content">提醒的内容</param>
        /// <param name="tipIcon">提醒的图标</param>
        public static void ShowRemind(int timeout, string title, string content, System.Windows.Forms.ToolTipIcon tipIcon)
        {
            if (!main.Dispatcher.CheckAccess())
            {
                main.Dispatcher.Invoke(new Action(() =>
                {
                    if (!main.notifyIcon.Visible)
                    {
                        main.notifyIcon.Visible = true;
                        main.timerNotify.Change(10000, 10000);
                    }
                    main.notifyIcon.ShowBalloonTip(timeout, title, content, tipIcon);
                }));
            }
            else
            {
                if (!main.notifyIcon.Visible)
                {
                    main.notifyIcon.Visible = true;
                    main.timerNotify.Change(10000, 10000);
                }
                main.notifyIcon.ShowBalloonTip(timeout, title, content, tipIcon);
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            try
            {
                MemoryStream ms = new MemoryStream(data);
                GZipStream zip = new GZipStream(ms, CompressionMode.Decompress, true);
                MemoryStream msreader = new MemoryStream();
                byte[] buffer = new byte[0x1000];
                while (true)
                {
                    int reader = zip.Read(buffer, 0, buffer.Length);
                    if (reader <= 0)
                    {
                        break;
                    }
                    msreader.Write(buffer, 0, reader);
                }
                zip.Close();
                ms.Close();
                msreader.Position = 0;
                buffer = msreader.ToArray();
                msreader.Close();
                return buffer;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
