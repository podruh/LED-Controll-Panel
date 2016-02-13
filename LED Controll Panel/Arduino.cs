using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.IO.Ports;
using System.Windows.Threading;

namespace LED_Controll_Panel
{
    [Serializable()]
    public class Arduino
    {
        
        public string Port;

        [XmlIgnore()]
        public byte[] RGB = { 0, 0, 0, 0x0a };

        [XmlIgnore()]
        private SerialPort myPort = new SerialPort();

        public int R, G, B;
        public bool lightShowOn = false;
        public bool rndOn = false;
        public int timerMs = 20;
        public int ls_stepR = -1;
        public int ls_stepG = 1;
        public int ls_stepB = -1;

        [XmlIgnore()]
        public byte[] rnd_RGB = { 0, 0, 0 };


        public int rnd_step;

        [XmlIgnore()]
        private MainWindow window;

        public Arduino()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, timerMs);
            timer.Start();
        }

        public void SetWindow(MainWindow mw)
        {
            window = mw;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            int r = (int)RGB[0];
            int g = (int)RGB[1];
            int b = (int)RGB[2];
            #region Light Show
            if (lightShowOn)
            {


                r += ls_stepR;
                g += ls_stepG;
                b += ls_stepB;

                if (r >= 255)
                {
                    ls_stepR *= -1;
                    r = 255;
                }
                if (r <= 0)
                {
                    ls_stepR *= -1;
                    r = 0;
                }
                if (g >= 255)
                {
                    ls_stepG *= -1;
                    g = 255;
                }
                if (g <= 0)
                {
                    ls_stepG *= -1;
                    g = 0;
                }
                if (b >= 255)
                {
                    ls_stepB *= -1;
                    b = 255;
                }
                if (b <= 0)
                {
                    b = 0;
                    ls_stepB *= -1;
                }


                byte[] rgb = { (byte)r, (byte)g, (byte)b };
                SendRGB(rgb);

            }
            #endregion

            #region Random Show
            Random rndInt = new Random();
            bool[] RGB_InPlace = new bool[3];
            if (rndOn)
            {
                for (int i = 2; i >= 0; i--)
                {
                    if ((RGB[i] > (rnd_RGB[i] - rnd_step)) && (RGB[i] < (rnd_RGB[i] + rnd_step)))
                    {
                        RGB_InPlace[i] = true;
                    }
                    else if (RGB[i] <= (rnd_RGB[i] - rnd_step))
                    {
                        RGB[i] += (byte)rnd_step;
                        RGB_InPlace[i] = false;
                    }
                    else if (RGB[i] >= (rnd_RGB[i] + rnd_step))
                    {
                        RGB[i] -= (byte)rnd_step;
                        RGB_InPlace[i] = false;
                    }
                }
                if (RGB_InPlace[0] && RGB_InPlace[1] && RGB_InPlace[2])
                {
                    for (int i = 0; i < 3; i++)
                    {
                        rnd_RGB[i] = (byte)rndInt.Next(0, 256);
                    }
                }
                SendRGB();
            }
            #endregion

            if (window != null)
            {
                window.SetRGBPicker(RGB);
            }
        }

        public void SendRGB(byte[] rgb)
        {
            for (int i = 0; i < RGB.Length - 1; i++)
            {
                RGB[i] = rgb[i];
            }
            SendRGB();
        }

        public void SendRGB()
        {
            try
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
                R = RGB[0];
                G = RGB[1];
                B = RGB[2];
            }
            catch (Exception ex)
            {

                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
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
        public bool PortIsOpen()
        {
            if (myPort != null)
            {
                return myPort.IsOpen;
            }
            else
            {
                return false;
            }
        }
        public void ClosePort()
        {
            myPort.Close();
        }
        public void OpenPort(string portName)
        {
            SetPortName(portName);
            try
            {
                myPort.Open();
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
            }
            
        }
        public void SetPortName(string port)
        {
            myPort.PortName = port;
            Port = port;
        }
        public void SetLightShow(bool lightShow, int step, int speed)
        {
            if (lightShow)
            {
                lightShowOn = lightShow;
                ls_stepR = step;
                ls_stepG = step;
                ls_stepB = step;
                timerMs = speed;
            }
            else
            {
                SetLightShow(lightShow);
            }
        }
        public void SetLightShow(bool lightShow)
        {
            lightShowOn = lightShow;
        }
        public void SetRandom(bool enable, int step, int speed)
        {
            if (enable)
            {
                rndOn = enable;
                rnd_step = step;
                timerMs = speed;
                Random rnd = new Random();
                rnd.NextBytes(rnd_RGB);
            }
            else
            {
                SetRandom(enable);
            }
        }
        public void SetRandom(bool enable)
        {
            rndOn = enable;
        }
        public string GetPortName()
        {
            return Port;
        }

        public void SetRGB()
        {
            RGB[0] = (byte)R;
            RGB[1] = (byte)G;
            RGB[2] = (byte)B;
        }

        public void SetWindow()
        {
            byte[] rgb = { (byte)R, (byte)G, (byte)B };
            window.SetRGBPicker(rgb);
            window.rndOn = rndOn;
            window.lightShowOn = lightShowOn;
        }

    }
}
