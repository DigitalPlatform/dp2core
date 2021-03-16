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
        public KernelService _service = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _service?.Dispose();
            _kernelApp?.Close();
        }

        private void button_initialApplication_Click(object sender, EventArgs e)
        {
            string kernelDirectory = DataModel.GetKernelDataDirectory();
            _kernelApp = KernelService.NewApplication(kernelDirectory);

            _service = new KernelService(_kernelApp, new SessionInfo(_kernelApp));
        }

        private void button_test_login_Click(object sender, EventArgs e)
        {
            var result = _service.Login("root", "");
            MessageBox.Show(this, result.ToString());
        }

        private void button_test_search_Click(object sender, EventArgs e)
        {
            var result = _service.Search(@"
<target list='中文图书:题名'>
    <item>
        <word>中国</word>
        <match>left</match>
        <relation>=</relation>
        <dataType>string</dataType>
    </item>
    <lang>zh</lang>
</target>",
                "default",
                "");
            MessageBox.Show(this, result.ToString());
        }
    }
}
