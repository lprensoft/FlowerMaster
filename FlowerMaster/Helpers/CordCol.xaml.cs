using System;
using System.Runtime.InteropServices;

namespace FlowerMaster
{

    /// <summary>
    /// CordWindow.xaml 的交互逻辑
    /// </summary>
    public class CordCol
    {
        //大量API引用
        //[DllImport("user32.dll", SetLastError = true)]
        //static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        //[DllImport("user32.dll")]
        //static extern IntPtr WindowFromPoint(System.Drawing.Point p);
        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //internal static extern bool GetCursorPos(out Win32Point pt);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        //[DllImport("user32.dll", SetLastError = true)]
        //static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        /// 用于获取鼠标坐标的数据结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        /// <summary>
        /// 获取鼠标坐标
        /// </summary>
        /// <returns>当前鼠标坐标（绝对值）</returns>
        //public static System.Drawing.Point GetMousePosition()
        //{
        //    Win32Point w32Mouse = new Win32Point();
        //    GetCursorPos(out w32Mouse);
        //    return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
        //}

        /// <summary>
        /// 长方形数据结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }


        /// <summary>
        /// 获取最底层WebHandle用来抓取屏幕
        /// </summary>
        /// <param name="Top">顶层Handle</param>
        /// <returns></returns>
        //public static IntPtr GetWebHandle(IntPtr Top)
        //{
        //    StringBuilder className = new StringBuilder(100);

        //    do
        //    {
        //        GetClassName(Top, className, className.Capacity);
        //        Top = GetWindow(Top, 2);
        //    } while (className.ToString() != "Chrome_WidgetWin_0");

        //    return Top;
        //}

        /// <summary>
        /// 获取坐标像素颜色
        /// </summary>
        /// <param name="hwnd">Handle（需要使用最底层）</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns></returns>
        static public System.Drawing.Color GetPixelColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            System.Drawing.Color color = System.Drawing.Color.FromArgb(
                (Byte)(pixel),
                (Byte)(pixel >> 8),
                (Byte)(pixel >> 16));
            ReleaseDC(IntPtr.Zero, hdc);
            return color;
        }
        
        /// <summary>
        /// 获取当前鼠标坐标
        /// </summary>
        public CordCol()
        {
            //System.Drawing.Point Pointy = GetMousePosition();
            //GetWindowRect(WebHandle, out RECT lprect);
            
            //Pointy.X = Pointy.X - lprect.Left;
            //Pointy.Y = Pointy.Y - lprect.Top;
            
        }

        //public System.Drawing.Point Pointy { get; }
        //public System.Drawing.Color Color { get; }

    }
}
