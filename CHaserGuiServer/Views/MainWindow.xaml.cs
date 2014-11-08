using Microsoft.Win32;
using Oika.Apps.CHaserGuiServer.Messages;
using Oika.Apps.CHaserGuiServer.MVVM;
using Oika.Apps.CHaserGuiServer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Oika.Apps.CHaserGuiServer.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TextBoxLogger.SetTarget(this.txtLog);

            this.DataContext = new MainWindowViewModel();

            Messenger.Instance.Register<OpenFileDialogMessage>(this, onFileDialogMessageReceived);
        }

        private void onFileDialogMessageReceived(OpenFileDialogMessage msg)
        {
            var dlg = new OpenFileDialog();
            dlg.FileName = msg.FileName;
            dlg.Filter = msg.FilterText;
            msg.Result = dlg.ShowDialog();
            msg.FileName = dlg.FileName;
        }
    }
}
