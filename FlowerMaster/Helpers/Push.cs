using FlowerMaster.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading;

namespace FlowerMaster.Helpers
{

    public class Nodes
    {
        //自动推兔2.0函数

        //private IntPtr WebHandle = IntPtr.Zero;
        private int delay = 1000;
        private bool sblock = false;

        //核心功能


        private readonly Color Col = Color.Instance;
        private readonly Mouse Mou = Mouse.Instance;
        private readonly Counter PushTimes = Counter.Instance;

        public void ScInitialize()
        {
        }
        
        
        //Main
        /// <summary>
        /// 开始脚本与初始化数据
        /// </summary>
        public void Start()
        {
            Random rnd = new Random();
            while (PushTimes.Value() > 0 && DataUtil.Config.sysConfig.autoType == 0)
            { 
                if (DataUtil.Config.sysConfig.actionPrep == true && Col.Check(522, 51, 101, 42, 1) == false)
                {
                    ScActionPrep();
                }

                //随即延迟
                delay = rnd.Next(DataUtil.Config.sysConfig.delayTime, DataUtil.Config.sysConfig.delayTime * 2);
                sblock = DataUtil.Config.sysConfig.specialBlock;

                while (ScSelect() == false) { Thread.Sleep(delay); }
                ScDepart();
                while (ScCombat() == false) { Thread.Sleep(delay); }

                //CoHomeReturn();

                //if (Col.Check(625, 70, 243, 212, 0) == true &&
                //    DataUtil.Config.sysConfig.raidOther == true) //待處理
                //{
                //    CoHomeReturn();
                //    ScGranRaid();
                //}

                if (DataUtil.Config.sysConfig.sellTrue == true)
                {
                    //CoHomeReturn();
                    ScSell(); 
                }

                if (PushTimes.Value() == 1)
                {
                    CoHomeReturn(); 
                }

                PushTimes.Decrease();

                if (DataUtil.Config.sysConfig.exploreTrue == true)
                {
                    CoHomeReturn();
                    if (Col.Check(313, 141, 172, 172, 171) == false)
                    {
                        ScExplore();
                    } 
                }

                if (DataUtil.Config.sysConfig.gardenTrue == true)
                {
                    CoHomeReturn();
                    if (DataUtil.Game.player.plantTime < DataUtil.Game.serverTime)
                    {
                        ScGarden();
                    }
                }
            }

            while (PushTimes.Value() > 0 &&
                   DataUtil.Config.sysConfig.autoType == 1)
            {

                //随机延迟
                delay = rnd.Next(DataUtil.Config.sysConfig.delayTime, DataUtil.Config.sysConfig.delayTime * 2);
                Thread.Sleep(delay);

                if (Col.Check(625, 70, 243, 212, 0) == true &&
                    DataUtil.Config.sysConfig.raidOther == true)
                {
                    CoHomeReturn();
                    ScGranRaid();
                }

                if (DataUtil.Config.sysConfig.exploreTrue == true)
                {
                    CoHomeReturn();
                    if (Col.Check(258, 163, 99, 99, 99) == false &&
                       (Col.Check(520, 75, 50, 41, 37) == true ||
                        Col.Check(520, 77, 83, 81, 76) == true))
                    {
                        ScExplore();
                    }
                }

                if (DataUtil.Config.sysConfig.gardenTrue == true)
                {
                    CoHomeReturn();
                    if (DataUtil.Game.player.plantTime < DataUtil.Game.serverTime)
                    {
                        ScGarden();
                    }
                }

                CoHomeReturn();
                Thread.Sleep(delay);
            }

            return;
        }

        //脚本事件 - Sc系列
        /// <summary>
        /// 选择关卡，并恢复体力
        /// </summary>
        private bool ScSelect()
        {
            CoHomeDepart();

            //等待出击页面加载结束
            WaMainLoad();
            if (Col.Check(500, 80, 146, 118, 93) == false) return false;

            //根据选择点击出击页面

            //进入主线推兔页面 TODO: 待處理
            //if (DataUtil.Config.sysConfig.pushType == 0)
            //{
            //    while (Col.Check(280, 440, 20, 2, 2) == false)
            //    {
            //        Mou.Click(300, 140);
            //        Thread.Sleep(delay);
            //    }
            //    Mou.Click(280, 440);
            //    return true;
            //}

            //进入活动推兔页面
            if (DataUtil.Config.sysConfig.pushType == 1)
            {
                Mou.Click(330, 200);
                Thread.Sleep(500);
                Mou.Click(300, 160);
                return true;
            }

            ////进入水影推兔页面
            //if (DataUtil.Config.sysConfig.pushType == 2)
            //{
            //    while (Col.Check(620, 275, 249, 247, 240) == false)
            //    {
            //        Mou.Click(590, 140);
            //        Thread.Sleep(delay);
            //    }
            //    while (Col.Check(620, 275, 249, 247, 240) == true)
            //    {
            //        Mou.Click(600, 250);
            //        Thread.Sleep(delay);
            //    }
            //}

            //進入上一次推的圖
            if (DataUtil.Config.sysConfig.pushType == 2)
            {
                Mou.Click(475, sblock ? 400 : 300);

                return ScPreEnterDepart();
            }

            //進入下一張圖   TODO: 目前進度
            if (DataUtil.Config.sysConfig.pushType == 3)
            {
                int y = sblock ? 100 : 0;
                //檢查是否獲得三勳章
                if (DataUtil.Config.sysConfig.autoReStart &&
                    (Col.Check(659, 330 + y, 238, 54, 12) == false ||
                     Col.Check(683, 330 + y, 238, 54, 12) == false ||
                     Col.Check(707, 330 + y, 238, 54, 12) == false))
                { 
                    Mou.Click(550, 320 + y);
                }
                else
                {
                    //檢查是否有下一關的按鈕
                    if (Col.Check(650, 260 + y, 246, 246, 239) == true)
                    {
                        Mou.Click(650, 260 + y);
                    }
                    else
                    {
                        PushTimes.Reset();
                        System.Windows.Forms.MessageBox.Show("該任務已無關卡可推，停止推兔");
                        return false;
                    }
                }

                return ScPreEnterDepart();
            }

            return false;
        }

        /// <summary>
        /// 開始推兔前的體力&好友畫面檢測
        /// </summary>
        /// <returns></returns>
        private bool ScPreEnterDepart() 
        {
            //确认体力页面是否或者推兔页面是否出现
            while (true)
            {
                //判断体力恢复是否出现  
                if (Col.Check(360, 260, 235, 58, 132) == true && Col.Check(760, 260, 255, 92, 17) == true)
                {
                    //如果碎石失败，返回主页面
                    if (ScRefill() == false) return false;
                    WaMainLoad();
                    return false;
                }

                //判断队友选择是否出现
                if (Col.Check(260, 80, 5, 58, 33) == true || Col.Check(1080, 80, 92, 22, 12) == true)
                {
                    return true;
                }

                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 恢复体力脚本
        /// </summary>
        private bool ScRefill()
        {
            //如果不喝药+碎石，则退出碎石页面
            if (DataUtil.Config.sysConfig.potionTrue == false && DataUtil.Config.sysConfig.stoneTrue == false)
            {
                Mou.Click(570, 480);
                return false;
            }

            //喝药水
            if (DataUtil.Config.sysConfig.potionTrue == true)
            {
                //确认是否有药水喝
                if (Col.Check(350, 400, 234, 132, 116) == true)
                {
                    Thread.Sleep(delay);
                    Mou.Click(350, 400);

                    WaConfirmWindow();
                    //确定喝药回覆視窗
                    if (Col.Check(420, 430, 190, 88, 73) == true)
                    {
                        Mou.Click(420, 430);                        
                        CoPrevent();
                        return true;
                    }
                }
                else if (DataUtil.Config.sysConfig.stoneTrue == false)
                {
                    Mou.Click(570, 480);
                    return false;
                }
            }

            //碎石头
            if (DataUtil.Config.sysConfig.stoneTrue == true)
            {
                if (Col.Check(785, 400, 238, 136, 120) == true)
                {
                    Mou.Click(785, 400);
                    Thread.Sleep(delay);
                }

                //确定碎石红字出现
                if (Col.Check(425, 485, 76, 24, 16) == true)
                {
                    Mou.Click(425, 485);
                    CoPrevent();
                    return true;
                }
                //红字没出现，没石头，点击退出
                else
                {
                    Mou.Click(725, 485);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 开始推兔
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private void ScDepart()
        {
            WaMainLoad();
            CoAssistSelect();
            CoMisssionLaunch();

            //等待成功进图
            WaMainLoad();
            
            return;
        }

        /// <summary>
        /// 推兔过程中判定
        /// </summary>
        private bool ScCombat()
        {
            //如果出現劇情，直接略過
            if (Col.Check(565, 625, 53, 33, 1))
            {
                Mou.Click(565, 625);
                Thread.Sleep(delay * 2);
                Mou.Click(420, 420); 

                return false;
            }

            //如果出现弹窗，关闭并继续推兔
            //if (Col.Check(795, 205, 6, 90, 89) == true)
            //{
            //    Mou.Click(805, 205);
            //    return false;
            //}

            //同上
            //if (Col.Check(580, 400, 24, 154, 149) == true)
            //{
            //    Mou.Click(580, 400);
            //    return false;
            //}

            //如果出现加战友，取消并继续推兔
            else if (Col.Check(580, 275, 23, 23, 22) == true)
            {
                Mou.Click(730, 420);
                return false;
            }

            //如果出现Boss，根据选择启动函数
            if (Col.Check(315, 425, 230, 230, 223) == true &&
                     Col.Check(825, 425, 122, 105, 75) == true)
            {
                if (DataUtil.Config.sysConfig.raidSelf == true)
                {
                    CoBossStart();
                    ScAttackRaid();
                    return true;
                }
                else
                {
                    ScPublicRaid();
                    return true;
                }
            }

            //如果出现特命，根据选择启动函数
            else if (Col.Check(420, 420, 188, 82, 66) == true && Col.Check(720, 420, 3, 1, 1) == true)
            {
                ScSpecial();
                return true;
            }

            //如果出现眷属的自动Boss弹窗，关闭并判定推图结束
            //else if (Col.Check(340, 390, 253, 156, 142) == true &&
            //         Col.Check(460, 390, 253, 155, 141) == true &&
            //         Col.Check(510, 390, 101, 218, 215) == true)
            //{
            //    Mou.Click(550, 400);
            //    return true;
            //}

            //如果在出現三徽章獎勵，關閉視窗並結束
            else if (Col.Check(345, 200, 76, 54, 52) == true)
            {
                Mou.Click(345, 200);
                return true;
            }

            //如果在主页，返还
            else if (Col.Check(600, 80, 146, 118, 93) == true)
            {
                return true;
            }

            //如果无事件，继续推兔
            else
            {
                Mou.Click(1030, 550);
                Thread.Sleep(delay);
                return false;
            }
        }
        
        /// <summary>
        /// 点击Raid按钮，确定目前是眷属战或者普通Raid，并根据结果继续
        /// </summary>
        private void ScGranRaid()
        {
            Mou.Click(355, 160);
            while (Col.Check(500, 300, 119, 82, 69) == false &&
                   Col.Check(200, 170, 212, 184, 131) == false)
            { Thread.Sleep(delay); }

            if (Col.Check(500, 300, 119, 82, 69) == true)
            {
                ScGranBoss();
            }
            else ScAttackRaid();
        }

        /// <summary>
        /// 进入主页Boss战并放野
        /// </summary>
        /// <param name="Node"></param>
        private void ScAttackRaid()
        {
            //如果没Boss，退出Boss页面
            while (Col.Check(200, 170, 212, 184, 131) == false) { Thread.Sleep(delay); }
            while (Col.Check(340, 260, 207, 170, 110) == true)
            {
                Thread.Sleep(delay);
                WaMainLoad();

                //循环确认三个Boss，如果没有还没打的Boss的话，回主页
                for (int i = 0; i < 3; i++)
                {
                    if ((Col.Check(870, 260 + 148*i, 80, 26, 17) == false &&
                         Col.Check(510, 122 + 148*i, 249, 248, 240) == false))
                    {
                        CoHomeReturn();
                        return;
                    }
                }
            }

            CoBossEnter();
            CoMisssionLaunch();
            while (Col.Check(550, 600, 227, 210, 175) == false) { Thread.Sleep(delay); }
            CoBossAttack();

            //判定是否出现无Boss点碎石页面
            while (true)
            {
                //无Boss点 要求碎石
                if (Col.Check(397, 400, 255, 1, 1) == true)
                {
                    //取消碎石
                    Mou.Click(550, 460);
                    while (Col.Check(255, 135, 43, 24, 0) == false) { Thread.Sleep(delay); }
                    //请求援助
                    Mou.Click(220, 130);
                    CoBossAssist();
                    //等待并取消弹窗
                    CoPrevent();

                    return;
                }
                //有Boss点 成功进入
                if (Col.Check(630, 540, 0, 0, 0) == true) { break; }
                Thread.Sleep(100);
            }

            //等到能快时快进
            while (Col.Check(630, 540, 0, 0, 0) == true) { Thread.Sleep(delay); }
            CoBossSkip();

            //判定请求支援是否出现并采取措施
            while (true) 
            {
                if (Col.Check(290, 400, 175, 74, 59) == true &&
                    Col.Check(300, 400, 186, 84, 68) == true)
                {
                    CoBossAssist();
                    CoPrevent();
                    return;
                }
                if (Col.Check(5, 634, 71, 61, 21) == true)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 直接放野主页Boss
        /// </summary>
        /// <param name="Node"></param>
        private void ScPublicRaid()
        {
            CoBossPublic();
            //等待并取消弹窗
            CoPrevent();

            return;
        }

        /// <summary>
        /// 眷属战Boss时特用脚本，使用全部Boss点攻击眷属Boss
        /// </summary>
        private void ScGranBoss()
        {
            CoGranEnter();
            CoMisssionLaunch();
            CoGranStart();

            //眷属战结束前不停的点击跳过
            while (Col.Check(780, 85, 2, 85, 86) == false)
            {
                Mou.Click(910, 430);
                Thread.Sleep(delay);
            }

            Mou.Click(780, 85);

            return;
        }

        /// <summary>
        /// 特命脚本 包含进战过程
        /// </summary>
        private void ScSpecial()
        {
            //判定是否进入特命
            if (DataUtil.Config.sysConfig.specialTrue == true)
            {
                //确认开始特命副本
                Mou.Click(420, 420);

                WaMainLoad();

                //在還沒進入選擇關卡前一直重複判定是否有哪些關卡可以進入
                while (Col.Check(360, 130, 250, 246, 243) == false)
                {
                    //根据可以进入的图点击进图
                    if (Col.Check(300, 140, 51, 31, 0) == true)
                    {
                        Mou.Click(300, 140);
                    }

                    if (Col.Check(300, 240, 51, 31, 0) == true)
                    {
                        Mou.Click(300, 240);
                    }

                    if (Col.Check(300, 340, 51, 31, 0) == true)
                    {
                        Mou.Click(300, 340);
                    }
                }

                CoDepartFirst();
                Thread.Sleep(delay);

                if (Col.Check(360, 260, 235, 58, 132) == true && Col.Check(760, 260, 255, 92, 17) == true)
                {
                    //如果碎石失败，返回主页面
                    if (ScRefill() == false) return;

                    CoDepartFirst();
                }

                ScDepart();

                while (ScCombat() == false) { Thread.Sleep(delay); }
                return;
            }
            else
            {
                Mou.Click(730, 420);
            }
        }

        /// <summary>
        /// 出售花
        /// </summary>
        /// <param name="Node"></param>
        private void ScSell()
        {
            CoHomeTeam();
            CoTeamSell();

            do
            {
                CoSellAll();
                Thread.Sleep(2000);

                Mou.Click(485, 570);
                Thread.Sleep(delay);
            } while (Col.Check(485, 570, 112, 112, 112) == false && Col.Check(250, 150, 218, 116, 101) == true);

            Thread.Sleep(delay);
            Mou.Click(660, 570);
        }

        /// <summary>
        /// 确认是否有探索点并探索一次
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private void ScExplore()
        {
            //开始探索
            Mou.Click(313, 141);
            while (Col.Check(800, 420, 74, 73, 138) == false &&
                   Col.Check(920, 420, 231, 183, 223) == false) { Thread.Sleep(delay); } //進度
            //等到探索页面出现后开始连点
            while (Col.Check(18, 5, 149, 210, 149) == false)
            {
                Mou.Click(800, 200);
                Thread.Sleep(100);
            }
            return;
        }

        /// <summary>
        /// 确认是否有花园虫并捕获
        /// </summary>
        private void ScGarden()
        {
            while (Col.Check(510, 405, 149, 113, 100) == false)
            {
                //查看是否有花园虫，并收获
                Mou.Click(95, 560);
                Thread.Sleep(delay);
            }

            if (Col.Check(360, 380, 49, 15, 5) == true)
            {
                Mou.Click(510, 405);
                Thread.Sleep(delay);
            }

            //返回主页之前，关闭任何弹窗+点击返回主页
            while (Col.Check(660, 600, 122, 106, 88) == true)
            {
                Mou.Click(660, 600);
            }
            
            while (Col.Check(18, 5, 149, 210, 149) == false)
            {
                if (Col.Check(580, 500, 98, 89, 67) == true)
                {
                    Mou.Click(580, 500);
                }

                Mou.Click(90, 50);
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 提前恢复体力
        /// </summary>
        private void ScActionPrep()
        {
            WaMainLoad();
            Mou.Click(525, 40);
            while (Col.Check(300, 380, 202, 165, 144) == false) { Thread.Sleep(delay); }
            Mou.Click(300, 380);
            while (Col.Check(320, 320, 176, 31, 69) == false) { Thread.Sleep(delay); }
            ScRefill();
        }

        /* Place Holder
         */


        //鼠标验色与点击事件 - Co系列

        /// <summary>
        /// 等到弹窗出现之后，在回到主页面或画面恢复正常之前并且弹窗消失前，不停等待并关闭弹窗
        /// </summary>
        private void CoPrevent()
        {
            while (Col.Check(520, 180, 154, 30, 28) == false || Col.Check(995, 190, 249, 248, 241) == false)
            {
                Thread.Sleep(delay);
            }

            while (Col.Check(995, 190, 249, 248, 241) == true)
            {
                Mou.Click(995, 190);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 確認是否已在討伐頁面，若否則點一下進入討伐頁面
        /// </summary>
        /// <returns></returns>
        private void CoHomeDepart()
        {
            if (Col.Check(105, 125, 234, 251, 255) == false)
            {
                if (Col.Check(995, 190, 249, 248, 241) == true)
                {
                    Mou.Click(995, 190);
                    Thread.Sleep(delay);
                }

                Mou.Click(105, 125);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 等待下方黑条载入结束后 确认在主页并点击编队按钮
        /// </summary>
        /// <returns></returns>
        private void CoHomeTeam()
        {
            WaMainLoad();

            while (Col.Check(470, 100, 252, 245, 240) == false)
            {
                Mou.Click(105, 185);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 等待第一个可推兔出现并点击
        /// </summary>
        /// <returns></returns>
        private void CoDepartFirst()
        {
            while (Col.Check(360, 130, 250, 246, 243) == false) { Thread.Sleep(delay); }

            Mou.Click(360, 130);
            Thread.Sleep(delay);
        }

        /// <summary>
        /// 战友选择成功之前不停地点击二号战友位
        /// </summary>
        /// <returns></returns>
        private void CoAssistSelect()
        {
            while (Col.Check(245, 150, 158, 147, 90) == false || 
                (Col.Check(660, 150, 217, 205, 168) == false && Col.Check(660, 150, 206, 188, 115) == false))
            {
                Thread.Sleep(delay);
            }

            Mou.Click(700, 150);
        }

        /// <summary>
        /// 在准备页面或者维护窗口出现时，不停的点击出击按钮并确定是否弹出维护窗
        /// </summary>
        /// <returns></returns>
        private void CoMisssionLaunch()
        {
            while (Col.Check(870, 550, 64, 20, 12) == false)
            {
                Thread.Sleep(delay);
            }

            Mou.Click(870, 550);
        }

        /// <summary>
        /// 等待出现Boss并选择攻击Boss
        /// </summary>
        /// <returns></returns>
        private void CoBossStart()
        {
            while (Col.Check(575, 425, 82, 82, 82) == true)
            {
                Mou.Click(325, 420);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 等待出现Boss并选择放野Boss
        /// </summary>
        /// <returns></returns>
        private void CoBossPublic()
        {
            while (Col.Check(800, 420, 74, 66, 41) == true)
            {
                Mou.Click(800, 420);
                Thread.Sleep(delay);
            }

            while (Col.Check(375, 420, 246, 246, 239) == true)
            {
                Mou.Click(375, 420);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 等待Boss列表加载完毕并攻击第1/2/3个。
        /// 在进入准备页面之前不停地点击Boss的出击按钮。
        /// 每个Boss间距148
        /// </summary>
        /// <returns></returns>
        private void CoBossEnter()
        {
            //进入Boss页面之前开始等待
            while (Col.Check(870, 260, 80, 26, 17) == false) { Thread.Sleep(delay); }

            //确认第3个Boss没有在【参战中】
            if (Col.Check(510, 270, 249, 248, 240) == true)
            {
                //进入队友选择页面之前选择第1个Boss
                while (Col.Check(922, 229, 205, 167, 111) == false)
                {
                    Mou.Click(840, 250);
                    Thread.Sleep(delay);
                } 
            }

            //确认第2个Boss没有在【参战中】
            else if (Col.Check(510, 418, 249, 248, 240) == true)
            {
                //进入队友选择页面之前选择第2个Boss
                while (Col.Check(922, 229, 205, 167, 111) == false)
                {
                    Mou.Click(840, 398);
                    Thread.Sleep(delay);
                }
            }

            //前两个Boss都在参战中，直接进入第3个Boss
            else
            {
                //进入队友选择页面之前选择第3个Boss
                while (Col.Check(922, 229, 205, 167, 111) == false)
                {
                    Mou.Click(840, 546);
                    Thread.Sleep(delay);
                }
            }
        }

        /// <summary>
        /// 在进入下一步之前不停地选择普通攻击Boss。因延迟关系使用绝对值延迟100。
        /// 并同时确定是否弹出维护窗
        /// </summary>
        /// <returns></returns>
        private void CoBossAttack()
        {
            while (Col.Check(397, 400, 255, 1, 1) == false &&
                   Col.Check(630, 540, 0, 0, 0) == false)
            {
                Mou.Click(750, 555);
                CoMaintainConfirm();
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 等待申请援助选项出现并点击
        /// </summary>
        /// <returns></returns>
        private void CoBossAssist()
        {
            WaMainLoad();

            while (Col.Check(380, 425, 106, 106, 99) == false ||
                   Col.Check(760, 425, 254, 254, 247) == false) { Thread.Sleep(delay); }

            Mou.Click(380, 425);
        }

        /// <summary>
        /// 在眷属战页面，进入队伍确认之前不停地点击出击按钮
        /// </summary>
        private void CoGranEnter()
        {
            while (Col.Check(425, 200, 213, 185, 133) == false)
            {
                Mou.Click(750, 480);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 等待弹窗出现，选择最大Boss点消耗并开始
        /// </summary>
        private void CoGranStart()
        {
            while (Col.Check(425, 200, 154, 32, 18) == false) { Thread.Sleep(delay); }
            while (Col.Check(590, 310, 138, 138, 138) == false)
            {
                Mou.Click(590, 310);
                Thread.Sleep(delay);
            }
            Mou.Click(400, 500);
            return;
        }

        /// <summary>
        /// 确认进入编成页面并点击出售
        /// </summary>
        /// <returns></returns>
        private void CoTeamSell()
        {
            while (Col.Check(545, 350, 254, 249, 246) == false) { Thread.Sleep(delay); }
            Mou.Click(545, 350);
        }

        /// <summary>
        /// 确认进入出售页面并不停地点击出售
        /// 直到出售页面出现为止
        /// </summary>
        /// <returns></returns>
        private void CoSellAll()
        {
            if (Col.Check(485, 40, 154, 30, 28) == false)
            {
                Mou.Click(250, 180);
                WaMainLoad();
                Mou.Click(650, 490);
            }
        }

        /// <summary>
        /// 确认进入批量出售页面并点击出售
        /// </summary>
        /// <returns></returns>
        private void CoSellConfirm()
        {
            while (Col.Check(802, 530, 233, 216, 183) == false) { Thread.Sleep(delay); }
            Mou.Click(420, 560);
        }

        /// <summary>
        /// 连续点击Home与无效位置来返回主页
        /// </summary>
        /// <returns></returns>
        private void CoHomeReturn()
        {
            while (Col.Check(20, 5, 140, 205, 137) == false)
            {
                Mou.Click(85, 45);
                Thread.Sleep(delay / 2);
                Mou.Click(5, 5);
                Thread.Sleep(delay / 2);
            }
        }

        /// <summary>
        /// 快进主页Boss
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private void CoBossSkip()
        {
            while (Col.Check(290, 400, 175, 74, 59) == false &&
                   Col.Check(5, 634, 71, 61, 21) == false &&
                   Col.Check(290, 400, 175, 74, 59) == false)
            {
                Mou.Click(830, 550);
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// 处理在每日维护之前的20分钟出击时出现的弹窗
        /// </summary>
        private void CoMaintainConfirm()
        {
            if(DataUtil.Game.serverTime.Hour == 3 &&
               DataUtil.Game.serverTime.Minute >= 39 &&
               DataUtil.Game.serverTime.Second >= 59)
            {
                //延迟一秒等弹窗出现
                Thread.Sleep(1000);
                while (Col.Check(550, 330, 72, 55, 50) == true)
                {
                    Mou.Click(410, 400);
                    Thread.Sleep(delay);
                }
            }
        }


        //等待页面出现系列 - Wa

        /// <summary>
        /// 等待在主页时，下方黑条加载结束
        /// </summary>
        private void WaMainLoad()
        {
            while (Col.Check(1010, 610, 131, 131, 136) == true) { Thread.Sleep(delay); }
        }

        /// <summary>
        /// 等待体力恢复确认框出现
        /// </summary>
        private void WaConfirmWindow()
        {
            while (Col.Check(420, 430, 190, 88, 73) == false) { Thread.Sleep(delay); }
        }
    }
}

