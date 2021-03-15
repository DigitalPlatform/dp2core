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
        public HostInfo _hostInfo = null;
        public SessionInfo _sessionInfo = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button_initialApplication_Click(object sender, EventArgs e)
        {
            _hostInfo = new HostInfo
            {
                DataDir = "",
                InstanceName = ""
            };

            _sessionInfo = new SessionInfo();
        }
    }
}
