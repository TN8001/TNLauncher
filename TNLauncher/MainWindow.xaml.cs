using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace TNLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
        private void ListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            foreach(var file in files)
                ((ViewModel)DataContext).AddItemCommand.Execute(file);
        }
    }
}
