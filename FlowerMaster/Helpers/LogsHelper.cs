using FlowerMaster.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace FlowerMaster.Helpers
{
    /// <summary>
    /// 日志记录辅助类
    /// </summary>
    static class LogsHelper
    {
        /// <summary>
        /// 检测日志目录是否存在，不存在就尝试创建
        /// </summary>
        /// <returns>目录是否存在</returns>
        private static bool _checkLogDirectory()
        {
            if (!Directory.Exists("log"))
            {
                Directory.CreateDirectory("log");
            }
            return Directory.Exists("log");
        }

        /// <summary>
        /// 获取可以写入文件的玩家名字
        /// </summary>
        /// <returns>可写入文件的玩家名字字符串</returns>
        public static string GetFilePlayerName()
        {
            string[] notAllowed = {"?", ":", "*", "\\", "/", "\"", "<", ">", "|"};
            string name = DataUtil.Game.player.name;
            foreach (string s in notAllowed)
            {
                name = name.Replace(s, "");
            }
            return name;
        }

        /// <summary>
        /// 获得服务器中文名字归类
        /// </summary>
        /// <returns>服务器中文归类名</returns>
        public static string GetServerName()
        {
            switch (DataUtil.Game.gameServer)
            {
                case (int)GameInfo.ServersList.American:
                    return "美服";
                case (int)GameInfo.ServersList.Japan:
                case (int)GameInfo.ServersList.JapanR18:
                    return "日服";
                case (int)GameInfo.ServersList.Taiwan:
                    return "台服";
                default:
                    return "日服";
            }
        }

        /// <summary>
        /// 获取记录文件名
        /// </summary>
        /// <param name="type">记录类型</param>
        /// <param name="hasTime">是否包含时间</param>
        /// <returns>记录文件名</returns>
        private static string _getFileName(string type, bool hasTime = true)
        {
            string fileName = @"log\";
            fileName += GetServerName();
            fileName += "_" + GetFilePlayerName();
            fileName += "_" + type;
            fileName += hasTime ? "_" + DateTime.Now.ToString("yyyy-MM-dd") : "";
            fileName += ".txt";
            return fileName;
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="log">日志内容</param>
        public static void LogDebug(string log)
        {
            if (!_checkLogDirectory()) return;
            string file = @"log\Debug.txt";
            StreamWriter sw = new StreamWriter(file, true);
            sw.WriteLine(log);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 记录游戏日志
        /// </summary>
        /// <param name="log">日志内容</param>
        public static void LogGame(string log)
        {
            if (!_checkLogDirectory() || !DataUtil.Game.isOnline || !DataUtil.Config.sysConfig.logGame) return;
            string file = _getFileName("游戏日志");
            try
            {
                StreamWriter sw = new StreamWriter(file, true);
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + log);
                sw.Flush();
                sw.Close();
            }
            catch { }
        }

        /// <summary>
        /// 记录扭蛋日志
        /// </summary>
        /// <param name="list">日志内容</param>
        public static void LogGacha(List<string> list)
        {
            if (!_checkLogDirectory() || !DataUtil.Game.isOnline || !DataUtil.Config.sysConfig.logGacha) return;
            string file = _getFileName("扭蛋记录", false);
            string log = DataUtil.Game.serverTime.ToString("yyyy-MM-dd HH:mm:ss") + "|";
            foreach (string s in list)
            {
                log += s + ",";
            }
            try
            {
                StreamWriter sw = new StreamWriter(file, true);
                sw.WriteLine(log.Substring(0, log.Length - 1));
                sw.Flush();
                sw.Close();
            }
            catch { }
        }
    }
}
