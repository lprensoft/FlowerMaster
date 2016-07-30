using FlowerMaster.Models;
using Nekoxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace FlowerMaster.Helpers
{
    /// <summary>
    /// 游戏封包处理类
    /// </summary>
    static class PacketHelper
    {
        /// <summary>
        /// 封包数据结构体
        /// </summary>
        public struct PacketInfo
        {
            public string requestUrl; //请求URL
            public string funcUrl; //有效请求URL
            public string funcApi; //请求URL方法
            public string rawData; //原始回传数据字符串（JSON格式）
            public JObject data; //JSON转换后的回传数据
        }

        /// <summary>
        /// 主窗口对象
        /// </summary>
        public static MainWindow mainWindow = null;

        const int E_FALT_ERROR = -1; //操作异常
        const int E_FAILED = 0; //操作失败
        const int E_SUCCESS = 1; //操作成功

        /// <summary>
        /// 处理封包入口函数
        /// </summary>
        /// <param name="s">Nekoxy数据包</param>
        /// <returns>处理结果</returns>
        public static int ProcessPacket(Session s)
        {
            if (s.Response.StatusLine.StatusCode != 200) return E_FAILED;
            PacketInfo pack;
            if (PackPacket(s, out pack))
            {
                return Process(pack);
            }
            else
            {
                return E_FALT_ERROR;
            }
        }
        
        /// <summary>
        /// 将Nekoxy数据包打包成封包结构体
        /// </summary>
        /// <param name="s">Nekoxy数据包</param>
        /// <param name="pack">打包后的封包结构体</param>
        /// <returns>打包结果</returns>
        private static bool PackPacket(Session s, out PacketInfo pack)
        {
            pack = new PacketInfo();
            pack.requestUrl = s.Request.PathAndQuery;
            pack.rawData = s.Response.BodyAsString;

            if (s.Request.PathAndQuery.IndexOf("/api/v1/") != -1)
            {
                pack.funcUrl = s.Request.PathAndQuery.Substring(0, s.Request.PathAndQuery.IndexOf("/api/") + 8);
                pack.funcApi = s.Request.PathAndQuery.Substring(s.Request.PathAndQuery.IndexOf("/api/") + 7);
            }
            else if ((DataUtil.Game.gameServer == (int)GameInfo.ServersList.Japan || DataUtil.Game.gameServer == (int)GameInfo.ServersList.JapanR18)
                && s.Request.PathAndQuery.IndexOf("/social/") != -1)
            {
                pack.funcUrl = s.Request.PathAndQuery.Substring(0, s.Request.PathAndQuery.IndexOf("/social/") + 8);
                pack.funcApi = s.Request.PathAndQuery.Substring(s.Request.PathAndQuery.IndexOf("/social/") + 7);
                pack.funcApi = pack.funcApi.Substring(0, pack.funcApi.IndexOf("?"));
            }
            else if ((DataUtil.Game.gameServer == (int)GameInfo.ServersList.American || DataUtil.Game.gameServer == (int)GameInfo.ServersList.Taiwan)
                && s.Request.PathAndQuery.IndexOf("/rpc?") != -1)
            {
                pack.funcUrl = s.Request.PathAndQuery.Substring(0, s.Request.PathAndQuery.IndexOf("?"));
                pack.funcApi = pack.funcUrl;
            }
            try
            {
                if (pack.rawData.Substring(0, 1) == "[")
                {
                    JArray jsonArray = (JArray)JsonConvert.DeserializeObject(pack.rawData);
                    pack.data = (JObject)jsonArray[0];
                }
                else if (pack.rawData.Substring(0, 1) == "{")
                {
                    pack.data = (JObject)JsonConvert.DeserializeObject(pack.rawData);
                }
                else
                {
                    pack.data = null;
                    return false;
                }
            }
            catch
            {
                pack.data = null;
                return false;
            }

#if DEBUG
            MiscHelper.AddLog(pack.requestUrl + "\r\n" + pack.rawData, MiscHelper.LogType.Debug);
#endif
            return true;
        }

        /// <summary>
        /// 解析封包结构体
        /// </summary>
        /// <param name="pack">封包结构体</param>
        /// <returns>解析结果</returns>
        private static int Process(PacketInfo pack)
        {
            try
            {
                //处理日服DMM用户信息-获取用户昵称
                if ((DataUtil.Game.gameServer == (int)GameInfo.ServersList.Japan || DataUtil.Game.gameServer == (int)GameInfo.ServersList.JapanR18)
                    && pack.funcUrl.IndexOf("/social/") != -1)
                {
                    if (pack.funcApi == "/rpc")
                    {
                        return ProcessDMMUserInfo(pack);
                    }
                    else
                    {
                        return E_FAILED;
                    }
                }
                //处理美服/台服用户信息-获取用户昵称
                else if ((DataUtil.Game.gameServer == (int)GameInfo.ServersList.American || DataUtil.Game.gameServer == (int)GameInfo.ServersList.Taiwan) 
                    && pack.funcUrl.IndexOf("/rpc") != -1)
                {
                    return ProcessNutakuUserInfo(pack);
                }
                //判断是否为游戏接口封包
                else if (pack.funcUrl.IndexOf("/api/v1/") != -1)
                {
                    //更新服务器时间
                    if (pack.data["serverTime"] != null)
                    {
                        DataUtil.Game.serverTime = Convert.ToDateTime(pack.data["serverTime"].ToString());
                    }
                    //----- 游戏数据处理开始 -----
                    //游戏登录
                    if (pack.funcApi == "/user/login")
                    {
                        return ProcessPlayerLoginInfo(pack);
                    }
                    //好友列表
                    else if (pack.funcApi == "/friend/getFriendList")
                    {
                        return ProcessFriendList(pack);
                    }
                    //花盆开花时间信息
                    else if (pack.funcApi == "/garden/getUserGardenPlant")
                    {
                        return ProcessUserGardenPlant(pack);
                    }
                    //收获花盆
                    else if (pack.funcApi == "/garden/saveGardenPlantHarvest")
                    {
                        return ProcessGardenPlantHarvest(pack);
                    }
                    //游戏探索
                    else if (pack.funcApi == "/searchQuest/saveSearchQuest")
                    {
                        return ProcessPlayerSearchInfo(pack);
                    }
                    //主页BOSS战开始
                    else if (pack.funcApi == "/raidBoss/saveRaidBossStart")
                    {
                        return ProcessRaidBossStart(pack);
                    }
                    //主页BOSS战完成
                    else if (pack.funcApi == "/raidBoss/saveRaidBossFinish")
                    {
                        return ProcessRaidBossFinish(pack);
                    }
                    //个人BOSS战开始
                    else if (pack.funcApi == "/eventBoss/saveSummonBossStart")
                    {
                        return ProcessSummonBossStart(pack);
                    }
                    //个人BOSS战完成
                    else if (pack.funcApi == "/eventBoss/saveSummonBossFinish")
                    {
                        return ProcessSummonBossFinish(pack);
                    }
                    //进副本信息
                    else if (pack.funcApi == "/dungeon/saveStageStart" || pack.funcApi == "/dungeon/saveEventStageStart" || pack.funcApi == "/dungeon/saveEncounterStageStart" || pack.funcApi == "/dungeon/saveWhaleStageStart")
                    {
                        return ProcessDungeonStageStart(pack);
                    }
                    //副本完成信息
                    else if (pack.funcApi == "/dungeon/saveStageSuccess" || pack.funcApi == "/dungeon/saveEventStageSuccess" || pack.funcApi == "/dungeon/saveEncounterStageSuccess" || pack.funcApi == "/dungeon/saveWhaleStageSuccess")
                    {
                        return ProcessDungeonStageSuccess(pack);
                    }
                    //副本失败信息
                    else if (pack.funcApi == "/dungeon/saveStageFailed" || pack.funcApi == "/dungeon/saveEventStageFailed" || pack.funcApi == "/dungeon/saveEncounterStageFailed" || pack.funcApi == "/dungeon/saveWhaleStageFailed")
                    {
                        return ProcessDungeonStageFailed(pack);
                    }
                    //获取主页BOSS列表（副本失败检查）
                    else if (pack.funcApi == "/raidBoss/getRaidBossList")
                    {
                        return ProcessRaidBossList(pack);
                    }
                    //接受礼物盒子里的单件物品
                    else if (pack.funcApi == "/present/savePresentReceived")
                    {
                        return ProcessPresentReceived(pack);
                    }
                    //接收礼物盒子里的全部物品
                    else if (pack.funcApi == "/present/savePresentAllReceived")
                    {
                        return ProcessPresentReceivedAll(pack);
                    }
                    //角色合成
                    else if (pack.funcApi == "/character/saveSynthesis")
                    {
                        return E_SUCCESS;
                    }
                    //角色进化
                    else if (pack.funcApi == "/character/saveEvolve")
                    {
                        return E_SUCCESS;
                    }
                    //装备强化
                    else if (pack.funcApi == "/character/saveEquipmentSynthesis")
                    {
                        return E_SUCCESS;
                    }
                    //出售角色和装备
                    else if (pack.funcApi == "/character/saveSale" || pack.funcApi == "/character/saveSaleEquipment")
                    {
                        return ProcessSellSave(pack);
                    }
                    //扭蛋
                    else if (pack.funcApi == "/gacha/saveGacha")
                    {
                        return ProcessGachaResult(pack);
                    }
                    //其他不须解析的封包，只返回E_FAILED结果
                    else
                    {
                        return E_FAILED;
                    }
                }
                //非游戏接口的封包，只返回E_FAILED结果
                else
                {
                    return E_FAILED;
                }
            }
            //解析失败或者解析过程发生错误，返回E_FALT_ERROR
            catch
            {
                return E_FALT_ERROR;
            }
        }

        /// <summary>
        /// 更新主界面回满时间数据
        /// </summary>
        private static void UpdateTimeLeft()
        {
            DateTime apTime = DataUtil.Game.player.apTime.AddSeconds(GameInfo.TIMEOUT_AP * (DataUtil.Game.player.maxAP - DataUtil.Game.player.oldAP));
            DateTime bpTime = DataUtil.Game.player.bpTime.AddSeconds(GameInfo.TIMEOUT_BP * (DataUtil.Game.player.maxBP - DataUtil.Game.player.oldBP));
            DateTime spTime = DataUtil.Game.player.spTime.AddSeconds(GameInfo.TIMEOUT_SP * (DataUtil.Game.player.maxSP - DataUtil.Game.player.oldSP));

            mainWindow.Dispatcher.Invoke(new Action(() =>
            {
                mainWindow.lbAPTime.Content = "体力回满时间：" + apTime.ToString("MM-dd HH:mm:ss");
                mainWindow.lbBPTime.Content = "战点回满时间：" + bpTime.ToString("MM-dd HH:mm:ss");
                mainWindow.lbSPTime.Content = "探索回满时间：" + spTime.ToString("MM-dd HH:mm:ss");
                mainWindow.lbPlantTime.Content = DataUtil.Game.player.plantTime.Year == 1 ? "花盆全满时间：暂无" : "花盆全满时间：" + DataUtil.Game.player.plantTime.ToString("MM-dd HH:mm:ss");
            }));
        }

        /// <summary>
        /// 处理日服DMM登录封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessDMMUserInfo(PacketInfo pack)
        {
            JObject json = pack.data;
            if (json["data"]["nickname"] != null)
            {
                DataUtil.Game.player.name = json["data"]["nickname"].ToString();
                mainWindow.Dispatcher.Invoke(new Action(() =>
                {
                    mainWindow.notifyIcon.Text = "团长助理 - " + DataUtil.Game.player.name;
                    if (mainWindow.Title.IndexOf("-") == -1 && DataUtil.Config.sysConfig.changeTitle)
                    {
                        mainWindow.Title += " - " + DataUtil.Game.player.name;
                    }
                }));
                return E_SUCCESS;
            }
            else
            {
                return E_FAILED;
            }
        }

        /// <summary>
        /// 处理美服/台服登录封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessNutakuUserInfo(PacketInfo pack)
        {
            JObject json = pack.data;
            if (json["result"]["nickname"] != null)
            {
                DataUtil.Game.player.name = json["result"]["nickname"].ToString();
                mainWindow.Dispatcher.Invoke(new Action(() =>
                {
                    mainWindow.notifyIcon.Text = "团长助理 - " + DataUtil.Game.player.name;
                    if (mainWindow.Title.IndexOf("-") == -1 && DataUtil.Config.sysConfig.changeTitle)
                    {
                        mainWindow.Title += " - " + DataUtil.Game.player.name;
                    }
                }));
                return E_SUCCESS;
            }
            else
            {
                return E_FAILED;
            }
        }

        /// <summary>
        /// 处理游戏登录信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessPlayerLoginInfo(PacketInfo pack)
        {
            JObject json = pack.data;
            if (json["user"] == null) return E_FAILED;
            DataUtil.Game.player.lv = json["user"]["levelId"].ToString() != null ? int.Parse(json["user"]["levelId"].ToString()) : 0;
            DataUtil.Game.player.friendId = json["user"]["searchUserId"] != null ? json["user"]["searchUserId"].ToString() : "-";
            DataUtil.Game.CalcPlayerMaxAPExp();
            DataUtil.Game.player.maxBP = GameInfo.PLAYER_MAX_BP;
            if (DataUtil.Game.gameServer == (int)GameInfo.ServersList.American)
            {
                DataUtil.Game.player.maxBP = GameInfo.PLAYER_MAX_BP_A;
            }
            else if (DataUtil.Game.gameServer == (int)GameInfo.ServersList.Taiwan)
            {
                DataUtil.Game.player.maxBP = GameInfo.PLAYER_MAX_BP_T;
            }
            DataUtil.Game.player.maxSP = GameInfo.PLAYER_MAX_SP;
            DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.AP, json["user"]["stamina"], json["user"]["staminaTime"]);
            DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.BP, json["user"]["battlePoint"], json["user"]["battlePointTime"]);
            DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.SP, json["userSearchQuest"]["searchQuestPoint"], json["userSearchQuest"]["searchQuestPointTime"]);
            DataUtil.Game.player.money = json["user"]["gameMoney"] != null ? int.Parse(json["user"]["gameMoney"].ToString()) : 0;
            DataUtil.Game.player.stone = json["user"]["chargeMoney"] != null ? int.Parse(json["user"]["chargeMoney"].ToString()) : 0;
            DataUtil.Game.player.exp = json["user"]["levelExperience"] != null ? int.Parse(json["user"]["levelExperience"].ToString()) : 0;
            DataUtil.Game.isOnline = true;
            DataUtil.Game.notifyRecord.lastAP = DataUtil.Game.player.AP;
            DataUtil.Game.notifyRecord.lastBP = DataUtil.Game.player.BP;
            DataUtil.Game.notifyRecord.lastSP = DataUtil.Game.player.SP;
            UpdateTimeLeft();
            MiscHelper.AddLog("已经成功登录游戏", MiscHelper.LogType.System);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理游戏好友列表信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessFriendList(PacketInfo pack)
        {
            JObject json = pack.data;
            if (json["userFriendInformationList"] == null) return E_FAILED;
            JArray friends = (JArray)json["userFriendInformationList"];
            if (friends.Count <= 0) return E_FAILED;
            DataUtil.Game.friendList = new ObservableCollection<GameInfo.FriendInfo>();
            foreach (JObject friend in friends)
            {
                GameInfo.FriendInfo f = new GameInfo.FriendInfo();
                f.name = friend["userName"].ToString();
                f.lv = int.Parse(friend["userLevelNum"].ToString());
                f.card1 = string.Format("Lv.{0} {1}(技Lv{2})", friend["character1LevelNum"].ToString(), 
                    DataUtil.Cards.GetName(int.Parse(friend["character1Id"].ToString()), true, false), friend["character1SkillLevelNum"].ToString());
                f.card2 = friend["character2Id"] != null ? string.Format("Lv.{0} {1}(技Lv{2})", friend["character2LevelNum"].ToString(),
                    DataUtil.Cards.GetName(int.Parse(friend["character2Id"].ToString()), true, false), friend["character2SkillLevelNum"].ToString()) : "";
                f.card3 = friend["character3Id"] != null ? string.Format("Lv.{0} {1}(技Lv{2})", friend["character3LevelNum"].ToString(),
                    DataUtil.Cards.GetName(int.Parse(friend["character3Id"].ToString()), true, false), friend["character3SkillLevelNum"].ToString()) : "";
                f.card4 = friend["character4Id"] != null ? string.Format("Lv.{0} {1}(技Lv{2})", friend["character4LevelNum"].ToString(),
                    DataUtil.Cards.GetName(int.Parse(friend["character4Id"].ToString()), true, false), friend["character4SkillLevelNum"].ToString()) : "";
                f.card5 = friend["character5Id"] != null ? string.Format("Lv.{0} {1}(技Lv{2})", friend["character5LevelNum"].ToString(),
                    DataUtil.Cards.GetName(int.Parse(friend["character5Id"].ToString()), true, false), friend["character5SkillLevelNum"].ToString()) : "";
                f.lastTime = friend["lastPlayed"].ToString();
                f.regTime = friend["created"].ToString();
                f.totalPower = int.Parse(friend["totalPower"].ToString());
                f.leader = DataUtil.Cards.GetName(int.Parse(friend["deputyLeaderCharacterId"].ToString()), true, false);
                DataUtil.Game.friendList.Add(f);
            }
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理游戏花盆开花时间信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessUserGardenPlant(PacketInfo pack)
        {
            JObject json = pack.data;
            if (json["userGardenPlantPotList"] == null) return E_FAILED;
            JArray plants = (JArray)json["userGardenPlantPotList"];
            if (plants.Count <= 0) return E_FAILED;
            foreach (JObject plant in plants)
            {
                if (plant["floweringTime"] != null)
                {
                    DateTime pTime = Convert.ToDateTime(plant["floweringTime"].ToString());
                    if (DataUtil.Game.player.plantTime < pTime) DataUtil.Game.player.plantTime = pTime;
                }
            }
            UpdateTimeLeft();
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理游戏收获花盆封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessGardenPlantHarvest(PacketInfo pack)
        {
            JObject json = pack.data;
            string log = "收获花盆，获得：";
            JArray items = (JArray)json["gardenHarvestItemList"];
            foreach (JObject item in items)
            {
                if (item["itemId"].ToString() == "1")
                {
                    log += "金币" + item["amount"].ToString();
                }
            }
            JArray plants = (JArray)json["userGardenPlantPotList"];
            foreach (JObject plant in plants)
            {
                if (plant["floweringTime"] != null)
                {
                    DateTime pTime = Convert.ToDateTime(plant["floweringTime"].ToString());
                    if (DataUtil.Game.player.plantTime < pTime) DataUtil.Game.player.plantTime = pTime;
                }
            }
            if (json["staminaRevoceryNum"].ToString() != "0")
            {
                log += "，体力" + json["staminaRevoceryNum"].ToString();
                DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.AP, json["stamina"], json["staminaTime"]);
            }
            UpdateTimeLeft();
            MiscHelper.AddLog(log, MiscHelper.LogType.Search);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理探索信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessPlayerSearchInfo(PacketInfo pack)
        {
            JObject json = pack.data;
            DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.AP, json["stamina"], json["staminaTime"]);
            DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.BP, json["battlePoint"], json["battlePointTime"]);
            DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.SP, json["searchQuestPoint"], json["searchQuestPointTime"]);
            JArray items = (JArray)json["masterSearchQuestList"];
            int gold = 0, ap = 0, gp = 0;
            foreach (JObject item in items)
            {
                switch (item["searchQuestGivingItemId"].ToString())
                {
                    case "1": //金币
                        DataUtil.Game.player.money += int.Parse(item["value"].ToString());
                        gold += int.Parse(item["value"].ToString());
                        break;
                    case "1002": //体力
                        ap += int.Parse(item["value"].ToString());
                        break;
                    case "3": //种子
                        gp += int.Parse(item["value"].ToString());
                        break;
                }
            }
            UpdateTimeLeft();
            string log = string.Format("探索完成，获得体力{0:D}，金币{1:D}，种子{2:D}", ap, gold, gp);
            MiscHelper.AddLog(log, MiscHelper.LogType.Search);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理主页BOSS战开始封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessRaidBossStart(PacketInfo pack)
        {
            JObject json = pack.data;
            if (json["masterRaidBoss"].ToString() != "null")
            {
                string name = json["masterRaidBoss"]["name"].ToString();
                string hp = json["masterRaidBoss"]["hitPoint"].ToString();
                string mhp = json["masterRaidBoss"]["maxHitPoint"].ToString();
                string lv = json["masterRaidBoss"]["raidBossLevelNum"].ToString();
                string atk = json["masterRaidBoss"]["attack"].ToString();
                string def = json["masterRaidBoss"]["defense"].ToString();
                MiscHelper.AddLog("开始首页BOSS战，BOSS：" + name + "，Lv：" + lv + "，HP：" + hp + "/" + mhp + "，攻击：" + atk + "，防御：" + def, MiscHelper.LogType.Boss);
            }
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理主页BOSS战结果封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessRaidBossFinish(PacketInfo pack)
        {
            JObject json = pack.data;
            DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.BP, json["battlePoint"], json["battlePointTime"]);
            UpdateTimeLeft();
            MiscHelper.AddLog("首页BOSS战完成，剩余战点：" + DataUtil.Game.player.BP.ToString() , MiscHelper.LogType.Boss);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理召唤BOSS战开始信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessSummonBossStart(PacketInfo pack)
        {
            if (pack.data["masterSummonBoss"] != null)
            {
                JToken json = pack.data["masterSummonBoss"];
                string name = json["name"] != null ? json["name"].ToString() : "[未知]";
                string lv = json["level"] != null ? json["level"].ToString() : "0";
                string hp = json["hitPoint"] != null ? json["hitPoint"].ToString() : "0";
                string mhp = json["maxHitPoint"] != null ? json["maxHitPoint"].ToString() : "0";
                string atk = json["attack"] != null ? json["attack"].ToString() : "0";
                string def = json["defense"] != null ? json["defense"].ToString() : "0";
                MiscHelper.AddLog("开始召唤BOSS战，BOSS：" + name + "，Lv：" + lv + "，HP：" + hp + "/" + mhp + "，攻击：" + atk + "，防御：" + def, MiscHelper.LogType.Boss);
            }
            else
            {
                MiscHelper.AddLog("开始召唤BOSS战", MiscHelper.LogType.Boss);
            }
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理召唤BOSS战完成信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessSummonBossFinish(PacketInfo pack)
        {
            MiscHelper.AddLog("召唤BOSS战完成", MiscHelper.LogType.Boss);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理进副本封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessDungeonStageStart(PacketInfo pack)
        {
            JObject json = pack.data;
            DataUtil.Game.CalcPlayerGamePoint(GameInfo.PlayerPointType.AP, json["stamina"], json["staminaTime"]);
            string dungeonType = "普通";
            switch (pack.funcApi)
            {
                case "/dungeon/saveEventStageStart":
                    dungeonType = "活动";
                    break;
                case "/dungeon/saveEncounterStageStart":
                    dungeonType = "隐藏";
                    break;
                case "/dungeon/saveWhaleStageStart":
                    dungeonType = "鲸鱼";
                    break;
            }
            if (json["bossList"] != null && json["bossList"].ToString() != "")
            {
                DataUtil.Game.bossList = new ObservableCollection<GameInfo.BossInfo>();
                try
                {
                    JArray bossList = (JArray)json["bossList"];
                    foreach (JObject boss in bossList)
                    {
                        GameInfo.BossInfo bossInfo = new GameInfo.BossInfo();
                        bossInfo.group = int.Parse(boss["bossGroupId"].ToString());
                        bossInfo.name = boss["name"].ToString();
                        bossInfo.hp = int.Parse(boss["hitPoint"].ToString());
                        bossInfo.atk = int.Parse(boss["attack"].ToString());
                        bossInfo.def = int.Parse(boss["defense"].ToString());
                        bossInfo.skill = boss["bossSkillName"].ToString();
                        bossInfo.money = int.Parse(boss["dropGameMoney"].ToString());
                        bossInfo.gp = int.Parse(boss["dropGachaPoint"].ToString());
                        DataUtil.Game.bossList.Add(bossInfo);
                    }
                }
                catch { }
            }
            UpdateTimeLeft();
            MiscHelper.AddLog("成功进入" + dungeonType + "副本，剩余体力：" + DataUtil.Game.player.AP.ToString(), MiscHelper.LogType.Stage);
            DataUtil.Game.canAuto = true;
            MiscHelper.ShowMapInfoButton();
            if (DataUtil.Config.sysConfig.autoGoInMaps) mainWindow.btnAuto_Click(mainWindow, new System.Windows.RoutedEventArgs());
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理副本成功完成封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessDungeonStageSuccess(PacketInfo pack)
        {
            JObject json = pack.data;
            DataUtil.Game.player.money += json["givingGameMoney"] != null ? int.Parse(json["givingGameMoney"].ToString()) : 0;
            DataUtil.Game.player.stone += json["givingChargeMoney"] != null ? int.Parse(json["givingChargeMoney"].ToString()) : 0;
            DataUtil.Game.IncreasePlayerExp(json["givingExperience"]);
            string dungeonType = "普通";
            switch (pack.funcApi)
            {
                case "/dungeon/saveEventStageSuccess":
                    dungeonType = "活动";
                    break;
                case "/dungeon/saveEncounterStageSuccess":
                    dungeonType = "隐藏";
                    break;
                case "/dungeon/saveWhaleStageSuccess":
                    dungeonType = "鲸鱼";
                    break;
            }
            string log = "成功通关" + dungeonType + "副本，获得";
            log += json["givingGameMoney"] != null && json["givingGameMoney"].ToString() != "0" ? "金币" + json["givingGameMoney"].ToString() + "，" : "";
            log += json["givingGachaPoint"] != null && json["givingGachaPoint"].ToString() != "0" ? "种子" + json["givingGachaPoint"].ToString() + "，" : "";
            log += json["givingExperience"] != null && json["givingExperience"].ToString() != "0" ? "经验值" + json["givingExperience"].ToString() + "，" : "";
            log += json["givingChargeMoney"] != null && json["givingChargeMoney"].ToString() != "0" ? "华灵石" + json["givingChargeMoney"].ToString() + "，" : "";
            log += json["givingEventItemPoint"] != null && json["givingEventItemPoint"].ToString() != "0" ? "活动点数" + json["givingEventItemPoint"].ToString() + "，" : "";
            log += (json["givingUserCharacterList"] as JArray).Count > 0 ? "角色" + (json["givingUserCharacterList"] as JArray).Count.ToString() + "，" : "";
            log += (json["givingUserCharacterEquipmentList"] as JArray).Count > 0 ? "装备" + (json["givingUserCharacterEquipmentList"] as JArray).Count.ToString() + "，" : "";
            JArray items = (JArray)json["givingUserPointItemList"];
            foreach (JObject item in items)
            {
                if (item["itemId"].ToString() == "10")
                {
                    log += "中级装备种子" + item["point"].ToString() + "，";
                }
                else if (item["itemId"].ToString() == "101")
                {
                    log += "生命结晶" + item["point"].ToString() + "，";
                }
            }
            items = (JArray)json["givingUserEventItemList"];
            int itemAmount = 0;
            foreach (JObject item in items)
            {
                itemAmount += int.Parse(item["amount"].ToString());
            }
            if (itemAmount > 0) log += "活动物品" + itemAmount.ToString() + "，";
            if (json["encounterStageName"] != null && json["encounterStageName"].ToString() != "")
            {
                log += "出现隐藏副本：" + json["encounterStageName"].ToString() + "（需要体力：" + json["encounterStageUseStamina"].ToString() + "），";
                if (DataUtil.Config.sysConfig.foundStageNotify)
                {
                    MiscHelper.ShowRemind(10, DataUtil.Game.player.name + " - 隐藏副本出现通知", "出现隐藏副本：" + json["encounterStageName"].ToString()
                            + "，需要体力：" + json["encounterStageUseStamina"].ToString(), System.Windows.Forms.ToolTipIcon.Info);
                }
            }
            if (json["masterRaidBoss"] != null && json["masterRaidBoss"].ToString() != "")
            {
                log += "出现主页BOSS：" + json["masterRaidBoss"]["name"].ToString() + "（Lv：" + json["masterRaidBoss"]["raidBossLevelNum"].ToString() + "），";
                if (DataUtil.Config.sysConfig.foundBossNotify)
                {
                    MiscHelper.ShowRemind(10, DataUtil.Game.player.name + " - 主页BOSS出现通知", "出现主页BOSS：" + json["masterRaidBoss"]["name"].ToString()
                            + "，Lv：" + json["masterRaidBoss"]["raidBossLevelNum"].ToString(), System.Windows.Forms.ToolTipIcon.Info);
                }
            }
            MiscHelper.AddLog(log.Substring(0, log.Length - 1), MiscHelper.LogType.Stage);
            DataUtil.Game.canAuto = false;
            MiscHelper.ShowMapInfoButton(false);
            MiscHelper.SetAutoGo(false);
            UpdateTimeLeft();
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理副本退出封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessDungeonStageFailed(PacketInfo pack)
        {
            JObject json = pack.data;
            DataUtil.Game.player.money += json["givingGameMoney"] != null ? int.Parse(json["givingGameMoney"].ToString()) : 0;
            DataUtil.Game.player.stone += json["givingChargeMoney"] != null ? int.Parse(json["givingChargeMoney"].ToString()) : 0;
            DataUtil.Game.IncreasePlayerExp(json["givingExperience"]);
            string dungeonType = "普通";
            switch (pack.funcApi)
            {
                case "/dungeon/saveEventStageFailed":
                    dungeonType = "活动";
                    break;
                case "/dungeon/saveEncounterStageFailed":
                    dungeonType = "隐藏";
                    break;
                case "/dungeon/saveWhaleStageFailed":
                    dungeonType = "鲸鱼";
                    break;
            }
            string log = "你已退出" + dungeonType + "副本，获得";
            log += json["givingGameMoney"] != null && json["givingGameMoney"].ToString() != "0" ? "金币" + json["givingGameMoney"].ToString() + "，" : "";
            log += json["givingGachaPoint"] != null && json["givingGachaPoint"].ToString() != "0" ? "种子" + json["givingGachaPoint"].ToString() + "，" : "";
            log += json["givingExperience"] != null && json["givingExperience"].ToString() != "0" ? "经验值" + json["givingExperience"].ToString() + "，" : "";
            log += json["givingChargeMoney"] != null && json["givingChargeMoney"].ToString() != "0" ? "华灵石" + json["givingChargeMoney"].ToString() + "，" : "";
            log += json["givingEventItemPoint"] != null && json["givingEventItemPoint"].ToString() != "0" ? "活动点数" + json["givingEventItemPoint"].ToString() + "，" : "";
            log += (json["givingUserCharacterList"] as JArray).Count > 0 ? "角色" + (json["givingUserCharacterList"] as JArray).Count.ToString() + "，" : "";
            log += (json["givingUserCharacterEquipmentList"] as JArray).Count > 0 ? "装备" + (json["givingUserCharacterEquipmentList"] as JArray).Count.ToString() + "，" : "";
            JArray items = (JArray)json["givingUserPointItemList"];
            foreach (JObject item in items)
            {
                if (item["itemId"].ToString() == "10")
                {
                    log += "中级装备种子" + item["point"].ToString() + "，";
                }
                else if (item["itemId"].ToString() == "101")
                {
                    log += "生命结晶" + item["point"].ToString() + "，";
                }
            }
            items = (JArray)json["givingUserEventItemList"];
            foreach (JObject item in items)
            {
                log += "活动物品" + item["amount"].ToString() + "，";
            }
            MiscHelper.AddLog(log.Substring(0, log.Length - 1), MiscHelper.LogType.Stage);
            DataUtil.Game.canAuto = false;
            MiscHelper.ShowMapInfoButton(false);
            MiscHelper.SetAutoGo(false);
            UpdateTimeLeft();
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理主页BOSS列表封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessRaidBossList(PacketInfo pack)
        {
            DataUtil.Game.canAuto = false;
            MiscHelper.ShowMapInfoButton(false);
            DataUtil.Game.canAuto = false;
            MiscHelper.SetAutoGo(false);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理礼品箱物品取出单件物品信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessPresentReceived(PacketInfo pack)
        {
            JObject json = pack.data;
            DataUtil.Game.player.money += json["givingGameMoney"] != null ? int.Parse(json["givingGameMoney"].ToString()) : 0;
            DataUtil.Game.player.stone += json["givingChargeMoney"] != null ? int.Parse(json["givingChargeMoney"].ToString()) : 0;
            string log = "取出礼品箱内物品，获得";
            log += json["givingGameMoney"] != null && json["givingGameMoney"].ToString() != "0" ? "金币" + json["givingGameMoney"].ToString() + "，" : "";
            log += json["givingChargeMoney"] != null && json["givingChargeMoney"].ToString() != "0" ? "华灵石" + json["givingChargeMoney"].ToString() + "，" : "";
            log += json["givingGachaPoint"] != null && json["givingGachaPoint"].ToString() != "0" ? "种子" + json["givingGachaPoint"].ToString() + "，" : "";
            log += json["givingRaidBossGachaPoint"] != null && json["givingRaidBossGachaPoint"].ToString() != "0" ? "初级装备种子" + json["givingRaidBossGachaPoint"].ToString() + "，" : "";
            log += json["givingUserCharacter"].ToString() != "" ? "角色1，" : "";
            log += json["givingUserCharacterEquipment"].ToString() != "" ? "装备1，" : "";
            log += json["givingUserGift"].ToString() != "" ? "赠物1，" : "";
            if (json["givingUserPointItem"].ToString() != "")
            {
                if (json["givingUserPointItem"]["itemId"].ToString() == "10")
                {
                    log += "中级装备种子" + json["givingUserPointItem"]["point"].ToString() + "，";
                }
                else if (json["givingUserPointItem"]["itemId"].ToString() == "101")
                {
                    log += "生命结晶" + json["givingUserPointItem"]["point"].ToString() + "，";
                }
            }
            log += json["givingUserEventItem"].ToString() != "" ? "活动物品1，" : "";
            log += json["givingUserGachaTicket"].ToString() != "" ? "扭蛋券1，" : "";
            log += (json["givingUserGardenMakeoverItemList"] as JArray).Count > 0 ? "庭院物品1，" : "";
            MiscHelper.AddLog(log.Substring(0, log.Length - 1), MiscHelper.LogType.Mailbox);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理礼品箱物品取出全部物品信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessPresentReceivedAll(PacketInfo pack)
        {
            JObject json = pack.data;
            DataUtil.Game.player.money += json["givingGameMoney"] != null ? int.Parse(json["givingGameMoney"].ToString()) : 0;
            DataUtil.Game.player.stone += json["givingChargeMoney"] != null ? int.Parse(json["givingChargeMoney"].ToString()) : 0;
            string log = "取出礼品箱内物品，获得";
            log += json["givingGameMoney"] != null && json["givingGameMoney"].ToString() != "0" ? "金币" + json["givingGameMoney"].ToString() + "，" : "";
            log += json["givingChargeMoney"] != null && json["givingChargeMoney"].ToString() != "0" ? "华灵石" + json["givingChargeMoney"].ToString() + "，" : "";
            log += json["givingGachaPoint"] != null && json["givingGachaPoint"].ToString() != "0" ? "种子" + json["givingGachaPoint"].ToString() + "，" : "";
            log += json["givingRaidBossGachaPoint"] != null && json["givingRaidBossGachaPoint"].ToString() != "0" ? "初级装备种子" + json["givingRaidBossGachaPoint"].ToString() + "，" : "";
            log += (json["givingUserCharacterList"] as JArray).Count > 0 ? "角色" + (json["givingUserCharacterList"] as JArray).Count.ToString() + "，" : "";
            log += (json["givingUserCharacterEquipmentList"] as JArray).Count > 0 ? "装备" + (json["givingUserCharacterEquipmentList"] as JArray).Count.ToString() + "，" : "";
            log += (json["givingUserGiftList"] as JArray).Count > 0 ? "赠物" + (json["givingUserGiftList"] as JArray).Count.ToString() + "，" : "";
            JArray items = (JArray)json["givingUserPointItemList"];
            foreach (JObject item in items)
            {
                if (item["itemId"].ToString() == "10")
                {
                    log += "中级装备种子" + item["point"].ToString() + "，";
                }
                else if (item["itemId"].ToString() == "101")
                {
                    log += "生命结晶" + item["point"].ToString() + "，";
                }
            }
            log += (json["givingUserEventItemList"] as JArray).Count > 0 ? "活动物品" + (json["givingUserEventItemList"] as JArray).Count.ToString() + "，" : "";
            log += (json["givingUserGachaTicketList"] as JArray).Count > 0 ? "扭蛋券" + (json["givingUserGachaTicketList"] as JArray).Count.ToString() + "，" : "";
            log += (json["givingUserGardenMakeoverItemList"] as JArray).Count > 0 ? "庭院物品" + (json["givingUserGardenMakeoverItemList"] as JArray).Count.ToString() + "，" : "";
            MiscHelper.AddLog(log.Substring(0, log.Length - 1), MiscHelper.LogType.Mailbox);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理出售角色/装备信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessSellSave(PacketInfo pack)
        {
            JObject json = pack.data;
            string type = "角色";
            if (pack.funcApi == "/character/saveSaleEquipment") type = "装备";
            if (json["totalGameMoney"] != null) DataUtil.Game.player.money = int.Parse(json["totalGameMoney"].ToString());
            if (json["totalGameMoney"] != null) MiscHelper.AddLog("出售了一些" + type + "，当前金币：" + json["totalGameMoney"].ToString(), MiscHelper.LogType.Sell);
            return E_SUCCESS;
        }

        /// <summary>
        /// 处理扭蛋结果信息封包
        /// </summary>
        /// <param name="pack">封包数据结构体</param>
        /// <returns>处理结果标志</returns>
        private static int ProcessGachaResult(PacketInfo pack)
        {
            JArray cards = (JArray)pack.data["userCharacterList"];
            if (cards.Count > 0)
            {
                List<string> list = new List<string>();
                foreach (JObject card in cards)
                {
                    list.Add(card["characterId"].ToString());
                }
                MiscHelper.AddGachaLog(cards);
                LogsHelper.LogGacha(list);
            }
            return E_SUCCESS;
        }
    }
}
