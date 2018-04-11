using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Drawing;
using mshtml;
using Nekoxy;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Media;
using FlowerMaster.Models;
using FlowerMaster.Helpers;
using FlowerMaster.Properties;
using static FlowerMaster.CordCol;

namespace FlowerMaster
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public System.Windows.Forms.NotifyIcon notifyIcon = null;
        private bool styleSheetApplied = false;
        private bool loginSubmitted = false;
        private bool newsHadShown = false;

        private Timer timerCheck = null; //提醒检查计时器
        private Timer timerClock = null; //时钟计时器
        public Timer timerAuto = null; //自动推兔定时器
        public int autoGoLastConf = 0; //自动推兔点击上次配置计数器
        public Timer timerNotify = null; //提醒计时器

        private readonly Counter PushTimes = Counter.Instance; //自动推兔2.0状态

        private IntPtr webHandle = IntPtr.Zero;

        //模拟鼠标操作相关API引入
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern bool PostMessage(IntPtr WindowHandle, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);


        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 启用老板键
        /// </summary>
        private void EnableHotKey()
        {
            System.Windows.Forms.Keys k = (System.Windows.Forms.Keys)Enum.Parse(typeof(System.Windows.Forms.Keys), DataUtil.Config.sysConfig.hotKey.ToString().ToUpper());
            HotKeyHelper.KeyModifiers m = HotKeyHelper.KeyModifiers.None;
            if (DataUtil.Config.sysConfig.hotKeyCtrl) m = HotKeyHelper.KeyModifiers.Ctrl;
            if (DataUtil.Config.sysConfig.hotKeyAlt)
            {
                if (m == HotKeyHelper.KeyModifiers.None)
                {
                    m = HotKeyHelper.KeyModifiers.Alt;
                }
                else
                {
                    m = m | HotKeyHelper.KeyModifiers.Alt;
                }
            }
            if (DataUtil.Config.sysConfig.hotKeyShift)
            {
                if (m == HotKeyHelper.KeyModifiers.None)
                {
                    m = HotKeyHelper.KeyModifiers.Shift;
                }
                else
                {
                    m = m | HotKeyHelper.KeyModifiers.Shift;
                }
            }
            IntPtr handle = new WindowInteropHelper(this).Handle;
            if (HotKeyHelper.isRegistered) HotKeyHelper.UnregisterHotKey(handle, HotKeyHelper.hotKeyId);
            HotKeyHelper.isRegistered = HotKeyHelper.RegisterHotKey(handle, HotKeyHelper.hotKeyId, m, k);
            HotKeyHelper.InstallHotKeyHook(this);
        }

        /// <summary>
        /// 系统初始化
        /// </summary>
        private void SystemInit()
        {
            MiscHelper.main = this;
            PacketHelper.mainWindow = this;
            MiscHelper.SetIEConfig();

            DataUtil.Config = new SysConfig();
            DataUtil.Game = new GameInfo();
            DataUtil.Cards = new CardInfo();
            DataUtil.Config.LoadConfig();
            DataUtil.Game.gameServer = DataUtil.Config.sysConfig.gameServer;

            if (DataUtil.Config.sysConfig.enableHotKey) EnableHotKey();

            InitTrayIcon(DataUtil.Config.sysConfig.alwaysShowTray);

            ShowConfigToSettings();

            timerCheck = new Timer(new TimerCallback(checkTimeLeft), this, 0, 1000);
            timerClock = new Timer(new TimerCallback(tickServerTime), this, 0, 1000);
            timerAuto = new Timer(new TimerCallback(AutoClickMouse), this, Timeout.Infinite, DataUtil.Config.sysConfig.autoGoTimeout);
            timerNotify = new Timer(new TimerCallback(closeNotify), this, Timeout.Infinite, 10000);

            //默认推兔状态
            PushTimes.Reset();

            dgDaliy.ItemsSource = DataUtil.Game.daliyInfo;
            dgMainExp.ItemsSource = DataUtil.Game.expTable;

            ResizeWeb();

            InitProxy();

            mainWeb.Navigate("about:blank");
            MiscHelper.SuppressScriptErrors(mainWeb, true);
            MiscHelper.AddLog("系统初始化完毕，等待登录游戏...", MiscHelper.LogType.System);
        }

        /// <summary>
        /// 提醒定时器
        /// </summary>
        /// <param name="obj">对象参数</param>
        private void checkTimeLeft(object obj)
        {
            if (!DataUtil.Game.isOnline || notifyIcon == null || !notifyIcon.Visible) return;
            if (DataUtil.Config.sysConfig.apTargetNotify > 0 && DataUtil.Game.player.AP == DataUtil.Config.sysConfig.apTargetNotify && DataUtil.Game.notifyRecord.lastAP < DataUtil.Game.player.AP)
            {
                MiscHelper.ShowRemind(10, DataUtil.Game.player.name + " - 体力回复通知", "当前体力已经达到" + DataUtil.Config.sysConfig.apTargetNotify.ToString(), System.Windows.Forms.ToolTipIcon.Info);
            }
            if (DataUtil.Config.sysConfig.bpTargetNotify > 0 && DataUtil.Game.player.BP == DataUtil.Config.sysConfig.bpTargetNotify && DataUtil.Game.notifyRecord.lastBP < DataUtil.Game.player.BP)
            {
                MiscHelper.ShowRemind(10, DataUtil.Game.player.name + " - 战点回复通知", "当前战点已经达到" + DataUtil.Config.sysConfig.bpTargetNotify.ToString(), System.Windows.Forms.ToolTipIcon.Info);
            }
            if (DataUtil.Config.sysConfig.apFullNotify && DataUtil.Game.player.AP == DataUtil.Game.player.maxAP && DataUtil.Game.notifyRecord.lastAP < DataUtil.Game.player.maxAP)
            {
                MiscHelper.ShowRemind(10, DataUtil.Game.player.name + " - 体力回复通知", "当前体力已经回复满了！", System.Windows.Forms.ToolTipIcon.Info);
            }
            if (DataUtil.Config.sysConfig.bpFullNotify && DataUtil.Game.player.BP == DataUtil.Game.player.maxBP && DataUtil.Game.notifyRecord.lastBP < DataUtil.Game.player.maxBP)
            {
                MiscHelper.ShowRemind(10, DataUtil.Game.player.name + " - 战点回复通知", "当前战点已经回复满了！", System.Windows.Forms.ToolTipIcon.Info);
            }
            if (DataUtil.Config.sysConfig.spEveryNotify && DataUtil.Game.notifyRecord.lastSP < DataUtil.Game.player.SP && DataUtil.Game.notifyRecord.lastSP < DataUtil.Game.player.maxSP)
            {
                MiscHelper.ShowRemind(10, DataUtil.Game.player.name + " - 探索回复通知", "当前探索点数回复了1点！", System.Windows.Forms.ToolTipIcon.Info);
            }
            if (DataUtil.Config.sysConfig.spFullNotify && DataUtil.Game.player.SP == DataUtil.Game.player.maxSP && DataUtil.Game.notifyRecord.lastSP < DataUtil.Game.player.maxSP)
            {
                MiscHelper.ShowRemind(10, DataUtil.Game.player.name + " - 探索回复通知", "当前探索已经回复满了！", System.Windows.Forms.ToolTipIcon.Info);
            }
            DataUtil.Game.notifyRecord.lastAP = DataUtil.Game.player.AP;
            DataUtil.Game.notifyRecord.lastBP = DataUtil.Game.player.BP;
            DataUtil.Game.notifyRecord.lastSP = DataUtil.Game.player.SP;
        }

        /// <summary>
        /// 服务器时间计算定时器
        /// </summary>
        /// <param name="obj">对象参数</param>
        private void tickServerTime(object obj)
        {
            if (!DataUtil.Game.isOnline) return;
            DataUtil.Game.serverTime = DataUtil.Game.serverTime.AddSeconds(1);

            DataUtil.Game.CalcPlayerGamePoint();

            this.Dispatcher.Invoke(new Action(() =>
            {
                stTime.Text = "服务器时间：" + DataUtil.Game.serverTime.ToString("yyyy-MM-dd HH:mm:ss");
            }));
        }

        /// <summary>
        /// 取消提醒定时器
        /// </summary>
        /// <param name="obj">对象参数</param>
        private void closeNotify(object obj)
        {
            timerNotify.Change(Timeout.Infinite, 10000);
            notifyIcon.Visible = false;
        }

        /// <summary>
        /// 将设置模块设置显示到图形界面
        /// </summary>
        private void ShowConfigToSettings()
        {
            chkShowLogin.IsChecked = DataUtil.Config.sysConfig.showLoginDialog;
            chkShowNews.IsChecked = DataUtil.Config.sysConfig.showLoginNews;
            cbGameServer.SelectedIndex = DataUtil.Config.sysConfig.gameServer;
            cbLoginPage.SelectedIndex = DataUtil.Config.sysConfig.gameHomePage;

            switch (DataUtil.Config.sysConfig.proxyType)
            {
                case SysConfig.ProxySettingsType.DirectAccess:
                    rbNotUseProxy.IsChecked = true;
                    break;
                case SysConfig.ProxySettingsType.UseSystemProxy:
                    rbUseIEProxy.IsChecked = true;
                    break;
                case SysConfig.ProxySettingsType.UseUserProxy:
                    rbUseCusProxy.IsChecked = true;
                    break;
            }
            tbProxyServer.Text = DataUtil.Config.sysConfig.proxyServer;
            tbProxyPort.Text = DataUtil.Config.sysConfig.proxyPort.ToString();

            chkAPTarget.IsChecked = DataUtil.Config.sysConfig.apTargetNotify > 0 ? true : false;
            tbAPTarget.Text = DataUtil.Config.sysConfig.apTargetNotify.ToString();
            chkAPFull.IsChecked = DataUtil.Config.sysConfig.apFullNotify;
            chkBPTarget.IsChecked = DataUtil.Config.sysConfig.bpTargetNotify > 0 ? true : false;
            tbBPTarget.Text = DataUtil.Config.sysConfig.bpTargetNotify.ToString();
            chkBPFull.IsChecked = DataUtil.Config.sysConfig.bpFullNotify;
            chkSPEvery.IsChecked = DataUtil.Config.sysConfig.spEveryNotify;
            chkSPFull.IsChecked = DataUtil.Config.sysConfig.spFullNotify;
            chkFoundStage.IsChecked = DataUtil.Config.sysConfig.foundStageNotify;
            chkFoundBoss.IsChecked = DataUtil.Config.sysConfig.foundBossNotify;

            chkGameLog.IsChecked = DataUtil.Config.sysConfig.logGame;
            chkGachaLog.IsChecked = DataUtil.Config.sysConfig.logGacha;

            chkAutoGo.IsChecked = DataUtil.Config.sysConfig.autoGoInMaps;
            slAutoRate.Value = DataUtil.Config.sysConfig.autoGoTimeout;

            chkTitleChange.IsChecked = DataUtil.Config.sysConfig.changeTitle;
            chkAlwaysTray.IsChecked = DataUtil.Config.sysConfig.alwaysShowTray;
            chkMiniToTray.IsChecked = DataUtil.Config.sysConfig.miniToTray;
            chkAutoMute.IsChecked = DataUtil.Config.sysConfig.miniToMute;
            chkExitConfrim.IsChecked = DataUtil.Config.sysConfig.exitConfirm;

            chkEnableHotKey.IsChecked = DataUtil.Config.sysConfig.enableHotKey;
            chkHotKeyCtrl.IsChecked = DataUtil.Config.sysConfig.hotKeyCtrl;
            chkHotKeyAlt.IsChecked = DataUtil.Config.sysConfig.hotKeyAlt;
            chkHotKeyShift.IsChecked = DataUtil.Config.sysConfig.hotKeyShift;
            tbHotKey.Text = DataUtil.Config.sysConfig.hotKey.ToString().ToUpper();

            cbCapFormat.SelectedIndex = (int)DataUtil.Config.sysConfig.capFormat;

            if (DataUtil.Config.sysConfig.gameServer == (int)GameInfo.ServersList.American || DataUtil.Config.sysConfig.gameServer == (int)GameInfo.ServersList.AmericanR18)
            {
                tbCssStyle.Text = DataUtil.Config.sysConfig.userCSSAmerican;
            }
            else
            {
                tbCssStyle.Text = DataUtil.Config.sysConfig.userCSS;
            }

            //自动推兔2.0设置
            cbAutoType.SelectedIndex = DataUtil.Config.sysConfig.autoType;
            cbPushType.SelectedIndex = DataUtil.Config.sysConfig.pushType;
            tbPushTimes.Text = DataUtil.Config.sysConfig.pushTimes.ToString();

            chkPotionTrue.IsChecked = DataUtil.Config.sysConfig.potionTrue;
            chkStoneTrue.IsChecked = DataUtil.Config.sysConfig.stoneTrue;

            chkRaidOther.IsChecked = DataUtil.Config.sysConfig.raidOther;
            chkRaidSelf.IsChecked = DataUtil.Config.sysConfig.raidSelf;
            chkSpecialTrue.IsChecked = DataUtil.Config.sysConfig.specialTrue;
            
            tbDelayTime.Text = DataUtil.Config.sysConfig.delayTime.ToString();

            chkSellTrue.IsChecked = DataUtil.Config.sysConfig.sellTrue;
            chkExploreTrue.IsChecked = DataUtil.Config.sysConfig.exploreTrue;
            chkGardenTrue.IsChecked = DataUtil.Config.sysConfig.gardenTrue;
            chkActionPrep.IsChecked = DataUtil.Config.sysConfig.actionPrep;

            chkGameRestart.IsChecked = DataUtil.Config.sysConfig.gameRestart;

            chkSpecialBlock.IsChecked = DataUtil.Config.sysConfig.specialBlock;

        }

        /// <summary>
        /// 初始化托盘图标
        /// </summary>
        /// <param name="visible">托盘图标是否隐藏</param>
        public void InitTrayIcon(bool visible = true)
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "团长助理";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = visible;
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.BalloonTipClicked += new EventHandler(notifyIcon_DoubleClick);

            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();

            System.Windows.Forms.MenuItem showItem = new System.Windows.Forms.MenuItem();
            showItem.DefaultItem = true;
            showItem.Text = "显示窗口(&O)";
            showItem.Click += new EventHandler(notifyIcon_DoubleClick);

            System.Windows.Forms.MenuItem closeItem = new System.Windows.Forms.MenuItem();
            closeItem.Text = "退出程序(&X)";
            closeItem.Click += new EventHandler(delegate { this.Close(); });

            menu.MenuItems.Add(showItem);
            menu.MenuItems.Add(closeItem);

            notifyIcon.ContextMenu = menu;
        }

        /// <summary>
        /// 初始化Nekoxy代理
        /// </summary>
        private void InitProxy()
        {
            HttpProxy.Shutdown();
            HttpProxy.Startup(DataUtil.Config.localProxyPort, false, false);
            HttpProxy.AfterSessionComplete += s => Task.Run(() => ProcessData(s));
            ApplyProxySettings();
        }

        /// <summary>
        /// 应用代理设置
        /// </summary>
        private void ApplyProxySettings()
        {
            if (DataUtil.Config.sysConfig.proxyType == SysConfig.ProxySettingsType.DirectAccess)
            {
                HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.DirectAccess);
                WinInetUtil.SetProxyInProcess($"http=127.0.0.1:{DataUtil.Config.localProxyPort}", "local");
            }
            else if (DataUtil.Config.sysConfig.proxyType == SysConfig.ProxySettingsType.UseSystemProxy)
            {
                HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.SystemProxy);
                WinInetUtil.SetProxyInProcessForNekoxy(DataUtil.Config.localProxyPort);
            }
            else if (DataUtil.Config.sysConfig.proxyType == SysConfig.ProxySettingsType.UseUserProxy)
            {
                HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.SpecificProxy, DataUtil.Config.sysConfig.proxyServer, DataUtil.Config.sysConfig.proxyPort);
                WinInetUtil.SetProxyInProcess(
                                    $"http=127.0.0.1:{DataUtil.Config.localProxyPort};"
                                    + $"https={DataUtil.Config.sysConfig.proxyServer}:{DataUtil.Config.sysConfig.proxyPort};"
                                    + $"ftp={DataUtil.Config.sysConfig.proxyServer}:{DataUtil.Config.sysConfig.proxyPort};"
                                    , "local");
            }
        }

        /// <summary>
        /// 根据DPI调整浏览器组件大小
        /// </summary>
        private void ResizeWeb()
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                double oldWidth = mainWeb.Width;
                double oldHeight = mainWeb.Height;
                float dpiX = graphics.DpiX;
                float dpiY = graphics.DpiY;
                mainWeb.Width = mainWeb.Width * (96.0 / dpiX);
                mainWeb.Height = mainWeb.Height * (96.0 / dpiY);
                this.Width -= (oldWidth - mainWeb.Width);
                this.Height -= (oldHeight - mainWeb.Height);
                this.Top += (oldHeight - mainWeb.Height) / 2;
                this.Left += (oldWidth - mainWeb.Width) / 2;
            }
        }

        /// <summary>
        /// 处理数据包
        /// </summary>
        /// <param name="data">捕获的数据包</param>
        private void ProcessData(object data)
        {
            Session s = data as Session;
            if (s.Request.PathAndQuery.IndexOf("/api/v1/") != -1)
            {
                PacketHelper.ProcessPacket(s);
            }
            else if (DataUtil.Game.gameServer == (int)GameInfo.ServersList.Japan || DataUtil.Game.gameServer == (int)GameInfo.ServersList.JapanR18 ||
                    DataUtil.Game.gameServer == (int)GameInfo.ServersList.TradChinese || DataUtil.Game.gameServer == (int)GameInfo.ServersList.TradChineseR18)
            {
                if (s.Request.PathAndQuery.IndexOf("/social/rpc") != -1)
                {
                    PacketHelper.ProcessPacket(s);
                    if (DataUtil.Config.sysConfig.showLoginNews && !newsHadShown)
                    {
                        newsHadShown = true;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            NewsWindow news = new NewsWindow();
                            news.Show();
                        }));
                    }
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        btnNews.Visibility = Visibility.Visible;
                    }));
                }
                /*else if (s.Request.PathAndQuery.IndexOf("/news/news_") != -1 && s.Request.PathAndQuery.IndexOf(".html?") != -1)
                {
                    if (DataUtil.Config.sysConfig.showLoginNews && !newsHadShown)
                    {
                        newsHadShown = true;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            NewsWindow news = new NewsWindow();
                            news.Show();
                        }));
                    }
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        btnNews.Visibility = Visibility.Visible;
                    }));
                }*/
            }
            else if (DataUtil.Game.gameServer == (int)GameInfo.ServersList.American || DataUtil.Game.gameServer == (int)GameInfo.ServersList.AmericanR18)
            {
                if (s.Request.PathAndQuery.IndexOf("/rpc?st=") != -1)
                {
                    PacketHelper.ProcessPacket(s);
                    if (DataUtil.Config.sysConfig.showLoginNews && !newsHadShown)
                    {
                        newsHadShown = true;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            NewsWindow news = new NewsWindow();
                            news.Show();
                        }));
                    }
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        btnNews.Visibility = Visibility.Visible;
                    }));
                }
                /*else if (s.Request.PathAndQuery.IndexOf("/news/news_") != -1 && s.Request.PathAndQuery.IndexOf(".html?") != -1 && s.Request.RequestLine.URI.IndexOf("http") != -1)
                {
                    if (DataUtil.Config.sysConfig.showLoginNews && !newsHadShown)
                    {
                        newsHadShown = true;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            NewsWindow news = new NewsWindow();
                            news.Show();
                        }));
                    }
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        btnNews.Visibility = Visibility.Visible;
                    }));
                }*/
            }
        }

        /// <summary>
        /// 将图形界面设置保存到设置模块
        /// </summary>
        private async Task<bool> SaveSettingsToConfig()
        {
            tbHotKey.Text = tbHotKey.Text.ToUpper();

            int tryInt;
            if (!int.TryParse(tbProxyPort.Text, out tryInt))
            {
                await this.ShowMessageAsync("错误 - 保存失败", "代理服务器端口号必须是整数！");
                return false;
            }
            if (!int.TryParse(tbAPTarget.Text, out tryInt))
            {
                await this.ShowMessageAsync("错误 - 保存失败", "指定体力必须是整数！");
                return false;
            }
            if (!int.TryParse(tbBPTarget.Text, out tryInt))
            {
                await this.ShowMessageAsync("错误 - 保存失败", "指定战点必须是整数！");
                return false;
            }
            if (!int.TryParse(tbPushTimes.Text, out tryInt))
            {
                await this.ShowMessageAsync("错误 - 保存失败", "代理服务器端口号必须是整数！");
                return false;
            }
            if (!int.TryParse(tbDelayTime.Text, out tryInt))
            {
                await this.ShowMessageAsync("错误 - 保存失败", "代理服务器端口号必须是整数！");
                return false;
            }
            if (chkEnableHotKey.IsChecked.HasValue && (bool)chkEnableHotKey.IsChecked)
            {
                if (!(bool)chkHotKeyAlt.IsChecked && !(bool)chkHotKeyCtrl.IsChecked && !(bool)chkHotKeyShift.IsChecked)
                {
                    await this.ShowMessageAsync("错误 - 保存失败", "老板键必须要选择Ctrl、Alt、Shift中的一个或者多个！");
                    return false;
                }
            }

            DataUtil.Config.sysConfig.showLoginDialog = chkShowLogin.IsChecked.HasValue ? (bool)chkShowLogin.IsChecked : false;
            DataUtil.Config.sysConfig.showLoginNews = chkShowNews.IsChecked.HasValue ? (bool)chkShowNews.IsChecked : false;
            DataUtil.Config.sysConfig.gameServer = cbGameServer.SelectedIndex;
            DataUtil.Config.sysConfig.gameHomePage = cbLoginPage.SelectedIndex;

            if (rbNotUseProxy.IsChecked.HasValue && (bool)rbNotUseProxy.IsChecked)
            {
                DataUtil.Config.sysConfig.proxyType = SysConfig.ProxySettingsType.DirectAccess;
            }
            else if (rbUseIEProxy.IsChecked.HasValue && (bool)rbUseIEProxy.IsChecked)
            {
                DataUtil.Config.sysConfig.proxyType = SysConfig.ProxySettingsType.UseSystemProxy;
            }
            else if (rbUseCusProxy.IsChecked.HasValue && (bool)rbUseCusProxy.IsChecked)
            {
                DataUtil.Config.sysConfig.proxyType = SysConfig.ProxySettingsType.UseUserProxy;
            }
            DataUtil.Config.sysConfig.proxyServer = tbProxyServer.Text;
            DataUtil.Config.sysConfig.proxyPort = int.Parse(tbProxyPort.Text);

            DataUtil.Config.sysConfig.apTargetNotify = chkAPTarget.IsChecked.HasValue ? (bool)chkAPTarget.IsChecked ? short.Parse(tbAPTarget.Text) : (short)0 : (short)0;
            DataUtil.Config.sysConfig.apFullNotify = chkAPFull.IsChecked.HasValue ? (bool)chkAPFull.IsChecked : false;
            DataUtil.Config.sysConfig.bpTargetNotify = chkBPTarget.IsChecked.HasValue ? (bool)chkBPTarget.IsChecked ? short.Parse(tbBPTarget.Text) : (short)0 : (short)0;
            DataUtil.Config.sysConfig.bpFullNotify = chkBPFull.IsChecked.HasValue ? (bool)chkBPFull.IsChecked : false;
            DataUtil.Config.sysConfig.spEveryNotify = chkSPEvery.IsChecked.HasValue ? (bool)chkSPEvery.IsChecked : false;
            DataUtil.Config.sysConfig.spFullNotify = chkSPFull.IsChecked.HasValue ? (bool)chkSPFull.IsChecked : false;
            DataUtil.Config.sysConfig.foundStageNotify = chkFoundStage.IsChecked.HasValue ? (bool)chkFoundStage.IsChecked : false;
            DataUtil.Config.sysConfig.foundBossNotify = chkFoundBoss.IsChecked.HasValue ? (bool)chkFoundBoss.IsChecked : false;

            DataUtil.Config.sysConfig.logGame = chkGameLog.IsChecked.HasValue ? (bool)chkGameLog.IsChecked : false;
            DataUtil.Config.sysConfig.logGacha = chkGachaLog.IsChecked.HasValue ? (bool)chkGachaLog.IsChecked : false;

            DataUtil.Config.sysConfig.autoGoInMaps = chkAutoGo.IsChecked.HasValue ? (bool)chkAutoGo.IsChecked : false;
            DataUtil.Config.sysConfig.autoGoTimeout = (int)slAutoRate.Value;

            DataUtil.Config.sysConfig.changeTitle = chkTitleChange.IsChecked.HasValue ? (bool)chkTitleChange.IsChecked : false;
            DataUtil.Config.sysConfig.alwaysShowTray = chkAlwaysTray.IsChecked.HasValue ? (bool)chkAlwaysTray.IsChecked : false;
            DataUtil.Config.sysConfig.miniToTray = chkMiniToTray.IsChecked.HasValue ? (bool)chkMiniToTray.IsChecked : false;
            DataUtil.Config.sysConfig.miniToMute = chkAutoMute.IsChecked.HasValue ? (bool)chkAutoMute.IsChecked : false;
            DataUtil.Config.sysConfig.exitConfirm = chkExitConfrim.IsChecked.HasValue ? (bool)chkExitConfrim.IsChecked : false;

            DataUtil.Config.sysConfig.enableHotKey = chkEnableHotKey.IsChecked.HasValue ? (bool)chkEnableHotKey.IsChecked : false;
            DataUtil.Config.sysConfig.hotKeyCtrl = chkHotKeyCtrl.IsChecked.HasValue ? (bool)chkHotKeyCtrl.IsChecked : false;
            DataUtil.Config.sysConfig.hotKeyAlt = chkHotKeyAlt.IsChecked.HasValue ? (bool)chkHotKeyAlt.IsChecked : false;
            DataUtil.Config.sysConfig.hotKeyShift = chkHotKeyShift.IsChecked.HasValue ? (bool)chkHotKeyShift.IsChecked : false;
            DataUtil.Config.sysConfig.hotKey = tbHotKey.Text[0];

            //自动推兔设置
            DataUtil.Config.sysConfig.autoType = cbAutoType.SelectedIndex;
            DataUtil.Config.sysConfig.pushType = cbPushType.SelectedIndex;
            DataUtil.Config.sysConfig.pushTimes = int.Parse(tbPushTimes.Text);

            DataUtil.Config.sysConfig.potionTrue = chkPotionTrue.IsChecked.HasValue ? (bool)chkPotionTrue.IsChecked : false;
            DataUtil.Config.sysConfig.stoneTrue = chkStoneTrue.IsChecked.HasValue ? (bool)chkStoneTrue.IsChecked : false;

            DataUtil.Config.sysConfig.raidOther = chkRaidOther.IsChecked.HasValue ? (bool)chkRaidOther.IsChecked : false;
            DataUtil.Config.sysConfig.raidSelf = chkRaidSelf.IsChecked.HasValue ? (bool)chkRaidSelf.IsChecked : false;
            DataUtil.Config.sysConfig.specialTrue = chkSpecialTrue.IsChecked.HasValue ? (bool)chkSpecialTrue.IsChecked : false;

            DataUtil.Config.sysConfig.delayTime = int.Parse(tbDelayTime.Text);

            DataUtil.Config.sysConfig.sellTrue = chkSellTrue.IsChecked.HasValue ? (bool)chkSellTrue.IsChecked : false;
            DataUtil.Config.sysConfig.exploreTrue = chkExploreTrue.IsChecked.HasValue ? (bool)chkExploreTrue.IsChecked : false;
            DataUtil.Config.sysConfig.gardenTrue = chkGardenTrue.IsChecked.HasValue ? (bool)chkGardenTrue.IsChecked : false;
            DataUtil.Config.sysConfig.actionPrep = chkActionPrep.IsChecked.HasValue ? (bool)chkActionPrep.IsChecked : false;

            DataUtil.Config.sysConfig.gameRestart = chkGameRestart.IsChecked.HasValue ? (bool)chkGameRestart.IsChecked : false;

            DataUtil.Config.sysConfig.specialBlock = chkSpecialBlock.IsChecked.HasValue ? (bool)chkSpecialBlock.IsChecked : false;


            DataUtil.Config.sysConfig.capFormat = (SysConfig.ScreenShotFormat)cbCapFormat.SelectedIndex;

            if (cbGameServer.SelectedIndex == (int)GameInfo.ServersList.American || cbGameServer.SelectedIndex == (int)GameInfo.ServersList.AmericanR18)
            {
                DataUtil.Config.sysConfig.userCSSAmerican = tbCssStyle.Text;
            }
            else
            {
                DataUtil.Config.sysConfig.userCSS = tbCssStyle.Text;
            }

            if (DataUtil.Config.sysConfig.alwaysShowTray)
            {
                notifyIcon.Visible = true;
            }
            else
            {
                notifyIcon.Visible = false;
            }

            if (DataUtil.Config.sysConfig.enableHotKey)
            {
                EnableHotKey();
            }
            else if (HotKeyHelper.isRegistered)
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                HotKeyHelper.UnregisterHotKey(handle, HotKeyHelper.hotKeyId);
            }

            ApplyProxySettings();

            return true;
        }


        /// <summary>
        /// 自动推兔定时器
        /// </summary>
        /// <param name="data">对象参数</param>
        private void AutoClickMouse(object data)
        {
            if (!DataUtil.Game.isOnline || !DataUtil.Game.canAuto || webHandle == IntPtr.Zero)
            {
                timerAuto.Change(Timeout.Infinite, DataUtil.Config.sysConfig.autoGoTimeout);
                return;
            }
            int x = 855, y = 545;
            if (autoGoLastConf > 0)
            {
                x = 765;
                y = 475;
                autoGoLastConf--;
            }
            /* 
            临时暂停原本的鼠标按键功能
            IntPtr lParam = (IntPtr)((y << 16) | x); //坐标信息
            IntPtr wParam = IntPtr.Zero; // 附加的按键信息（如：Ctrl）
            const uint downCode = 0x201; // 鼠标左键按下
            const uint upCode = 0x202; // 鼠标左键抬起
            PostMessage(webHandle, downCode, wParam, lParam); // 发送鼠标按键按下消息
            PostMessage(webHandle, upCode, wParam, lParam); // 发送鼠标按键抬起消息
            */
            MouseLeftClick(x, y);
        }

        /// <summary>
        /// 鼠标左键点击（坐标）
        /// </summary>
        /// <param name="x">x-横向坐标</param>
        /// <param name="y">y-竖向坐标</param>
        private void MouseLeftClick(int x, int y)
        {
            IntPtr lParam = (IntPtr)((y << 16) | x); //坐标信息
            IntPtr wParam = IntPtr.Zero; // 附加的按键信息（如：Ctrl）
            const uint downCode = 0x201; // 鼠标左键按下
            const uint upCode = 0x202; // 鼠标左键抬起
            PostMessage(webHandle, downCode, wParam, lParam); // 发送鼠标按键按下消息
            PostMessage(webHandle, upCode, wParam, lParam); // 发送鼠标按键抬起消息
        }

        /// <summary>
        /// 登录窗口发送过来的登录指令
        /// </summary>
        public void StartGame()
        {
            DataUtil.Game.gameServer = DataUtil.Config.currentAccount.gameServer;
            DataUtil.Config.sysConfig.gameServer = DataUtil.Game.gameServer;
            DataUtil.Config.sysConfig.gameHomePage = 1;
            mainWeb.Navigate(DataUtil.Game.gameUrl);
        }

        /// <summary>
        /// 手动静音/恢复声音
        /// </summary>
        private void MuteSound()
        {
            SoundHelper.Mute(true);
            if (SoundHelper.isMute)
            {
                btnMute.Visibility = Visibility.Hidden;
                btnUnMute.Visibility = Visibility.Visible;
            }
            else
            {
                btnMute.Visibility = Visibility.Visible;
                btnUnMute.Visibility = Visibility.Hidden;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确实要重新载入页面吗？", "操作确认", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                //MiscHelper.AddLog("正在重新载入游戏页面...", MiscHelper.LogType.System);
                //styleSheetApplied = false;
                //loginSubmitted = false;
                //newsHadShown = false;
                //DataUtil.Game.isOnline = false;
                //DataUtil.Game.canAuto = false;
                //mainWeb.Navigate(DataUtil.Game.gameUrl);
                Refresh();
            }
        }
        
        /// <summary>
        /// 独立出的刷新游戏代码
        /// </summary>
        private void Refresh()
        {
            MiscHelper.AddLog("正在重新载入游戏页面...", MiscHelper.LogType.System);
            styleSheetApplied = false;
            loginSubmitted = false;
            newsHadShown = false;
            DataUtil.Game.isOnline = false;
            DataUtil.Game.canAuto = false;
            mainWeb.Navigate(DataUtil.Game.gameUrl);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SystemInit();
            if (DataUtil.Config.sysConfig.showLoginDialog)
            {
                DataUtil.Config.currentAccount = new SysConfig.AccountList();
                LoginWindow login = new LoginWindow();
                login.Owner = this;
                login.ShowDialog();
            }
            else
            {
                mainWeb.Navigate(DataUtil.Game.gameUrl);
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataUtil.Config.sysConfig.exitConfirm && MessageBox.Show("是否确定要退出团长助理？", "退出确认", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
            timerCheck.Dispose();
            timerClock.Dispose();
            timerAuto.Dispose();
            if (DataUtil.Config.sysConfig.enableHotKey)
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                HotKeyHelper.UnregisterHotKey(handle, HotKeyHelper.hotKeyId);
            }
            if (SoundHelper.isMute) SoundHelper.Mute();
            if (notifyIcon != null)
            {
                notifyIcon.Dispose();
                notifyIcon = null;
            }
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (DataUtil.Config.sysConfig.miniToTray)
                {
                    Hide();
                    notifyIcon.Visible = true;
                }
                if (DataUtil.Config.sysConfig.miniToMute && !SoundHelper.isMute && !SoundHelper.userMute) SoundHelper.Mute();
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            if (!DataUtil.Config.sysConfig.alwaysShowTray) notifyIcon.Visible = false;
            if (DataUtil.Config.sysConfig.miniToMute && SoundHelper.isMute && !SoundHelper.userMute) SoundHelper.Mute();
        }

        private void mainWeb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (styleSheetApplied) return;
            var document = mainWeb.Document as HTMLDocument;
            if (document == null) return;

            //抽取Flash，应用CSS样式
            IHTMLElement gameFrame = null;
            if (DataUtil.Game.gameServer == (int)GameInfo.ServersList.American || DataUtil.Game.gameServer == (int)GameInfo.ServersList.AmericanR18)
            {
                gameFrame = document.getElementById("game_frame");
                if (gameFrame != null)
                {
                    mainWeb.Navigate(Convert.ToString(gameFrame.getAttribute("src")));
                    return;
                }
                else
                {
                    gameFrame = document.getElementById("externalContainer");
                }
            }
            else
            {
                gameFrame = document.getElementById("game_frame");
            }
            if (gameFrame != null)
            {
                var target = gameFrame?.document as HTMLDocument;
                if (target != null)
                {
                    if (DataUtil.Game.gameServer == (int)GameInfo.ServersList.American || DataUtil.Game.gameServer == (int)GameInfo.ServersList.AmericanR18)
                    {
                        target.createStyleSheet().cssText = DataUtil.Config.sysConfig.userCSSAmerican;
                    }
                    else
                    {
                        target.createStyleSheet().cssText = DataUtil.Config.sysConfig.userCSS;
                    }
                    styleSheetApplied = true;
                    MiscHelper.AddLog("抽取Flash样式应用成功！", MiscHelper.LogType.System);
                }
            }

            //自动登录相关
            if (!loginSubmitted && DataUtil.Config.currentAccount.username != null && DataUtil.Config.currentAccount.username.Trim() != "")
            {
                IHTMLElement username = null;
                IHTMLElement password = null;
                if (DataUtil.Game.gameServer == (int)GameInfo.ServersList.American || DataUtil.Game.gameServer == (int)GameInfo.ServersList.AmericanR18)
                {
                    username = document.getElementById("s-email");
                    password = document.getElementById("s-password");
                }
                else
                {
                    username = document.getElementById("login_id");
                    password = document.getElementById("password");
                }

                if (username == null || password == null) return;

                DESHelper des = new DESHelper();

                username.setAttribute("value", des.Decrypt(DataUtil.Config.currentAccount.username));
                password.setAttribute("value", des.Decrypt(DataUtil.Config.currentAccount.password));

                if (DataUtil.Config.currentAccount.username.Trim() == "" || DataUtil.Config.currentAccount.password == "")
                {
                    loginSubmitted = true;
                    return;
                }

                //点击登录按钮
                if (DataUtil.Game.gameServer == (int)GameInfo.ServersList.American || DataUtil.Game.gameServer == (int)GameInfo.ServersList.AmericanR18)
                {
                    IHTMLElement autoLogin = document.getElementById("autoLogin");
                    IHTMLElement login = document.getElementById("login-button");
                    if (autoLogin != null) autoLogin.click();
                    if (login != null)
                    {
                        login.click();
                        loginSubmitted = true;
                    }
                }
                else
                {
                    foreach (IHTMLElement element in document.all)
                    {
                        if (Convert.ToString(element.getAttribute("value")) == "ログイン")
                        {
                            element.click();
                            loginSubmitted = true;
                            break;
                        }
                    }
                }
            }
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            MuteSound();
        }

        private void btnUnMute_Click(object sender, RoutedEventArgs e)
        {
            MuteSound();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ShowConfigToSettings();
            mainTab.SelectedIndex = 0;
        }

        private async void btnReset_Click(object sender, RoutedEventArgs e)
        {
            tbCssStyle.Text = SysConfig.DefaultCSSJapan;
            if (cbGameServer.SelectedIndex == (int)GameInfo.ServersList.American || cbGameServer.SelectedIndex == (int)GameInfo.ServersList.AmericanR18)
            {
                tbCssStyle.Text = SysConfig.DefaultCSSAmerican;
            }
            await this.ShowMessageAsync("提示", "已经重置为默认抽取样式！");
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool result = await SaveSettingsToConfig();
            if (result)
            {
                if (DataUtil.Config.SaveConfig())
                {
                    DataUtil.Game.gameServer = DataUtil.Config.sysConfig.gameServer;
                    await this.ShowMessageAsync("提示", "已成功保存设置！");
                    mainTab.SelectedIndex = 0;
                }
                else
                {
                    await this.ShowMessageAsync("错误", "设置保存失败！");
                }
            }
        }
        

        private async void btnClearCache_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "RunDll32.exe";
            process.StartInfo.Arguments = "InetCpl.cpl,ClearMyTracksByProcess 8";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            await this.ShowMessageAsync("提示", "浏览器缓存文件清理完毕！");
        }

        private async void btnClearCookies_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "RunDll32.exe";
            process.StartInfo.Arguments = "InetCpl.cpl,ClearMyTracksByProcess 2";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            await this.ShowMessageAsync("提示", "浏览器Cookies清理完毕！");
        }

        public void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            if (!DataUtil.Game.isOnline) return;
            if (webHandle == IntPtr.Zero)
            {
                webHandle = mainWeb.Handle;
                webHandle = CordCol.GetWebHandle(webHandle);
                //独立出的获取句柄
                //StringBuilder className = new StringBuilder(100);
                //while (className.ToString() != "Internet Explorer_Server") // 浏览器组件类获取
                //{
                //    webHandle = GetWindow(webHandle, 5); // 获取子窗口的句柄
                //    GetClassName(webHandle, className, className.Capacity);
                //}
            }
            if (DataUtil.Game.isAuto)
            {
                MiscHelper.SetAutoGo(false);
            }
            else if (DataUtil.Game.canAuto)
            {
                MiscHelper.SetAutoGo(true);
            }
        }

        private void cbGameServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (cbGameServer.SelectedIndex == (int)GameInfo.ServersList.American || cbGameServer.SelectedIndex == (int)GameInfo.ServersList.AmericanR18)
            {
                tbCssStyle.Text = DataUtil.Config.sysConfig.userCSSAmerican;
            }
            else
            {
                tbCssStyle.Text = DataUtil.Config.sysConfig.userCSS;
            }
        }

        private void btnNews_Click(object sender, RoutedEventArgs e)
        {
            NewsWindow news = new NewsWindow();
            news.Show();
        }

        private void btnTopMost_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void dgDaliy_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            dgDaliy.SelectedIndex = -1;
        }

        private void mainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTab.SelectedIndex == 3)
            {
                int day = short.Parse(DataUtil.Game.serverTime.DayOfWeek.ToString("d"));
                for (int i = 0; i < dgDaliy.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)dgDaliy.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row == null) continue;
                    if (i == day)
                    {
                        row.Background = new SolidColorBrush(Colors.LightYellow);
                        row.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        row.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 37, 37));
                        row.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }
        }

        private void btnGacha_Click(object sender, RoutedEventArgs e)
        {
            GachaWindow gacha = new GachaWindow();
            gacha.Show();
        }

        private void btnFriendViewer_Click(object sender, RoutedEventArgs e)
        {
            FriendsWindow friends = new FriendsWindow();
            friends.Show();
        }

        private void btnCap_Click(object sender, RoutedEventArgs e)
        {
            if (!DataUtil.Game.isOnline) return;
            MiscHelper.ScreenShot();
        }

        private void btnMapInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!DataUtil.Game.isOnline && DataUtil.Game.canAuto) return;
            MapInfoWindow mapInfo = new MapInfoWindow();
            mapInfo.Show();
        }

        private void btnGameLogViewer_Click(object sender, RoutedEventArgs e)
        {
            GameLogsWindow gamelogs = new GameLogsWindow();
            gamelogs.Show();
        }

        private void btnGachaLogViewer_Click(object sender, RoutedEventArgs e)
        {
            GachaLogsWindow gachalogs = new GachaLogsWindow();
            gachalogs.Show();
        }

        /// <summary>
        /// 打开游戏坐标窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCord_Click(object sender, RoutedEventArgs e)
        {
            CordWindow cords = new CordWindow(Process.GetCurrentProcess().MainWindowHandle);
            cords.Show();
        }

        /// <summary>
        /// 自动推兔2.0按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPush_Click(object sender, RoutedEventArgs e)
        {
            if (PushTimes.Value() > 0)
            {
                MessageBox.Show("请点击下面的按钮暂停", "推兔中", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            //如果状态为0，启动推兔功能
            else
            {
                MessageBoxResult type = MessageBox.Show("点击OK开始自动推兔\r\n请在游戏主页开启此功能\r\n双击下面的X暂停，使用愉快", "脚本开始", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (type == MessageBoxResult.OK)
                {
                    PushTimes.Load(DataUtil.Config.sysConfig.pushTimes);
                    AutoPush();
                }
            }
            
        }

        /// <summary>
        /// 启动自动推兔
        /// </summary>
        private async void AutoPush()
        {
            IntPtr Han = GetWebHandle(mainWeb.Handle);
            MiscHelper.AddLog("开始推兔!", MiscHelper.LogType.System);
            Nodes Node = new Nodes();

            Node.ScInitialize(Han);

            Thread PushThread = new Thread(Node.Start);
            PushThread.Start();
            while (PushThread.IsAlive == true)
            {
                await Task.Delay(1000);
                if (PushTimes.Value() == 0)
                {
                    PushThread.Abort();
                }
                if (DataUtil.Game.isOnline == false &&
                    DataUtil.Config.sysConfig.gameRestart == true)
                {
                    PushThread.Abort();
                    Refresh();
                    Helpers.Color Col = Helpers.Color.Instance;
                    Mouse Mou = Mouse.Instance;
                    while (Col.Check(437, 177, 211, 209, 205) == false)
                    {
                        Mou.Click(800, 200);
                        await Task.Delay(1000);
                    }
                    AutoPush();
                }
            }
        }

        /// <summary>
        /// 自动推兔停止按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPush_Set(object sender, RoutedEventArgs e)
        {
            if(PushTimes.Value() > 0)
            {
                PushTimes.Reset();
                MessageBox.Show("暂停成功，推完这把就结束。", "暂停成功", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            }
        }
        
    }
}
