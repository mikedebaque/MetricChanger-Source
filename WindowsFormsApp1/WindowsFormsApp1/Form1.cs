using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Sockets;
using System.IO;

namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            this.listBox1.AllowDrop = true;//added this line
            this.listBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDown);
            this.listBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBox1_DragDrop);
            this.listBox1.DragOver += new System.Windows.Forms.DragEventHandler(this.listBox1_DragOver);
            //Get interfaces list at startup
            foreach (string element in GetInterfacesInfo().Values)
            {
                listBox1.Items.Add(element);
            }

        }

        public bool IsDhcpEnabled { get; }

        public Dictionary<Int32, string> GetInterfacesInfo() // Get all interfaces minus Loopback in a dict
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            Dictionary<Int32, string> Listinterface = new Dictionary<int, string>();
            int loopIndex = 0;
            foreach (NetworkInterface adapter in adapters)
            {
                if (adapter.GetIPProperties().GatewayAddresses.FirstOrDefault() != null) // Don't display or use adapters without a gateway
                {
                    Listinterface.Add(loopIndex, adapter.Name);
                    loopIndex += 1;
                }
            }

            return Listinterface;
        }

        public Dictionary<string, string> ListBoxToDict()
        {
            Dictionary<string, string> lsBoxDict = new Dictionary<string, string>();
            List<string> metricList = new List<string>();
            metricList.Add("5");
            metricList.Add("25");
            metricList.Add("30");
            metricList.Add("35");
            metricList.Add("40");
            metricList.Add("50");
            int loopIndex = 0;
            foreach (string item in listBox1.Items)
            {
                lsBoxDict.Add(item, metricList[loopIndex]);
                loopIndex += 1;
            }
            return lsBoxDict;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            foreach (string element in GetInterfacesInfo().Values)
            {
                listBox1.Items.Add(element);
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {

            int ix = listBox1.IndexFromPoint(e.Location);

            if (ix != -1)
            {
                listBox1.DoDragDrop(ix.ToString(),
                DragDropEffects.Move);
            }

        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                int dix = Convert.ToInt32(e.Data.GetData(DataFormats.Text));//changed this line
                int ix = listBox1.IndexFromPoint(listBox1.PointToClient(new Point(e.X, e.Y)));
                if (ix != -1)
                {
                    object obj = listBox1.Items[dix];
                    listBox1.Items.Remove(obj);           
                    listBox1.Items.Insert(ix, obj);
                }
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void listBox1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect =
                DragDropEffects.Move;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (string item in listBox1.Items)
            {
                if (ListBoxToDict().ContainsKey(item))
                {
                    // Change interface metric using netsh
                    Console.WriteLine("int ip set interface interface=\"" + item + "\" metric=" + ListBoxToDict()[item] + "");
                    Process p = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo("netsh", "int ip set interface interface=\""+item+"\" metric="+ListBoxToDict()[item]+"");
                    p.StartInfo = psi;
                    psi.UseShellExecute = true;
                    psi.CreateNoWindow = true;
                    p.StartInfo.Verb = "runas";
                    p.Start();
                    //p.WaitForExit();
                    p.Close();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (string item in listBox1.Items)
            {
                if (ListBoxToDict().ContainsKey(item))
                {
                    Process p = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo("netsh", "int ip set interface interface=\"" + item + "\" metric=automatic");
                    p.StartInfo = psi;
                    p.StartInfo.Verb = "runas";
                   
                    psi.UseShellExecute = true;
                    psi.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit(1000);
                }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = "exe";
            openFileDialog1.Filter = "exe files (*.exe)|*.exe";
            openFileDialog1.Multiselect = false;
            openFileDialog1.DereferenceLinks = true;
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog1.ShowDialog();
            textBox1.Text = openFileDialog1.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                if (listBox1.SelectedItem != null)
                {
                    if (checkBox1.Checked == true)
                    {
                        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                        foreach (var adapter in adapters)
                        {
                            if (adapter.Name == listBox1.SelectedItem.ToString())
                            {
                                foreach (var x in adapter.GetIPProperties().UnicastAddresses)
                                {
                                    if (x.Address.AddressFamily == AddressFamily.InterNetwork && x.IsDnsEligible)
                                    {
                                        Process forceBind32 = new Process();
                                        ProcessStartInfo fB32psi = new ProcessStartInfo("cmd", "/C ForceBindIP.exe " + x.Address.ToString() + " \"" + textBox1.Text + "\"");
                                        forceBind32.StartInfo = fB32psi;
                                        fB32psi.UseShellExecute = false;
                                        fB32psi.CreateNoWindow = true;
                                        forceBind32.Start();
                                        forceBind32.WaitForExit(1000);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                        foreach (var adapter in adapters)
                        {
                            if (adapter.Name == listBox1.SelectedItem.ToString())
                            {
                                foreach (var x in adapter.GetIPProperties().UnicastAddresses)
                                {
                                    if (x.Address.AddressFamily == AddressFamily.InterNetwork && x.IsDnsEligible)
                                    {
                                        Console.WriteLine(Directory.GetCurrentDirectory().ToString() + "\\" + "ForceBindIP64.exe " + x.Address.ToString() + " \"" + textBox1.Text + "\"");

                                        Process forceBind64 = new Process();
                                        ProcessStartInfo fB64psi = new ProcessStartInfo("cmd", "/C ForceBindIP64.exe " + x.Address.ToString() + " \"" + textBox1.Text + "\"");
                                        forceBind64.StartInfo = fB64psi;
                                        fB64psi.UseShellExecute = false;
                                        fB64psi.CreateNoWindow = true;                                       
                                        forceBind64.Start();
                                        forceBind64.WaitForExit(1000);
                                    }
                                }
                            }
                        }

                    }
                }
                else
                {
                    MessageBox.Show("You need to select an interface", "No interface selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("You need to choose a software to launch", "Invalid path",
                 MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }




}

