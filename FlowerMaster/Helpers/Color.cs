﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
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
        
        private IntPtr WebHandle { get; set; }

        private Color()
        {
        }

        public void Load(IntPtr Handle)
        {
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
        public bool Check(int X, int Y, int Red, int Green, int Blue)
        {

            //判定颜色是否在容错范围内
            System.Drawing.Color color = CordCol.GetPixelColor(WebHandle, X, Y);
            if (color.R - 5 <= Red &&
                color.R + 5 >= Red &&
                color.G - 5 <= Green &&
                color.G + 5 >= Green &&
                color.B - 5 <= Blue &&
                color.B + 5 >= Blue)
            {
                return true;
            }
            
            else
            {
                return false;
            }
        }
    }
}
