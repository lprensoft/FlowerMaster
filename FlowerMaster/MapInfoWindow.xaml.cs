using FlowerMaster.Models;
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
    /// MapInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MapInfoWindow
    {
        public MapInfoWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataUtil.Game.bossList == null) return;
            dgBossList.ItemsSource = DataUtil.Game.bossList;
        }
    }
}
