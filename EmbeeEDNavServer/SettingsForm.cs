using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmbeeEDNavServer
{
    public partial class SettingsForm : Form
    {
        private Config _config;

        public SettingsForm(Config config)
        {
            _config = config;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                EDDirText.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            EDDirText.Text = _config["EDFolder"];
            EDDirText.Enabled = false;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            _config["EDFolder"] = EDDirText.Text;
            _config.Save();
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
