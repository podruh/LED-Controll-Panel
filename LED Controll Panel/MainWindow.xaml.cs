using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.DataGrid;
using Microsoft.Win32;
using System.Xml.Serialization;

namespace LED_Controll_Panel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string Port;
        public bool lightShowOn = false;
        public bool rndOn = false;
        

        private Arduino Arduino;
        private RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

        public MainWindow()
        {
            InitializeComponent();

            //myPort = new SerialPort();
        }

        public void SetPorts()
        {
            comboBoxPort.Items.Clear();
            foreach (String portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                comboBoxPort.Items.Add(portName);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetPorts();
            Arduino = new Arduino();
            LoadSettings();
            Arduino.SetWindow(this);
            Arduino.OpenPort(Arduino.Port);
            Arduino.SetRGB();
            Arduino.SetWindow();
            Arduino.SendRGB();
            comboBoxPort.SelectedIndex = 1;
            if (key.GetValue("LED Controll Panel") == null)
            {
                // The value doesn't exist, the application is not set to run at Startup
                startupCheckBox.IsChecked = false;
            }
            else
            {
                // The value exists, the application is set to run at Startup
                startupCheckBox.IsChecked = true;
            }

        }

        private void comboBoxPort_DropDownOpened(object sender, EventArgs e)
        {
            SetPorts();
        }

        private void comboBoxPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxPort.SelectedItem != null)
            {
                if (Port != comboBoxPort.SelectedItem.ToString())
                {
                    Port = comboBoxPort.SelectedItem.ToString();
                    if (Arduino.PortIsOpen())
                        Arduino.ClosePort();
                    Arduino.OpenPort(Port);
                    Arduino.SendRGB();
                }
                else if (!Arduino.PortIsOpen())
                {
                    Arduino.OpenPort(Port);
                }
                
            }
             
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Arduino.ClosePort();
        }

               

        private void ColorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (!Arduino.lightShowOn)
            {
                byte[] rgb = { rgbPicker.R, rgbPicker.G, rgbPicker.B };
                Arduino.SendRGB(rgb);
            }

        }

        private void lightShowButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxPort.SelectedItem == null)
                {
                    System.Windows.MessageBox.Show("You have to choose port!");
                }
                else if (lightShowOn)
                {
                    lightShowOn = false;
                    Arduino.SetLightShow(lightShowOn);
                    rndButton.IsEnabled = true;
                }
                else
                {
                    lightShowOn = true;
                    rndButton.IsEnabled = false;
                    Arduino.SetLightShow(lightShowOn, (int)lightShowStepUpDown.Value, (int)lightShowSpeedUpDown.Value);
                }
            }
            catch (Exception)
            {

                System.Windows.MessageBox.Show("Fields with steps and speed cannot be empty!");
            }
            
        }

        private void rndButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxPort.SelectedItem == null)
                {
                 System.Windows.MessageBox.Show("You have to choose port!");
                }
                else if (rndOn)
                {
                    rndOn = false;
                    lightShowButton.IsEnabled = true;
                    Arduino.SetRandom(rndOn);
                }
                else 
                {
                    rndOn = true;
                    lightShowButton.IsEnabled = false;
                    Arduino.SetRandom(rndOn, (int)rndStepUpDown.Value, (int)rndSpeedUpDown.Value);
                }
            }
            catch (Exception)
            {

                System.Windows.MessageBox.Show("Fields with steps and speed cannot be empty!");
            }
        }
        private void Startup(bool add)
        {            
            if (add)
            {                
                key.SetValue("LED Controll Panel", System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
                key.DeleteValue("LED Controll Panel");

            key.Close();
        }

        private void startupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Startup((bool)startupCheckBox.IsChecked);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        public void SetRGBPicker(byte[] rgb)
        {
            rgbPicker.R = rgb[0];
            rgbPicker.G = rgb[1];
            rgbPicker.B = rgb[2];
        }

        public void SaveSettings()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(Arduino.GetType());

                using (StreamWriter sw = new StreamWriter("Startup.xml"))
                {
                    serializer.Serialize(sw, Arduino);
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
            }
        }

        public void LoadSettings()
        {

            try
            {
                XmlSerializer serializer = new XmlSerializer(Arduino.GetType());

                using (StreamReader sw = new StreamReader("Startup.xml"))
                {
                    Arduino = (Arduino)serializer.Deserialize(sw);
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
            }
        }

    }
}
