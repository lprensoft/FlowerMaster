using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FlowerMaster.Helpers
{
    public sealed class Mouse
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern bool PostMessage(IntPtr WindowHandle, uint Msg, IntPtr wParam, IntPtr lParam);

        private static readonly Lazy<Mouse> lazy = new Lazy<Mouse>(() => new Mouse());
        public static Mouse Instance { get { return lazy.Value; } }

        private IntPtr WebHandle { get; set; }

        private Mouse()
        {
        }

        public void Load(IntPtr Handle)
        {
            WebHandle = Handle;
        }

        /// <summary>
        /// 鼠标点击
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        public void Click(int x, int y)
        {
            Random rnd = new Random();
            x = x + rnd.Next(-2, 3);
            y = y + rnd.Next(-2, 3);
            IntPtr lParam1 = (IntPtr)((y + 2 << 16) | x + 2); //坐标信息1
            IntPtr lParam2 = (IntPtr)((y - 2 << 16) | x - 2); //坐标信息1
            IntPtr wParam = IntPtr.Zero; // 附加的按键信息（如：Ctrl）
            const uint downCode = 0x201; // 鼠标左键按下
            const uint upCode = 0x202; // 鼠标左键抬起
            PostMessage(WebHandle, 0x0200, wParam, lParam2); // 随机移动鼠标
            PostMessage(WebHandle, downCode, wParam, lParam1); // 发送鼠标按键按下消息
            PostMessage(WebHandle, 0x0200, wParam, lParam1); // 随机移动鼠标
            PostMessage(WebHandle, upCode, wParam, lParam2); // 发送鼠标按键抬起消息
        }
    }
}
