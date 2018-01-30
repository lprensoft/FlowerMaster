using FlowerMaster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace FlowerMaster
{

    /// <summary>
    /// CordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CordWindow
    {


        //大量API引用
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Win32Point pt);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);
        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref System.Drawing.Point lpPoint);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

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
        public static System.Drawing.Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(out w32Mouse);
            return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
        }

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
        /// 获取坐标像素颜色
        /// </summary>
        /// <param name="hwnd">Handle（需要使用最底层）</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>返还RGB颜色</returns>
        static public System.Drawing.Color GetPixelColor(IntPtr hwnd, int x, int y)
        {
            IntPtr hdc = GetDC(hwnd);
            uint pixel = GetPixel(hdc, x, y);
            System.Drawing.Color color = System.Drawing.Color.FromArgb(
                (Byte)(pixel),
                (Byte)(pixel >> 8),
                (Byte)(pixel >> 16));
            ReleaseDC(hwnd, hdc);
            return color;
        }
        
        /// <summary>
        /// 获取当前鼠标坐标与像素并输出到窗口
        /// </summary>
        /// <param name="Handle">表层Handle（坐标）</param>
        /// <param name="Hand">底层Handle（像素）</param>
        private void GetCord(IntPtr Handle,IntPtr ActHand)
        {
            System.Drawing.Point Pointy = GetMousePosition();
            
            GetWindowRect(ActHand, out RECT lprect);


            int a;
            int b;
            a = Pointy.X - lprect.Left;
            b = Pointy.Y - lprect.Top;

            IntPtr c = ActHand;

            System.Drawing.Color colorout = GetPixelColor(ActHand, Pointy.X - lprect.Left, Pointy.Y - lprect.Top);

            Dispatcher.Invoke(() =>
            {
                int Xin = Int32.Parse(XBox.Text);
                int Yin = Int32.Parse(YBox.Text);
                System.Drawing.Color colorin = GetPixelColor(ActHand, Xin, Yin);
                text1.Text = "( " + a + ", " + b + ")";
                text2.Text = colorout.R.ToString() + " " + colorout.G.ToString() + " " + colorout.B.ToString();
                Output.Text = colorin.R.ToString() + " " + colorin.G.ToString() + " " + colorin.B.ToString();
            });
        }


        /// <summary>
        /// 启动坐标与颜色窗口
        /// </summary>
        /// <param name="Handle">浏览器Handle</param>
        /// <param name="WHandle">整个助理Handle</param>
        public CordWindow(IntPtr Handle, IntPtr WHandle)
        {
            InitializeComponent();
            IntPtr ActHand = IntPtr.Zero;

            ActHand = FindWindowEx(WHandle, IntPtr.Zero, "Shell Embedding", null);
            ActHand = FindWindowEx(ActHand, IntPtr.Zero, "Shell DocObject View", null);
            ActHand = FindWindowEx(ActHand, IntPtr.Zero, "Internet Explorer_Server", null);


            System.Timers.Timer aTimer = new System.Timers.Timer(100);

            XBox.Text = "0";
            YBox.Text = "0";

            aTimer.Elapsed += (s, e) => GetCord(Handle, ActHand);
            aTimer.Enabled = true;
        }
    }
}
