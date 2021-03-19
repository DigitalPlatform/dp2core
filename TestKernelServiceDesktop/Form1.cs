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
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.IO;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;
using DigitalPlatform.Text;
using System.Net.Http;

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

            _service = new KernelService(_kernelApp, new KernelSessionInfo(_kernelApp));
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

        private void button_test_compile_Click(object sender, EventArgs e)
        {
            // https://docs.microsoft.com/en-us/archive/msdn-magazine/2017/may/net-core-cross-platform-code-generation-with-roslyn-and-net-core
            const string code = @"using System;
using System.IO;
namespace RoslynCore
{
...
 public static class Helper
 {
  public static double CalculateCircleArea(double radius)
  {
    return radius * radius * Math.PI;
  }
  }
}";
            GenerateAssembly(code);
        }

        public void GenerateAssembly(string code)
        {
            List<string> errors = new List<string>();

            var tree = SyntaxFactory.ParseSyntaxTree(code);
            string fileName = "mylib.dll";
            // Detect the file location for the library that defines the object type
            var systemRefLocation = typeof(object).GetTypeInfo().Assembly.Location;
            // Create a reference to the library
            var systemReference = MetadataReference.CreateFromFile(systemRefLocation);
            // A single, immutable invocation to the compiler
            // to produce a library
            var compilation = CSharpCompilation.Create(fileName)
              .WithOptions(
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .AddReferences(systemReference)
              .AddSyntaxTrees(tree);
            string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            EmitResult compilationResult = compilation.Emit(path);
            if (compilationResult.Success)
            {
                // Load the assembly
                Assembly asm =
                  AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                // Invoke the RoslynCore.Helper.CalculateCircleArea method passing an argument
                double radius = 10;
                object result =
                  asm.GetType("RoslynCore.Helper").GetMethod("CalculateCircleArea").
                  Invoke(null, new object[] { radius });
                errors.Add($"Circle area with radius = {radius} is {result}");
            }
            else
            {
                foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
                {
                    string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()},Location: { codeIssue.Location.GetLineSpan()},Severity: { codeIssue.Severity}";
                    errors.Add(issue);
                }
            }

            MessageBox.Show(this, StringUtil.MakePathList(errors, "\r\n"));
        }

        private async void button_test_callTestServer_Click(object sender, EventArgs e)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:59483/");
            TestWebApiServerClient client = new TestWebApiServerClient(httpClient);
            var res = await client.LoginAsync("supervisor", "test", "parameters", "instance1");     //consume a webApi get action
            MessageBox.Show(this, res.ToString());
        }
    }
}
