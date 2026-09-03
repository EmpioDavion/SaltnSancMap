#:sdk Microsoft.NET.Sdk.WindowsDesktop
#:property UseWindowsForms=true

using System;
using System.IO;
using System.Windows.Forms;

class Program
{
	[STAThread]
	static void Main()
	{
		OpenFileDialog ofd = new OpenFileDialog()
		{
			Filter = $"Executable Files|*.exe",
			Title = "Select salt.exe File"
		};

		if (ofd.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(ofd.FileName))
			File.WriteAllText("../.salt", Path.GetDirectoryName(ofd.FileName));
	}
}
