﻿using System;
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

        private void timerSample_Tick(object sender, EventArgs e)
        {
            try
            {
                controller.WriteLine("VOLTAGE 1");
                Thread.Sleep(10);
                textBoxVoltage.Text = controller.ReadTo("\r\n");
            }
            catch (Exception)
            {
                timerSample.Stop();
                //TODO: close the port and so forth...
            }
        }

        private void FindController()
        {
            controller = null;
            labelStatus.Text = "Searching for controller...";
            string[] ports = SerialPort.GetPortNames();
            progressBarStatus.Value = 0;
            progressBarStatus.Maximum = ports.Length;
            progressBarStatus.Step = 1;

            SerialPort temp = new SerialPort();
            temp.ReadTimeout = 250;
            temp.BaudRate = 9600;
            temp.DtrEnable = true;
            temp.RtsEnable = true;
            foreach (string port in ports)
            {
                temp.PortName = port;
                temp.Open();
                temp.WriteLine("EV CONTROLLER");
                Thread.Sleep(10);
                try
                {
                    if (temp.ReadTo("\r\n") == "EV Controller")
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