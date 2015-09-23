using System;
using System.Windows.Forms;

namespace ScreenCaptureExample
{
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var client = new DemoClient();
            client.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var server = new DemoServer();
            server.Show();
        }
    }
}