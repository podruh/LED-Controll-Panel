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
using System.IO.Ports;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.DataGrid;

namespace LED_Controll_Panel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string Port;
        private byte[] RGB = { 0, 0, 0, 0x0a };
        private SerialPort myPort;
        private bool lightShow = false;


        public MainWindow()
        {
            InitializeComponent();
            myPort = new SerialPort();
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
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if (lightShow)
            {
                int r = RGB[0];
                int g = RGB[1];
                int b = RGB[2];
                b++;
                if (b == 255)
                {
                    g++;
                    b = 0;
                }
                if (g == 255)
                {
                    r++;
                    g = 0;
                }
                if (r == 255)
                {
                    r = 0;
                }
                byte[] rgb = { (byte)r, (byte)g, (byte)b };
                SendRGB(rgb);
                rgbPicker.R = RGB[0];
                rgbPicker.G = RGB[1];
                rgbPicker.B = RGB[2];
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
                    if (myPort.IsOpen)
                        myPort.Close();
                    myPort.PortName = Port;
                    myPort.Open();
                    SendRGB();
                }
                else if (!myPort.IsOpen)
                {
                    myPort.Open();
                }
                
            }
             
        }

        public void SendRGB(string color, byte value)
        {
            switch (color)
            {
                case "R":
                    RGB[0] = value;
                    break;
                case "G":
                    RGB[1] = value;
                    break;
                case "B":
                    RGB[2] = value;
                    break;
                default:
                    break;
            }
            SendRGB();
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            myPort.Close();
        }

        public void SendRGB(byte[] rgb)
        {
            for (int i = 0; i < RGB.Length -1; i++)
            {
                RGB[i] = rgb[i];
            }
            SendRGB();
        }

        public void SendRGB()
        {
            if (Port != null)
            {
                myPort.BaudRate = 115200;
                myPort.NewLine = "\n";
                myPort.WriteTimeout = 500;



                for (int i = 0; i < RGB.Length - 1; i++)
                {
                    if (RGB[i] == 0x0A)
                    {
                        RGB[i] = 0x0B;
                    }
                }
                myPort.Write(RGB, 0, 4);


            }
        }

        private void ColorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (!lightShow)
            {
                byte[] rgb = { rgbPicker.R, rgbPicker.G, rgbPicker.B };
                SendRGB(rgb);
            }
            
        }

        private void lightShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (lightShow)
            {
                lightShow = false;
            }
            else
            {
                lightShow = true;
            }
        }
    }
}
