using FlowerMaster.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Xml;

namespace FlowerMaster.Models
{
    /// <summary>
    /// 系统配置类
    /// </summary>
    class SysConfig
    {
        /// <summary>
        /// 自动推图定时器触发时间间隔（毫秒）
        /// </summary>
        public const int AUTO_GO_TIMEOUT = 330;

        /// <summary>
        /// 系统配置结构体
        /// </summary>
        public struct Config
        {
            /// <summary>
            /// 显示游戏登录窗口
            /// </summary>
            public bool showLoginDialog;
            /// <summary>
            /// 显示游戏公告窗口
            /// </summary>
            public bool showLoginNews;
            /// <summary>
            /// 游戏服务器
            /// </summary>
            public int gameServer;
            /// <summary>
            /// 游戏加载页面
            /// </summary>
            public int gameHomePage;

            /// <summary>
            /// 代理类型
            /// </summary>
            public ProxySettingsType proxyType;
            /// <summary>
            /// 代理服务器地址
            /// </summary>
            public string proxyServer;
            /// <summary>
            /// 代理服务器端口
            /// </summary>
            public int proxyPort;

            /// <summary>
            /// 体力回复到指定值提醒
            /// </summary>
            public short apTargetNotify;
            /// <summary>
            /// 体力回复满提醒
            /// </summary>
            public bool apFullNotify;
            /// <summary>
            /// 战点回复到指定值提醒
            /// </summary>
            public short bpTargetNotify;
            /// <summary>
            /// 战点回复满提醒
            /// </summary>
            public bool bpFullNotify;
            /// <summary>
            /// 探索点每回复一点提醒
            /// </summary>
            public bool spEveryNotify;
            /// <summary>
            /// 探索点回复满提醒
            /// </summary>
            public bool spFullNotify;
            /// <summary>
            /// 发现隐藏副本提醒
            /// </summary>
            public bool foundStageNotify;
            /// <summary>
            /// 发现主页BOSS提醒
            /// </summary>
            public bool foundBossNotify;

            /// <summary>
            /// 记录游戏日志
            /// </summary>
            public bool logGame;
            /// <summary>
            /// 记录扭蛋日志
            /// </summary>
            public bool logGacha;

            /// <summary>
            /// 进图后自动推图
            /// </summary>
            public bool autoGoInMaps;
            /// <summary>
            /// 自动推图间隔时间
            /// </summary>
            private int _autoGoTimeout;
            /// <summary>
            /// 自动推图间隔时间
            /// </summary>
            public int autoGoTimeout
            {
                get
                {
                    return _autoGoTimeout;
                }
                set
                {
                    if (value < 150 || value > 500)
                    {
                        _autoGoTimeout = AUTO_GO_TIMEOUT;
                    }
                    else
                    {
                        _autoGoTimeout = value;
                    }
                }
            }

            /// <summary>
            /// 登录后标题栏显示角色名
            /// </summary>
            public bool changeTitle;
            /// <summary>
            /// 始终显示托盘图标
            /// </summary>
            public bool alwaysShowTray;
            /// <summary>
            /// 最小化到托盘
            /// </summary>
            public bool miniToTray;
            /// <summary>
            /// 最小化自动静音
            /// </summary>
            public bool miniToMute;
            /// <summary>
            /// 退出时弹出确认框
            /// </summary>
            public bool exitConfirm;

            /// <summary>
            /// 启用老板键
            /// </summary>
            public bool enableHotKey;
            /// <summary>
            /// 老板键Ctrl
            /// </summary>
            public bool hotKeyCtrl;
            /// <summary>
            /// 老板键Alt
            /// </summary>
            public bool hotKeyAlt;
            /// <summary>
            /// 老板键Shift
            /// </summary>
            public bool hotKeyShift;
            /// <summary>
            /// 老板键
            /// </summary>
            public char hotKey;

            /// <summary>
            /// 截图文件格式
            /// </summary>
            public ScreenShotFormat capFormat;

            /// <summary>
            /// 用户抽取日服Flash样式
            /// </summary>
            public string userCSS;
            /// <summary>
            /// 用户抽取美服Flash样式
            /// </summary>
            public string userCSSAmerican;
            /// <summary>
            /// 用户抽取台服Flash样式
            /// </summary>
            public string userCSSTaiwan;
        }
        /// <summary>
        /// 系统配置结构体
        /// </summary>
        public Config sysConfig;
        /// <summary>
        /// 账号列表结构体
        /// </summary>
        public struct AccountList
        {
            /// <summary>
            /// 账号用户名
            /// </summary>
            public string username;
            /// <summary>
            /// 账号密码
            /// </summary>
            public string password;
            /// <summary>
            /// 游戏服务器
            /// </summary>
            public int gameServer;
        }
        /// <summary>
        /// 保存的账号密码列表
        /// </summary>
        public List<AccountList> accountList = null;
        /// <summary>
        /// 当前登录的账号信息
        /// </summary>
        public AccountList currentAccount;
        /// <summary>
        /// 最后一次登录的游戏服务器
        /// </summary>
        public int LastLoginServer;
        /// <summary>
        /// 最后一次登录的账号用户名
        /// </summary>
        public string LastLoginAccount;
        /// <summary>
        /// 本地代理端口号
        /// </summary>
        public int localProxyPort;
        /// <summary>
        /// 代理类型枚举
        /// </summary>
        public enum ProxySettingsType
        {
            /// <summary>
            /// 不使用代理直接访问
            /// </summary>
            DirectAccess,
            /// <summary>
            /// 使用系统IE代理配置
            /// </summary>
            UseSystemProxy,
            /// <summary>
            /// 使用用户自定义代理配置
            /// </summary>
            UseUserProxy,
        }
        /// <summary>
        /// 截图文件格式
        /// </summary>
        public enum ScreenShotFormat
        {
            /// <summary>
            /// PNG格式
            /// </summary>
            PNG,
            /// <summary>
            /// JPG格式
            /// </summary>
            JPG,
        }

        /// <summary>
        /// 可用端口号查找GUID
        /// </summary>
        private const string PortReleaseGuid = "8875BD8E-4D5B-11DE-B2F4-691756D89593";

        /// <summary>
        /// 默认抽取日服Flash的CSS样式
        /// </summary>
        public const string DefaultCSSJapan = "body {\r\n    margin:0;\r\n    overflow:hidden;\r\n}\r\n\r\n#game_frame {\r\n    position:fixe" +
                    "d;\r\n    left:50%;\r\n    top:0px;\r\n    margin-left:-480px;\r\n    z-index:1;\r\n}\r\n\r\n" +
                    ".area-pickupgame,\r\n.area-menu\r\n{\r\n    display:none!important;\r\n}";
        /// <summary>
        /// 默认抽取美服Flash的CSS样式
        /// </summary>
        public const string DefaultCSSAmerican = "body {\r\n    margin:0;\r\n    overflow:hidden;\r\n}\r\n\r\n#externalContainer {\r\n    position:rela" +
                    "tive;\r\n    left:50%;\r\n    top:0px;\r\n    left:0px;\r\n    z-index:1;\r\n}\r\n\r\n" +
                    ".area-pickupgame,\r\n.area-menu\r\n{\r\n    display:none!important;\r\n}";
        /// <summary>
        /// 默认抽取台服Flash的CSS样式
        /// </summary>
        public const string DefaultCSSTaiwan = "body {\r\n    margin:0;\r\n    overflow:hidden;\r\n}\r\n\r\n#externalContainer {\r\n    position:fixe" +
                    "d;\r\n    left:50%;\r\n    top:0px;\r\n    margin-left:-480px;\r\n    z-index:1;\r\n}\r\n\r\n" +
                    ".area-pickupgame,\r\n.area-menu\r\n{\r\n    display:none!important;\r\n}";

        /// <summary>
        /// 初始化 FlowerMaster.Models.SysConfig 类的新实例。
        /// </summary>
        public SysConfig()
        {
            InitDefaultConfig();
            localProxyPort = 30001;
            FindEmptyPort();
        }
        
        /// <summary>
        /// 初始化默认设置
        /// </summary>
        private void InitDefaultConfig()
        {
            sysConfig.showLoginDialog = false;
            sysConfig.showLoginNews = false;
            sysConfig.gameServer = 0;
            sysConfig.gameHomePage = 1;

            sysConfig.proxyType = ProxySettingsType.DirectAccess;
            sysConfig.proxyServer = "127.0.0.1";
            sysConfig.proxyPort = 8099;

            sysConfig.apTargetNotify = 0;
            sysConfig.apFullNotify = false;
            sysConfig.bpTargetNotify = 0;
            sysConfig.bpFullNotify = false;
            sysConfig.spEveryNotify = false;
            sysConfig.spFullNotify = false;
            sysConfig.foundStageNotify = false;
            sysConfig.foundBossNotify = false;

            sysConfig.logGame = true;
            sysConfig.logGacha = true;

            sysConfig.autoGoInMaps = false;
            sysConfig.autoGoTimeout = AUTO_GO_TIMEOUT;

            sysConfig.changeTitle = false;
            sysConfig.alwaysShowTray = false;
            sysConfig.miniToTray = false;
            sysConfig.miniToMute = false;
            sysConfig.exitConfirm = false;

            sysConfig.enableHotKey = false;
            sysConfig.hotKeyCtrl = true;
            sysConfig.hotKeyAlt = true;
            sysConfig.hotKeyShift = false;
            sysConfig.hotKey = 'X';

            sysConfig.capFormat = ScreenShotFormat.PNG;

            sysConfig.userCSS = DefaultCSSJapan;
            sysConfig.userCSSAmerican = DefaultCSSAmerican;
            sysConfig.userCSSTaiwan = DefaultCSSTaiwan;
        }

        /// <summary>
        /// 查找空闲的端口号
        /// </summary>
        private void FindEmptyPort()
        {
            bool isAvailable = true;
            Mutex mutex = new Mutex(false, string.Concat("Global/", PortReleaseGuid));
            mutex.WaitOne();
            try
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints = ipGlobalProperties.GetActiveTcpListeners();
                do
                {
                    if (!isAvailable)
                    {
                        localProxyPort++;
                        isAvailable = true;
                    }
                    foreach (IPEndPoint endPoint in endPoints)
                    {
                        if (endPoint.Port != localProxyPort) continue;
                        isAvailable = false;
                        break;
                    }
                } while (!isAvailable && localProxyPort < IPEndPoint.MaxPort);
                if (!isAvailable)
                    throw new ApplicationException("找不到可用的代理端口！");
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// 载入系统配置
        /// </summary>
        public void LoadConfig()
        {
            if (!File.Exists("config.xml")) return;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("config.xml");
                XmlNode xn = xmlDoc.SelectSingleNode("/Config/Login");
                XmlElement xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.showLoginDialog = xe.GetAttribute("ShowLoginDialog") != "" ? bool.Parse(xe.GetAttribute("ShowLoginDialog")) : sysConfig.showLoginDialog;
                    sysConfig.showLoginNews = xe.GetAttribute("ShowNewsDialog") != "" ? bool.Parse(xe.GetAttribute("ShowNewsDialog")) : sysConfig.showLoginNews;
                    sysConfig.gameServer = xe.GetAttribute("GameServer") != "" ? int.Parse(xe.GetAttribute("GameServer")) : sysConfig.gameServer;
                    sysConfig.gameHomePage = xe.GetAttribute("GameHomePage") != "" ? int.Parse(xe.GetAttribute("GameHomePage")) : sysConfig.gameHomePage;
                }

                xn = xmlDoc.SelectSingleNode("/Config/Proxy");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.proxyType = xe.GetAttribute("ProxyType") != "" ? (ProxySettingsType)Enum.Parse(typeof(ProxySettingsType), xe.GetAttribute("ProxyType")) : sysConfig.proxyType;
                    sysConfig.proxyServer = xe.GetAttribute("ProxyServer");
                    sysConfig.proxyPort = xe.GetAttribute("ProxyPort") != "" ? int.Parse(xe.GetAttribute("ProxyPort")) : sysConfig.proxyPort;
                }

                xn = xmlDoc.SelectSingleNode("/Config/Notify");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.apTargetNotify = xe.GetAttribute("APTarget") != "" ? short.Parse(xe.GetAttribute("APTarget")) : sysConfig.apTargetNotify;
                    sysConfig.apFullNotify = xe.GetAttribute("APFull") != "" ? bool.Parse(xe.GetAttribute("APFull")) : sysConfig.apFullNotify;
                    sysConfig.bpTargetNotify = xe.GetAttribute("BPTarget") != "" ? short.Parse(xe.GetAttribute("BPTarget")) : sysConfig.bpTargetNotify;
                    sysConfig.bpFullNotify = xe.GetAttribute("BPFull") != "" ? bool.Parse(xe.GetAttribute("BPFull")) : sysConfig.bpFullNotify;
                    sysConfig.spEveryNotify = xe.GetAttribute("SPEvery") != "" ? bool.Parse(xe.GetAttribute("SPEvery")) : sysConfig.spEveryNotify;
                    sysConfig.spFullNotify = xe.GetAttribute("SPFull") != "" ? bool.Parse(xe.GetAttribute("SPFull")) : sysConfig.spFullNotify;
                    sysConfig.foundStageNotify = xe.GetAttribute("FoundStage") != "" ? bool.Parse(xe.GetAttribute("FoundStage")) : sysConfig.foundStageNotify;
                    sysConfig.foundBossNotify = xe.GetAttribute("FoundBoss") != "" ? bool.Parse(xe.GetAttribute("FoundBoss")) : sysConfig.foundBossNotify;
                }

                xn = xmlDoc.SelectSingleNode("/Config/Logs");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.logGame = xe.GetAttribute("GameLog") != "" ? bool.Parse(xe.GetAttribute("GameLog")) : sysConfig.logGame;
                    sysConfig.logGacha = xe.GetAttribute("GachaLog") != "" ? bool.Parse(xe.GetAttribute("GachaLog")) : sysConfig.logGacha;
                }

                xn = xmlDoc.SelectSingleNode("/Config/Assist");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.autoGoInMaps = xe.GetAttribute("AutoGoInMaps") != "" ? bool.Parse(xe.GetAttribute("AutoGoInMaps")) : sysConfig.autoGoInMaps;
                    sysConfig.autoGoTimeout = xe.GetAttribute("AutoGoTimeout") != "" ? int.Parse(xe.GetAttribute("AutoGoTimeout")) : sysConfig.autoGoTimeout;
                }

                xn = xmlDoc.SelectSingleNode("/Config/System");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.changeTitle = xe.GetAttribute("ChangeTitle") != "" ? bool.Parse(xe.GetAttribute("ChangeTitle")) : sysConfig.changeTitle;
                    sysConfig.alwaysShowTray = xe.GetAttribute("AlwaysShowTrayIcon") != "" ? bool.Parse(xe.GetAttribute("AlwaysShowTrayIcon")) : sysConfig.alwaysShowTray;
                    sysConfig.miniToTray = xe.GetAttribute("MinimizeToTray") != "" ? bool.Parse(xe.GetAttribute("MinimizeToTray")) : sysConfig.miniToTray;
                    sysConfig.miniToMute = xe.GetAttribute("MinimizeToMute") != "" ? bool.Parse(xe.GetAttribute("MinimizeToMute")) : sysConfig.miniToMute;
                    sysConfig.exitConfirm = xe.GetAttribute("ExitConfirm") != "" ? bool.Parse(xe.GetAttribute("ExitConfirm")) : sysConfig.exitConfirm;
                }

                xn = xmlDoc.SelectSingleNode("/Config/HotKey");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.enableHotKey = xe.GetAttribute("Enabled") != "" ? bool.Parse(xe.GetAttribute("Enabled")) : sysConfig.enableHotKey;
                    sysConfig.hotKeyCtrl = xe.GetAttribute("Ctrl") != "" ? bool.Parse(xe.GetAttribute("Ctrl")) : sysConfig.hotKeyCtrl;
                    sysConfig.hotKeyAlt = xe.GetAttribute("Alt") != "" ? bool.Parse(xe.GetAttribute("Alt")) : sysConfig.hotKeyAlt;
                    sysConfig.hotKeyShift = xe.GetAttribute("Shift") != "" ? bool.Parse(xe.GetAttribute("Shift")) : sysConfig.hotKeyShift;
                    sysConfig.hotKey = xe.GetAttribute("Key") != "" ? char.Parse(xe.GetAttribute("Key")) : sysConfig.hotKey;
                }

                xn = xmlDoc.SelectSingleNode("/Config/ScreenShot");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.capFormat = xe.GetAttribute("FileFormat") != "" ? (ScreenShotFormat)Enum.Parse(typeof(ScreenShotFormat), xe.GetAttribute("FileFormat")) : sysConfig.capFormat;
                }

                xn = xmlDoc.SelectSingleNode("/Config/UserCssStyle");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.userCSS = xe.GetAttribute("CssStyle") != "" ? xe.GetAttribute("CssStyle") : DefaultCSSJapan;
                }
                xn = xmlDoc.SelectSingleNode("/Config/UserCssStyleAmerican");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.userCSSAmerican = xe.GetAttribute("CssStyle") != "" ? xe.GetAttribute("CssStyle") : DefaultCSSAmerican;
                }
                xn = xmlDoc.SelectSingleNode("/Config/UserCssStyleTaiwan");
                xe = (XmlElement)xn;
                if (xe != null)
                {
                    sysConfig.userCSSTaiwan = xe.GetAttribute("CssStyle") != "" ? xe.GetAttribute("CssStyle") : DefaultCSSTaiwan;
                }
            }
            catch{ }
        }

        /// <summary>
        /// 保存系统配置
        /// </summary>
        /// <returns>返回保存成功与否结果</returns>
        public bool SaveConfig()
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                if (!File.Exists("config.xml"))
                {
                    XmlDeclaration Declaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                    xmlDoc.AppendChild(Declaration);
                    XmlNode rootNode = xmlDoc.CreateElement("Config");
                    xmlDoc.AppendChild(rootNode);

                    XmlElement accounts = xmlDoc.CreateElement("Accounts");
                    rootNode.AppendChild(accounts);

                    XmlElement login = xmlDoc.CreateElement("Login");
                    login.SetAttribute("ShowLoginDialog", sysConfig.showLoginDialog.ToString());
                    login.SetAttribute("ShowNewsDialog", sysConfig.showLoginNews.ToString());
                    login.SetAttribute("GameServer", sysConfig.gameServer.ToString());
                    login.SetAttribute("GameHomePage", sysConfig.gameHomePage.ToString());
                    rootNode.AppendChild(login);

                    XmlElement proxy = xmlDoc.CreateElement("Proxy");
                    proxy.SetAttribute("ProxyType", sysConfig.proxyType.ToString());
                    proxy.SetAttribute("ProxyServer", sysConfig.proxyServer);
                    proxy.SetAttribute("ProxyPort", sysConfig.proxyPort.ToString());
                    rootNode.AppendChild(proxy);

                    XmlElement notify = xmlDoc.CreateElement("Notify");
                    notify.SetAttribute("APTarget", sysConfig.apTargetNotify.ToString());
                    notify.SetAttribute("APFull", sysConfig.apFullNotify.ToString());
                    notify.SetAttribute("BPTarget", sysConfig.bpTargetNotify.ToString());
                    notify.SetAttribute("BPFull", sysConfig.bpFullNotify.ToString());
                    notify.SetAttribute("SPEvery", sysConfig.spEveryNotify.ToString());
                    notify.SetAttribute("SPFull", sysConfig.spFullNotify.ToString());
                    notify.SetAttribute("FoundStage", sysConfig.foundStageNotify.ToString());
                    notify.SetAttribute("FoundBoss", sysConfig.foundBossNotify.ToString());
                    rootNode.AppendChild(notify);

                    XmlElement logs = xmlDoc.CreateElement("Logs");
                    logs.SetAttribute("GameLog", sysConfig.logGame.ToString());
                    logs.SetAttribute("GachaLog", sysConfig.logGacha.ToString());
                    rootNode.AppendChild(logs);

                    XmlElement assist = xmlDoc.CreateElement("Assist");
                    assist.SetAttribute("AutoGoInMaps", sysConfig.autoGoInMaps.ToString());
                    assist.SetAttribute("AutoGoTimeout", sysConfig.autoGoTimeout.ToString());
                    rootNode.AppendChild(assist);

                    XmlElement system = xmlDoc.CreateElement("System");
                    system.SetAttribute("ChangeTitle", sysConfig.changeTitle.ToString());
                    system.SetAttribute("AlwaysShowTrayIcon", sysConfig.alwaysShowTray.ToString());
                    system.SetAttribute("MinimizeToTray", sysConfig.miniToTray.ToString());
                    system.SetAttribute("MinimizeToMute", sysConfig.miniToMute.ToString());
                    system.SetAttribute("ExitConfirm", sysConfig.exitConfirm.ToString());
                    rootNode.AppendChild(system);

                    XmlElement hotKey = xmlDoc.CreateElement("HotKey");
                    hotKey.SetAttribute("Enabled", sysConfig.enableHotKey.ToString());
                    hotKey.SetAttribute("Ctrl", sysConfig.hotKeyCtrl.ToString());
                    hotKey.SetAttribute("Alt", sysConfig.hotKeyAlt.ToString());
                    hotKey.SetAttribute("Shift", sysConfig.hotKeyShift.ToString());
                    hotKey.SetAttribute("Key", sysConfig.hotKey.ToString());
                    rootNode.AppendChild(hotKey);

                    XmlElement screenShot = xmlDoc.CreateElement("ScreenShot");
                    screenShot.SetAttribute("FileFormat", sysConfig.capFormat.ToString());
                    rootNode.AppendChild(screenShot);

                    XmlElement css = xmlDoc.CreateElement("UserCssStyle");
                    css.SetAttribute("CssStyle", sysConfig.userCSS);
                    rootNode.AppendChild(css);
                    XmlElement cssA = xmlDoc.CreateElement("UserCssStyleAmerican");
                    css.SetAttribute("CssStyle", sysConfig.userCSSAmerican);
                    rootNode.AppendChild(cssA);
                    XmlElement cssT = xmlDoc.CreateElement("UserCssStyleTaiwan");
                    css.SetAttribute("CssStyle", sysConfig.userCSSTaiwan);
                    rootNode.AppendChild(cssT);

                    xmlDoc.Save("config.xml");
                }
                else
                {
                    xmlDoc.Load("config.xml");
                    XmlNode rootNode = xmlDoc.SelectSingleNode("Config");

                    XmlNode xn = xmlDoc.SelectSingleNode("/Config/Login");
                    XmlElement xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("Login");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("ShowLoginDialog", sysConfig.showLoginDialog.ToString());
                    xe.SetAttribute("ShowNewsDialog", sysConfig.showLoginNews.ToString());
                    xe.SetAttribute("GameServer", sysConfig.gameServer.ToString());
                    xe.SetAttribute("GameHomePage", sysConfig.gameHomePage.ToString());

                    xn = xmlDoc.SelectSingleNode("/Config/Proxy");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("Proxy");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("ProxyType", sysConfig.proxyType.ToString());
                    xe.SetAttribute("ProxyServer", sysConfig.proxyServer);
                    xe.SetAttribute("ProxyPort", sysConfig.proxyPort.ToString());

                    xn = xmlDoc.SelectSingleNode("/Config/Notify");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("Notify");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("APTarget", sysConfig.apTargetNotify.ToString());
                    xe.SetAttribute("APFull", sysConfig.apFullNotify.ToString());
                    xe.SetAttribute("BPTarget", sysConfig.bpTargetNotify.ToString());
                    xe.SetAttribute("BPFull", sysConfig.bpFullNotify.ToString());
                    xe.SetAttribute("SPEvery", sysConfig.spEveryNotify.ToString());
                    xe.SetAttribute("SPFull", sysConfig.spFullNotify.ToString());
                    xe.SetAttribute("FoundStage", sysConfig.foundStageNotify.ToString());
                    xe.SetAttribute("FoundBoss", sysConfig.foundBossNotify.ToString());

                    xn = xmlDoc.SelectSingleNode("/Config/Logs");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("Logs");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("GameLog", sysConfig.logGame.ToString());
                    xe.SetAttribute("GachaLog", sysConfig.logGacha.ToString());

                    xn = xmlDoc.SelectSingleNode("/Config/Assist");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("Assist");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("AutoGoInMaps", sysConfig.autoGoInMaps.ToString());
                    xe.SetAttribute("AutoGoTimeout", sysConfig.autoGoTimeout.ToString());

                    xn = xmlDoc.SelectSingleNode("/Config/System");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("System");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("ChangeTitle", sysConfig.changeTitle.ToString());
                    xe.SetAttribute("AlwaysShowTrayIcon", sysConfig.alwaysShowTray.ToString());
                    xe.SetAttribute("MinimizeToTray", sysConfig.miniToTray.ToString());
                    xe.SetAttribute("MinimizeToMute", sysConfig.miniToMute.ToString());
                    xe.SetAttribute("ExitConfirm", sysConfig.exitConfirm.ToString());

                    xn = xmlDoc.SelectSingleNode("/Config/HotKey");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("HotKey");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("Enabled", sysConfig.enableHotKey.ToString());
                    xe.SetAttribute("Ctrl", sysConfig.hotKeyCtrl.ToString());
                    xe.SetAttribute("Alt", sysConfig.hotKeyAlt.ToString());
                    xe.SetAttribute("Shift", sysConfig.hotKeyShift.ToString());
                    xe.SetAttribute("Key", sysConfig.hotKey.ToString());

                    xn = xmlDoc.SelectSingleNode("/Config/ScreenShot");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("ScreenShot");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("FileFormat", sysConfig.capFormat.ToString());

                    xn = xmlDoc.SelectSingleNode("/Config/UserCssStyle");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("UserCssStyle");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("CssStyle", sysConfig.userCSS);

                    xn = xmlDoc.SelectSingleNode("/Config/UserCssStyleAmerican");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("UserCssStyleAmerican");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("CssStyle", sysConfig.userCSSAmerican);

                    xn = xmlDoc.SelectSingleNode("/Config/UserCssStyleTaiwan");
                    xe = (XmlElement)xn;
                    if (xe == null)
                    {
                        xe = xmlDoc.CreateElement("UserCssStyleTaiwan");
                        rootNode.AppendChild(xe);
                    }
                    xe.SetAttribute("CssStyle", sysConfig.userCSSTaiwan);

                    xmlDoc.Save("config.xml");
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 载入账号信息
        /// </summary>
        public void LoadAccounts()
        {
            if (!File.Exists("config.xml")) return;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("config.xml");
            XmlNode xn = xmlDoc.SelectSingleNode("/Config/Accounts");
            XmlElement xe = (XmlElement)xn;
            LastLoginServer = xe.GetAttribute("LastServer") == "" ? 0 : int.Parse(xe.GetAttribute("LastServer"));
            LastLoginAccount = xe.GetAttribute("LastAccount") == "" ? "" : xe.GetAttribute("LastAccount");
            XmlNodeList xnl = xn.ChildNodes;
            if (xnl.Count <= 0) return;

            try
            {
                accountList = new List<AccountList>();
                foreach (XmlNode acc in xnl)
                {
                    xe = (XmlElement)acc;
                    AccountList tmp = new AccountList();
                    tmp.username = xe.GetAttribute("Username");
                    tmp.password = xe.GetAttribute("Password");
                    tmp.gameServer = int.Parse(xe.GetAttribute("Server"));
                    if (tmp.username.Trim() == "" || tmp.password.Trim() == "") continue;
                    accountList.Add(tmp);
                }
            }
            catch { }
        }

        /// <summary>
        /// 保存账号信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="server">游戏服务器</param>
        public void SaveAccounts(string username, string password, int server)
        {
            if (username.Trim() == "" || password == "") return;

            AccountList acc = new AccountList();
            DESHelper des = new DESHelper();
            acc.username = des.Encrypt(username);
            acc.password = des.Encrypt(password);
            acc.gameServer = server;

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration Declaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlNode rootNode = null;
            XmlElement accounts = null;
            if (!File.Exists("config.xml"))
            {
                xmlDoc.AppendChild(Declaration);
                xmlDoc.CreateElement("Config");
                xmlDoc.AppendChild(rootNode);
                accounts = xmlDoc.CreateElement("Accounts");
                rootNode.AppendChild(accounts);
            }
            else
            {
                xmlDoc.Load("config.xml");
                accounts = (XmlElement)xmlDoc.SelectSingleNode("/Config/Accounts");
            }
            bool found = false;
            if (accountList != null)
            {
                for (int i=0; i<accountList.Count; i++)
                {
                    if (accountList[i].username == acc.username && accountList[i].gameServer == acc.gameServer)
                    {
                        accountList[i] = acc;
                        found = true;
                        break;
                    }
                }
            }
            if (found)
            {
                XmlNodeList xnl = accounts.ChildNodes;
                foreach (XmlNode account in xnl)
                {
                    XmlElement xe = (XmlElement)account;
                    if (xe.GetAttribute("Username") == des.Encrypt(username) && int.Parse(xe.GetAttribute("Server")) == server)
                    {
                        xe.SetAttribute("Password", des.Encrypt(password));
                        break;
                    }
                }
            }
            else
            {
                XmlElement account = xmlDoc.CreateElement("Account");
                accounts.AppendChild(account);
                account.SetAttribute("Username", acc.username);
                account.SetAttribute("Password", acc.password);
                account.SetAttribute("Server", acc.gameServer.ToString());
            }

            accounts.SetAttribute("LastServer", server.ToString());
            accounts.SetAttribute("LastAccount", acc.username);

            xmlDoc.Save("config.xml");
        }

        /// <summary>
        /// 删除账号信息
        /// </summary>
        /// <param name="username">要删除的用户名</param>
        /// <param name="server">游戏服务器</param>
        public void DeleteAccount(string username, int server)
        {
            if (accountList == null || username.Trim() == "") return;
            DESHelper des = new DESHelper();
            for (int i=0; i<accountList.Count; i++)
            {
                if (accountList[i].username == des.Encrypt(username))
                {
                    accountList.RemoveAt(i);
                    break;
                }
            }

            if (!File.Exists("config.xml")) return;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("config.xml");
            XmlNode xn = xmlDoc.SelectSingleNode("/Config/Accounts");
            XmlNodeList xnl = xn.ChildNodes;
            if (xnl.Count <= 0) return;

            foreach (XmlNode acc in xnl)
            {
                XmlElement xe = (XmlElement)acc;
                if (xe.GetAttribute("Username") == des.Encrypt(username) && int.Parse(xe.GetAttribute("Server")) == server)
                {
                    xe.ParentNode.RemoveChild(xe);
                    break;
                }
            }
            xmlDoc.Save("config.xml");
        }
    }
}
