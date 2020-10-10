using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Desktop_Audio_Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			MainWindow wnd;
            if (e.Args.Length > 0)
            {
                wnd = new MainWindow(true, e.Args[0]);
            }
            else
            {
                wnd = new MainWindow(false);
            }
            wnd.Show();
		}
	}
}
