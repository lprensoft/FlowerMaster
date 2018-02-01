using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using FlowerMaster.Push;
using System.Drawing;

namespace FlowerMaster.Helpers
{
    public sealed class Color
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        private static readonly Lazy<Color> lazy = new Lazy<Color>(() => new Color());
        public static Color Instance { get { return lazy.Value; } }

        private int Delay { get; set; }
        private IntPtr WebHandle { get; set; }

        private Color()
        {
        }

        public void Load(int delay, IntPtr Handle)
        {
            Delay = delay;
            WebHandle = Handle;
        }

        /// <summary>
        /// 配合While实现等待颜色出现，可用TorF辨别为判断颜色是否存在
        /// </summary>
        /// <param name="WebHandle"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Red"></param>
        /// <param name="Green"></param>
        /// <param name="Blue"></param>
        /// <param name="TorF">False为等待颜色出现，True为判断颜色存在（无影响，主要用途为可读性）</param>
        /// <returns></returns>
        public async Task<bool> Check(int X, int Y, int Red, int Green, int Blue, bool TorF = false)
        {
            //方法内带延迟这样外面就不用带async delay了
            await Task.Delay(Delay/2);

            //判定颜色是否在容错范围内
            System.Drawing.Color color = CordCol.GetPixelColor(WebHandle, X, Y);
            if (color.R - 2 <= Red &&
                color.R + 2 >= Red &&
                color.G - 2 <= Green &&
                color.G + 2 >= Green &&
                color.B - 2 <= Blue &&
                color.B + 2 >= Blue)
            {
                await Task.Delay(Delay/2);
                return true;
            }

            //如果延迟为0，将方法当做颜色true/false来用
            else if (TorF == true)
            {
                await Task.Delay(Delay/2);
                return false;
            }
            
            else
            {
                await Task.Delay(Delay/2);
                return false;
            }
        }
    }
}
