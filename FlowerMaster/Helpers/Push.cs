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
using System.Windows.Interop;
using System.Windows.Media;
using FlowerMaster;
using FlowerMaster.Models;
using FlowerMaster.Helpers;
using FlowerMaster.Properties;
using static FlowerMaster.CordCol;
using System.Threading;

namespace FlowerMaster.Helpers
{

    public class Nodes
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern bool PostMessage(IntPtr WindowHandle, uint Msg, IntPtr wParam, IntPtr lParam);


        //自动推图2.0函数
        /*需要数据：
         *  推图：
         *      1：图类型（主线0，活动1，水影2，上一次推的4）
         *      2：只能推最新图
         *      3：推几次
         *  体力：
         *      1：是否喝药水
         *      2：是否吃石头
         *  Boss：
         *      1：打或者放 （不会有人碎石打Boss吧？希望我没低估土豪的力量）
         *  特命：
         *      1：打或者放
         *  延迟：
         *      1：根据电脑性能设置
         * 
         */

        private IntPtr Webhandle = IntPtr.Zero;
        private int delay = 1000;
        private Color Col = Color.Instance;

        //核心功能
        /// <summary>
        /// 鼠标点击
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        private void Click(int x, int y)
        {
            Random rnd = new Random();
            x = x + rnd.Next(-2, 3);
            y = y + rnd.Next(-2, 3);
            IntPtr lParam1 = (IntPtr)((y + 2 << 16) | x + 2); //坐标信息1
            IntPtr lParam2 = (IntPtr)((y - 2 << 16) | x - 2); //坐标信息1
            IntPtr wParam = IntPtr.Zero; // 附加的按键信息（如：Ctrl）
            const uint downCode = 0x201; // 鼠标左键按下
            const uint upCode = 0x202; // 鼠标左键抬起
            PostMessage(Webhandle, 0x0200, wParam, lParam2); // 随机移动鼠标
            PostMessage(Webhandle, downCode, wParam, lParam1); // 发送鼠标按键按下消息
            PostMessage(Webhandle, 0x0200, wParam, lParam1); // 随机移动鼠标
            PostMessage(Webhandle, upCode, wParam, lParam2); // 发送鼠标按键抬起消息
        }

        public void ScInitialize(IntPtr Hand)
        {
            Webhandle = Hand;
            Col.Load(Webhandle);
        }
        
        
        //Main
        /// <summary>
        /// 开始脚本于初始化数据
        /// </summary>
        public void Start()
        {
            Random rnd = new Random();
            while (MainWindow.AutoPushS > 0)
            {
                if (DataUtil.Config.sysConfig.actionPrep == true && 
                    Col.Check(540, 75, 55, 47, 44) == true) { ScActionPrep(); }

                delay = rnd.Next(DataUtil.Config.sysConfig.delayTime, DataUtil.Config.sysConfig.delayTime * 2);
                Thread.Sleep(delay);

                while (ScSelect() == false) { }
                ScDepart();
                while (ScCombat() == false) { }


                if (DataUtil.Config.sysConfig.sellTrue == true)
                {
                    CoHomeReturn();
                    ScSell(); 
                }

                if (DataUtil.Config.sysConfig.exploreTrue == true)
                {
                    CoHomeReturn();
                    if (Col.Check(250, 140, 216, 184, 111) == true &&
                        Col.Check(258, 163, 99, 99, 99) == false &&
                        Col.Check(520, 75, 50, 41, 37) == true)
                    {
                        ScExplore();
                    } 
                }

                if (DataUtil.Config.sysConfig.gardenTrue == true)
                {
                    CoHomeReturn();
                    if (DataUtil.Game.player.plantTime < DataUtil.Game.serverTime)
                    {
                        ScGarden();
                    } 
                }

                MainWindow.AutoPushS--;
            }

            return;
        }

        //脚本事件 - Sc系列
        /// <summary>
        /// 选择关卡，并恢复体力
        /// </summary>
        /// <param name="Node"></param>
        private bool ScSelect()
        {
            CoHomeDepart();

            //等待出击页面加载结束
            while (Col.Check(600, 400, 28, 29, 19) == false) { Thread.Sleep(delay); }


            //根据选择点击出击页面

            //进入主线推图页面
            if (DataUtil.Config.sysConfig.pushType == 0)
            {
                while (Col.Check(218, 242, 213, 200, 159) == false)
                {
                    Click(300, 140);
                    Thread.Sleep(delay);
;                }
                while (Col.Check(218, 242, 213, 200, 159) == false) { Thread.Sleep(delay); }
                Click(300, 260);
            }

            //进入活动推图页面
            if (DataUtil.Config.sysConfig.pushType == 1)
            {
                while (Col.Check(218, 325, 214, 202, 163) == false)
                {
                    Click(400, 140);
                    Thread.Sleep(delay);
                }
                while (Col.Check(218, 325, 214, 202, 163) == false) { Thread.Sleep(delay); }
                Click(300, 330);
            }

            //进图水影推图页面
            if (DataUtil.Config.sysConfig.pushType == 2)
            {
                while (Col.Check(470, 275, 249, 247, 240) == false)
                {
                    Click(590, 140);
                    Thread.Sleep(delay);
                }
                while (Col.Check(620, 275, 249, 247, 240) == true)
                {
                    Click(600, 250);
                    Thread.Sleep(delay);
                }
            }
            
            //尝试进入队友选择
            if (DataUtil.Config.sysConfig.pushType != 3)
            {
                CoDepartFirst();
                
                //确认体力页面是否或者推图页面是否出现
                while (true)
                {
                    Thread.Sleep(2 * delay);
                    //判断体力恢复是否出现
                    if (Col.Check(320, 320, 176, 31, 69) == true)
                    {
                        //如果碎石失败，返回主页面
                        if(ScRefill() == false) return false;

                        CoDepartFirst();
                        return true;
                    }

                    //判断队友选择是否出现
                    if (Col.Check(934, 200, 55, 46, 5) == true ||
                        Col.Check(730, 200, 213, 185, 132) == true)
                    {
                        return true;
                    }
                }
            }

            //进入上一次推的图
            if (DataUtil.Config.sysConfig.pushType == 3)
            {
                CoDepartPrevious();
                //确认体力页面是否或者推图页面是否出现
                while (true)
                {
                    //判断体力恢复是否出现
                    if (Col.Check(320, 320, 176, 31, 69) == true)
                    {
                        //如果碎石失败，返回主页面
                        if (ScRefill() == false) return false;
                        CoDepartPrevious();
                        return true;
                    }

                    //判断队友选择是否出现
                    if (Col.Check(934, 200, 55, 46, 5) == true ||
                        Col.Check(730, 200, 213, 185, 132) == true)
                    {
                        return true;
                    }
                    Thread.Sleep(2 * delay);
                }
            }
            return true;
        }
        
        /// <summary>
        /// 恢复体力脚本
        /// </summary>
        /// <param name="Node"></param>
        private bool ScRefill()
        {
            //如果不喝药+碎石，则退出碎石页面
            if (DataUtil.Config.sysConfig.potionTrue == false && DataUtil.Config.sysConfig.stoneTrue == false)
            {
                Click(500, 460);
                Thread.Sleep(delay);
                return false;
            }

            //喝药水
            if (DataUtil.Config.sysConfig.potionTrue == true)
            {
                if (Col.Check(300, 400, 128, 128, 128) == false)
                {
                    Click(300, 400);
                    while (true)
                    {
                        //等到确认框出现
                        while (Col.Check(410, 400, 190, 88, 73) == false) { Thread.Sleep(delay); }
                        //确定喝药红字出现
                        if (Col.Check(341, 323, 255, 1, 1) == true)
                        {
                            Click(410, 400);
                            CoPreventC();
                            return true;
                        }
                        //没药 - 取消窗口
                        else
                        {
                            Click(500, 400);
                            break;
                        }
                    } 
                }
                else
                {
                    Click(500, 465);
                    return false;
                }
            }

            //碎石头
            if (DataUtil.Config.sysConfig.stoneTrue == true)
            {
                Click(650, 400);
                while (true)
                {
                    //等确认窗口出现
                    while (Col.Check(410, 400, 190, 88, 73) == false) { Thread.Sleep(delay); }
                    //确定碎石红字出现
                    if (Col.Check(341, 323, 255, 1, 1) == true)
                    {
                        Click(410, 400);
                        CoPreventC();
                        return true;
                    }
                    else
                    {
                        Click(500, 400);
                        break;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 开始推图
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private void ScDepart()
        {
            CoAssistSelect();
            CoMisssionLaunch();
            //等待成功进图
            while(Col.Check(900, 25, 1, 45, 44) == false) { Thread.Sleep(delay); }
            
            return;
        }

        /// <summary>
        /// 推图过程中判定
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="wait">开始前等待</param>
        private bool ScCombat()
        {
            //等待并点击前进
            Thread.Sleep(delay);
            Click(855, 545);

            //如果出现弹窗，关闭并继续推图
            if (Col.Check(795, 205, 6, 90, 89) == true)
            {
                Click(805, 205);
                return false;
            }

            //如果出现加战友，取消并继续推图
            else if (Col.Check(680, 190, 76, 64, 47) == true)
            {
                Click(550, 430);
                return false;
            }

            //如果在主页，返还
            else if (Col.Check(170, 40, 163, 148, 66) == true && 
                     Col.Check(5, 634, 71, 62, 21) == true)
            {
                return true;
            }

            //如果出现Boss，根据选择启动函数
            else if (Col.Check(290, 400, 249, 248, 241) == true &&
                        Col.Check(720, 400, 85, 76, 48) == true)
            {
                if (DataUtil.Config.sysConfig.raidTrue == true)
                {
                    ScAttackRaid();
                    return true;
                }
                else
                {
                    ScPublicRaid();
                    return true;
                }
            }

            //如果出现特命，根据选择启动函数
            else if (Col.Check(450, 400, 192, 89, 73) == true && 
                    Col.Check(590, 400, 35, 152, 149) == true)
            {
                ScSpecial();
                return true;
            }
            
            //如果无事件，继续推图
            else
            {
                Thread.Sleep(delay);
                Click(855, 545);
                return false;
            }

        }

        /// <summary>
        /// 进入主页Boss战并放野
        /// </summary>
        /// <param name="Node"></param>
        private void ScAttackRaid()
        {
            CoBossStart();
            CoBossFirst();
            CoMisssionLaunch();
            while (Col.Check(550, 600, 227, 210, 175) == false) { Thread.Sleep(delay); }
            CoBossAttack();

            //判定是否出现无Boss点碎石页面
            while (true)
            {
                //无Boss点 要求碎石
                if (Col.Check(500, 305, 58, 39, 35) == true || 
                    Col.Check(500, 297, 58, 39, 35) == true)
                {
                    //取消碎石
                    Click(550, 400);
                    while (Col.Check(255, 135, 43, 24, 0) == false) { Thread.Sleep(delay); }
                    //请求援助
                    Click(220, 130);
                    CoBossAssist();
                    //等待并取消弹窗
                    Thread.Sleep(delay * 2);
                    while (Col.Check(5, 634, 71, 62, 21) == false)
                    {
                        CoPreventW();
                    }

                    return;
                }
                //有Boss点 成功进入
                if (Col.Check(630, 540, 0, 0, 0) == true) { break; }
                Thread.Sleep(100);
            }
            
            //等到能快时快进
            while(Col.Check(630, 540, 0, 0, 0) == true){ Thread.Sleep(delay); }
            CoBossSkip();
            CoBossAssist();
            //等待并取消弹窗
            Thread.Sleep(delay * 2);
            while (Col.Check(5, 634, 71, 62, 21) == false)
            {
                CoPreventW();
            }

            return;
        }

        /// <summary>
        /// 直接放野主页Boss
        /// </summary>
        /// <param name="Node"></param>
        private void ScPublicRaid()
        {
            CoBossPublic();
            CoBossAssist();
            //等待并取消弹窗
            Thread.Sleep(delay * 2);
            while (Col.Check(5, 634, 71, 62, 21) == false)
            {
                CoPreventW();
            }

            return;
        }

        /// <summary>
        /// 特命
        /// </summary>
        /// <param name="Node"></param>
        private void ScSpecial()
        {
            //判定是否进入特命]
            if (DataUtil.Config.sysConfig.specialTrue == true)
            {
                //确认开始特命副本
                Click(400, 400);
                while (Col.Check(218, 325, 214, 202, 163) == false) { Thread.Sleep(delay); }
                CoDepartFirst();
                while (true)
                {
                    Thread.Sleep(2 * delay);
                    //判断体力恢复是否出现
                    if (Col.Check(320, 320, 176, 31, 69) == true)
                    {
                        //如果碎石失败，返回主页面
                        if (ScRefill() == false) return;

                        CoDepartFirst();
                        break;
                    }

                    //判断队友选择是否出现
                    if (Col.Check(934, 200, 55, 46, 5) == true)
                    {
                        break;
                    }
                }
                ScDepart();
                while (ScCombat() == false) { }
                return;
            }
            else
            {
                CoSpecialExit();
            }

            return;
        }

        /// <summary>
        /// 出售花
        /// </summary>
        /// <param name="Node"></param>
        private void ScSell()
        {
            CoHomeTeam();
            CoTeamSell();
            while (Col.Check(765, 620, 62, 35, 33) == false) { Thread.Sleep(delay); }
            CoSellAll();

            //判定是否有花可卖
            while (true)
            {
                Thread.Sleep(delay * 2);
                //没花 点击取消
                if (Col.Check(420, 560, 51, 51, 51) == true)
                {
                    Click(810, 65);
                    Thread.Sleep(delay);
                    return;
                }
                //有花 点击确认
                else if (Col.Check(420, 560, 95, 34, 25) == true)
                {
                        CoSellConfirm();
                        Thread.Sleep(delay); 
                    return;
                }
                else { Thread.Sleep(delay); }
            }
        }

        /// <summary>
        /// 确认是否有探索点并探索一次
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private void ScExplore()
        {
            //开始探索
            Click(275, 150);
            //等到探索页面出现后开始连点
            Thread.Sleep(delay * 3);
            while (Col.Check(170, 40, 163, 148, 66) == false)
            {
                Click(800, 200);
                Thread.Sleep(delay);
            }
            return;
        }

        /// <summary>
        /// 确认是否有花园虫并捕获
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private void ScGarden()
        {
            //查看是否有花园虫，并收获
            Click(435, 150);
            while (Col.Check(380, 615, 34, 34, 34) == false)
            {
                if (Col.Check(375, 610, 234, 234, 234) == true)
                {
                    Click(380, 615);
                }
            }

            //返回主页之前，关闭任何弹窗+点击返回主页
            while(Col.Check(170, 40, 163, 148, 66) == false)
            {
                if (Col.Check(850, 150, 1, 75, 73) == true)
                {
                    Click(850, 150);
                }
                CoHomeReturn();
                Thread.Sleep(delay);
            }
            return;
        }

        /// <summary>
        /// 提前恢复体力
        /// </summary>
        private void ScActionPrep()
        {
            while (Col.Check(5, 634, 71, 62, 21) == false) { Thread.Sleep(delay); }
            Click(80, 360);
            while (Col.Check(300, 380, 202, 165, 144) == false) { Thread.Sleep(delay); }
            Click(300, 380);
            while (Col.Check(320, 320, 176, 31, 69) == false) { Thread.Sleep(delay); }
            ScRefill();

        }

        /* Place Holder
         */


        //鼠标验色与点击事件 - Co系列

        /// <summary>
        /// 等待弹窗出现后关闭、并等到回到主页面或画面恢复正常
        /// </summary>
        private void CoPreventC()
        {
            while (Col.Check(795, 205, 6, 90, 89) == false) { Thread.Sleep(delay); }
            Click(805, 205);
            while (Col.Check(5, 634, 71, 62, 21) == false) { Thread.Sleep(delay); }
        }

        /// <summary>
        /// 如果目前有弹窗 则关闭 需要配合while使用
        /// </summary>
        private void CoPreventW()
        {
            while (Col.Check(795, 205, 6, 90, 89) == true)
            {
                Thread.Sleep(delay);
                Click(805, 205);
            }
        }

        /// <summary>
        /// 等待一定时间后取消弹窗
        /// </summary>
        private void CoCancle()
        { 
            Thread.Sleep(delay);
            Click(550, 400);
        }

        /// <summary>
        /// 等待下方黑条载入结束后 确认在主页并点击出击按钮
        /// </summary>
        /// <returns></returns>
        private void CoHomeDepart()
        {
            while (Col.Check(600, 400, 28, 29, 19) == false)
            {
                while (Col.Check(5, 634, 71, 62, 21) == false) { Thread.Sleep(delay); }
                Click(80, 155);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 等待下方黑条载入结束后 确认在主页并点击编队按钮
        /// </summary>
        /// <returns></returns>
        private void CoHomeTeam()
        {
            while (Col.Check(5, 634, 71, 62, 21) == false) { Thread.Sleep(delay); }
            Click(85, 210);
        }

        /// <summary>
        /// 等待第一个可推图出现并点击
        /// </summary>
        /// <returns></returns>
        private void CoDepartFirst()
        {
            while (Col.Check(600, 265, 28, 29, 19) == false) { Thread.Sleep(delay); }
            Click(430, 245);
        }

        /// <summary>
        /// 确认上一次图可点，在进入选队友、出击、或者体力恢复之前不停点上一次图
        /// </summary>
        private void CoDepartPrevious()
        {
            while (Col.Check(600, 400, 28, 29, 19) == true)
            {
                while (Col.Check(934, 200, 55, 46, 5) == false &&
                    Col.Check(730, 200, 213, 185, 132) == false &&
                    Col.Check(320, 320, 176, 31, 69) == false)
                {
                    Click(250, 400);
                    Thread.Sleep(delay);
                }
            }
        }

        /// <summary>
        /// 战友选择成功之前不停地点击二号战友位
        /// </summary>
        /// <returns></returns>
        private void CoAssistSelect()
        {
            while (Col.Check(730, 200, 213, 185, 132) == false)
            {
                while (Col.Check(934, 200, 55, 46, 5) == false) { Thread.Sleep(delay); }
                Click(600, 250);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 等待出击按钮出现并开始推图
        /// </summary>
        /// <returns></returns>
        private void CoMisssionLaunch()
        {
            while (Col.Check(730, 200, 213, 185, 132) == false) { Thread.Sleep(delay); }
            Click(850, 555);
        }

        /// <summary>
        /// 等待出现Boss并选择攻击Boss
        /// </summary>
        /// <returns></returns>
        private void CoBossStart()
        {
            while (Col.Check(290, 400, 249, 248, 241) == false) { Thread.Sleep(delay); }
            Click(285, 400);
        }

        /// <summary>
        /// 等待出现Boss并选择放野Boss
        /// </summary>
        /// <returns></returns>
        private void CoBossPublic()
        {
            while (Col.Check(290, 400, 249, 248, 241) == false) { Thread.Sleep(delay); }
            Click(650, 400);
        }

        /// <summary>
        /// 等待Boss列表加载完毕并攻击第一个
        /// </summary>
        /// <returns></returns>
        private void CoBossFirst()
        {
            while (Col.Check(840, 250, 87, 73, 52) == false) { Thread.Sleep(delay); }
            Click(840, 250);
        }

        /// <summary>
        /// 在进入下一步之前不停地选择普通攻击Boss
        /// </summary>
        /// <returns></returns>
        private void CoBossAttack()
        {
            while (Col.Check(500, 305, 58, 39, 35) == false &&
                   Col.Check(500, 297, 58, 39, 35) == false &&
                   Col.Check(630, 540, 0, 0, 0) == false)
            {
                Click(750, 555);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 等待申请援助选项出现并点击
        /// </summary>
        /// <returns></returns>
        private void CoBossAssist()
        {
            while (Col.Check(290, 400, 175, 74, 59) == false) { Thread.Sleep(delay); }
            Click(290, 400);
        }

        /// <summary>
        /// 等待特命弹窗出现并选择取消
        /// </summary>
        /// <returns></returns>
        private void CoSpecialExit()
        {
            while (Col.Check(450, 400, 192, 89, 73) == false) { Thread.Sleep(delay); }
            Click(550, 400);
        }

        /// <summary>
        /// 确认进入编成页面并点击出售
        /// </summary>
        /// <returns></returns>
        private void CoTeamSell()
        {
            while (Col.Check(201, 235, 130, 184, 201) == false) { Thread.Sleep(delay); }
            Click(535, 137);
        }

        /// <summary>
        /// 确认进入出售页面并不停地点击出售
        /// 直到出售页面出现为止
        /// </summary>
        /// <returns></returns>
        private void CoSellAll()
        {
            while (Col.Check(780, 580, 237, 225, 198) == false)
            {
                Click(220, 295);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 确认进入批量出售页面并点击出售
        /// </summary>
        /// <returns></returns>
        private void CoSellConfirm()
        {
            while (Col.Check(802, 530, 233, 216, 183) == false) { Thread.Sleep(delay); }
            Click(420, 560);
        }

        /// <summary>
        /// 连续点击Home与无效位置来返回主页
        /// </summary>
        /// <returns></returns>
        private void CoHomeReturn()
        {
            while (Col.Check(437, 177, 211, 209, 205) == false )
            {
                Thread.Sleep(delay);
                Click(80, 80);
                Thread.Sleep(delay);
                Click(5, 5);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 快进主页Boss
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private void CoBossSkip()
        {
            while (Col.Check(855, 555, 229, 229, 229) == true)
            {
                Thread.Sleep(delay);
                Click(855, 555);
            }
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private void Cotemplate()
        {
            while (Col.Check(0, 0, 0, 0, 0) == false) { Thread.Sleep(delay); }
            Click(0, 0);
        }
        */

    }
}

