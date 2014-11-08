using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Oika.Apps.CHaserGuiServer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private static App _current;
        public static new App Current
        {
            get
            {
                return _current;
            }
        }

        public string StartUpPath;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _current = this;

            var exePath = Path.GetFullPath(Environment.GetCommandLineArgs()[0]);
            this.StartUpPath = Path.GetDirectoryName(exePath);
        }
    }
}
