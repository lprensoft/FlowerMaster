using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FlowerMaster.Models;
using FlowerMaster.Helpers;

namespace FlowerMaster
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataUtil.Config.LoadAccounts();
        }

        /// <summary>
        /// 显示保存的账号列表
        /// </summary>
        private void ShowSavedAccounts()
        {
            if (DataUtil.Config.accountList == null) return;
            DESHelper des = new DESHelper();
            cbUsername.Items.Clear();
            int cnt = 0;
            foreach (SysConfig.AccountList acc in DataUtil.Config.accountList)
            {
                if (acc.gameServer == cbGameServer.SelectedIndex)
                {
                    cbUsername.Items.Add(des.Decrypt(acc.username));
                    if (cnt == 0 || (DataUtil.Config.LastLoginAccount != "" && acc.username == DataUtil.Config.LastLoginAccount))
                    {
                        cbUsername.SelectedIndex = cnt;
                        tbPassword.Password = des.Decrypt(acc.password);
                    }
                    cnt++;
                }
            }
            if (cnt == 0) tbPassword.Password = "";
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (cbRemember.IsChecked.HasValue && (bool)cbRemember.IsChecked)
            {
                DataUtil.Config.SaveAccounts(cbUsername.Text, tbPassword.Password, cbGameServer.SelectedIndex);
            }

            DESHelper des = new DESHelper();
            DataUtil.Config.currentAccount.username = des.Encrypt(cbUsername.Text);
            DataUtil.Config.currentAccount.password = des.Encrypt(tbPassword.Password);
            DataUtil.Config.currentAccount.gameServer = cbGameServer.SelectedIndex;

            ((MainWindow)(Owner)).StartGame();
            Close();
        }

        private void cbGameServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ShowSavedAccounts();
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            cbGameServer.SelectedIndex = DataUtil.Config.LastLoginServer;
            ShowSavedAccounts();
        }

        private void cbUsername_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataUtil.Config.accountList == null || cbUsername.SelectedIndex < 0) return;
            DESHelper des = new DESHelper();
            foreach (SysConfig.AccountList acc in DataUtil.Config.accountList)
            {
                if (acc.gameServer == cbGameServer.SelectedIndex && des.Decrypt(acc.username) == cbUsername.Items[cbUsername.SelectedIndex].ToString())
                {
                    tbPassword.Password = des.Decrypt(acc.password);
                    break;
                }
            }
        }

        private void cbUsername_KeyDown(object sender, KeyEventArgs e)
        {
            tbPassword.Password = "";
        }

        private void lbDelete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (cbUsername.SelectedIndex < 0) return;
            if (MessageBox.Show("確定要刪除這個保存的帳號嗎?", "刪除確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DataUtil.Config.DeleteAccount(cbUsername.Text, cbGameServer.SelectedIndex);
                cbUsername.Items.RemoveAt(cbUsername.SelectedIndex);
                if (cbUsername.Items.Count > 0)
                {
                    cbUsername.SelectedIndex = 0;
                }
                else
                {
                    tbPassword.Password = "";
                }
            }
        }
    }
}
