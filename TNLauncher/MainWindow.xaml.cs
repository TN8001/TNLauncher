using System.Windows;

namespace TNLauncher
{
    //uriスキーム指定のイメージ
    //tn-launcher:c:\hoge.exe           ＯＫ
    //tn-launcher:hoge.exe              Process.Startで通れば絶対パスでなくてもＯＫ
    //tn-launcher:c:\fuga.txt           関連付けができていれば実行ファイルでなくてもＯＫ 
    //tn-launcher:hoge.exe?-d -f        オプション等
    //tn-launcher:hoge.exe?c:\fuga.txt  ファイル指定
    //tn-launcher:hoge.exe?fuga.txt     HKLM(HKCU)\SOFTWARE\Microsoft\
    //                                  Windows\CurrentVersion\App Paths\Path
    //                                  に値があればそこからの相対パスはＯＫ
    //tn-launcher:ho ge.exe?"fu ga.txt" スペースが入る場合


    public partial class MainWindow : Window
    {
        private ViewModel vm => DataContext as ViewModel;
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
                vm.AddItem(file);
        }
    }
}
