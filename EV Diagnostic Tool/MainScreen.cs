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
        private bool import;
        private bool find;

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

            import = true;

            timerSample.Stop();
            Thread.Sleep(500);
            controller.DiscardInBuffer();
            controller.DiscardOutBuffer();
            controller.WriteLine("IMPORT");            
        }

        private void timerSample_Tick(object sender, EventArgs e)
        {
            try
            {
                controller.WriteLine("SENSORS" + numericUpDownBatteryBank.Value);
            }
            catch (Exception)
            {
                timerSample.Stop();
                controller = null;
                labelStatus.Text = "Controller not detected";
            }
        }

        private void FindController()
        {
            timerSample.Stop();
            find = true;

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
            //temp.DtrEnable = true; //Enables this if debugging with an Arduino Micro
            //temp.RtsEnable = true; //Enables this if debugging with an Arduino Micro
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
            find = false;
        }

        private void Controller_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (find)
                return;

            string response = controller.ReadTo("\r\n");

            if (import)
            {
                if (response.Contains("NEXT"))
                {
                    category++;
                    Invoke((MethodInvoker)delegate
                    {
                        progressBarImport.Value = 0;
                    });
                }
                else if (response.Contains("END"))
                {
                    timerSample.Start();
                    Invoke((MethodInvoker)delegate
                    {
                        progressBarImport.Value = 100;
                    });
                    import = false;
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
                                chartAcceleratorPedal.Series[0].Points.AddXY(entry_vals[3], entry_vals[0]);
                                chartBrakePedal.Series[0].Points.AddXY(entry_vals[3], entry_vals[1]);
                                chartMotorTemp.Series[0].Points.AddXY(entry_vals[3], entry_vals[2]);
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
            else
            {
                double[] sensor_vals = Array.ConvertAll(response.Split(','), double.Parse);
                Invoke((MethodInvoker)delegate
                {
                    labelMotorC.Text = sensor_vals[0] + " C";
                    labelAcceleratorV.Text = sensor_vals[1] + " V";
                    labelBrakeV.Text = sensor_vals[2].ToString();

                    labelChip1Cell1V.Text = sensor_vals[3] + " V";
                    labelChip1Cell2V.Text = sensor_vals[4] + " V";
                    labelChip1Cell3V.Text = sensor_vals[5] + " V";
                    labelChip1Cell4V.Text = sensor_vals[6] + " V";
                    labelChip1Cell5V.Text = sensor_vals[7] + " V";
                    labelChip1Cell6V.Text = sensor_vals[8] + " V";
                    labelChip2Cell1V.Text = sensor_vals[9] + " V";
                    labelChip2Cell2V.Text = sensor_vals[10] + " V";
                    labelChip2Cell3V.Text = sensor_vals[11] + " V";
                    labelChip2Cell4V.Text = sensor_vals[12] + " V";
                    labelChip2Cell5V.Text = sensor_vals[13] + " V";
                    labelChip2Cell6V.Text = sensor_vals[14] + " V";
                    labelChip3Cell1V.Text = sensor_vals[15] + " V";
                    labelChip3Cell2V.Text = sensor_vals[16] + " V";
                    labelChip3Cell3V.Text = sensor_vals[17] + " V";
                    labelChip3Cell4V.Text = sensor_vals[18] + " V";
                    labelChip3Cell5V.Text = sensor_vals[19] + " V";
                    labelChip3Cell6V.Text = sensor_vals[20] + " V";
                    labelChip4Cell1V.Text = sensor_vals[21] + " V";
                    labelChip4Cell2V.Text = sensor_vals[22] + " V";
                    labelChip4Cell3V.Text = sensor_vals[23] + " V";
                    labelChip4Cell4V.Text = sensor_vals[24] + " V";
                    labelChip4Cell5V.Text = sensor_vals[25] + " V";
                    labelChip4Cell6V.Text = sensor_vals[26] + " V";

                    labelChip1Cell1C.Text = sensor_vals[27] + " C";
                    labelChip1Cell2C.Text = sensor_vals[28] + " C";
                    labelChip1Cell3C.Text = sensor_vals[29] + " C";
                    labelChip1Cell4C.Text = sensor_vals[30] + " C";
                    labelChip1Cell5C.Text = sensor_vals[31] + " C";
                    labelChip1Cell6C.Text = sensor_vals[32] + " C";
                    labelChip2Cell1C.Text = sensor_vals[33] + " C";
                    labelChip2Cell2C.Text = sensor_vals[34] + " C";
                    labelChip2Cell3C.Text = sensor_vals[35] + " C";
                    labelChip2Cell4C.Text = sensor_vals[36] + " C";
                    labelChip2Cell5C.Text = sensor_vals[37] + " C";
                    labelChip2Cell6C.Text = sensor_vals[38] + " C";
                    labelChip3Cell1C.Text = sensor_vals[39] + " C";
                    labelChip3Cell2C.Text = sensor_vals[40] + " C";
                    labelChip3Cell3C.Text = sensor_vals[41] + " C";
                    labelChip3Cell4C.Text = sensor_vals[42] + " C";
                    labelChip3Cell5C.Text = sensor_vals[43] + " C";
                    labelChip3Cell6C.Text = sensor_vals[44] + " C";
                    labelChip4Cell1C.Text = sensor_vals[45] + " C";
                    labelChip4Cell2C.Text = sensor_vals[46] + " C";
                    labelChip4Cell3C.Text = sensor_vals[47] + " C";
                    labelChip4Cell4C.Text = sensor_vals[48] + " C";
                    labelChip4Cell5C.Text = sensor_vals[49] + " C";
                    labelChip4Cell6C.Text = sensor_vals[50] + " C";
                });
            }
        }
    }
}
