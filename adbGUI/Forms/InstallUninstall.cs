using System;
using System.Windows.Forms;

namespace adbGUI.Forms
{
    public partial class InstallUninstall : Form
    {
        private CmdProcess adb;
        private FormMethods formMethods;

        public InstallUninstall(CmdProcess adbFrm, FormMethods formMethodsFrm)
        {
            InitializeComponent();
            this.btn_InstallUninstallInstall.DragEnter += new DragEventHandler(Btn_InstallUninstallInstall_DragEnter);
            this.btn_InstallUninstallInstall.DragDrop += new DragEventHandler(Btn_InstallUninstallInstall_DragDrop);

            adb = adbFrm;
            formMethods = formMethodsFrm;
        }
        private void Btn_InstallUninstallInstall_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        private void Btn_InstallUninstallInstall_DragDrop(object sender,DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            for(int i = 0; i < s.Length; i++)
            {
                txt_InstallUninstallPackageInstall.Text = s[i];
                if (s[i].EndsWith(".apk"))
                {
                    var filename = "\"" + txt_InstallUninstallPackageInstall.Text + "\"";

                    adb.StartProcessing("adb install " + s, formMethods.SelectedDevice());
                    RefreshInstalledApps();
                }
                
            }
        }

        private void Btn_InstallUninstallInstall_Click(object sender, EventArgs e)
        {
            // Funktionen einbauen: starten, stoppen, cache leeren, apk ziehen, aktivieren, deaktivieren
            var s = "\"" + txt_InstallUninstallPackageInstall.Text + "\"";
            string serial = " -s " + formMethods.SelectedDevice();

            if (txt_InstallUninstallPackageInstall.Text != "")
            {
                adb.StartProcessing("adb install " + s, formMethods.SelectedDevice());

                RefreshInstalledApps();
            }
            else
            {
                MessageBox.Show("Please select a file!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Btn_InstallUninstallBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Filter = " .apk|*.apk";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txt_InstallUninstallPackageInstall.Text = openFileDialog.FileName;
            }
        }

        private void Btn_InstallUninstallUninstall_Click(object sender, EventArgs e)
        {
            var s = "\"" + cbx_InstallUninstallPackageUninstall.SelectedItem + "\"";

            adb.StartProcessing("adb uninstall " + s, formMethods.SelectedDevice());

            RefreshInstalledApps();
        }

        private void Btn_InstallUninstallRefreshApps_Click(object sender, EventArgs e)
        {
            groupBox1.Enabled = false;
            groupBox3.Enabled = false;
            RefreshInstalledApps();
            groupBox1.Enabled = true;
            groupBox3.Enabled = true;
        }

        private void RefreshInstalledApps()
        {
            cbx_InstallUninstallPackageUninstall.Items.Clear();

            cbx_InstallUninstallPackageUninstall.Enabled = false;

            string output = adb.StartProcessingInThread("adb shell pm list packages -3", formMethods.SelectedDevice());

            if (!String.IsNullOrEmpty(output))
            {
                foreach (var item in output.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    cbx_InstallUninstallPackageUninstall.Items.Add(item.Remove(0, 8));
                }

                cbx_InstallUninstallPackageUninstall.Sorted = true;

                if (cbx_InstallUninstallPackageUninstall.Items.Count > 0)
                {
                    cbx_InstallUninstallPackageUninstall.SelectedIndex = 0;
                }
            }


            cbx_InstallUninstallPackageUninstall.Enabled = true;
        }

        
    }
}