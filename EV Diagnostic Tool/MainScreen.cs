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
        private SerialPort controller;
        private int category;

        public MainScreen()
        {
            InitializeComponent();
        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            Thread.Sleep(3000);
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

            progressBarImport.Value = 0;
            category = 0;

            timerSample.Stop();
            Thread.Sleep(500);
            controller.DiscardInBuffer();
            controller.DiscardOutBuffer();
            controller.WriteLine("IMPORT");            
        }

        private void timerSample_Tick(object sender, EventArgs e)
        {
            //Don't forget that the data received event will be triggered by the real-time readings

            /*string response = string.Empty;
            try
            {
                controller.DiscardInBuffer();
                controller.DiscardOutBuffer();

                controller.WriteLine("SENSORS");
                response = controller.ReadTo("\r\n");

                string[] sensor_vals = response.Split(',');
                labelChip1Cell1C.Text = sensor_vals[0] + " C";
                labelChip1Cell2C.Text = sensor_vals[1] + " C";
                labelChip1Cell3C.Text = sensor_vals[2] + " C";
                labelChip1Cell4C.Text = sensor_vals[3] + " C";
                labelChip1Cell5C.Text = sensor_vals[4] + " C";
                labelAcceleratorV.Text = sensor_vals[5] + " %";
            }
            catch (Exception)
            {
                timerSample.Stop();
                controller = null;
                labelStatus.Text = "Controller not detected";
            }*/
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
                Thread.Sleep(500);
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
                controller.DataReceived += Controller_DataReceived;
                timerSample.Start();
            }
        }

        private void Controller_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string response = controller.ReadTo("\r\n");

            if (response == "NEXT")
            {
                category++;
                Invoke((MethodInvoker)delegate
                {
                    progressBarImport.Value = 0;
                });
            }
            else if (response == "END")
            {
                timerSample.Start();
                Invoke((MethodInvoker)delegate
                {
                    progressBarImport.Value = 100;
                });
            }
            else
            {
                if (response.Substring(0, 10) == "COMPLETION")
                {
                    Invoke((MethodInvoker)delegate
                    {
                        progressBarImport.Value = Convert.ToInt16(response.Substring(10));
                    });
                }                    
                else
                {
                    double[] entry_vals = Array.ConvertAll(response.Split(','), double.Parse);
                    if (category == 0)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            chartMotorTemp.Series[0].Points.AddXY(entry_vals[3], entry_vals[0]);
                            chartAcceleratorPedal.Series[0].Points.AddXY(entry_vals[3], entry_vals[1]);
                            chartBrakePedal.Series[0].Points.AddXY(entry_vals[3], entry_vals[2]);
                        });
                    }
                    else if (category == 1)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            for (int count = 0; count < 6; count++)
                            {
                                chartBox1Chip1Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count]);
                                chartBox1Chip2Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 6]);
                                chartBox1Chip3Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 12]);
                                chartBox1Chip4Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 18]);

                                chartBox1Chip1Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 24]);
                                chartBox1Chip2Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 30]);
                                chartBox1Chip3Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 36]);
                                chartBox1Chip4Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 42]);
                            }
                        });
                    }
                    else if (category == 2)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            for (int count = 0; count < 6; count++)
                            {
                                chartBox2Chip1Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count]);
                                chartBox2Chip2Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 6]);
                                chartBox2Chip3Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 12]);
                                chartBox2Chip4Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 18]);

                                chartBox2Chip1Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 24]);
                                chartBox2Chip2Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 30]);
                                chartBox2Chip3Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 36]);
                                chartBox2Chip4Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 42]);
                            }
                        });
                    }
                    else if (category == 3)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            for (int count = 0; count < 6; count++)
                            {
                                chartBox3Chip1Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count]);
                                chartBox3Chip2Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 6]);
                                chartBox3Chip3Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 12]);
                                chartBox3Chip4Voltage.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 18]);

                                chartBox3Chip1Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 24]);
                                chartBox3Chip2Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 30]);
                                chartBox3Chip3Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 36]);
                                chartBox3Chip4Temp.Series[count].Points.AddXY(entry_vals[48], entry_vals[count + 42]);
                            }
                        });
                    }
                }
            }                       
        }
    }
}
