
namespace TestKernelServiceDesktop
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_test = new System.Windows.Forms.TabPage();
            this.button_test_search = new System.Windows.Forms.Button();
            this.button_test_login = new System.Windows.Forms.Button();
            this.button_initialApplication = new System.Windows.Forms.Button();
            this.tabPage_compile = new System.Windows.Forms.TabPage();
            this.button_test_compile = new System.Windows.Forms.Button();
            this.tabPage_restClient = new System.Windows.Forms.TabPage();
            this.button_test_callTestServer = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage_test.SuspendLayout();
            this.tabPage_compile.SuspendLayout();
            this.tabPage_restClient.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 413);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 37);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(228, 28);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_test);
            this.tabControl1.Controls.Add(this.tabPage_compile);
            this.tabControl1.Controls.Add(this.tabPage_restClient);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 49);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 364);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage_test
            // 
            this.tabPage_test.AutoScroll = true;
            this.tabPage_test.Controls.Add(this.button_test_search);
            this.tabPage_test.Controls.Add(this.button_test_login);
            this.tabPage_test.Controls.Add(this.button_initialApplication);
            this.tabPage_test.Location = new System.Drawing.Point(4, 37);
            this.tabPage_test.Name = "tabPage_test";
            this.tabPage_test.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_test.Size = new System.Drawing.Size(792, 323);
            this.tabPage_test.TabIndex = 0;
            this.tabPage_test.Text = "Test";
            this.tabPage_test.UseVisualStyleBackColor = true;
            // 
            // button_test_search
            // 
            this.button_test_search.Location = new System.Drawing.Point(20, 139);
            this.button_test_search.Name = "button_test_search";
            this.button_test_search.Size = new System.Drawing.Size(306, 46);
            this.button_test_search.TabIndex = 2;
            this.button_test_search.Text = "Search()";
            this.button_test_search.UseVisualStyleBackColor = true;
            this.button_test_search.Click += new System.EventHandler(this.button_test_search_Click);
            // 
            // button_test_login
            // 
            this.button_test_login.Location = new System.Drawing.Point(20, 87);
            this.button_test_login.Name = "button_test_login";
            this.button_test_login.Size = new System.Drawing.Size(306, 46);
            this.button_test_login.TabIndex = 1;
            this.button_test_login.Text = "Login()";
            this.button_test_login.UseVisualStyleBackColor = true;
            this.button_test_login.Click += new System.EventHandler(this.button_test_login_Click);
            // 
            // button_initialApplication
            // 
            this.button_initialApplication.Location = new System.Drawing.Point(20, 28);
            this.button_initialApplication.Name = "button_initialApplication";
            this.button_initialApplication.Size = new System.Drawing.Size(306, 52);
            this.button_initialApplication.TabIndex = 0;
            this.button_initialApplication.Text = "Initial Application";
            this.button_initialApplication.UseVisualStyleBackColor = true;
            this.button_initialApplication.Click += new System.EventHandler(this.button_initialApplication_Click);
            // 
            // tabPage_compile
            // 
            this.tabPage_compile.Controls.Add(this.button_test_compile);
            this.tabPage_compile.Location = new System.Drawing.Point(4, 37);
            this.tabPage_compile.Name = "tabPage_compile";
            this.tabPage_compile.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_compile.Size = new System.Drawing.Size(792, 323);
            this.tabPage_compile.TabIndex = 1;
            this.tabPage_compile.Text = "Compile";
            this.tabPage_compile.UseVisualStyleBackColor = true;
            // 
            // button_test_compile
            // 
            this.button_test_compile.Location = new System.Drawing.Point(25, 59);
            this.button_test_compile.Name = "button_test_compile";
            this.button_test_compile.Size = new System.Drawing.Size(309, 54);
            this.button_test_compile.TabIndex = 0;
            this.button_test_compile.Text = "Test Compile";
            this.button_test_compile.UseVisualStyleBackColor = true;
            this.button_test_compile.Click += new System.EventHandler(this.button_test_compile_Click);
            // 
            // tabPage_restClient
            // 
            this.tabPage_restClient.Controls.Add(this.button_test_callTestServer);
            this.tabPage_restClient.Location = new System.Drawing.Point(4, 37);
            this.tabPage_restClient.Name = "tabPage_restClient";
            this.tabPage_restClient.Size = new System.Drawing.Size(792, 323);
            this.tabPage_restClient.TabIndex = 2;
            this.tabPage_restClient.Text = "Rest Client";
            this.tabPage_restClient.UseVisualStyleBackColor = true;
            // 
            // button_test_callTestServer
            // 
            this.button_test_callTestServer.Location = new System.Drawing.Point(18, 34);
            this.button_test_callTestServer.Name = "button_test_callTestServer";
            this.button_test_callTestServer.Size = new System.Drawing.Size(412, 60);
            this.button_test_callTestServer.TabIndex = 0;
            this.button_test_callTestServer.Text = "Call Test Server";
            this.button_test_callTestServer.UseVisualStyleBackColor = true;
            this.button_test_callTestServer.Click += new System.EventHandler(this.button_test_callTestServer_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "TestKernelServiceDesktop";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage_test.ResumeLayout(false);
            this.tabPage_compile.ResumeLayout(false);
            this.tabPage_restClient.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_test;
        private System.Windows.Forms.TabPage tabPage_compile;
        private System.Windows.Forms.Button button_initialApplication;
        private System.Windows.Forms.Button button_test_login;
        private System.Windows.Forms.Button button_test_search;
        private System.Windows.Forms.Button button_test_compile;
        private System.Windows.Forms.TabPage tabPage_restClient;
        private System.Windows.Forms.Button button_test_callTestServer;
    }
}

