using System;
using System.Windows.Forms;

namespace Demo.Bonjour
{
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
        }

        private void showClientButton_Click(object sender, EventArgs e)
        {
            var client = new DemoClient();
            client.Show();
        }

        private void showServerButton_Click(object sender, EventArgs e)
        {
            var server = new DemoServer();
            server.Show();
        }
    }
}