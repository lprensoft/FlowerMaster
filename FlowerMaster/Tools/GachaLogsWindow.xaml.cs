using FlowerMaster.Models;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FlowerMaster
{
    /// <summary>
    /// GachaLogsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GachaLogsWindow
    {
        public GachaLogsWindow()
        {
            InitializeComponent();
        }

        private void LoadAccounts()
        {
            if (!Directory.Exists("log")) return;
            DirectoryInfo logs = new DirectoryInfo("log");
            foreach (FileInfo file in logs.GetFiles("*.txt"))
            {
                if (file.Name.IndexOf("扭蛋记录") == -1) continue;
                string files = file.Name.Substring(0, file.Name.Length - 4);
                string[] logInfo = files.Split('_');
                if (logInfo.Count() < 3) continue;
                string account = logInfo[0] + "-" + logInfo[1];
                for (int i = 2; i < logInfo.Count() - 1; i++)
                {
                    account += "_" + logInfo[i];
                }
                bool found = false;
                for (int i = 0; i < cbAccount.Items.Count; i++)
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
        }

        private void LoadLog(string account)
        {
            string[] info = account.Split('-');
            string file = @"log\" + info[0] + "_" + info[1];
            for (int i = 2; i < info.Count(); i++)
            {
                file += "-" + info[i];
            }
            file += "_扭蛋记录.txt";
            if (File.Exists(file))
            {
                rtLog.Document.Blocks.Clear();
                using (StreamReader sr = new StreamReader(file))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        string[] txt = line.Split('|');
                        if (txt.Count() != 2)
                        {
                            line = sr.ReadLine();
                            continue;
                        }
                        Paragraph p = new Paragraph();
                        Run timeText = new Run() { Text = txt[0] + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                        p.Inlines.Add(timeText);
                        Run logText = new Run() { Text = "扭蛋获得：" };
                        p.Inlines.Add(logText);
                        if (txt[1].IndexOf(",") != -1)
                        {
                            string[] cards = txt[1].Split(',');
                            if (chkFilter.IsChecked.HasValue && (bool)chkFilter.IsChecked && cards.Count() != 11)
                            {
                                line = sr.ReadLine();
                                continue;
                            }
                            for (int i=0; i<cards.Count(); i++)
                            {
                                string cardStr = DataUtil.Cards.GetName(int.Parse(cards[i]));
                                Color color = Colors.White;
                                if (cardStr.IndexOf("★1") != -1)
                                {
                                    color = Colors.LightSteelBlue;
                                }
                                else if (cardStr.IndexOf("★2") != -1)
                                {
                                    color = Colors.Aquamarine;
                                }
                                else if (cardStr.IndexOf("★3") != -1)
                                {
                                    color = Colors.Chocolate;
                                }
                                else if (cardStr.IndexOf("★4") != -1)
                                {
                                    color = Colors.Silver;
                                }
                                else if (cardStr.IndexOf("★5") != -1)
                                {
                                    color = Colors.Gold;
                                }
                                else if (cardStr.IndexOf("★6") != -1)
                                {
                                    color = Colors.Violet;
                                }
                                Run cardText = new Run() { Text = cardStr, Foreground = new SolidColorBrush(color) };
                                p.Inlines.Add(cardText);
                                if (i != cards.Count() - 1)
                                {
                                    Run Text = new Run() { Text = "、" };
                                    p.Inlines.Add(Text);
                                }
                            }
                        }
                        else
                        {
                            string cardStr = DataUtil.Cards.GetName(int.Parse(txt[1]));
                            Color color = Colors.White;
                            if (cardStr.IndexOf("★1") != -1)
                            {
                                color = Colors.LightSteelBlue;
                            }
                            else if (cardStr.IndexOf("★2") != -1)
                            {
                                color = Colors.Aquamarine;
                            }
                            else if (cardStr.IndexOf("★3") != -1)
                            {
                                color = Colors.Chocolate;
                            }
                            else if (cardStr.IndexOf("★4") != -1)
                            {
                                color = Colors.Silver;
                            }
                            else if (cardStr.IndexOf("★5") != -1)
                            {
                                color = Colors.Gold;
                            }
                            else if (cardStr.IndexOf("★6") != -1)
                            {
                                color = Colors.Violet;
                            }
                            Run cardText = new Run() { Text = cardStr, Foreground = new SolidColorBrush(color) };
                            p.Inlines.Add(cardText);
                        }
                        p.LineHeight = 3;
                        rtLog.Document.Blocks.Add(p);
                        line = sr.ReadLine();
                    }
                }
            }
        }

        private void cbAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAccount.SelectedIndex < 0) return;
            LoadLog(cbAccount.Items[cbAccount.SelectedIndex].ToString());
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAccounts();
        }

        private void chkFilter_Click(object sender, RoutedEventArgs e)
        {
            if (cbAccount.SelectedIndex < 0) return;
            LoadLog(cbAccount.Items[cbAccount.SelectedIndex].ToString());
        }
    }
}
