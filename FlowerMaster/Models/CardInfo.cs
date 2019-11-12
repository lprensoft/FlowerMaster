using FlowerMaster.Helpers;
using System.Collections.Generic;
using System.IO;

namespace FlowerMaster.Models
{
    /// <summary>
    /// 角色信息类
    /// </summary>
    class CardInfo
    {
        /// <summary>
        /// 角色信息结构体
        /// </summary>
        public struct Cards
        {
            public int id;
            public string name;
            public short rare;
            public string type;
            public float rank;
        }
        /// <summary>
        /// 角色信息列表数组
        /// </summary>
        private List<Cards> _cardsList = null;

        /// <summary>
        /// 初始化 FlowerMaster.Models.CardInfo 类的新实例。
        /// </summary>
        public CardInfo()
        {
            _LoadCards();
        }
        
        /// <summary>
        /// 加载角色信息文件到数组
        /// </summary>
        private void _LoadCards()
        {
            if (File.Exists("cards.xml"))
            {
                try
                {
                    _cardsList = new List<Cards>();
                    System.Xml.XmlDocument file = new System.Xml.XmlDocument();
                    file.Load("cards.xml");
                    System.Xml.XmlNode ixn = file.SelectSingleNode("cardList");
                    System.Xml.XmlNodeList ixnl = ixn.ChildNodes;
                    foreach (System.Xml.XmlNode inxf in ixnl)
                    {
                        System.Xml.XmlElement ixe = inxf as System.Xml.XmlElement;
                        if (ixe.Name == "card")
                        {
                            Cards info = new Cards();
                            info.id = int.Parse(ixe.GetAttribute("id"));
                            info.name = ixe.GetAttribute("name");
                            info.rare = short.Parse(ixe.GetAttribute("rare"));
                            info.type = ixe.GetAttribute("type");
                            info.rank = (info.rare >= 5 && ixe.GetAttribute("rank") != "") ? float.Parse(ixe.GetAttribute("rank")) : -1F;
                            _cardsList.Add(info);
                        }
                    }
                }
                catch
                {
                    MiscHelper.AddLog("角色訊息載入錯誤，請檢查cards.xml文件是否損毀！", MiscHelper.LogType.System);
                }
            }
        }

        /// <summary>
        /// 加载角色信息文件到数组的公共实现
        /// </summary>
        public void LoadCards()
        {
            _LoadCards();
        }

        /// <summary>
        /// 当前角色信息数量
        /// </summary>
        public int Count
        {
            get
            {
                return _cardsList.Count;
            }
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="card">输出角色信息结构变量</param>
        /// <returns>成功获取返回true，否则返回false</returns>
        public bool GetCards(int id, out Cards card)
        {
            card = new Cards();
            if (_cardsList == null || _cardsList.Count <= 0) return false;
            foreach (Cards c in _cardsList)
            {
                if (c.id == id)
                {
                    card = c;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取角色名字
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="onlyName">是否只获取名字，默认false</param>
        /// <param name="showRank">是否显示评级，默认true</param>
        /// <returns>返回格式化后的角色名字字符串</returns>
        public string GetName(int id, bool onlyName = false, bool showRank = true)
        {
            if (_cardsList == null || _cardsList.Count <= 0) return "[角色" + id.ToString() + "]";
            foreach (Cards c in _cardsList)
            {
                if (c.id == id)
                {
                    if (onlyName)
                    {
                        return c.name;
                    }
                    else
                    {
                        if (showRank && c.rare >= 5 && c.rank >= 0)
                        {
                            return c.id > 400000 ? "★" + c.rare.ToString() + c.name + "[" + c.rank.ToString() + "級]" : "★" + c.rare.ToString() + c.name + "[" + c.rank.ToString() + "檔]";
                        }
                        else
                        {
                            return "★" + c.rare.ToString() + c.name;
                        }
                    }
                }
            }
            return "[角色" + id.ToString() + "]";
        }
    }
}
