using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace ColoClock
{
    class CutDownTask:TimerTask
    {
        static private BitmapImage imgres_icon;
        static private BitmapImage imgres_start;
        static private BitmapImage imgres_stop;
        static private BitmapImage imgres_reset;

        public TimeSpan FromTime;
        private Label lab_title = null;
        private Button btn_control = null;

        static CutDownTask()
        {
            imgres_icon = new BitmapImage(new Uri(".\\Resource\\CutdownTimer.png", UriKind.Relative));
            imgres_stop = new BitmapImage(new Uri(".\\Resource\\stop.png", UriKind.Relative));
            imgres_start = new BitmapImage(new Uri(".\\Resource\\start.png", UriKind.Relative));
            imgres_reset = new BitmapImage(new Uri(".\\Resource\\reset.png", UriKind.Relative));
        }

        public CutDownTask() : base("None")
        {
            this.AimTime = new TimeSpan(0);
        }

        public CutDownTask(TimeSpan tickTime, string name = "None") : base(name)
        {
            Enabled = true;
            this.dateTime = tickTime;
            this.FromTime = tickTime;
            this.AimTime = new TimeSpan(0);
            Enabled = false;
            onAttachAimTime += AttachAimTime;
        }

        public override void datatimeUpdate()
        {
            datatimeUpdate(TimeSpan.FromSeconds(-1));
        }

        public void datatimeUpdate(TimeSpan span)
        {
            if (Enabled && dateTime>TimeSpan.Zero)
            {
                this.dateTime += span;
                update_GUI();
                if (dateTime == AimTime)
                {
                    onAttachAimTime(this);
                }
            }
        }

        public new Grid init_GUI()
        {
            base.init_GUI();
            foreach (var UIEle in bindGrid.Children)
            {
                if (UIEle is Button && (UIEle as Button).Name == "btn_img_icon")
                {
                    Button icon = UIEle as Button;
                    (icon.Background as ImageBrush).ImageSource = imgres_icon;
                    (icon.Background as ImageBrush).Stretch = Stretch.Fill;
                }
                if (UIEle is Label && (UIEle as Label).Name == "lab_TaskName")
                {
                    lab_title = (UIEle as Label);
                    Label tx = lab_title;
                    tx.MouseEnter += lab_TaskName_OnMouseEnter;
                    tx.MouseLeave += lab_TaskName_OnMouseLeave;
                    tx.Content = TaskName;
                }
                if (UIEle is Button && (UIEle as Button).Name == "btn_Setting")
                {
                    btn_control = (UIEle as Button);
                    Button btn = btn_control;
                    btn.Background = new ImageBrush(imgres_start);
                    (btn.Background as ImageBrush).Stretch = Stretch.Uniform;
                    btn.Click += Btn_Control_Click;
                }
            }
            bindGrid.MouseEnter += grid_TaskName_OnMouseEnter;
            bindGrid.MouseLeave += grid_TaskName_OnMouseLeave;
            return bindGrid;
        }

        private void Btn_Control_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dateTime <= TimeSpan.Zero && Enabled == false)
            {
                dateTime = FromTime;
                (btn_control.Background as ImageBrush).ImageSource = imgres_start;
                
                update_GUI();
                return;
            }
            Enabled = !Enabled;
            update_GUI();
        }

        private void lab_TaskName_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            label.Content = dateTime.ToString(@"hh\:mm\:ss");
        }

        private void lab_TaskName_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            label.Content = this.TaskName;
        }

        private void grid_TaskName_OnMouseEnter(object sender, MouseEventArgs e)
        {
            bindGrid.Background.Opacity = 75;
        }

        private void grid_TaskName_OnMouseLeave(object sender, MouseEventArgs e)
        {
            bindGrid.Background.Opacity = 0;
        }

        protected new void AttachAimTime(object sender)
        {
            Enabled = false;
        }

        public new void update_GUI()
        {
            
            if (lab_title.IsMouseOver)
            {
                lab_title.Content = dateTime.ToString(@"hh\:mm\:ss");
            }
            else
            {
                lab_title.Content = TaskName;
            }

            if (Enabled)
            {
                (btn_control.Background as ImageBrush).ImageSource = imgres_stop;
            }
            else
            {
                (btn_control.Background as ImageBrush).ImageSource = imgres_start;
            }
            if (dateTime <= TimeSpan.Zero)
            {
                (btn_control.Background as ImageBrush).ImageSource = imgres_reset;
            }
        }

        public override XmlElement saveAsXml(XmlDocument doc, XmlElement preNode)
        {
            XmlElement el = doc.CreateElement("CUTDOWN");
            el.SetAttribute("Name", TaskName);
            el.SetAttribute("FromTime", FromTime.ToString());
            return el;
        }

        public override void loadFromXml(XmlElement node)
        {
            this.TaskName = node.GetAttribute("Name");
            this.FromTime = TimeSpan.Parse(node.GetAttribute("FromTime"));
            this.dateTime = FromTime;
        }
    }
}
