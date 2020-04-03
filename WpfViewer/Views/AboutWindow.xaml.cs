using System.Windows;
using Viewer.ViewModels;

namespace Viewer
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            DataContext = new AboutVM();

            InitializeComponent();
        }
    }
}
