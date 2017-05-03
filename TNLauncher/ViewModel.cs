using Microsoft.Win32;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

//URIスキーム指定のイメージ
//tn-launcher:c:\hoge.exe           ＯＫ
//tn-launcher:hoge.exe              Process.Startで通れば絶対パスでなくてもＯＫ
//tn-launcher:c:\fuga.txt           関連付けができていれば実行ファイルでなくてもＯＫ 
//tn-launcher:hoge.exe?-d -f        オプション等
//tn-launcher:hoge.exe?c:\fuga.txt  ファイル指定
//tn-launcher:hoge.exe?fuga.txt     HKLM(HKCU)\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Path
//                                  に値があればそこからの相対パスはＯＫ
//tn-launcher:ho ge.exe?"fu ga.txt" スペースが入る場合


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
                    throw new AggregateException("最低4文字は入れてください"); //4文字の根拠はありません
                if(value.Count() > 200)
                    throw new AggregateException("長すぎます"); //ルートから255文字以内 https://msdn.microsoft.com/ja-jp/library/windows/desktop/ms724872(v=vs.85).aspx
                if(!(Regex.Match(value, "^[a-z0-9]+$")).Success)
                    throw new AggregateException("半角小文字英数字[a-z0-9]のみで入力してください");
                UriScheme = $"{SCHEME_PREFIX}-{value}";
            }
        }
        ///<summary>ブックマークアイテムを追加 引数:ファイルパス</summary>
        public DelegateCommand<string> AddItemCommand { get; }
        ///<summary>URIスキームを作成 変更</summary>
        public DelegateCommand CreateUriSchemeCommand { get; }
        ///<summary>レジストリを削除</summary>
        public DelegateCommand DeleteRegistryCommand { get; }
        ///<summary>ブックマークインポートファイルを作成</summary>
        public DelegateCommand ExportCommand { get; }
        public DelegateCommand ShowAboutCommand { get; }

        public ViewModel()
        {
            PropertyChanged += ((s, e) =>
            {
                if(e.PropertyName == nameof(UriScheme)) OnPropertyChanged(nameof(Password));
            });

            AddItemCommand = new DelegateCommand<string>((str) => AddItem(str));
            CreateUriSchemeCommand = new DelegateCommand(() =>
            {
                var result = MessageBox.Show("URIスキームを変更すると、作成済みのブックマークが使えなくなります。\nよろしいですか？",
                    "TNLauncher", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if(result == MessageBoxResult.No)
                    return;
                DeleteUriScheme();
                RegisterUriScheme(UriScheme);
            });
            DeleteRegistryCommand = new DelegateCommand(() =>
            {
                var result = MessageBox.Show("レジストリを削除すると、作成済みのブックマークが使えなくなります。\nよろしいですか？",
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
            ShowAboutCommand = new DelegateCommand(() =>
            {
                var dlg = new About();
                dlg.Owner = Application.Current.MainWindow;
                dlg.ShowDialog();
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
                //逆変換（インポートファイルの読み込み）は、アイコン周りが面倒そうなので実装予定なし
            }
        }
        ///<summary>初回確認画面表示 同意ならtrue</summary>
        private bool Agreement()
        {
            var dlg = new Welcome();
            return dlg.ShowDialog() == true;
        }
    }
}
