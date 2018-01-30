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
        public static int status = 0; //　0:停止脚本　1:重启脚本


        /// <summary>
        /// 开始脚本
        /// </summary>
        /// <param name="Node_o">特殊数据结构Node（见上）</param>
        public async Task Start(Nodes Node_o)
        {
            Nodes Node = new Nodes(Node_o.Choice, Node_o.Hand);

            Webhandle = Node.Hand.BotHand;

            status = 1;
            while (status != 0)
            {
                await Select(Node);
                await Task.Delay(delay);
                await Depart(Node);
                await Task.Delay(delay);
                await Combat(Node);
            }
            
            return;
        }

        /// <summary>
        /// 防止并关闭弹窗
        /// </summary>
        /// <param name="Node"></param>
        private async void Prevent(Nodes Node)
        {
            if (await Waitcol(795, 205, 6, 90, 89, 0))
            {
                Node.Click(805, 205);
            }
        }

        /// <summary>
        /// 取消弹窗
        /// </summary>
        /// <param name="Node"></param>
        private async void Cancle(Nodes Node, int multi = 1)
        {
            await Task.Delay(delay * multi);
            Click(550, 400);
        }


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
            IntPtr lParam1 = (IntPtr)((y+2 << 16) | x+2); //坐标信息1
            IntPtr lParam2 = (IntPtr)((y-2 << 16) | x-2); //坐标信息1
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
        private async Task<bool> Waitcol(int x, int y, byte red = 0, byte green = 0, byte blue = 0, int delay = 300)
        {
            Random rnd = new Random();
            await Task.Delay(delay +1);
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

        /// <summary>
        /// 选择关卡，并恢复体力
        /// </summary>
        /// <param name="Node"></param>
        private async Task Select(Nodes Node)
        {

            await Waitcol(350, 35, 209, 195, 147, 2*delay);
            
            Click(85, 160);

            await Waitcol(180, 230, 142, 113, 83);


            if (Node.Choice == 0)
                Click(300, 140);

            else if (Node.Choice == 1)
                Click(400, 140);


            await Waitcol(180, 400, 146, 122, 96);

            if (Choice == 0)
                Click(300, 260);

            else if (Choice == 1)
                Click(300, 330);

            await Waitcol(180, 400, 146, 122, 96);

            Click(430, 245);

            await Task.Delay((4 * delay) - 1);

            if (await Waitcol(320, 320, 176, 31, 69, 0))
            {
                await Refill(Node);
                await Waitcol(670, 235, 76, 66, 25);
                Click(430, 245);
            }
            
            return;
        }



        
        /// <summary>
        /// 恢复体力
        /// </summary>
        /// <param name="Node"></param>
        private async Task Refill(Nodes Node)
        {
            if (await Waitcol(320, 320, 176, 31, 69, 0))
            {
                Click(300, 400);
                if (await Waitcol(180, 450, 104, 88, 72))
                {
                    if (await Waitcol(341, 323, 255, 1, 1, 0))
                    {
                        Click(410, 400);
                        await Waitcol(795, 205, 6, 90, 89);
                        Click(805, 205);
                        return;
                    }
                    else
                        Click(500, 400);
                }

                await Waitcol(180, 230, 142, 113, 83);

                Click(430, 245);

                await Waitcol(320, 320, 176, 31, 69);

                Click(650, 400);
                if (await Waitcol(180, 450, 104, 88, 72))
                {
                    if (await Waitcol(341, 323, 255, 1, 1, 0))
                    {
                        Click(410, 400);
                        await Waitcol(795, 205, 6, 90, 89);
                        Click(805, 205);
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
        private async Task Depart(Nodes Node)
        {

            await Waitcol(750, 200, 228,218,178);

            Click(750, 250);

            await Waitcol(730, 200, 213, 185, 132);

            Click(800, 555);
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
        private async Task Combat(Nodes Node, int wait = 0)
        {
            await Task.Delay(wait);

            Click(855, 545);
            await Task.Delay(delay);

            if (await Waitcol(795, 205, 6, 90, 89, 0))
            {
                Click(795, 205);
                await Combat(Node);
            }

            else if (await Waitcol(680, 190, 76, 64, 47, 0))
            {
                Click(550, 430);
                await Combat(Node);
            }

            else if (await Waitcol(350, 35, 209, 195, 147, 0))
            {
                await Rcombat(Node);
                return;
            }

            else if (await Waitcol(290, 400, 249, 248, 241, 0))
            {
                if (Choice == 0)
                {
                    await Enraid(Node);
                    return;
                }
                else if (Choice == 1)
                {
                    await Flraid(Node);
                    return;
                }
            }

            else if (await Waitcol(450, 400, 192, 89, 73, 0))
            {
                await Special(Node);
                return;
            }
            
            else
            {
                await Task.Delay(delay);
                Click(855, 545);
                await Combat(Node);
                return;
            }

        }


        /// <summary>
        /// 进入主页Boss战并放野
        /// </summary>
        /// <param name="Node"></param>
        private async Task Enraid(Nodes Node)
        {
            Click(285, 400);

            await Waitcol(840, 250, 87, 73, 52);

            Click(840, 250);

            await Waitcol(730, 200, 213, 185, 132);

            Click(750, 555);

            await Waitcol(550, 600, 227, 210, 175);

            Click(750, 555);

            Cancle(Node, 5);

            await Waitcol(630, 540);

            while(await Waitcol(630,540,0,0,0,0))
            {
                await Task.Delay(delay);
            }

            Click(855, 555);

            await Waitcol(290, 400, 175, 74, 59);

            Click(290, 400);


            await Rcombat(Node);
            return;
        }

        /// <summary>
        /// 直接放野主页Boss
        /// </summary>
        /// <param name="Node"></param>
        private async Task Flraid(Nodes Node)
        {
            Click(650, 400);

            await Waitcol(290, 400, 175, 74, 59);

            Click(290, 400);

            await Task.Delay(3 * delay);

            Click(805, 205);

            await Rcombat(Node);
            return;
        }

        /// <summary>
        /// 特命 - 目前直接退出
        /// </summary>
        /// <param name="Node"></param>
        private async Task Special(Nodes Node)
        {

            Click(550,400);

            await Rcombat(Node);
            return;
        }

        /// <summary>
        /// 出售花
        /// </summary>
        /// <param name="Node"></param>
        private async Task Sell(Nodes Node)
        {
            Click(85, 210);

            await Waitcol(201, 235, 130, 184, 201);

            Click(535, 137);

            await Waitcol(206, 329, 80, 26, 17);

            Click(220, 295);

            await Waitcol(802, 530, 233, 216, 183);

            Click(420, 560);

            await Task.Delay(delay);

            Click(810, 65);

            await Task.Delay(delay * 3);

            return;
        }

        /// <summary>
        /// 重新开始推图
        /// </summary>
        /// <param name="Node"></param>
        private async Task Rcombat(Nodes Node)
        {
            await Waitcol(350, 35, 209, 195, 147);

            await Sell(Node);

            return;
        }


        
    }


}

