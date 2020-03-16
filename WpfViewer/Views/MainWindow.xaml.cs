using System.Windows;
using WpfViewer.ViewModels;

namespace WpfViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainVM vm = new MainVM();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
