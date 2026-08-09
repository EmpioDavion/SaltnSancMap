using SaltMapEdit;
using System;

internal class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

		void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
		{
			throw new NotImplementedException();
		}

		using MainForm form = new MainForm();
		{
			MainGame game = new MainGame(form);

			form.Show();

			game.Run();
			game.Dispose();
		}
	}
}