using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FlowerMaster
{
    /// <summary>
    /// GameLogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameLogsWindow
    {
        public GameLogsWindow()
        {
            InitializeComponent();
        }

        private void LoadAccounts()
        {
            if (!Directory.Exists("log")) return;
            DirectoryInfo logs = new DirectoryInfo("log");
            foreach (FileInfo file in logs.GetFiles("*.txt"))
            {
                if (file.Name.IndexOf("游戏日志") == -1) continue;
                string files = file.Name.Substring(0, file.Name.Length - 4);
                string[] logInfo = files.Split('_');
                if (logInfo.Count() < 4) continue;
                string account = logInfo[0] + "-" + logInfo[1];
                for (int i=2; i<logInfo.Count()-2; i++)
                {
                    account += "_" + logInfo[i];
                }
                bool found = false;
                for (int i=0; i<cbAccount.Items.Count; i++)
                {
                    if (account == cbAccount.Items[i].ToString())
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    cbAccount.Items.Add(account);
                }
            }
            if (cbAccount.Items.Count > 0) cbAccount.SelectedIndex = 0;
        }

        private void LoadDates(string account)
        {
            cbDate.Items.Clear();
            string[] info = account.Split('-');
            string file = info[0] + "_" + info[1];
            for (int i=2; i<info.Count(); i++)
            {
                file += "-" + info[i];
            }
            file += "_游戏日志_";
            DirectoryInfo logs = new DirectoryInfo("log");
            foreach (FileInfo log in logs.GetFiles("*.txt"))
            {
                if (log.Name.IndexOf(file) == 0)
                {
                    string fileName = log.Name.Substring(0, log.Name.Length - 4);
                    string[] date = fileName.Split('_');
                    cbDate.Items.Add(date[date.Count() - 1]);
                }
            }
        }

        private void LoadLogs(string account, string date)
        {
            string[] info = account.Split('-');
            string file = @"log\" + info[0] + "_" + info[1];
            for (int i = 2; i < info.Count(); i++)
            {
                file += "-" + info[i];
            }
            file += "_游戏日志_" + date + ".txt";
            if (File.Exists(file))
            {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                using (fs)
                {
                    TextRange text = new TextRange(rtLog.Document.ContentStart, rtLog.Document.ContentEnd);
                    text.Load(fs, DataFormats.Text);
                }
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAccounts();
        }

        private void cbAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAccount.SelectedIndex < 0) return;
            LoadDates(cbAccount.Items[cbAccount.SelectedIndex].ToString());
        }

        private void cbDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDate.SelectedIndex < 0) return;
            LoadLogs(cbAccount.Items[cbAccount.SelectedIndex].ToString(), cbDate.Items[cbDate.SelectedIndex].ToString());
        }
    }
}
