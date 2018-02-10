using FlowerMaster.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FlowerMaster.Models
{
    /// <summary>
    /// 游戏信息类
    /// </summary>
    class GameInfo : IGameInfo
    {
        /// <summary>
        /// 玩家信息结构体
        /// </summary>
        public struct PlayerInfo
        {
            public string name;
            public int lv;
            public int exp;
            public int maxExp;
            public int oldAP;
            public int AP;
            public int maxAP;
            public DateTime apTime;
            public int oldBP;
            public int BP;
            public int maxBP;
            public DateTime bpTime;
            public int oldSP;
            public int SP;
            public int maxSP;
            public DateTime spTime;
            public int money;
            public int stone;
            public string friendId;
            public DateTime plantTime;
        }
        /// <summary>
        /// 玩家信息
        /// </summary>
        public PlayerInfo player;

        /// <summary>
        /// 提醒信息结构体
        /// </summary>
        public struct NotifyInfo
        {
            public int lastAP;
            public int lastBP;
            public int lastSP;
        }
        /// <summary>
        /// 提醒信息
        /// </summary>
        public NotifyInfo notifyRecord;

        /// <summary>
        /// 好友信息类
        /// </summary>
        public class FriendInfo
        {
            public string name { get; set; }
            public int lv { get; set; }
            public int totalPower { get; set; }
            public string regTime { get; set; }
            public string lastTime { get; set; }
            public string leader { get; set; }
            public string card1 { get; set; }
            public string card2 { get; set; }
            public string card3 { get; set; }
            public string card4 { get; set; }
            public string card5 { get; set; }
        }
        /// <summary>
        /// 好友列表集合
        /// </summary>
        public ObservableCollection<FriendInfo> friendList = null;

        /// <summary>
        /// 怪物信息类
        /// </summary>
        public class BossInfo
        {
            public string name { get; set; }
            public int hp { get; set; }
            public int atk { get; set; }
            public int def { get; set; }
            public string skill { get; set; }
            public int group { get; set; }
            public int money { get; set; }
            public int gp { get; set; }
        }
        /// <summary>
        /// 怪物列表集合
        /// </summary>
        public ObservableCollection<BossInfo> bossList = null;

        /// <summary>
        /// 每日副本类
        /// </summary>
        public class DaliyInfo
        {
            public string day { get; set; }
            public string eventStage { get; set; }
        }

        /// <summary>
        /// 主线经验表类
        /// </summary>
        public class ExpTable
        {
            public string stage { get; set; }
            public int ap { get; set; }
            public int exp { get; set; }
            public string effect { get; set; }
        }

        /// <summary>
        /// 游戏服务器列表枚举
        /// </summary>
        public enum ServersList
        {
            Japan = 0,
            JapanR18 = 1,
            American = 2,
            AmericanR18 = 3,
        };

        /// <summary>
        /// 用户点数类型枚举
        /// </summary>
        public enum PlayerPointType
        {
            AP = 0,
            BP = 1,
            SP = 2,
        };

        public const int PLAYER_MAX_BP = 6; //玩家最大BP点数
        public const int PLAYER_MAX_SP = 3; //玩家最大探索点数

        public const int TIMEOUT_AP = 180; //AP回复时间，3分钟
        public const int TIMEOUT_BP = 1800; //BP回复时间，30分钟
        public const int TIMEOUT_SP = 7200; //探索回复时间，2小时

        //用户点数据操作锁
        private object locker = new object();
        //低等级经验表
        private int[] expLow = { 15, 30, 160, 200, 230, 260, 290, 320, 350, 400, 450, 500, 600, 700, 900 };
        //高等级经验表
        private int[] expHigh = { 300, 500, 800, 1200, 1500, 2500, 3500, 5000, 7500, 10000, 15000, 20000 };
        //极高等级经验表
        private int[] expExt = { 350, 365, 380, 395, 410, 425, 440, 455, 470, 485, 500, 515, 530, 545, 560, 575, 590, 605, 620, 635,
                                 650, 650, 650, 650, 650, 650, 650, 650, 650, 650, 740, 740, 740, 740, 740, 740, 740, 740, 740, 740, 
                                 830, 830, 830, 830, 830, 830, 830, 830, 830, 830};

        private Dictionary<int, string> _gameServers;
        private Dictionary<int, string> _gameUrls;
        private Dictionary<int, string> _gameNewsUrls;
        private int _gameServer;
        private string _gameUrl;
        private string _gameNewsUrl;
        private bool _isOnline;
        private bool _isAuto;
        private bool _canAuto;
        private DateTime _serverTime;
        private ObservableCollection<DaliyInfo> _daliyInfo;
        private ObservableCollection<ExpTable> _expTable;
        
        /// <summary>
        /// 初始化 FlowerMaster.Models.GameInfo 类的新实例。
        /// </summary>
        public GameInfo()
        {
            InitGameServers();
            InitGameUrls();
            InitGameNewsUrls();
            InitDaliyInfo();
            InitExpTable();
            this._isOnline = false;
            this._isAuto = false;
            this._canAuto = false;
            this._serverTime = DateTime.Now;
            player.spTime = this._serverTime;
            player.oldSP = 3;
        }

        /// <summary>
        /// 初始化服务器游戏首页列表
        /// </summary>
        private void InitGameServers()
        {
            _gameServers = new Dictionary<int, string>();
            _gameServers.Add((int)ServersList.Japan, "https://www.dmm.com/netgame_s/flower/");
            _gameServers.Add((int)ServersList.JapanR18, "https://www.dmm.co.jp/netgame_s/flower-x/");
            _gameServers.Add((int)ServersList.American, "https://www.nutaku.com/games/flower-knight-girl-online/");
            _gameServers.Add((int)ServersList.AmericanR18, "https://www.nutaku.net/games/flower-knight-girl/");
        }

        /// <summary>
        /// 初始化服务器游戏页列表
        /// </summary>
        private void InitGameUrls()
        {
            _gameUrls = new Dictionary<int, string>();
            _gameUrls.Add((int)ServersList.Japan, "https://pc-play.games.dmm.com/play/flower");
            _gameUrls.Add((int)ServersList.JapanR18, "https://pc-play.games.dmm.co.jp/play/flower-x/");
            _gameUrls.Add((int)ServersList.American, "https://www.nutaku.com/games/flower-knight-girl-online/play/");
            _gameUrls.Add((int)ServersList.AmericanR18, "https://www.nutaku.net/games/flower-knight-girl/play/");
        }

        /// <summary>
        /// 初始化服务器新闻页面列表
        /// </summary>
        private void InitGameNewsUrls()
        {
            _gameNewsUrls = new Dictionary<int, string>();
            _gameNewsUrls.Add((int)ServersList.Japan, "https://s3-ap-northeast-1.amazonaws.com/flower-help/index.html");
            _gameNewsUrls.Add((int)ServersList.JapanR18, "https://s3-ap-northeast-1.amazonaws.com/flower-help/index.html");
            _gameNewsUrls.Add((int)ServersList.American, "https://cdn.flowerknight.nutaku.net/index.html");
            _gameNewsUrls.Add((int)ServersList.AmericanR18, "https://cdn.flowerknight.nutaku.net/index.html");
        }

        /// <summary>
        /// 计算当前用户低于15级的递归经验算法
        /// </summary>
        /// <param name="lv">还需计算的等级</param>
        /// <returns></returns>
        private int _ReCalcPlayerMaxExpLow(int lv)
        {
            if (lv > 1)
            {
                return expLow[lv - 1] + _ReCalcPlayerMaxExpLow(lv - 1);
            }
            else
            {
                return expLow[0];
            }
        }

        /// <summary>
        /// 计算当前用户超过100级的递归经验算法
        /// </summary>
        /// <param name="lv">还需计算的等级</param>
        /// <returns></returns>
        private int _ReCalcPlayerMaxExpHigh(int lv)
        {
            if (lv <= 10)
            {
                return 300 * lv;
            }
            else if (lv % 10 > 0)
            {
                int exp = (lv % 10) * expHigh[lv / 10];
                return exp + _ReCalcPlayerMaxExpHigh(lv - lv % 10);
            }
            else
            {
                int exp = lv > 50 ? 9 * expHigh[lv / 10 - 1] + expHigh[lv / 10] : 10 * expHigh[lv / 10 - 1];
                return exp + _ReCalcPlayerMaxExpHigh(lv - 10);
            }
        }

        /// <summary>
        /// 计算玩家最大体力和经验值
        /// </summary>
        public void CalcPlayerMaxAPExp()
        {
            if (player.lv > 0)
            {
                if (player.lv <= 99)
                {
                    player.maxAP = 50 + 3 * (player.lv - 1);
                }
                else if (player.lv <= 155)
                {
                    player.maxAP = 344 + player.lv - 99;
                }
                else
                {
                    player.maxAP = 400 + (player.lv - 155) / 2;
                }
                if (player.lv <= 15)
                {
                    player.maxExp = _ReCalcPlayerMaxExpLow(player.lv);
                }
                else if (player.lv <= 100)
                {
                    player.maxExp = player.lv * 100 - 500;
                }
                else
                {
                    player.maxExp = 9500 + _ReCalcPlayerMaxExpHigh(player.lv - 100);
                }
            }
        }

        /// <summary>
        /// 用户点数变更处理
        /// </summary>
        /// <param name="timeType">点数类型</param>
        /// <param name="newVal">新值</param>
        /// <param name="newTime">新时间</param>
        public void CalcPlayerGamePoint(PlayerPointType timeType, JToken newVal, JToken newTime)
        {
            if (newVal == null || newTime == null) return;
            lock (locker)
            {
                switch (timeType)
                {
                    case PlayerPointType.AP:
                        player.oldAP = int.Parse(newVal.ToString());
                        player.apTime = Convert.ToDateTime(newTime.ToString());
                        TimeSpan span = DataUtil.Game.serverTime.Subtract(player.apTime);
                        player.AP = player.oldAP + (int)Math.Round(span.TotalSeconds) / TIMEOUT_AP;
                        if (player.AP > player.maxAP) player.AP = player.maxAP;
                        break;
                    case PlayerPointType.BP:
                        player.oldBP = int.Parse(newVal.ToString());
                        player.bpTime = Convert.ToDateTime(newTime.ToString());
                        span = DataUtil.Game.serverTime.Subtract(player.bpTime);
                        player.BP = player.oldBP + (int)Math.Round(span.TotalSeconds) / TIMEOUT_BP;
                        if (player.BP > player.maxBP) player.BP = player.maxBP;
                        break;
                    case PlayerPointType.SP:
                        player.oldSP = int.Parse(newVal.ToString());
                        player.spTime = Convert.ToDateTime(newTime.ToString());
                        span = DataUtil.Game.serverTime.Subtract(player.spTime);
                        player.SP = player.oldSP + (int)Math.Round(span.TotalSeconds) / TIMEOUT_SP;
                        if (player.SP > player.maxSP) player.SP = player.maxSP;
                        break;
                }
            }
        }

        /// <summary>
        /// 计算玩家当前点数值
        /// </summary>
        public void CalcPlayerGamePoint()
        {
            lock (locker)
            {
                TimeSpan span = serverTime.Subtract(player.apTime);
                player.AP = player.oldAP + (int)Math.Round(span.TotalSeconds, 0) / 180;
                if (player.AP > player.maxAP) player.AP = player.maxAP;
                span = serverTime.Subtract(player.bpTime);
                player.BP = player.oldBP + (int)Math.Round(span.TotalSeconds, 0) / 1800;
                if (player.BP > player.maxBP) player.BP = player.maxBP;
                span = serverTime.Subtract(player.spTime);
                player.SP = player.oldSP + (int)Math.Round(span.TotalSeconds, 0) / 7200;
                if (player.SP > player.maxSP) player.SP = player.maxSP;
            }
        }

        /// <summary>
        /// 游戏经验增加处理
        /// </summary>
        /// <param name="exp">增加的经验值</param>
        public void IncreasePlayerExp(JToken exp)
        {
            if (exp == null) return;
            player.exp += int.Parse(exp.ToString());
            if (player.exp >= player.maxExp)
            {
                lock (locker)
                {
                    player.lv++;
                    player.exp -= player.maxExp;
                    if (player.lv <= 15)
                    {
                        player.maxExp += expLow[player.lv - 1];
                    }
                    else if (player.lv <= 100)
                    {
                        player.maxExp += 100;
                    }
                    else if (player.lv >= 200)
                    {
                        player.maxExp = expExt[player.lv - 200] * 1000;
                    }
                    else
                    {
                        player.maxExp += player.lv > 159 ? expHigh[player.lv / 10 - 10] : expHigh[(player.lv - 1) / 10 - 10];
                    }
                    if (player.lv <= 99)
                    {
                        player.maxAP += 3;
                    }
                    else if (player.lv <= 155)
                    {
                        player.maxAP++;
                    }
                    else if (player.lv % 2 == 1)
                    {
                        player.maxAP++;
                    }
                    notifyRecord.lastAP = player.maxAP;
                    notifyRecord.lastBP = player.maxBP;
                    notifyRecord.lastSP = player.maxSP;
                    player.oldAP = player.maxAP;
                    player.oldBP = player.maxBP;
                    player.oldSP = player.maxSP;
                    player.apTime = serverTime;
                    player.bpTime = serverTime;
                    player.spTime = serverTime;
                    player.AP = player.maxAP;
                    player.BP = player.maxBP;
                    player.SP = player.maxSP;
                }
                MiscHelper.AddLog("升级了！你的等级提升到" + player.lv.ToString() + "级", MiscHelper.LogType.Levelup);
            }
        }

        /// <summary>
        /// 初始化日常副本信息列表集合
        /// </summary>
        public void InitDaliyInfo()
        {
            _daliyInfo = new ObservableCollection<DaliyInfo>();
            _daliyInfo.Add(new DaliyInfo() { day = "星期日", eventStage = "狗粮本" });
            _daliyInfo.Add(new DaliyInfo() { day = "星期一", eventStage = "斩（红）进化龙本" });
            _daliyInfo.Add(new DaliyInfo() { day = "星期二", eventStage = "打（蓝）进化龙本" });
            _daliyInfo.Add(new DaliyInfo() { day = "星期三", eventStage = "狗粮本" });
            _daliyInfo.Add(new DaliyInfo() { day = "星期四", eventStage = "突（黄）进化龙本" });
            _daliyInfo.Add(new DaliyInfo() { day = "星期五", eventStage = "魔（紫）进化龙本" });
            _daliyInfo.Add(new DaliyInfo() { day = "星期六", eventStage = "金币本" });
        }

        /// <summary>
        /// 初始化主线副本经验表集合
        /// </summary>
        public void InitExpTable()
        {
            _expTable = new ObservableCollection<ExpTable>();
            if (File.Exists("stages.xml"))
            {
                try
                {
                    System.Xml.XmlDocument file = new System.Xml.XmlDocument();
                    file.Load("stages.xml");
                    System.Xml.XmlNode ixn = file.SelectSingleNode("stageList");
                    System.Xml.XmlNodeList ixnl = ixn.ChildNodes;
                    foreach (System.Xml.XmlNode inxf in ixnl)
                    {
                        System.Xml.XmlElement ixe = inxf as System.Xml.XmlElement;
                        if (ixe.Name == "stage" && ixe.GetAttribute("name") != null && ixe.GetAttribute("name") != "")
                        {
                            _expTable.Add(new ExpTable() {
                                stage = ixe.GetAttribute("name"),
                                ap = int.Parse(ixe.GetAttribute("ap")),
                                exp = int.Parse(ixe.GetAttribute("exp")),
                                effect = (float.Parse(ixe.GetAttribute("exp")) / float.Parse(ixe.GetAttribute("ap"))).ToString("#0.00")
                            });
                        }
                    }
                }
                catch
                {
                    MiscHelper.AddLog("关卡经验信息加载出错，请检查stages.xml文件是否损坏！", MiscHelper.LogType.System);
                }
            }
        }

        /// <summary>
        /// 游戏服务器
        /// </summary>
        public int gameServer
        {
            get
            {
                return this._gameServer;
            }
            set
            {
                this._gameServer = value;
                if (this._gameServers == null) InitGameServers();
                if (this._gameUrls == null) InitGameUrls();
                if (this._gameNewsUrls == null) InitGameNewsUrls();
                if (DataUtil.Config.sysConfig.gameHomePage == 0)
                {
                    this._gameUrl = this._gameServers.ContainsKey(this._gameServer) ? this._gameServers[this._gameServer] : "";
                }
                else
                {
                    this._gameUrl = this._gameUrls.ContainsKey(this._gameServer) ? this._gameUrls[this._gameServer] : "";
                }
                this._gameNewsUrl = this._gameNewsUrls.ContainsKey(this._gameServer) ? this._gameNewsUrls[this._gameServer] : "";
            }
        }

        /// <summary>
        /// 游戏URL
        /// </summary>
        public string gameUrl
        {
            get
            {
                return this._gameUrl;
            }
        }

        /// <summary>
        /// 游戏新闻URL
        /// </summary>
        public string gameNewsUrl
        {
            get
            {
                return this._gameNewsUrl;
            }
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool isOnline
        {
            get
            {
                return this._isOnline;
            }
            set
            {
                this._isOnline = value;
            }
        }

        /// <summary>
        /// 是否在自动推兔
        /// </summary>
        public bool isAuto
        {
            get
            {
                return this._isAuto;
            }
            set
            {
                this._isAuto = value;
            }
        }

        /// <summary>
        /// 是否可以自动推兔
        /// </summary>
        public bool canAuto
        {
            get
            {
                return this._canAuto;
            }
            set
            {
                this._canAuto = value;
            }
        }

        /// <summary>
        /// 服务器时间
        /// </summary>
        public DateTime serverTime
        {
            get
            {
                return this._serverTime;
            }
            set
            {
                this._serverTime = value;
            }
        }

        /// <summary>
        /// 日常副本信息集合
        /// </summary>
        public ObservableCollection<DaliyInfo> daliyInfo
        {
            get
            {
                return this._daliyInfo;
            }
        }

        /// <summary>
        /// 主线副本经验表集合
        /// </summary>
        public ObservableCollection<ExpTable> expTable
        {
            get
            {
                return this._expTable;
            }
        }
    }
}
