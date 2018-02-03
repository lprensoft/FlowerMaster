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
        private bool Running = false;
        

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
            Col.Load(DataUtil.Config.sysConfig.delayTime, Webhandle);
        }
        
        
        //Main
        /// <summary>
        /// 开始脚本于初始化数据
        /// </summary>
        public async Task Start()
        {
            Running = true;
            while(Running == true)
            {
                    while (await ScSelect() == false) { }

                    await ScDepart();

                    while (await ScCombat() == false) { }
                    //将延迟恢复至正常水平（ScCombat降延迟为0）
                    Col.Load(delay, Webhandle);

                    await ScSell();

                    await CoHomeReturn();
                    if (await Col.Check(258, 163, 99, 99, 99, true) == false && await Col.Check(525, 75, 50, 41, 37, true) == true)
                    {
                        await ScExplore();
                    }
                    if (DataUtil.Game.player.plantTime < DataUtil.Game.serverTime)
                    {
                        await ScGarden();
                    }
            }

            return;
        }

        //脚本事件 - Sc系列
        /// <summary>
        /// 选择关卡，并恢复体力
        /// </summary>
        /// <param name="Node"></param>
        private async Task<bool> ScSelect()
        {
            await CoHomeDepart();

            //等待出击页面1加载结束
            while (await Col.Check(180, 400, 146, 122, 96) == false) {};
            await Task.Delay(delay);
            //根据选择点击出击页面1
            if (DataUtil.Config.sysConfig.pushType == 0)
                Click(300, 140);

            if (DataUtil.Config.sysConfig.pushType == 1)
                Click(400, 140);

            if (DataUtil.Config.sysConfig.pushType == 2)
                Click(590, 140);

            if (DataUtil.Config.sysConfig.pushType == 3)
                Click(250, 400);

            //根据选择等待并点击出击页面2
            if (DataUtil.Config.sysConfig.pushType == 0)
            {
                while (await Col.Check(218, 242, 213, 200, 159) == false) {};
                Click(300, 260);
            }
            if (DataUtil.Config.sysConfig.pushType == 1)
            {
                while (await Col.Check(218, 325, 214, 202, 163) == false) {};
                Click(300, 330);
            }

            if (DataUtil.Config.sysConfig.pushType == 2)
            {
                while (await Col.Check(470,275,249,247,240) == false) {};
                Click(300, 260);
                while (await Col.Check(625, 230, 253, 168, 2) == false) { };
                Click(300, 260);
            }


            if (DataUtil.Config.sysConfig.pushType != 3)
            {
                await CoDepartFirst();

                //等待体力恢复页面出现
                await Task.Delay(delay * 3);


                //确认体力页面是否出现2次
                for (int i = 0; i < 2; i++)
                {
                    if (await Col.Check(320, 320, 176, 31, 69, true))
                    {
                        await ScRefill();
                        await CoDepartFirst();
                        return true;
                    }
                    await Task.Delay(2 * delay);
                }
                return true;
            }
            
            if (DataUtil.Config.sysConfig.pushType == 3)
            {
                await Task.Delay(delay);
                await CoDepartPrevious();

                //等待体力恢复页面出现
                while (await Col.Check(536, 255, 30, 31, 21, true) == true)
                {
                    await Task.Delay(delay);
                }

                //确认体力页面是否出现2次
                for (int i = 0; i < 2; i++)
                {
                    if (await Col.Check(320, 320, 176, 31, 69, true) == true)
                    {
                        //如果没体力+没吃喝，回到主页
                        if (await ScRefill() == false)
                            return false;

                        await CoDepartPrevious();
                        return true;
                    }
                    await Task.Delay(delay);
                }
                return true;
            }
            return true;
        }
        
        /// <summary>
        /// 恢复体力脚本
        /// </summary>
        /// <param name="Node"></param>
        private async Task<bool> ScRefill()
        {
            await Task.Delay(delay);
            //确认是否出现药水瓶
            if (await Col.Check(320, 320, 176, 31, 69, true))
            {
                //喝药水
                await Task.Delay(delay);
                if (DataUtil.Config.sysConfig.potionTrue == true)
                {
                    Click(300, 400);
                    while (true)
                    {
                        //等到确认框出现
                        while (await Col.Check(180, 450, 104, 88, 72) == false) { }
                        if (await Col.Check(341, 323, 255, 1, 1, true))
                        {
                            Click(410, 400);
                            await CoPrevent();
                            return true;
                        }
                        else
                        {
                            Click(500, 400);
                            break;
                        }
                    }
                }

                await CoDepartFirst();

                //碎石头
                await Task.Delay(delay);
                if (DataUtil.Config.sysConfig.stoneTrue == true)
                {
                    while (await Col.Check(320, 320, 176, 31, 69) == false) {};
                    Click(650, 400);
                    while (true)
                    {
                        while (await Col.Check(180, 450, 104, 88, 72) == false) {}
                        if (await Col.Check(341, 323, 255, 1, 1, true))
                        {
                            Click(410, 400);
                            await CoPrevent();
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
            return true;
        }

        /// <summary>
        /// 开始推图
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task ScDepart()
        {
            await Task.Delay(delay);

            await CoAssistSecond();

            await CoMisssionLaunch();
            //点击推图四次
            for (int i = 0; i < 4; i++)
            {
                await Task.Delay(delay);
                Click(855, 555);
            }
            
            return;
        }

        /// <summary>
        /// 推图过程中判定
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="wait">开始前等待</param>
        private async Task<bool> ScCombat()
        {
            //推图时判定无延迟加快速度
            Col.Load(50, Webhandle);

            //等待并点击前进
            await Task.Delay(delay);
            Click(855, 545);

            //如果出现弹窗，关闭并继续推图
            if (await Col.Check(795, 205, 6, 90, 89, true) == true)
            {
                Click(805, 205);
                return false;
            }

            //如果出现加战友，取消并继续推图
            else if (await Col.Check(680, 190, 76, 64, 47, true) == true)
            {
                Click(550, 430);
                return false;
            }

            //如果在主页，返还
            else if (await Col.Check(170, 40, 163, 148, 66, true) == true)
            {
                return true;
            }

            //如果出现Boss，根据选择启动函数
            else if (await Col.Check(290, 400, 249, 248, 241, true) == true)
            {
                if (DataUtil.Config.sysConfig.raidTrue == true)
                {
                    await ScAttackRaid();
                    return true;
                }
                await ScPublicRaid();
                return true;
            }

            //如果出现特命，根据选择启动函数
            else if (await Col.Check(450, 400, 192, 89, 73, true) == true)
            {
                await ScSpecial();
                return true;
            }
            
            //如果无事件，继续推图
            else
            {
                await Task.Delay(delay);
                Click(855, 545);
                return false;
            }

        }

        /// <summary>
        /// 进入主页Boss战并放野
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScAttackRaid()
        {
            await Task.Delay(delay);
            await CoBossStart();
            await CoBossFirst();
            await CoMisssionLaunch();
            await CoBossAttack();

            //取消取消碎石拿Boss点页面
            for (int i = 0; i < 4; i++)
            {
                await Task.Delay(delay);
                if (await Col.Check(500,305,58,39,35,true) == true)
                {
                    Click(550, 400);
                    await Task.Delay(delay);
                    while (await Col.Check(255, 135, 43, 24, 0, true) == false) {}
                    Click(220, 130);

                    await CoBossAssist();
                    await Task.Delay(delay * 2);
                    while (await Col.Check(170, 40, 163, 148, 66, true) == false)
                    {
                        await CoPrevent();
                    }
                    await Task.Delay(delay);

                    return;
                }
            }

            //等待并确认是否需要再次点击出击
            for (int i = 0; i < 2; i++)
            {
                if (await Col.Check(550, 600, 227, 210, 175, true) == true)
                {
                    Click(750, 555);
                }
                await Task.Delay(delay);
            }
            
            //黑屏时等待
            while(await Col.Check(630,540,0,0,0,true))
            {
                await Task.Delay(delay);
            }

            Click(855, 555);

            await CoBossAssist();
            await Task.Delay(delay);
            await CoPrevent();

            return;
        }

        /// <summary>
        /// 直接放野主页Boss
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScPublicRaid()
        {
            await Task.Delay(delay);
            await CoBossPublic();
            await CoBossAssist();
            await Task.Delay(delay);
            while (await Col.Check(170, 40, 163, 148, 66, true) == false)
            {
                await CoPrevent();
            }

            await Task.Delay(delay);

            return;
        }

        /// <summary>
        /// 特命 - 目前直接退出
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScSpecial()
        {
            //判定是否进入特命]
            if (DataUtil.Config.sysConfig.specialTrue == true)
            {
                //确认开始特命副本
                Click(400, 400);
                while (await Col.Check(218, 325, 214, 202, 163, true) == false) { }
                await CoDepartFirst();
                for (int i = 0; i < 4; i++)
                {
                    if (await Col.Check(320, 320, 176, 31, 69, true))
                    {
                        await ScRefill();
                        await CoDepartFirst();
                        return;
                    }
                    await Task.Delay(delay);
                }
                while (await ScCombat() == false) { }
                return;
            }
            else
            {
                await CoSpecialExit();
                await Task.Delay(delay);
            }

            return;
        }

        /// <summary>
        /// 出售花
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScSell()
        {
            await CoHomeTeam();
            await CoTeamSell();
            await CoSellAll();
            await CoSellConfirm();

            //防止无花可卖并取消出售
            await Task.Delay(delay);
            Click(810, 65);
            await Task.Delay(delay * 3);

            return;
        }

        /// <summary>
        /// 确认是否有探索点并探索一次
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task ScExplore()
        {
            for (int i = 0; i < 2; i++)
            {
                await CoHomeReturn();
            }

            //开晒探索
            await Task.Delay(delay * 3);
            Click(275, 150);
            while (await Col.Check(170, 40, 163, 148, 66, true) == false)
            {
                Click(800, 200);
            }
            return;
        }

        /// <summary>
        /// 确认是否有花园虫并捕获
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task ScGarden()
        {
            for (int i = 0; i < 2; i++)
            {
                await CoHomeReturn();
            }

            //查看是否有花园虫，并收获
            Click(435, 150);
            while (await Col.Check(380, 615, 34, 34, 34, true) == false)
            {
                if (await Col.Check(375, 610, 234, 234, 234, true) == true)
                {
                    Click(380, 615);
                }
                if (await Col.Check(850, 150, 1, 75, 73, true) == true)
                {
                    Click(850, 150);
                }
            }
            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(delay);
                await CoHomeReturn();
            }
            return;
        }

        /* Place Holder
         */

        
        //鼠标验色与点击事件 - Co系列
        /// <summary>
        /// 等待并关闭弹窗
        /// </summary>
        private async Task CoPrevent()
        {
            while (await Col.Check(795, 205, 6, 90, 89) == false) {};
            Click(805, 205);
        }

        /// <summary>
        /// 等待一定时间后取消弹窗
        /// </summary>
        private async Task CoCancle()
        { 
            await Task.Delay(delay);
            Click(550, 400);
        }

        /// <summary>
        /// 确认在主页并点击出击按钮
        /// </summary>
        /// <returns></returns>
        private async Task CoHomeDepart()
        {
            while (await Col.Check(170, 40, 163, 148, 66) == false) {};
            Click(80, 155);
        }

        /// <summary>
        /// 确认在主页并点击编队按钮
        /// </summary>
        /// <returns></returns>
        private async Task CoHomeTeam()
        {
            while (await Col.Check(170, 40, 163, 148, 66) == false) {};
            Click(85, 210);
        }

        /// <summary>
        /// 等待第一个可推图出现并点击
        /// </summary>
        /// <returns></returns>
        private async Task CoDepartFirst()
        {
            while (await Col.Check(670, 255, 23, 23, 18) == false) {};
            Click(430, 245);
        }

        /// <summary>
        /// 等待二号战友出现并选择
        /// </summary>
        /// <returns></returns>
        private async Task CoAssistSecond()
        {
            while (await Col.Check(934, 200, 55, 46, 5) == false) {};
            Click(750, 250);
        }

        /// <summary>
        /// 等待出击按钮出现并开始推图
        /// </summary>
        /// <returns></returns>
        private async Task CoMisssionLaunch()
        {
            while (await Col.Check(730, 200, 213, 185, 132) == false) {};
            Click(850, 555);
        }

        /// <summary>
        /// 等待出现Boss并选择攻击Boss
        /// </summary>
        /// <returns></returns>
        private async Task CoBossStart()
        {
            while (await Col.Check(290, 400, 249, 248, 241) == false) {};
            Click(285, 400);
        }

        /// <summary>
        /// 等待出现Boss并选择放野Boss
        /// </summary>
        /// <returns></returns>
        private async Task CoBossPublic()
        {
            while (await Col.Check(290, 400, 249, 248, 241) == false) {};
            Click(650, 400);
        }

        /// <summary>
        /// 等待Boss列表加载完毕并攻击第一个
        /// </summary>
        /// <returns></returns>
        private async Task CoBossFirst()
        {
            while (await Col.Check(840, 250, 87, 73, 52) == false) {};
            Click(840, 250);
        }

        /// <summary>
        /// 等待攻击Boss页面出现并选择通常攻击
        /// </summary>
        /// <returns></returns>
        private async Task CoBossAttack()
        {
            while (await Col.Check(550, 600, 227, 210, 175) == false) {};
            Click(750, 555);
        }

        /// <summary>
        /// 等待申请援助选项出现并点击
        /// </summary>
        /// <returns></returns>
        private async Task CoBossAssist()
        {
            while (await Col.Check(290, 400, 175, 74, 59) == false) {};
            Click(290, 400);
        }

        /// <summary>
        /// 等待特命弹窗出现并选择取消
        /// </summary>
        /// <returns></returns>
        private async Task CoSpecialExit()
        {
            while (await Col.Check(450, 400, 192, 89, 73) == false) {};
            Click(550, 400);
        }

        /// <summary>
        /// 确认进入编成页面并点击出售
        /// </summary>
        /// <returns></returns>
        private async Task CoTeamSell()
        {
            while (await Col.Check(201, 235, 130, 184, 201) == false) {};
            Click(535, 137);
        }

        /// <summary>
        /// 确认进入出售页面并点击批量出售
        /// </summary>
        /// <returns></returns>
        private async Task CoSellAll()
        {
            while (await Col.Check(206, 329, 80, 26, 17) == false) {};
            Click(220, 295);
        }

        /// <summary>
        /// 确认进入批量出售页面并点击出售
        /// </summary>
        /// <returns></returns>
        private async Task CoSellConfirm()
        {
            while (await Col.Check(802, 530, 233, 216, 183) == false) {};
            Click(420, 560);
        }

        /// <summary>
        /// 连续点击Home与无效位置来返回主页
        /// </summary>
        /// <returns></returns>
        private async Task CoHomeReturn()
        {
            Click(80, 80);
            await Task.Delay(delay);
            Click(5, 5);
            await Task.Delay(delay);
        }

        private async Task CoDepartPrevious()
        {
            while (await Col.Check(643, 375, 92, 81, 46) == false) { };
            Click(250, 400);
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task Cotemplate()
        {
            await Col.Check(206, 329, 80, 26, 17);
            Click(220, 295);
        }
        */

    }
}

