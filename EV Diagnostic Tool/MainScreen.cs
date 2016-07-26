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
using System.Windows.Forms.DataVisualization.Charting;

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
            timerSample.Stop();
            Thread.Sleep(500);
            controller.DiscardInBuffer();
            controller.DiscardOutBuffer();
            controller.WriteLine("IMPORT");
            Thread.Sleep(50);

            List<string> data = new List<string>();
            string response = controller.ReadTo("\r\n");
            string series = response;
            while (true)
            {
                response = controller.ReadTo("\r\n");
                if (response != "END")
                    data.Add(response);
                else
                    break;
            }

            double[] temp1 = new double[data.Count];
            double[] temp2 = new double[data.Count];
            double[] temp3 = new double[data.Count];
            double[] temp4 = new double[data.Count];
            double[] temp5 = new double[data.Count];
            double[] accel = new double[data.Count];
            double[] time = new double[data.Count];
            for (int count = 0; count < data.Count; count++)
            {
                double[] entry_vals = Array.ConvertAll(data[count].Split(','), double.Parse);
                temp1[count] = entry_vals[0];
                temp2[count] = entry_vals[1];
                temp3[count] = entry_vals[2];
                temp4[count] = entry_vals[3];
                temp5[count] = entry_vals[4];
                accel[count] = entry_vals[5];
                time[count] = entry_vals[6];
            }

            string[] series_vals = series.Split(',');
            for (int count = 0; count < 6; count++)
            {
                Series sensor = chartLog.Series.Add(series_vals[count]);
                sensor.ChartType = SeriesChartType.Line;
                for (int count2 = 0; count2 < data.Count; count2++)
                {
                    switch (count)
                    {
                        case 0:
                            sensor.Points.AddXY(time[count2], temp1[count2]);
                            break;
                        case 1:
                            sensor.Points.AddXY(time[count2], temp2[count2]);
                            break;
                        case 2:
                            sensor.Points.AddXY(time[count2], temp3[count2]);
                            break;
                        case 3:
                            sensor.Points.AddXY(time[count2], temp4[count2]);
                            break;
                        case 4:
                            sensor.Points.AddXY(time[count2], temp5[count2]);
                            break;
                        case 5:
                            sensor.Points.AddXY(time[count2], accel[count2]);
                            break;
                    }
                }
            }
            timerSample.Start();
        }

        private void timerSample_Tick(object sender, EventArgs e)
        {
            controller.DiscardInBuffer();
            controller.DiscardOutBuffer();
            try
            {
                /*controller.WriteLine("TEMP 0");
                labelChip1Cell1C.Text = controller.ReadTo("\r\n");

                controller.WriteLine("TEMP 1");
                labelChip1Cell2C.Text = controller.ReadTo("\r\n");

                controller.WriteLine("TEMP 2");
                labelChip1Cell3C.Text = controller.ReadTo("\r\n");

                controller.WriteLine("TEMP 3");
                labelChip1Cell4C.Text = controller.ReadTo("\r\n");

                controller.WriteLine("TEMP 4");
                labelChip1Cell5C.Text = controller.ReadTo("\r\n");

                controller.WriteLine("ACCEL");
                labelAcceleratorV.Text = controller.ReadTo("\r\n");*/
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
            temp.ReadTimeout = 500;
            temp.BaudRate = 9600;
            temp.DataBits = 8;
            temp.Parity = Parity.Even;
            temp.StopBits = StopBits.One;   
            //temp.DtrEnable = true; //Enables this if using an Arduino Micro
            //temp.RtsEnable = true; //Enables this if using an Arduino Micro
            foreach (string port in ports)
            {
                temp.PortName = port;
                temp.Open();
                temp.DiscardOutBuffer();
                temp.DiscardInBuffer();
                temp.WriteLine("EV CONTROLLER");
                Thread.Sleep(100);
                try
                {
                    string response = temp.ReadTo("\r\n");
                    if (response == "EV Controller")
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
    }
}
