using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;
using System.Xml;
namespace ColoClock
{
    public enum TimerTaskKind
    {
        CLOCK,
        CUTDOWNTIMER,
    }
    public abstract class TimerTask
    {
        private static string xamlTaskGrid;
        public Grid bindGrid;
        private TimeSpan gDateTime;

        public bool Enabled = false;
        public Action<object> onAttachAimTime;
        public string TaskName { get; set; }
        public TimeSpan dateTime
        {
            get { return gDateTime; }
            set
            {
                gDateTime = value;
                if (value == AimTime)
                {
                    onAttachAimTime(this);
                }
            }
        }

        static TimerTask()
        {
            xamlTaskGrid = File.ReadAllText(".\\Resource\\TimerTaskGrid.xaml");
        }

        public TimerTask():this("None"){ }

        public TimerTask(string taskName = "None")
        {
            bindGrid = new Grid();
            TaskName = taskName;
            onAttachAimTime += AttachAimTime;
        }

        public TimeSpan AimTime { get; set; }
        
        public abstract void datatimeUpdate();

        protected void AttachAimTime(object sender) { }

        public Grid init_GUI()
        {
            bindGrid = System.Windows.Markup.XamlReader.Parse(xamlTaskGrid) as Grid;
            bindGrid.Background.Opacity = 0;
            return bindGrid;
        }

        protected void update_GUI() { }

        public abstract void loadFromXml(XmlElement node);
        public abstract XmlElement saveAsXml(XmlDocument doc, XmlElement preNode);

    }
}
