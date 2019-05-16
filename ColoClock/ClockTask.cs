using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace ColoClock
{
    class ClockTask : TimerTask
    {
        static private BitmapImage imgres_icon;

        Label lab_title;
        static ClockTask()
        {
            imgres_icon = new BitmapImage(new Uri(".\\Resource\\Clock.png", UriKind.Relative));
        }

        public ClockTask():base("None")
        {
            this.AimTime = TimeSpan.Zero;
            this.dateTime = ExpendTool.TimeSpan_Now();
        }

        public ClockTask(TimeSpan aimTime,string name = "None") :base(name)
        {
            Enabled = true;
            this.AimTime = aimTime;
            this.dateTime = ExpendTool.TimeSpan_Now();
            Enabled = false;
            onAttachAimTime += AttachAimTime;
        }

        public override void datatimeUpdate()
        {
            datatimeUpdate(TimeSpan.FromSeconds(1));
        }

        public void datatimeUpdate(TimeSpan span)
        {
            this.dateTime += span;
            update_GUI();
            if (dateTime == AimTime)
            {
                onAttachAimTime(this);
            }
        }

        public new Grid init_GUI()
        {
            base.init_GUI();
            foreach (var UIEle in bindGrid.Children)
            {
                if (UIEle is Button && (UIEle as Button).Name == "btn_img_icon")
                {
                    Button icon = (UIEle as Button);
                    (icon.Background as ImageBrush).ImageSource = imgres_icon;
                    (icon.Background as ImageBrush).Stretch = Stretch.Fill;
                }
                if (UIEle is Label && (UIEle as Label).Name == "lab_TaskName")
                {
                    Label tx = (UIEle as Label);
                    lab_title = tx;
                    tx.Content = TaskName;
                    tx.MouseEnter += lab_TaskName_OnMouseEnter;
                    tx.MouseLeave += lab_TaskName_OnMouseLeave;
                }
                if (UIEle is Button && (UIEle as Button).Name == "btn_Setting")
                {
                    Button btn = UIEle as Button;
                    btn.Content = "";
                }
            }
            bindGrid.MouseEnter += grid_TaskName_OnMouseEnter;
            bindGrid.MouseLeave += grid_TaskName_OnMouseLeave;
            return bindGrid;
        }

        private void lab_TaskName_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            label.Content = AimTime.ToString(@"hh\:mm\:ss");
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

        public new void update_GUI()
        {
            if (lab_title.IsMouseOver)
            {
                lab_title.Content = AimTime.ToString(@"hh\:mm\:ss");
            }
            else
            {
                lab_title.Content = TaskName;
            }
        }

        public override XmlElement saveAsXml(XmlDocument doc,XmlElement preNode)
        {
            XmlElement el = doc.CreateElement("CLOCK");
            el.SetAttribute("Name", TaskName);
            el.SetAttribute("AimTime",AimTime.ToString());
            return el;
        }

        public override void loadFromXml(XmlElement node)
        {
            this.TaskName = node.GetAttribute("Name");
            this.AimTime = TimeSpan.Parse(node.GetAttribute("AimTime"));
        }
    }
}
