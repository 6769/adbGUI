﻿using adbGUI.Forms;
using adbGUI.Methods;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace adbGUI
{
    public partial class MainForm : Form
    {
        private ScreenRecord screenRecord;
        private SpoofMac spoofMac;
        private ResolutionChange resolutionChange;
        private Density densityChange;
        private FileOps fileOps;
        private InstallUninstall installUninstall;
        private Sideload sideLoad;
        private BackupRestore backupRestore;
        private LogcatAdvanced logcatAdvanced;

        public FormMethods formMethods;

        private CmdProcess cmdProcess = new CmdProcess();

        public MainForm()
        {
            InitializeComponent();

            // pass formMethods the created Form this
            formMethods = new FormMethods(this);

            cmdProcess.GetProcess.Start();


            // Begin and cancel so the RichTextBox will stay clean. Otherwise it will start in line 2.
            cmdProcess.GetProcess.BeginOutputReadLine();
            cmdProcess.GetProcess.CancelOutputRead();

            cmdProcess.GetProcess.OutputDataReceived += AppendReceivedData;
            cmdProcess.GetProcess.ErrorDataReceived += AppendReceivedData;

            Thread.Sleep(20);

            cmdProcess.GetProcess.BeginOutputReadLine();
            cmdProcess.GetProcess.BeginErrorReadLine();
            rtb_console.Clear();

            cmdProcess.CommandExecutionStarted += CommandExecutionStarted;
            cmdProcess.ClearConsole += () => { rtb_console.Clear(); };

            // Select custom command control
            cbx_customCommand.Select();

            // Start the watcher which fires if adb devices changed
            AdbDeviceWatcher.DeviceChanged += DwAdb_DeviceChanged;
            AdbDeviceWatcher.StartDeviceWatcher();

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                cmdProcess.StopProcessing();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void CommandExecutionStarted()
        {
            BeginInvoke((MethodInvoker)delegate ()
            {
                if (tsb_AlwaysClearConsole.Checked)
                {
                    rtb_console.Clear();
                }
            });

        }

        private void DwAdb_DeviceChanged(AdbDeviceList e)
        {
            try
            {
                BeginInvoke((MethodInvoker)delegate ()
                {
                    formMethods.RefreshAdbSerialsInCombobox(e.GetDevicesList);
                    txt_DevicesAdb.Text = e.GetDevicesRaw.ToUpper().TrimEnd();
                });
            }
            catch (Exception)
            {

            }
        }

        private void Btn_consoleClear_Click(object sender, EventArgs e)
        {
            rtb_console.Clear();
        }

        private void Btn_consoleStop_Click(object sender, EventArgs e)
        {
            cmdProcess.StopProcessing();
        }

        private void Btn_executeCommand_Click(object sender, EventArgs e)
        {
            string command = cbx_customCommand.Text;

            if (!String.IsNullOrEmpty(command))
            {
                cbx_customCommand.Items.Add(command);

                cmdProcess.StartProcessing(command, formMethods.SelectedDevice());
            }
            else
            {
                MessageBox.Show("Please enter a command!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        Color clr = new Color();
        private void AppendReceivedData(object sender, DataReceivedEventArgs e)
        {
            try
            {
                BeginInvoke((MethodInvoker)delegate () { rtb_console.AppendText(e.Data + Environment.NewLine); });
                Thread.Sleep(2);
            }
            catch (Exception)
            { }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            trv_commandTreeView.ExpandAll();
            trv_commandTreeView.SelectedNode = trv_commandTreeView.Nodes[0];
        }

        private void Rtb_console_Resize(object sender, EventArgs e)
        {
            rtb_console.ScrollToCaret();
        }

        private void Trv_commandTreeView_DoubleClick(object sender, EventArgs e)
        {
            // todo add network capture tcpdump
            try
            {
                string tag;

                if ((tag = trv_commandTreeView.SelectedNode.Tag.ToString()) != null)
                {

                    if (tag.StartsWith("adb ") || tag.StartsWith("fastboot "))
                    {
                        // seriennummer für fastboot implementieren
                        cmdProcess.StartProcessing(tag, formMethods.SelectedDevice());
                    }

                    else if (tag.StartsWith("#"))
                    {
                        switch (tag)
                        {
                            case "#prop":
                                new SetProp(cmdProcess, formMethods).Show();
                                break;

                            case "#screenshot":

                                if (!String.IsNullOrEmpty(formMethods.SelectedDevice()))
                                {
                                    string timeofnow = DateTime.Now.ToString();
                                    saveFileDialog.FileName = "screenshot_" +timeofnow.Replace(' ', '_').Replace(':', '.').Replace('/','-');
                                    saveFileDialog.Filter = "PNG Image(.png)|*.png";
                                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                                    {
                                        cmdProcess.StartProcessing("adb shell screencap -p > " + saveFileDialog.FileName, formMethods.SelectedDevice());
                                    }
                                }

                                break;

                            case "#screenrecord":
                                if (screenRecord == null || screenRecord.IsDisposed)
                                {
                                    screenRecord = new ScreenRecord(cmdProcess, formMethods);
                                    screenRecord.Show();
                                }
                                else
                                {
                                    screenRecord.Focus();
                                }
                                break;

                            case "#spoofmac":
                                if (spoofMac == null || spoofMac.IsDisposed)
                                {
                                    spoofMac = new SpoofMac(cmdProcess, formMethods);
                                    spoofMac.Show();
                                }
                                else
                                {
                                    spoofMac.Focus();
                                }
                                break;

                            case "#resolution":
                                if (resolutionChange == null || resolutionChange.IsDisposed)
                                {
                                    resolutionChange = new ResolutionChange(cmdProcess, formMethods);
                                    resolutionChange.Show();
                                }
                                else
                                {
                                    resolutionChange.Focus();
                                }
                                break;

                            case "#density":
                                if (densityChange == null || densityChange.IsDisposed)
                                {
                                    densityChange = new Density(cmdProcess, formMethods);
                                    densityChange.Show();
                                }
                                else
                                {
                                    densityChange.Focus();
                                }
                                break;

                            case "#files":
                                if (fileOps == null || fileOps.IsDisposed)
                                {
                                    fileOps = new FileOps(cmdProcess, formMethods);
                                    fileOps.Show();
                                }
                                else
                                {
                                    fileOps.Focus();
                                }
                                break;

                            case "#installuninstall":
                                if (installUninstall == null || installUninstall.IsDisposed)
                                {
                                    installUninstall = new InstallUninstall(cmdProcess, formMethods);
                                    installUninstall.Show();
                                }
                                else
                                {
                                    installUninstall.Focus();
                                }
                                break;

                            case "#sideload":
                                if (sideLoad == null || sideLoad.IsDisposed)
                                {
                                    sideLoad = new Sideload(cmdProcess, formMethods);
                                    sideLoad.Show();
                                }
                                else
                                {
                                    sideLoad.Focus();
                                }
                                break;

                            case "#backuprestore":
                                if (backupRestore == null || backupRestore.IsDisposed)
                                {
                                    backupRestore = new BackupRestore(cmdProcess, formMethods);
                                    backupRestore.Show();
                                }
                                else
                                {
                                    backupRestore.Focus();
                                }
                                break;

                            case "#logcatadvanced":
                                if (logcatAdvanced == null || logcatAdvanced.IsDisposed)
                                {
                                    logcatAdvanced = new LogcatAdvanced(cmdProcess, formMethods);
                                    logcatAdvanced.Show();
                                }
                                else
                                {
                                    logcatAdvanced.Focus();
                                }
                                break;

                            case "#credits":
                                new Credits().ShowDialog();
                                break;
                        }
                    }

                }
            }
            catch (Exception) { }
        }

        private void Btn_adbRoot_Click(object sender, EventArgs e)
        {
            cmdProcess.StartProcessing("adb root", formMethods.SelectedDevice());
        }

        private void Cbx_customCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                btn_executeCommand.PerformClick();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Kill the process
            // todo rename Forms
            try
            {
                //adb.StopProcessing();
                cmdProcess.GetProcess.Kill();
                cmdProcess.GetProcess.Dispose();
            }
            catch (Exception)
            { }
        }

        private void Tsb_OpenShell_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(formMethods.SelectedDevice()))
            {
                string serial = "";

                serial += "-s " + formMethods.SelectedDevice() + " ";

                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = "/K adb " + serial + " shell",
                    }
                };

                process.Start();
            }
            else
            {
                MessageBox.Show("No device connected. Please connect a device for adb commands.", "Error - No Device Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Tsm_WirelessConnect_Click(object sender, EventArgs e)
        {
            var r = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{1,5}$");

            string ipadress = tst_IpAdress.Text;

            if (r.Match(ipadress).Success)
            {
                cmdProcess.StartProcessing("adb connect " + ipadress, "");
            }
            else
            {
                MessageBox.Show("Please enter a valid IP adress", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Tsm_WirelessDisconnect_Click(object sender, EventArgs e)
        {
            cmdProcess.StartProcessing("adb disconnect", "");
        }

        private void Tsb_KillServer_Click(object sender, EventArgs e)
        {
            formMethods.KillServer();
        }

        private void Tsb_RemountSystem_Click(object sender, EventArgs e)
        {
            cmdProcess.StartProcessing("adb remount", formMethods.SelectedDevice());
        }

        private void Tsb_AdbRoot_Click(object sender, EventArgs e)
        {
            cmdProcess.StartProcessing("adb root", formMethods.SelectedDevice());
        }

        private void Tsb_AdbUnroot_Click(object sender, EventArgs e)
        {
            cmdProcess.StartProcessing("adb unroot", formMethods.SelectedDevice());
        }

        private void Tsb_AlwaysClearConsole_Click(object sender, EventArgs e)
        {
            if (tsb_AlwaysClearConsole.Checked = !tsb_AlwaysClearConsole.Checked) ;
        }

        private void Tsb_Power_Click(object sender, EventArgs e)
        {
            switch (sender.ToString())
            {
                case "Reboot Normal":
                    cmdProcess.StartProcessing("adb reboot", formMethods.SelectedDevice());
                    break;


                case "Reboot Recovery":
                    cmdProcess.StartProcessing("adb reboot recovery", formMethods.SelectedDevice());
                    break;


                case "Reboot Bootloader":
                    cmdProcess.StartProcessing("adb reboot bootloader", formMethods.SelectedDevice());
                    break;


                case "Reboot Fastboot":
                    cmdProcess.StartProcessing("adb reboot fastboot", formMethods.SelectedDevice());
                    break;


                case "Sideload Mode":
                    cmdProcess.StartProcessing("adb reboot sideload", formMethods.SelectedDevice());
                    break;


                case "Shutdown":
                    cmdProcess.StartProcessing("adb shell reboot -p", formMethods.SelectedDevice());
                    break;


                case "Sleep":
                    cmdProcess.StartProcessing("adb shell input keyevent POWER", formMethods.SelectedDevice());
                    break;

                default:
                    break;
            }

        }
    }
}