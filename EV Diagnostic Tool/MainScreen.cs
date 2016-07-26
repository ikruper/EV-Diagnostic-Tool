using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EV_Diagnostic_Tool
{
    public partial class MainScreen : Form
    {
        SerialPort controller;

        public MainScreen()
        {
            InitializeComponent();
        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            FindController();
        }

        private void buttonScan_Click(object sender, EventArgs e)
        {
            FindController();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            if (controller == null)
                return;
            controller.WriteLine("IMPORT");
            //TODO: read the lines of data from the controller, save them locally, and plot them on a chart
        }

        private void timerSample_Tick(object sender, EventArgs e)
        {
            try
            {
                controller.WriteLine("BATT 1 VOLT");
                //textBoxVoltage.Text = controller.ReadTo("\r\n");

                controller.WriteLine("BATT 1 TEMP 1");
                //textBoxBatt1Temp1.Text = ConvertToDegreesC(controller.ReadTo("\r\n"));

                controller.WriteLine("ACCELERATOR");
                //textBoxAccelerator.Text = controller.ReadTo("\r\n");
            }
            catch (Exception)
            {
                timerSample.Stop();
            }
        }

        private void FindController()
        {
            if (controller != null)
            {
                if (controller.IsOpen)
                    controller.Close();
                controller = null;
            }
            labelStatus.Text = "Searching for controller...";
            string[] ports = SerialPort.GetPortNames();
            progressBarStatus.Value = 0;
            progressBarStatus.Maximum = ports.Length;
            progressBarStatus.Step = 1;

            SerialPort temp = new SerialPort();
            temp.ReadTimeout = 250;
            temp.BaudRate = 9600;
            //temp.DtrEnable = true; //Enables this if using an Arduino Micro
            //temp.RtsEnable = true; //Enables this if using an Arduino Micro
            foreach (string port in ports)
            {
                temp.PortName = port;
                temp.Open();
                temp.WriteLine("EV CONTROLLER");
                Thread.Sleep(10);
                try
                {
                    string test = temp.ReadTo("\r\n");
                    if (test == "EV Controller")
                    {
                        controller = temp;
                        break;
                    }
                }
                catch (TimeoutException)
                {
                    progressBarStatus.PerformStep();
                    temp.Close();
                }
            }

            progressBarStatus.Value = 0;
            if (controller == null)
            {
                labelStatus.Text = "Controller not detected";
                timerSample.Stop();
            }
            else
            {
                labelStatus.Text = "Controller detected on " + controller.PortName;
                timerSample.Start();
            }
        }

        private string ConvertToDegreesC(string voltage)
        {
            return (Convert.ToDouble(voltage) * 100).ToString();
        }
    }
}
