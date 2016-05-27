using System.Collections.Generic;
using System.IO;

namespace FlowerMaster.Models
{
    class CardInfo
    {
        public struct Cards
        {
            public int id;
            public string name;
            public short rare;
            public string type;
            public float rank;
        }
        private List<Cards> _cardsList = null;

        public CardInfo()
        {
            _LoadCards();
        }

        private void _LoadCards()
        {
            if (File.Exists("cards.xml"))
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
        }

        public void LoadCards()
        {
            _LoadCards();
        }

        public int Count
        {
            get
            {
                return _cardsList.Count;
            }
        }

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
                            return "★" + c.rare.ToString() + c.name + "[评级" + c.rank.ToString() + "]";
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
