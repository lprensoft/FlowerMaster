using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace FlowerMaster
{
    /// <summary>
    /// CordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CordWindow
    {
        //API引用
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out CordCol.RECT lpRect);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Win32Point pt);
        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

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
            GetCursorPos(out Win32Point w32Mouse);
            return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
        }

        /// <summary>
        /// 获取当前鼠标坐标与像素并输出到窗口
        /// </summary>
        /// <param name="ActHand">底层Handle（像素）</param>
        private void GetCord(IntPtr ActHand)
        {
            System.Drawing.Point Pointy = GetMousePosition();

            GetWindowRect(ActHand, out CordCol.RECT lprect);

            int a;
            int b;
            a = Pointy.X - lprect.Left;
            b = Pointy.Y - lprect.Top;

            System.Drawing.Color colorout = CordCol.GetPixelColor(Pointy.X, Pointy.Y);

            try
            {
                Dispatcher.Invoke(() =>
                {
                    int.TryParse(XBox.Text, out int Xin);
                    int.TryParse(YBox.Text, out int Yin);
                    System.Drawing.Point point = Helpers.MiscHelper.main.mainWeb.PointToScreen(new System.Drawing.Point(0, 0));
                    System.Drawing.Color colorin = CordCol.GetPixelColor(point.X + Xin, point.Y + Yin);
                    text1.Text = a + ", " + b;
                    text2.Text = colorout.R.ToString() + ", " + colorout.G.ToString() + ", " + colorout.B.ToString();
                    Output.Text = colorin.R.ToString() + ", " + colorin.G.ToString() + ", " + colorin.B.ToString();
                });
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 启动坐标与颜色窗口
        /// </summary>
        /// <param name="TopHandle">整个助理Handle</param>
        public CordWindow(IntPtr TopHandle)
        {
            InitializeComponent();
            IntPtr Handle = TopHandle;

            System.Timers.Timer aTimer = new System.Timers.Timer(100);

            XBox.Text = "0";
            YBox.Text = "0";

            aTimer.Elapsed += (s, e) => GetCord(TopHandle);
            aTimer.Enabled = true;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            Helpers.MiscHelper.main.isOpenCordWindow = false;
        }

        private void btn_Mouse_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(text1.Text + ", " + text2.Text);
        }

        private void btn_Input_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(XBox.Text + ", " + YBox.Text + ", " + Output.Text);
        }

        private void XBox_GotFocus(object sender, RoutedEventArgs e)
        {
            XBox.SelectAll();
        }

        private void YBox_GotFocus(object sender, RoutedEventArgs e)
        {
            YBox.SelectAll();
        }
    }
}
