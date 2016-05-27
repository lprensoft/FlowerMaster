using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FlowerMaster
{
    /// <summary>
    /// GachaWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GachaWindow
    {
        public GachaWindow()
        {
            InitializeComponent();
        }

        private Random gacha;
        private int stone;
        private int fRare6 = 199;
        private short rare6 = 0;
        private short rare5 = 0;
        private short rare4 = 0;
        private short rare3 = 0;
        private int gachaCount = 0;

        private void DoGacha(int cnt)
        {
            stone -= cnt == 1 ? 5 : 50;
            Paragraph p = new Paragraph();
            Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
            Run head = new Run() { Text = cnt == 1 ? "进行了一次单抽，获得：" : "进行了一次11连，获得：" };
            p.Inlines.Add(timeText);
            p.Inlines.Add(head);
            p.LineHeight = 3;

            for (int i=0; i<cnt; i++)
            {
                int r = (gachaCount > 0 && gachaCount % 200 == fRare6) ? 0 : gacha.Next(200);
                if (r == 0)
                {
                    rare6++;
                    Run g = new Run() { Text = "★6 ", Foreground = new SolidColorBrush(Colors.Violet) };
                    p.Inlines.Add(g);
                }
                else if (r <= 12)
                {
                    rare5++;
                    Run g = new Run() { Text = "★5 ", Foreground = new SolidColorBrush(Colors.Gold) };
                    p.Inlines.Add(g);
                }
                else if (r <= 72)
                {
                    rare4++;
                    Run g = new Run() { Text = "★4 ", Foreground = new SolidColorBrush(Colors.Silver) };
                    p.Inlines.Add(g);
                }
                else
                {
                    rare3++;
                    Run g = new Run() { Text = "★3 ", Foreground = new SolidColorBrush(Colors.Chocolate) };
                    p.Inlines.Add(g);
                }
                gachaCount++;
            }

            gachaLog.Document.Blocks.Add(p);
            gachaLog.ScrollToEnd();
            lbResult.Content = string.Format("总扭蛋次数：{0}，★6={1}({2}%)，★5={3}({4}%)，★4={5}({6}%)，★3={7}({8}%)",
                gachaCount, 
                rare6, Math.Round((double)rare6 / gachaCount * 100, 2), rare5, Math.Round((double)rare5 / gachaCount * 100, 2),
                rare4, Math.Round((double)rare4 / gachaCount * 100, 2), rare3, Math.Round((double)rare3 / gachaCount * 100, 2));
            lbStone.Content = "华灵石：" + stone.ToString();
        }

        private async void btnSingle_Click(object sender, RoutedEventArgs e)
        {
            if (stone < 5)
            {
                await this.ShowMessageAsync("提示", "你的华灵石不足了哦！");
            }
            else
            {
                DoGacha(1);
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            gacha = new Random();
            fRare6 = gacha.Next(200);
            stone = 1000;
            lbStone.Content = "华灵石：" + stone.ToString();
        }

        private async void btnMulti_Click(object sender, RoutedEventArgs e)
        {
            if (stone < 50)
            {
                await this.ShowMessageAsync("提示", "你的华灵石不足了哦！");
            }
            else
            {
                DoGacha(11);
            }
        }
    }
}
