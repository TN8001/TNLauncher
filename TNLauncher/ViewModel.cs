using Microsoft.Win32;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace TNLauncher
{
    public class ViewModel : Model
    {
        ///<summary>URIスキームのランダム部分</summary>
        public string Password
        {
            get { return UriScheme?.Replace($"{SCHEME_PREFIX}-", ""); }
            set
            {
                if(value.Count() < 4)
                    throw new AggregateException("4文字以上が必要です"); //4文字の根拠はありません
                if(!(Regex.Match(value, "^[a-z0-9]+$")).Success)
                    throw new AggregateException("[a-z0-9] 半角小文字英数字のみで入力してください");
                UriScheme = $"{SCHEME_PREFIX}-{value}";
            }
        }
        public DelegateCommand<string> AddItemCommand { get; }
        public DelegateCommand CreateRegistryCommand { get; }
        public DelegateCommand DeleteRegistryCommand { get; }
        public DelegateCommand ExportCommand { get; }

        public ViewModel()
        {
            PropertyChanged += ((s, e) =>
            {
                if(e.PropertyName == nameof(UriScheme)) OnPropertyChanged(nameof(Password));
            });

            AddItemCommand = new DelegateCommand<string>((str) => AddItem(str));
            CreateRegistryCommand = new DelegateCommand(() =>
            {
                var result = MessageBox.Show("URIスキームを変更すると、以前のブックマークが使えなくなります。\nよろしいですか？",
                    "TNLauncher", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if(result == MessageBoxResult.No)
                    return;
                DeleteUriScheme();
                RegisterUriScheme(UriScheme);
            });
            DeleteRegistryCommand = new DelegateCommand(() =>
            {
                var result = MessageBox.Show("レジストリを削除すると、以前のブックマークが使えなくなります。\nよろしいですか？",
                    "TNLauncher", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if(result == MessageBoxResult.No)
                    return;
                DeleteUriScheme();
            });
            ExportCommand = new DelegateCommand(() =>
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "Chrome HTML Document|*.html";
                dlg.FileName = "inport.html";
                if(dlg.ShowDialog() != true) return;

                Export(dlg.FileName);
            });

            Run();
        }
        ///<summary>起動時の処理 SchemeからならProcessStart後終了 そうでなければWindow表示</summary>
        private void Run()
        {
            UriScheme = GetUriScheme();
            if(UriScheme == null) //レジストリに登録がなかった場合 初回起動とみなす
            {
                if(Agreement())
                    UriScheme = CreateUriScheme();
                else
                {
                    Application.Current.MainWindow.Close();
                    return;
                }
            }

            var args = Environment.GetCommandLineArgs();
            if(args.Count() <= 1)//自身の単独起動の場合 コマンドライン1個目は自身のパス
            {

            }
            else if(args[1].StartsWith(UriScheme)) //uriスキームから起動の場合
            {
                try
                {
                    //すべての文字列がコマンドライン2個目になるっぽい?（ブラウザ依存の模様）
                    //tn-launcher...が最初に付くので取らないと無限に起動し続けてしまう
                    ProcessStart(args[1].Replace($"{UriScheme}:", ""));
                    Application.Current.MainWindow.Close();
                    //Application.Current.Shutdown(); //何が正解??
                    return;
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message, "エラー - TNLauncher");
                }
            }
            else //自身の起動にコマンドラインが付いていた場合
            {

            }
        }
        ///<summary>初回確認画面表示 同意ならtrue</summary>
        private bool Agreement() => new Welcome().ShowDialog() == true;
    }
}
