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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Windows.Forms; // NotifyIcon control
using System.Xml;

namespace ColoClock
{
    delegate void ClockElapsedTask_Handle();

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //Timer
        ClockTimer clockTimer;

        //Collection
        List<TimerTask> list_tasks = new List<TimerTask>();
        TimerTask chosenTask = null;

        //UI
        BitmapImage imgres_setting = new BitmapImage(new Uri(".\\Resource\\setting.png", UriKind.Relative));
        BitmapImage imgres_clock = new BitmapImage(new Uri(".\\Resource\\Clock.png", UriKind.Relative));
        BitmapImage imgres_cutdown = new BitmapImage(new Uri(".\\Resource\\CutdownTimer.png", UriKind.Relative));
        BitmapImage imgres_icon = new BitmapImage(new Uri(".\\Resource\\icon.ico", UriKind.Relative));
        BitmapImage imgres_bgchange = new BitmapImage(new Uri(".\\Resource\\bg_change.png", UriKind.Relative));
        System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
        string now_bg_path = "";

        MediaPlayer MusicPlayer = new MediaPlayer();

        //Pro
        TimerTaskKind gsetting_Chosen_Mode = TimerTaskKind.CLOCK;
        TimerTaskKind setting_Chosen_Mode
        {
            get
            {
                return gsetting_Chosen_Mode;
            }
            set
            {
                gsetting_Chosen_Mode = value;
                switch (gsetting_Chosen_Mode)
                {
                    case TimerTaskKind.CLOCK:
                        {
                            btn_setting_ClockChosen.BorderBrush = new SolidColorBrush(Colors.White);
                            btn_setting_CutdownChosen.BorderBrush = new SolidColorBrush(Colors.Transparent);
                            break;
                        }
                    case TimerTaskKind.CUTDOWNTIMER:
                        {
                            btn_setting_CutdownChosen.BorderBrush = new SolidColorBrush(Colors.White);
                            btn_setting_ClockChosen.BorderBrush = new SolidColorBrush(Colors.Transparent);
                            break;
                        }
                }
            }
        }



        static MainWindow()
        { 
        }

        public MainWindow()
        {
            InitializeComponent();
            UpdateXaml();
        }

        private void Win_Main_Loaded(object sender, RoutedEventArgs e)
        {
            this.Icon = imgres_icon;
            clockTimer = new ClockTimer();
            clockTimer.Add_Elapsed_Event(this,()=>ContrlUpdate(sender));

            initWinGui();
            ToolBarIcon_init();
        }

        private void ToolBarIcon_init()
        {
            this.notifyIcon.BalloonTipText = "ColoClock  为你计时";
            this.notifyIcon.Text = "ColoClock";
            this.notifyIcon.Icon = new System.Drawing.Icon(".\\Resource\\icon.ico");
            this.notifyIcon.Visible = true;

            //右键菜单--打开
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("Show");
            open.Click += new EventHandler((s, e) =>
            {
                this.ShowInTaskbar = true;
                this.WindowState = WindowState.Normal;
            });

            //右键菜单--最小化
            System.Windows.Forms.MenuItem miniSize = new System.Windows.Forms.MenuItem("MiniSize");
            miniSize.Click += new EventHandler((s, e) =>
            {
                this.ShowInTaskbar = false;
                this.Hide();
            });

            //右键菜单--退出
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Exit");
            exit.Click += new EventHandler((s, e) => this.Close());

            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, miniSize, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            notifyIcon.MouseDoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Minimized;
                this.WindowState = WindowState.Normal;
            };
            this.notifyIcon.ShowBalloonTip(1000);
        }

        private void initWinGui()
        {
            win_ChangeBackground();

            grid_Tasks.Children.Remove(grid_Task_Exmaple);
            grid_Task_Add.Margin = new Thickness(0, 0, 0, 0);

            LoadTasks();
            setting_Chosen_Mode = TimerTaskKind.CLOCK;

            (btn_setting_ClockChosen.Background as ImageBrush).ImageSource = imgres_clock;
            (btn_setting_CutdownChosen.Background as ImageBrush).ImageSource = imgres_cutdown;
            (btn_bg_change.Background as ImageBrush).ImageSource = imgres_bgchange;
        }

        private void win_ChangeBackground()
        {
            Random rand = new Random();

            string path = ".\\Resource\\bg";
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            string im_path = "";
            int trycount = 5;
            do
            {
                im_path = ".\\Resource\\bg\\" + files[rand.Next(files.Length)].Name;
                BitmapImage imgres_bg = new BitmapImage(new Uri(im_path, UriKind.Relative));
                (grid_bg.Background as ImageBrush).ImageSource = imgres_bg;
                trycount--;
            }while (trycount > 0 && im_path == now_bg_path);
            now_bg_path = im_path;
        }

        private void SaveTasks()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("Data.xml"))
                {
                    writer.WriteLine("<Datas>");
                    writer.WriteLine("</Datas>");
                }
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("Data.xml");
                XmlNode root = xmlDoc.SelectSingleNode("Datas");

                foreach (TimerTask task in list_tasks)
                {
                    XmlElement e = task.saveAsXml(xmlDoc, root as XmlElement);
                    root.AppendChild(e);
                }
                xmlDoc.Save("Data.xml");
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists("Data.xml"))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load("Data.xml");
                    XmlNode root = xmlDoc.SelectSingleNode("Datas");

                    foreach (XmlElement el in root.ChildNodes)
                    {
                        switch (el.Name)
                        {
                            case "CLOCK":
                                {
                                    ClockTask clockTimer = new ClockTask();
                                    clockTimer.loadFromXml(el);
                                    Add_Task(TimerTaskKind.CLOCK, clockTimer);
                                    break;
                                }
                            case "CUTDOWN":
                                {
                                    CutDownTask cutDownTask = new CutDownTask();
                                    cutDownTask.loadFromXml(el);
                                    Add_Task(TimerTaskKind.CUTDOWNTIMER, cutDownTask);
                                    break;
                                }
                        }
                    }
                    xmlDoc.Save("Data.xml");
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("文件加载异常，加载未成功");
                throw;
            }
        }

        private void ContrlUpdate(object sender)
        {
            lab_Main_Time.Content = clockTimer.Now_Time.ToString("HH : mm : ss");
            lab_Main_Date.Content = clockTimer.Now_Time.ToString("yyyy年MM月dd日");
        }

        private void Grid_Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Add_Task(TimerTaskKind kind, TimerTask task)
        {
            Grid grid = null;
            switch (kind)
            {
                case TimerTaskKind.CLOCK:
                    {
                        ClockTask clockTask = task as ClockTask;
                        grid = clockTask.init_GUI();
                        clockTimer.Add_Elapsed_Event(this, () => clockTask.datatimeUpdate());
                        clockTask.onAttachAimTime += ClockTask_AttachAimTime_Win;
                        clockTask.Enabled = true;
                        list_tasks.Add(clockTask);
                        break;
                    }
                case TimerTaskKind.CUTDOWNTIMER:
                    {
                        CutDownTask cutDownTask = task as CutDownTask;
                        grid = cutDownTask.init_GUI();
                        clockTimer.Add_Elapsed_Event(this, () => cutDownTask.datatimeUpdate());
                        cutDownTask.onAttachAimTime += CutDownTimerTask_AttachAimTime_Win;
                        cutDownTask.Enabled = false;
                        list_tasks.Add(cutDownTask);
                        break;
                    }
            }

            grid.MouseLeftButtonDown += (o, e) =>
            {
                chosenTask = task;
                if (e.ClickCount == 2)
                {
                    task.Enabled = false;
                    LoadToTaskSetting(task, kind);
                    ShowSettingGrid();
                }
            };
            grid.Margin = new Thickness(0, grid.Height * (grid_Tasks.Children.Count - 1), 0, 0);
            grid.MouseWheel += Dock_Tasks_MouseWheel;
            grid_Tasks.Children.Add(grid);
            grid_Task_Add.Margin = new Thickness(0, grid.Height * (grid_Tasks.Children.Count - 1), 0, 0);
        }

        private void Add_Task(TimerTaskKind kind,string TaskName, TimeSpan aimTime)
        {
            switch (kind)
            {
                case TimerTaskKind.CLOCK:
                    {
                        ClockTask clockTask = new ClockTask(aimTime,TaskName);
                        Add_Task(kind, clockTask);
                        break;
                    }
                case TimerTaskKind.CUTDOWNTIMER:
                    {
                        CutDownTask cutDownTask = new CutDownTask(aimTime, TaskName);
                        Add_Task(kind, cutDownTask);
                        break;
                    }
            }
        }

        private void ClockTask_AttachAimTime_Win(object task)
        {
            ClockTask clockTask = task as ClockTask;
            this.notifyIcon.ShowBalloonTip(10, clockTask.TaskName,clockTask.AimTime.ToString(), ToolTipIcon.None);
            try
            {
                lock (MusicPlayer)
                {
                    MusicPlayer.Open(new Uri(@".\\Resource\BGM.flac", UriKind.Relative));
                    MusicPlayer.Play();
                    System.Timers.Timer timer = new System.Timers.Timer(60 * 1000);
                    timer.Elapsed += (o, e) =>
                    {
                        this.Dispatcher.Invoke(() => MusicPlayer.Stop());
                        timer.Dispose();
                    };
                    timer.AutoReset = false;
                    timer.Start();
                }
            }
            catch
            {

            }
        }

        private void CutDownTimerTask_AttachAimTime_Win(object task)
        {
            CutDownTask cutkTask = task as CutDownTask;
            this.notifyIcon.ShowBalloonTip(10, cutkTask.TaskName,"Time Up!!!", ToolTipIcon.None);
        }

        private void UpdateXaml()
        {
            File.WriteAllText(@".\Resource\TimerTaskGrid.xaml", System.Windows.Markup.XamlWriter.Save(grid_Task_Exmaple));
        }

        private void Btn_showTasks_Click(object sender, RoutedEventArgs e)
        {
            if (dock_Tasks.Width >= 290)
            {
                RunAnimation(dock_Tasks, 300, 0, WidthProperty, action: () => { btn_showTasks.IsEnabled = true; });
                dock_Tasks.Width = 0;
                btn_showTasks.Content = "<";
            }
            else
            {
                RunAnimation(dock_Tasks, 0, 300, WidthProperty, action: () => { btn_showTasks.IsEnabled = true; });

                dock_Tasks.Width = 300;
                btn_showTasks.Content = ">";
            }
        }
        
        private void RunAnimation(UIElement ui,double From,double To,DependencyProperty property,double Duration = 0.3,Action action = null)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.Duration = TimeSpan.FromSeconds(Duration);
            if(action != null)
            {
                animation.Completed += (w, q) => { action(); };
            }
            animation.AccelerationRatio = 0.7;
            animation.From = From;
            animation.To = To;
            dock_Tasks.BeginAnimation(property, animation);
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_MiniSize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.WindowState = WindowState.Minimized;
            }
        }

        private void Btn_Mode_MaxSzie_Click(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = true;
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void Grid_Task_Add_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid_Task_Add.Background.Opacity = 0.9;
        }

        private void Grid_Task_Add_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid_Task_Add.Background.Opacity = 0;
        }

        private void Btn_Task_Add_Click(object sender, RoutedEventArgs e)
        {
            chosenTask = null;
            txt_setting_TaskName.Text = "";
            txt_setting_Time_Hour.Text = "00";
            txt_setting_Time_Minute.Text = "00";
            txt_setting_Time_Second.Text = "00";
            ShowSettingGrid();
        }

        private void ShowSettingGrid()
        {
            grid_Task_Settings.Width = dock_Tasks.ActualWidth;
            grid_Task_Settings.Visibility = Visibility.Visible;
            RunAnimation(grid_Task_Settings, 0, 300, WidthProperty,
                action: () =>
                {
                    grid_Task_Settings.Width = 300;
                });
        }

        private void Txt_setting_Time_Hour_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int Val = int.Parse(txt_setting_Time_Hour.Text);
                if (txt_setting_Time_Hour.Text.Trim(' ').Equals("") || Val >= 24 || Val < 0)
                {
                    throw new Exception();
                }
                else
                {
                    txt_setting_Time_Hour.Foreground = new SolidColorBrush(Colors.White);
                    txt_setting_Time_Hour.BorderBrush = new SolidColorBrush(Colors.White);
                }
            }
            catch
            {
                txt_setting_Time_Hour.Foreground = new SolidColorBrush(Colors.Red);
                txt_setting_Time_Hour.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private void Txt_setting_Time_Minute_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int Val = int.Parse(txt_setting_Time_Minute.Text);
                if (txt_setting_Time_Hour.Text.Trim(' ').Equals("") || Val >= 60 || Val < 0)
                {
                    throw new Exception();
                }
                else
                {
                    txt_setting_Time_Minute.Foreground = new SolidColorBrush(Colors.White);
                    txt_setting_Time_Minute.BorderBrush = new SolidColorBrush(Colors.White);
                }
            }
            catch
            {
                txt_setting_Time_Minute.Foreground = new SolidColorBrush(Colors.Red);
                txt_setting_Time_Minute.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private void Txt_setting_Time_Second_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int Val = int.Parse(txt_setting_Time_Second.Text);
                if (txt_setting_Time_Hour.Text.Trim(' ').Equals("") || Val >= 60 || Val < 0)
                {
                    throw new Exception();
                }
                else
                {
                    txt_setting_Time_Second.Foreground = new SolidColorBrush(Colors.White);
                    txt_setting_Time_Second.BorderBrush = new SolidColorBrush(Colors.White);
                }
            }
            catch
            {
                txt_setting_Time_Second.Foreground = new SolidColorBrush(Colors.Red);
                txt_setting_Time_Second.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private void Btn_setting_ClockChosen_Click(object sender, RoutedEventArgs e)
        {
            if (chosenTask == null)
                setting_Chosen_Mode = TimerTaskKind.CLOCK;
        }

        private void Btn_setting_CutdownChosen_Click(object sender, RoutedEventArgs e)
        {
            if (chosenTask == null)
                setting_Chosen_Mode = TimerTaskKind.CUTDOWNTIMER;
        }

        private void Txt_setting_Confirm_Click(object sender, RoutedEventArgs e)
        {
            txt_setting_Error.Visibility = Visibility.Hidden;
            if ((txt_setting_Time_Hour.Foreground as SolidColorBrush).Color == Colors.Red ||
                (txt_setting_Time_Minute.Foreground as SolidColorBrush).Color == Colors.Red ||
                (txt_setting_Time_Second.Foreground as SolidColorBrush).Color == Colors.Red
                )
            {
                txt_setting_Error.Text = "时间格式错误";
                txt_setting_Error.Visibility = Visibility.Visible;
                return;
            }


            TimeSpan span = new TimeSpan(
                int.Parse(txt_setting_Time_Hour.Text),
                int.Parse(txt_setting_Time_Minute.Text),
                int.Parse(txt_setting_Time_Second.Text)
                );

            if (chosenTask == null)
            {
                Add_Task(setting_Chosen_Mode, txt_setting_TaskName.Text, span);
            }
            else
            {
                LoadFromTaskSetting(chosenTask, setting_Chosen_Mode);
            }
            HideSettingGrid();
        }

        private void Txt_setting_Cannel_Click(object sender, RoutedEventArgs e)
        {
            grid_Task_Settings.Visibility = Visibility.Hidden;
            RunAnimation(grid_Tasks, 0, 300, WidthProperty,
                action: () =>
                {
                    grid_Task_Settings.Width = 0;
                });
        }

        private void Btn_setting_remove_Click(object sender, RoutedEventArgs e)
        {
            if (chosenTask != null)
            {
                grid_Tasks.Children.Remove(chosenTask.bindGrid);
                list_tasks.Remove(chosenTask);

                for (int i = 0; i < list_tasks.Count; i++)
                {
                    Grid grid = list_tasks[i].bindGrid;
                    grid.Margin = new Thickness(0, grid.Height * (grid_Tasks.Children.Count - 2), 0, 0);
                }
                grid_Task_Add.Margin = new Thickness(0, chosenTask.bindGrid.Height * (grid_Tasks.Children.Count - 1), 0, 0);
                HideSettingGrid();
            }
        }

        private void HideSettingGrid()
        {
            grid_Task_Settings.Visibility = Visibility.Hidden;
            RunAnimation(grid_Tasks, 0, 300, WidthProperty,
                action: () =>
                {
                    grid_Task_Settings.Width = 0;
                });
        }

        private void Dock_Tasks_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Thickness t = grid_Tasks.Margin;
            if (e.Delta > 0)
            {
                if (t.Top <= -30)
                    t.Top = t.Top + 30;
            }
            else
            {
                if (t.Top >= -(75 * grid_Tasks.Children.Count - 75 ) + this.ActualHeight -105)
                    t.Top = t.Top -30;
            }
            grid_Tasks.Margin = t;
        }

        private void Win_Main_Closed(object sender, EventArgs e)
        {
            SaveTasks();
            MusicPlayer.Stop();
        }

        public void LoadToTaskSetting(TimerTask task,TimerTaskKind kind)
        {
            string name = "";
            TimeSpan t = TimeSpan.Zero;
            switch(kind)
            {
                case TimerTaskKind.CLOCK:
                    {
                        ClockTask c = task as ClockTask;
                        t = c.AimTime;
                        name = c.TaskName;
                        setting_Chosen_Mode = TimerTaskKind.CLOCK;
                        break;
                    }
                case TimerTaskKind.CUTDOWNTIMER:
                    {
                        CutDownTask c = task as CutDownTask;
                        t = c.FromTime;
                        name = c.TaskName;
                        setting_Chosen_Mode = TimerTaskKind.CUTDOWNTIMER;
                        break;
                    }
            }
            txt_setting_TaskName.Text = name;
            txt_setting_Time_Hour.Text = t.Hours.ToString();
            txt_setting_Time_Minute.Text = t.Minutes.ToString();
            txt_setting_Time_Second.Text = t.Seconds.ToString();
        }

        public void LoadFromTaskSetting(TimerTask task, TimerTaskKind kind)
        {
            string name = txt_setting_TaskName.Text;
            TimeSpan t = new TimeSpan(
                int.Parse(txt_setting_Time_Hour.Text),
                int.Parse(txt_setting_Time_Minute.Text),
                int.Parse(txt_setting_Time_Second.Text));
            switch (kind)
            {
                case TimerTaskKind.CLOCK:
                    {
                        ClockTask c = task as ClockTask;
                        c.AimTime = t;
                        c.TaskName = name;
                        setting_Chosen_Mode = TimerTaskKind.CLOCK;
                        break;
                    }
                case TimerTaskKind.CUTDOWNTIMER:
                    {
                        CutDownTask c = task as CutDownTask;
                        c.FromTime = t;
                        c.TaskName = name;
                        c.dateTime = t;
                        setting_Chosen_Mode = TimerTaskKind.CUTDOWNTIMER;
                        break;
                    }
            }
        }

        private void Btn_bg_change_Click(object sender, RoutedEventArgs e)
        {
            //旋转动画
            btn_bg_change.IsEnabled = false;
            RotateTransform rtf = new RotateTransform();
            rtf.CenterX = btn_bg_change.ActualWidth / 2;
            rtf.CenterY = btn_bg_change.ActualHeight / 2;
            btn_bg_change.RenderTransform = rtf;
            DoubleAnimation dbAscending = new DoubleAnimation(0, 360 * 2, new Duration(TimeSpan.FromSeconds(1)));
            rtf.BeginAnimation(RotateTransform.AngleProperty, dbAscending);

            // 
            DoubleAnimation backgroundAnimationIN = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(1)));
            backgroundAnimationIN.AccelerationRatio = 0.7;
            backgroundAnimationIN.Completed += (s, args) =>
            {
                win_ChangeBackground();
                DoubleAnimation backgroundAnimationOUT = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(1)));
                backgroundAnimationOUT.Completed += (ss, argss) =>
                {
                    btn_bg_change.IsEnabled = true;
                };
                backgroundAnimationOUT.AccelerationRatio = 0.7;
                grid_bg.BeginAnimation(Grid.OpacityProperty, backgroundAnimationOUT);
            };
            grid_bg.BeginAnimation(Grid.OpacityProperty, backgroundAnimationIN);

        }

        private void Grid_bg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed
                && e.ClickCount == 2)
            {
                MusicPlayer.Stop();
            }
        }


    }
}
