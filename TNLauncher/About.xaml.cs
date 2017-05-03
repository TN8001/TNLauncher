using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace TNLauncher
{
    public partial class About : Window
    {
        public string AssemblyVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version.Major + "."
                                               + Assembly.GetExecutingAssembly().GetName().Version.Minor + "."
                                               + Assembly.GetExecutingAssembly().GetName().Version.Build;
        public string AssemblyName { get; } = Assembly.GetEntryAssembly().GetName().Name;
        public string AssemblyCopyright { get; } = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute))).Copyright;
        public About()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
