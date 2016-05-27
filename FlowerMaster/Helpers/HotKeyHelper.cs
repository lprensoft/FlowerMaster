using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace FlowerMaster.Helpers
{
    class HotKeyHelper
    {
        //如果函数执行成功，返回值不为0。
        //如果函数执行失败，返回值为0。要得到扩展错误信息，调用GetLastError。
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(
            IntPtr hWnd,                //要定义热键的窗口的句柄
            int id,                     //定义热键ID（不能与其它ID重复）
            KeyModifiers fsModifiers,   //标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效
            Keys vk                     //定义热键的内容
        );
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(
            IntPtr hWnd,                //要取消热键的窗口的句柄
            int id                      //要取消热键的ID
        );
        //定义了辅助键的名称（将数字转变为字符以便于记忆，也可去除此枚举而直接使用数值）
        [Flags()]
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            WindowsKey = 8
        }
        public static int hotKeyId = 319;
        public static bool isRegistered = false;
        private static bool isInstalled = false;
        private static Window main = null;

        /// <summary>
        /// 安装热键处理挂钩
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns>
        ///     <c>true</c>：安装成功<br/>
        ///     <c>false</c>：安装失败
        /// </returns>
        /// <value>消息源</value>
        /// <remarks></remarks>
        public static bool InstallHotKeyHook(Window window)
        {
            //判断是否已经安装了挂钩
            if (isInstalled) return true;
            //判断组件是否有效
            if (null == window)
            {
                //如果无效，则直接返回
                return false;
            }
            //获得窗体的句柄
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(window);
            //判断窗体句柄是否有效
            if (IntPtr.Zero == helper.Handle)
            {
                //如果句柄无效，则直接返回
                return false;
            }
            //获得消息源
            System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
            //判断消息源是否有效
            if (null == source)
            {
                //如果消息源无效，则直接返回
                return false;
            }
            //挂接事件
            source.AddHook(HotKeyHook);
            //写入局部变量
            main = window;
            //返回安装成功
            isInstalled = true;
            return true;
        }
        /// <summary>
        /// 热键处理过程
        /// </summary>
        /// <param name="hwnd">触发消息的窗口句柄</param>
        /// <param name="msg">要被处理的消息编号</param>
        /// <param name="wParam">消息参数</param>
        /// <param name="lParam">消息参数</param>
        /// <param name="handled">消息是否被处理</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //判断是否为热键消息
            if (msg == WM_HOTKEY)
            {
                //检查热键ID是否为本程序
                if (wParam.ToInt32() == hotKeyId && main != null)
                {
                    if (main.Visibility == Visibility.Hidden)
                    {
                        main.Show();
                    }
                    else
                    {
                        main.Hide();
                    }
                }
            }
            return IntPtr.Zero;
        }
        /// <summary>
        /// 热键消息编号
        /// </summary> 
        private const int WM_HOTKEY = 0x0312;
    }
}
