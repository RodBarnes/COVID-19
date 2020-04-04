using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Viewer.ViewModels;
using Common;

namespace Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainVM vm = new MainVM();
        private static AboutProperties aboutProperties = new AboutProperties();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;
            aboutProperties = vm.AboutProperties;
        }

        #region System Menu

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddMenuItemsToSystemMenu();
        }

        // Related to managing the System Menu
        private IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }

        private const int AboutMenuID = 1000;
        //private const Int32 ScrapeMenuID = 1001;

        private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg == WM_SYSCOMMAND)
            {
                // Execute the appropriate code for the clicked System Menu item
                switch (wParam.ToInt32())
                {
                    case AboutMenuID:
                        AboutWindow wdw = new AboutWindow(aboutProperties);
                        wdw.ShowDialog();
                        handled = true;
                        break;
                        //case ScrapeMenuID:
                        //    var content = vm.GetPageHtml(browser.Document);
                        //    vm.WriteToFile(content);
                        //    MessageBox.Show("\"Settings\" was clicked");
                        //    handled = true;
                        //    break;
                }
            }

            return IntPtr.Zero;
        }

        private void AddMenuItemsToSystemMenu()
        {
            // Get the Handle for the Forms System Menu
            IntPtr systemMenuHandle = GetSystemMenu(this.Handle, false);

            // Create new System Menu items just before the Close menu item
            InsertMenu(systemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty); // separator
            InsertMenu(systemMenuHandle, 6, MF_BYPOSITION, AboutMenuID, "About");
            //InsertMenu(systemMenuHandle, 7, MF_BYPOSITION, ScrapeMenuID, "Scrape");

            // Attach the WndProc handler
            HwndSource source = HwndSource.FromHwnd(this.Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, int wPosition, int wFlags, int wIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern bool AppendMenu(IntPtr hMenu, int wPosition, int wFlags, int wIDNewItem, string lpNewItem);

        // Constants
        public const int WM_SYSCOMMAND = 0x112;
        public const int MF_SEPARATOR = 0x800;
        public const int MF_BYPOSITION = 0x400;
        public const int MF_STRING = 0x0;

        #endregion
    }
}
