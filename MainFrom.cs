using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PowerConfig;

namespace testPowerConfig
{
    public partial class MainFrom : DevExpress.XtraEditors.XtraForm
    {
        public MainFrom()
        {
            InitializeComponent();
            Config.Set("Database.Server", "localhost");
            Config.Set("Database.Port", "1433");
            Config.Set("Database.Credentials.User", "sa");
            Config.Set("Database.Credentials.Password", "123456");

            // 读取
            string server = Config.Get("Database.Server");
            string port = Config.Get("Database.Port");
            string user = Config.Get("Database.Credentials.User");

            // 使用 Dynamic（需用索引器，因为属性名不能带点）
            Config.Dynamic["Database.Server"] = "192.168.1.100";
            string newServer = Config.Dynamic["Database.Server"];

        }

        private void MainFrom_Load(object sender, EventArgs e)
        {
            label1.Text = Config.Root.lab;
            textBox1.Text = Config.Get("name");
            ShowDatabaseConfig();
        }
        private void ShowDatabaseConfig()
        {
            listBox1.Items.Clear();
            foreach (string key in Config.Key("Database").GetKeys())
            {
                string value = Config.Key("Database").Key(key).GetValue();

                listBox1.Items.Add($"{key}={value}");

            }
            listBox1.Items.Add(Config.Root.Name.Nickname);
            listBox1.Items.Add(Config.Root.Database.GetValue());
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Config.Set("name", textBox1.Text);
            MessageBox.Show(Config.Root.Name.Nickname);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
