using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace ColoClock
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void Btn_add_pic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(".\\Resource\\bg");
            }
            catch(Exception er)
            {
                Console.Write(er.Message);
            }
        }

        public void loadSetting()
        {
            if(File.Exists("Setting.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Setting.xml");
                XmlNode root = doc.SelectSingleNode("Settings");
                XmlElement xmlElement = root.SelectSingleNode("MusicPath") as XmlElement;
                if (xmlElement != null)
                    txt_Music_Path.Text = xmlElement.GetAttribute("Path");
            }
        }

        public void saveSetting()
        {
            if (File.Exists("Setting.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Setting.xml");
                XmlNode root = doc.SelectSingleNode("Settings");
                XmlElement xmlElement = root.SelectSingleNode("MusicPath") as XmlElement;
                if (xmlElement != null)
                    xmlElement.SetAttribute("Path", txt_Music_Path.Text);
                else
                {
                    XmlElement xml_music = doc.CreateElement("MusicPath");
                    xml_music.SetAttribute("Path", txt_Music_Path.Text);
                    root.AppendChild(xml_music);
                }
                doc.Save("Setting.xml");
            }
            else
            {
                using (StreamWriter writer = new StreamWriter("Setting.xml"))
                {
                    writer.WriteLine("<Settings>");
                    writer.WriteLine("</Settings>");
                }
                XmlDocument doc = new XmlDocument();
                doc.Load("Setting.xml");
                XmlNode root = doc.SelectSingleNode("Settings");
                XmlElement xml_music =  doc.CreateElement("MusicPath");
                xml_music.SetAttribute("Path", txt_Music_Path.Text);
                root.AppendChild(xml_music);
                doc.Save("Setting.xml");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            saveSetting();
            this.Hide();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Txt_Music_Path_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.ShowDialog();
            if (open.FileName != "")
            {
                txt_Music_Path.Text = open.FileName;
            }
        }

    }
}
