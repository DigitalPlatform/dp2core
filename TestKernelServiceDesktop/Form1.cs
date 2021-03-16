using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using dp2Kernel;
using DigitalPlatform.rms;

namespace TestKernelServiceDesktop
{
    public partial class Form1 : Form
    {
        public KernelApplication _kernelApp = null;
        public SessionInfo _sessionInfo = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button_initialApplication_Click(object sender, EventArgs e)
        {
            string kernelDirectory = DataModel.GetKernelDataDirectory();
            _kernelApp = KernelService.NewApplication(kernelDirectory);

            _sessionInfo = new SessionInfo();
        }

        private void button_test_login_Click(object sender, EventArgs e)
        {
            using (KernelService service = new KernelService(_kernelApp, _sessionInfo))
            {
                var result = service.Login("root", "");
                MessageBox.Show(this, result.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _kernelApp?.Close();
        }
    }
}
