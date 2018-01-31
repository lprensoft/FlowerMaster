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

        public int Choice { get; }
        public Handles Hand { get; }

        /// <summary>
        /// 包含推图选择与句柄信息
        /// </summary>
        /// <param name="Cho">推图选择</param>
        /// <param name="Han">句柄信息</param>
        public Nodes(int Cho, Handles Han)
        {
            Choice = Cho;
            Hand = Han;
        }

        private IntPtr Webhandle = IntPtr.Zero;
        int delay = 256;

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
        private async Task<bool> Waitcol(int x, int y, byte red = 0, byte green = 0, byte blue = 0, int delay = 128)
        {
            Random rnd = new Random();
            await Task.Delay(delay + 1);
            System.Drawing.Color color = GetPixelColor(Hand.BotHand, x, y);
            if (color.R - 2 <= red &&
                color.R + 2 >= red &&
                color.G - 2 <= green &&
                color.G + 2 >= green &&
                color.B - 2 <= blue &&
                color.B + 2 >= blue)
            {
                await Task.Delay(delay + 1);
                return true;
            }

            else if (delay == 0)
            {
                await Task.Delay(delay + 1);
                return false;
            }

            else
            {
                await Task.Delay(delay + 1);
                return await Waitcol(x, y, red, green, blue);
            }
        }

        //Main
        /// <summary>
        /// 开始脚本于初始化数据
        /// </summary>
        /// <param name="Node_o">特殊数据结构Node（见上）</param>
        public async Task Start(Nodes Node_o)
        {
            Nodes Node = new Nodes(Node_o.Choice, Node_o.Hand);

            Webhandle = Node.Hand.BotHand;

            while (MainWindow.AutoPushS == true)
            {
                await ScSelect(Node);
                await Task.Delay(delay);

                await ScDepart(Node);
                await Task.Delay(delay);

                await ScComat(Node);
                await Task.Delay(delay);

                await ScSell(Node);
                await Task.Delay(delay);

                await ScExplore(Node);
                await ScGarden(Node);
                await Task.Delay(delay);
            }

            return;
        }

        //脚本事件 - Sc系列
        /// <summary>
        /// 选择关卡，并恢复体力
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScSelect(Nodes Node)
        {

            await CoHomeDepart(Node);

            //等待出击页面1加载结束
            await Waitcol(180, 400, 146, 122, 96);
            //根据选择点击出击页面1
            if (Node.Choice == 0)
            {
                Click(300, 140);
            }
            else if (Node.Choice == 1)
            {
                Click(400, 140);
            }
            
            //根据选择等待并点击出击页面2
            if (Choice == 0)
            {
                await Waitcol(218, 242, 213, 200, 159);
                Click(300, 260);
            }
            else if (Choice == 1)
            {
                await Waitcol(218, 325, 214, 202, 163);
                Click(300, 330);
            }

            await CoDepartFirst(Node);

            //等待体力恢复页面出现
            while(await Waitcol(670, 255, 23, 23, 18, 0) == true)
            {
                await Task.Delay(delay);
            }

            //确认体力页面是否出现五次
            for (int i = 0; i < 5; i++)
            {
                if (await Waitcol(320, 320, 176, 31, 69, 0))
                {
                    await ScRefill(Node);
                    await CoDepartFirst(Node);
                }
                await Task.Delay(delay);
            }
            
            return;
        }
        
        /// <summary>
        /// 恢复体力
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScRefill(Nodes Node)
        {
            if (await Waitcol(320, 320, 176, 31, 69, 0))
            {
                Click(300, 400);
                if (await Waitcol(180, 450, 104, 88, 72))
                {
                    if (await Waitcol(341, 323, 255, 1, 1, 0))
                    {
                        Click(410, 400);
                        await CoPrevent(Node);
                        return;
                    }
                    else
                        Click(500, 400);
                }

                await CoDepartFirst(Node);

                await Waitcol(320, 320, 176, 31, 69);

                Click(650, 400);
                if (await Waitcol(180, 450, 104, 88, 72))
                {
                    if (await Waitcol(341, 323, 255, 1, 1, 0))
                    {
                        Click(410, 400);
                        await CoPrevent(Node);
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
        private async Task ScDepart(Nodes Node)
        {
            await CoAssistSecond(Node);

            await CoMisssionLaunch(Node);

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
        private async Task ScComat(Nodes Node, int wait = 0)
        {
            //等待并点击前进
            await Task.Delay(wait);
            Click(855, 545);
            await Task.Delay(delay);

            //如果出现弹窗，关闭并继续推图
            if (await Waitcol(795, 205, 6, 90, 89, 0))
            {
                Click(805, 205);
                await ScComat(Node);
            }

            //如果出现加战友，取消并继续推图
            else if (await Waitcol(680, 190, 76, 64, 47, 0))
            {
                Click(550, 430);
                await ScComat(Node);
            }

            //如果在主页，返还
            else if (await Waitcol(350, 35, 209, 195, 147, 0))
            {
                return;
            }

            //如果出现Boss，根据选择启动函数
            else if (await Waitcol(290, 400, 249, 248, 241, 0))
            {
                if (Choice == 0)
                {
                    await ScAttackRaid(Node);
                    return;
                }
                else if (Choice == 1)
                {
                    await ScPublicRaid(Node);
                    return;
                }
            }

            //如果出现特命，根据选择启动函数
            else if (await Waitcol(450, 400, 192, 89, 73, 0))
            {
                await ScSpecial(Node);
                return;
            }
            
            //如果无事件，继续推图
            else
            {
                await Task.Delay(delay);
                Click(855, 545);
                await ScComat(Node);
                return;
            }

        }

        /// <summary>
        /// 进入主页Boss战并放野
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScAttackRaid(Nodes Node)
        {
            await CoBossStart(Node);
            await CoBossFirst(Node);
            await CoMisssionLaunch(Node);
            await CoBossAttack(Node);

            //取消取消碎石拿Boss点页面
            await CoCancle(Node, 5);

            //等待并确认是否需要再次点击出击
            for (int i = 0; i < 2; i++)
            {
                if (await Waitcol(550, 600, 227, 210, 175, 0) == true)
                {
                    Click(750, 555);
                }
                await Task.Delay(delay);
            }
            
            //黑屏时等待
            while(await Waitcol(630,540,0,0,0,0))
            {
                await Task.Delay(delay);
            }

            Click(855, 555);

            await CoBossAssist(Node);
            await Task.Delay(delay);
            await CoPrevent(Node);

            return;
        }

        /// <summary>
        /// 直接放野主页Boss
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScPublicRaid(Nodes Node)
        {
            await CoBossPublic(Node);
            await CoBossAssist(Node);

            //通过等待确保弹窗出现
            await Task.Delay(2*delay);
            await CoPrevent(Node);
            await Task.Delay(delay);

            return;
        }

        /// <summary>
        /// 特命 - 目前直接退出
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScSpecial(Nodes Node)
        {
            await CoSpecialExit(Node);
            await Task.Delay(delay);

            return;
        }

        /// <summary>
        /// 出售花
        /// </summary>
        /// <param name="Node"></param>
        private async Task ScSell(Nodes Node)
        {
            await CoHomeTeam(Node);
            await CoTeamSell(Node);
            await CoSellAll(Node);
            await CoSellConfirm(Node);

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
        private async Task ScExplore(Nodes Node)
        {
            await CoHomeReturn(Node);
            await Task.Delay(delay);

            //查看是否有探索点，并探索
            if (await Waitcol(255,160,234,116,37,0) == true)
            {
                Click(275, 150);
                while (await Waitcol(350, 35, 209, 195, 147, 0) == false)
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
        private async Task ScGarden(Nodes Node)
        {
            await CoHomeReturn(Node);
            await Task.Delay(delay);

            //查看是否有花园虫，并收获
            if (await Waitcol(475, 135, 143, 0, 1, 0) == true)
            {
                Click(435, 150);
                while (await Waitcol(380, 615, 34, 34, 34, 0) == false)
                {
                    if (await Waitcol(375, 610, 234, 234, 234, 0) == true)
                    {
                        Click(380, 615);
                    }
                }
                await Task.Delay(delay);
                await CoHomeReturn(Node);
            }
            return;
        }

        /* Place Holder
         */

        
        //鼠标验色与点击事件 - Co系列
        /// <summary>
        /// 等待并关闭弹窗
        /// </summary>
        /// <param name="Node"></param>
        private async Task CoPrevent(Nodes Node)
        {
            await Waitcol(795, 205, 6, 90, 89);
            Click(805, 205);
        }

        /// <summary>
        /// 等待一定时间后取消弹窗
        /// </summary>
        /// <param name="Node"></param>
        private async Task CoCancle(Nodes Node, int multi = 1)
        {
            await Task.Delay(delay * multi);
            Click(550, 400);
        }

        /// <summary>
        /// 确认在主页并点击出击按钮
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoHomeDepart(Nodes Node)
        {
            await Waitcol(350, 35, 209, 195, 147);
            Click(80, 155);
        }

        /// <summary>
        /// 确认在主页并点击编队按钮
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoHomeTeam(Nodes Node)
        {
            await Waitcol(350, 35, 209, 195, 147);
            Click(85, 210);
        }

        /// <summary>
        /// 等待第一个可推图出现并点击
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoDepartFirst(Nodes Node)
        {
            await Waitcol(670, 255, 23, 23, 18);
            Click(430, 245);
        }

        /// <summary>
        /// 等待二号战友出现并选择
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="multi">等待时间delay的乘数</param>
        /// <returns></returns>
        private async Task CoAssistSecond(Nodes Node)
        {
            await Waitcol(750, 200, 228, 218, 178);
            Click(750, 250);
        }

        /// <summary>
        /// 等待出击按钮出现并开始推图
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoMisssionLaunch(Nodes Node)
        {
            await Waitcol(730, 200, 213, 185, 132);
            Click(800, 555);
        }

        /// <summary>
        /// 等待出现Boss并选择攻击Boss
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoBossStart(Nodes Node)
        {
            await Waitcol(290, 400, 249, 248, 241);
            Click(285, 400);
        }

        /// <summary>
        /// 等待出现Boss并选择放野Boss
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoBossPublic(Nodes Node)
        {
            await Waitcol(290, 400, 249, 248, 241);
            Click(650, 400);
        }

        /// <summary>
        /// 等待Boss列表加载完毕并攻击第一个
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoBossFirst(Nodes Node)
        {
            await Waitcol(840, 250, 87, 73, 52);
            Click(840, 250);
        }

        /// <summary>
        /// 等待攻击Boss页面出现并选择通常攻击
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoBossAttack(Nodes Node)
        {
            await Waitcol(550, 600, 227, 210, 175);
            Click(750, 555);
        }

        /// <summary>
        /// 等待申请援助选项出现并点击
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoBossAssist(Nodes Node)
        {
            await Waitcol(290, 400, 175, 74, 59);
            Click(290, 400);
        }

        /// <summary>
        /// 等待特命弹窗出现并选择取消
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoSpecialExit(Nodes Node)
        {
            await Waitcol(450, 400, 192, 89, 73);
            Click(550, 400);
        }

        /// <summary>
        /// 确认进入编成页面并点击出售
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoTeamSell(Nodes Node)
        {
            await Waitcol(201, 235, 130, 184, 201);
            Click(535, 137);
        }

        /// <summary>
        /// 确认进入出售页面并点击批量出售
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoSellAll(Nodes Node)
        {
            await Waitcol(206, 329, 80, 26, 17);
            Click(220, 295);
        }

        /// <summary>
        /// 确认进入批量出售页面并点击出售
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private async Task CoSellConfirm(Nodes Node)
        {
            await Waitcol(802, 530, 233, 216, 183);
            Click(420, 560);
        }

        /// <summary>
        /// 连续点击Home与无效位置来返回主页
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="multi"></param>
        /// <returns></returns>
        private async Task CoHomeReturn(Nodes Node)
        {
            while (await Waitcol(300, 140, 158, 123, 72, 0) == false)
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
        private async Task Cotemplate(Nodes Node)
        {
            await Waitcol(206, 329, 80, 26, 17);
            Click(220, 295);
        }
        */

    }
}

