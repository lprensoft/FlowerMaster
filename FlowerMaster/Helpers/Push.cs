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

namespace FlowerMaster.Push
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
        public int Choice { get; }
        public Handles Hand { get; }

        /// <summary>
        /// 包含推图选择与句柄信息
        /// </summary>
        /// <param name="Cho">推图选择</param>
        /// <param name="Han">句柄信息</param>
        public Nodes(int Cho, IntPtr TopHand)
        {
            Choice = Cho;
            Hand = new Handles(TopHand);
        }

        private IntPtr Webhandle = IntPtr.Zero;
        private int delay = 256;
        private Helpers.Color Col = Helpers.Color.Instance;
        

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
        

        /// <summary>
        /// 等待颜色出现
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="red">红色度</param>
        /// <param name="green">绿色度</param>
        /// <param name="blue">蓝色度</param>
        /// <param name="delay">默认延迟300，设为0则会变为判断颜色是否存在</param>
        /// <returns>正常延迟：颜色出现后返还true 0延迟：判断颜色是否在像素点</returns>

        //Main
        /// <summary>
        /// 开始脚本于初始化数据
        /// </summary>
        /// <param name="Node_o">特殊数据结构Node（见上）</param>
        public async void Start(Nodes Node_o)
        {
            Nodes Node = new Nodes(Node_o.Choice, Node_o.Hand.TopHand);
            Webhandle = Node.Hand.BotHand;

            Col.Load(delay, Webhandle);

            while (MainWindow.AutoPushS == true)
            {
                await ScSelect();

                await ScDepart();

                while(await ScCombat() == false) { }

                await ScSell();

                await ScExplore();
                await ScGarden();
            }

            return;
        }

        //脚本事件 - Sc系列
        /// <summary>
        /// 选择关卡，并恢复体力
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScSelect()
        {

            await CoHomeDepart();

            //等待出击页面1加载结束
            await Col.Check(180, 400, 146, 122, 96);
            //根据选择点击出击页面1
            if (Choice == 0)
            {
                Click(300, 140);
            }
            else if (Choice == 1)
            {
                Click(400, 140);
            }
            
            //根据选择等待并点击出击页面2
            if (Choice == 0)
            {
                await Col.Check(218, 242, 213, 200, 159);
                Click(300, 260);
            }
            else if (Choice == 1)
            {
                await Col.Check(218, 325, 214, 202, 163);
                Click(300, 330);
            }

            await CoDepartFirst();

            //等待体力恢复页面出现
            while(await Col.Check(670, 255, 23, 23, 18, true) == true)
            {
                await Task.Delay(delay);
            }

            //确认体力页面是否出现五次
            for (int i = 0; i < 5; i++)
            {
                if (await Col.Check(320, 320, 176, 31, 69, true))
                {
                    await ScRefill();
                    await CoDepartFirst();
                    return;
                }
                await Task.Delay(delay);
            }
            
            return;
        }
        
        /// <summary>
        /// 恢复体力
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScRefill()
        {
            if (await Col.Check(320, 320, 176, 31, 69, true))
            {
                Click(300, 400);
                if (await Col.Check(180, 450, 104, 88, 72))
                {
                    if (await Col.Check(341, 323, 255, 1, 1, true))
                    {
                        Click(410, 400);
                        await CoPrevent();
                        return;
                    }
                    else
                        Click(500, 400);
                }

                await CoDepartFirst();

                await Col.Check(320, 320, 176, 31, 69);

                Click(650, 400);
                if (await Col.Check(180, 450, 104, 88, 72))
                {
                    if (await Col.Check(341, 323, 255, 1, 1, true))
                    {
                        Click(410, 400);
                        await CoPrevent();
                        return;
                    }
                    else
                        Click(500, 400);
                }
            }

            return;

        }

        /// <summary>
        /// 开始推图
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task ScDepart()
        {
            await CoAssistSecond();

            await CoMisssionLaunch();

            //确保进入图中
            await Task.Delay(4 * delay);
            Click(800, 555);
            await Task.Delay(4 * delay);
            Click(800, 555);
            
            return;
        }

        /// <summary>
        /// 推图过程中判定
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="wait">开始前等待</param>
        private async Task<bool> ScCombat()
        {
            //等待并点击前进
            await Task.Delay(delay);
            Click(855, 545);
            await Task.Delay(delay);

            //如果出现弹窗，关闭并继续推图
            if (await Col.Check(795, 205, 6, 90, 89, true))
            {
                Click(805, 205);
                return false;
            }

            //如果出现加战友，取消并继续推图
            else if (await Col.Check(680, 190, 76, 64, 47, true))
            {
                Click(550, 430);
                return false;
            }

            //如果在主页，返还
            else if (await Col.Check(350, 35, 209, 195, 147, true))
            {
                return true;
            }

            //如果出现Boss，根据选择启动函数
            else if (await Col.Check(290, 400, 249, 248, 241, true))
            {
                if (Choice == 0)
                {
                    await ScAttackRaid();
                    return true;
                }
                else
                {
                    await ScPublicRaid();
                    return true;
                }
            }

            //如果出现特命，根据选择启动函数
            else if (await Col.Check(450, 400, 192, 89, 73, true))
            {
                await ScSpecial();
                return true;
            }
            
            //如果无事件，继续推图
            else
            {
                await Task.Delay(delay);
                Click(855, 545);
                await ScCombat();
                return true;
            }

        }

        /// <summary>
        /// 进入主页Boss战并放野
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScAttackRaid()
        {
            await CoBossStart();
            await CoBossFirst();
            await CoMisssionLaunch();
            await CoBossAttack();

            //取消取消碎石拿Boss点页面
            await CoCancle(5);

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
            await CoBossPublic();
            await CoBossAssist();

            //通过等待确保弹窗出现
            await Task.Delay(2*delay);
            await CoPrevent();
            await Task.Delay(delay);

            return;
        }

        /// <summary>
        /// 特命 - 目前直接退出
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScSpecial()
        {
            await CoSpecialExit();
            await Task.Delay(delay);

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
            await CoHomeReturn();
            await Task.Delay(delay);

            //查看是否有探索点，并探索
            if (await Col.Check(255,160,234,116,37,true) == true)
            {
                Click(275, 150);
                while (await Col.Check(350, 35, 209, 195, 147, true) == false)
                {
                    Click(950, 280);
                }
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
            await CoHomeReturn();
            await Task.Delay(delay);

            //查看是否有花园虫，并收获
            if (await Col.Check(475, 135, 143, 0, 1, true) == true)
            {
                Click(435, 150);
                while (await Col.Check(380, 615, 34, 34, 34, true) == false)
                {
                    if (await Col.Check(375, 610, 234, 234, 234, true) == true)
                    {
                        Click(380, 615);
                    }
                }
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
            await Col.Check(795, 205, 6, 90, 89);
            Click(805, 205);
        }

        /// <summary>
        /// 等待一定时间后取消弹窗
        /// </summary>
        private async Task CoCancle(int multi = 1)
        {
            await Task.Delay(delay * multi);
            Click(550, 400);
        }

        /// <summary>
        /// 确认在主页并点击出击按钮
        /// </summary>
        /// <returns></returns>
        private async Task CoHomeDepart()
        {
            await Col.Check(350, 35, 209, 195, 147);
            Click(80, 155);
        }

        /// <summary>
        /// 确认在主页并点击编队按钮
        /// </summary>
        /// <returns></returns>
        private async Task CoHomeTeam()
        {
            await Col.Check(350, 35, 209, 195, 147);
            Click(85, 210);
        }

        /// <summary>
        /// 等待第一个可推图出现并点击
        /// </summary>
        /// <returns></returns>
        private async Task CoDepartFirst()
        {
            await Col.Check(670, 255, 23, 23, 18);
            Click(430, 245);
        }

        /// <summary>
        /// 等待二号战友出现并选择
        /// </summary>
        /// <returns></returns>
        private async Task CoAssistSecond()
        {
            await Col.Check(750, 200, 228, 218, 178);
            Click(750, 250);
        }

        /// <summary>
        /// 等待出击按钮出现并开始推图
        /// </summary>
        /// <returns></returns>
        private async Task CoMisssionLaunch()
        {
            await Col.Check(730, 200, 213, 185, 132);
            Click(800, 555);
        }

        /// <summary>
        /// 等待出现Boss并选择攻击Boss
        /// </summary>
        /// <returns></returns>
        private async Task CoBossStart()
        {
            await Col.Check(290, 400, 249, 248, 241);
            Click(285, 400);
        }

        /// <summary>
        /// 等待出现Boss并选择放野Boss
        /// </summary>
        /// <returns></returns>
        private async Task CoBossPublic()
        {
            await Col.Check(290, 400, 249, 248, 241);
            Click(650, 400);
        }

        /// <summary>
        /// 等待Boss列表加载完毕并攻击第一个
        /// </summary>
        /// <returns></returns>
        private async Task CoBossFirst()
        {
            await Col.Check(840, 250, 87, 73, 52);
            Click(840, 250);
        }

        /// <summary>
        /// 等待攻击Boss页面出现并选择通常攻击
        /// </summary>
        /// <returns></returns>
        private async Task CoBossAttack()
        {
            await Col.Check(550, 600, 227, 210, 175);
            Click(750, 555);
        }

        /// <summary>
        /// 等待申请援助选项出现并点击
        /// </summary>
        /// <returns></returns>
        private async Task CoBossAssist()
        {
            await Col.Check(290, 400, 175, 74, 59);
            Click(290, 400);
        }

        /// <summary>
        /// 等待特命弹窗出现并选择取消
        /// </summary>
        /// <returns></returns>
        private async Task CoSpecialExit()
        {
            await Col.Check(450, 400, 192, 89, 73);
            Click(550, 400);
        }

        /// <summary>
        /// 确认进入编成页面并点击出售
        /// </summary>
        /// <returns></returns>
        private async Task CoTeamSell()
        {
            await Col.Check(201, 235, 130, 184, 201);
            Click(535, 137);
        }

        /// <summary>
        /// 确认进入出售页面并点击批量出售
        /// </summary>
        /// <returns></returns>
        private async Task CoSellAll()
        {
            await Col.Check(206, 329, 80, 26, 17);
            Click(220, 295);
        }

        /// <summary>
        /// 确认进入批量出售页面并点击出售
        /// </summary>
        /// <returns></returns>
        private async Task CoSellConfirm()
        {
            await Col.Check(802, 530, 233, 216, 183);
            Click(420, 560);
        }

        /// <summary>
        /// 连续点击Home与无效位置来返回主页
        /// </summary>
        /// <returns></returns>
        private async Task CoHomeReturn()
        {
            while (await Col.Check(300, 140, 158, 123, 72, true) == false)
            {
                Click(80, 80);
                await Task.Delay(delay);
                Click(5, 5);
                await Task.Delay(delay);
            }
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

