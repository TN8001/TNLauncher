using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace TNLauncher
{
    public class ViewModel : BindableBase
    {
        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();
        public string UriScheme { get { return _UriScheme; } set { if(SetProperty(ref _UriScheme, value)) OnPropertyChanged(nameof(Password)); } }
        private string _UriScheme;
        public string Password
        {
            get { return _UriScheme.Replace($"{SCHEME_PREFIX}-", ""); }
            set
            {
                if(value.Count() < 4)
                    throw new AggregateException("4文字以上が必要です");
                if(!(Regex.Match(value, "^[a-z0-9]+$")).Success)
                    throw new AggregateException("[a-z0-9] 半角小文字英数字のみで入力してください");
                else
                    UriScheme = $"{SCHEME_PREFIX}-{value}";
            }
        }
        public DelegateCommand ExportCommand { get; }
        public DelegateCommand CreateRegistryCommand { get; }
        public DelegateCommand DeleteRegistryCommand { get; }

        private const string GUID = "{61C94E2B-8FFD-43C9-AB3F-4213BB5D3017}";
        private const string SCHEME_PREFIX = "tn-launcher";
        private string NewUriScheme => $"{SCHEME_PREFIX}-{Path.GetRandomFileName().Replace(".", "")}";

        public ViewModel()
        {
            UriScheme = GetScheme();
            if(UriScheme == null)
            {
                Application.Current.MainWindow.Close();
                return;
            }

            ExportCommand = new DelegateCommand(() =>
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "Chrome HTML Document|*.html";
                dlg.FileName = "inport.html";
                if(dlg.ShowDialog() != true) return;

                Export(dlg.FileName);
            });
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

            var args = Environment.GetCommandLineArgs();
            if(args.Count() <= 1)//自身の単独起動の場合 コマンドライン1個目は自身のパス
            {

            }
            else if(args[1].StartsWith(UriScheme)) //uriスキームから起動の場合
            {
                //すべての文字列がコマンドライン2個目になるっぽい?（ブラウザ依存の模様）
                //tn-launcher...が最初に付くので取らないと無限に起動し続けてしまう
                ProcessStart(args[1].Replace($"{UriScheme}:", ""));
            }
            else //自身の起動にコマンドラインが付いていた場合
            {

            }
        }
        public void AddItem(string path)
        {
            var item = new Item();
            if(Win32.GetFileInfo(path, ref item))
                Items.Add(item);
        }
        private void Export(string path)
        {
            var text = @"<!DOCTYPE NETSCAPE-Bookmark-file-1>
<!-- Created by TNLauncher
     DO NOT EDIT! -->
<META HTTP-EQUIV=""Content-Type"" CONTENT=""text/html; charset=UTF-8"">
<TITLE>Bookmarks</TITLE>
<H1>Bookmarks</H1>
<DL><p>
";
            foreach(var item in Items)
            {
                var commandLine = "";
                if(!string.IsNullOrEmpty(item.CommandLine))
                    commandLine = $"?{item.CommandLine}";
                text += $"<DT><A HREF=\"{UriScheme}:{item.Path}{commandLine}\" "
                      + $"ICON=\"data:image/png;base64,{item.Base64}\">{item.Title}</A>\n";
            }
            text += "</DL><p>\n";
            using(var sw = new StreamWriter(path, false, Encoding.UTF8))
                sw.Write(text);

            DeleteUriScheme();
            RegisterUriScheme(UriScheme);
        }
        ///<summary>パス?コマンドライン と来る前提でProcess.Start後Shutdown</summary>
        private void ProcessStart(string str)
        {
            var unescape = Uri.UnescapeDataString(str);
            var split = unescape.Split('?');
            var path = split[0];
            var commandLine = split.ElementAtOrDefault(1); ;

            try
            {
                Process.Start(path, commandLine);
                Application.Current.MainWindow.Close();
                //Application.Current.Shutdown();
            }
            catch(Exception e) { MessageBox.Show(e.Message, "エラー - TNLauncher"); }
        }

        private string FirstBoot()
        {
            if(new Welcome().ShowDialog() == true)
                return RegisterUriScheme(NewUriScheme);
            return null;
        }
        ///<summary>レジストリからURIスキームを取得 無ければ新規作成</summary>
        ///<remarks>不整合があった場合上書きする 複数登録されていた場合は最初のキーを採用する</remarks>
        private string GetScheme()
        {
            //HKCU\SOFTWARE\Classes
            using(var classesKey = Registry.CurrentUser.OpenSubKey($"Software\\Classes", false))
            {
                if(classesKey == null) return FirstBoot(); //レジストリに登録がなかった場合新規作成

                var keys = classesKey.GetSubKeyNames();
                //複数登録がある可能性があるが最初のを採用 不整合が起きた場合はGUI上での変更で対応
                keys = keys.Where(x => x.StartsWith(SCHEME_PREFIX)).ToArray();
                foreach(var scheme in keys)
                {
                    //HKCU\SOFTWARE\Classes\tn-launcher で始まるキー
                    using(var schemeKey = classesKey.OpenSubKey(scheme, true))
                    //HKCU\SOFTWARE\Classes\tn-launcher-********\shell\open\command
                    using(var CommandKey = schemeKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command"))
                    {
                        var g = schemeKey.GetValue("GUID") as string;
                        if(g != GUID) continue;

                        var s = schemeKey.GetValue("") as string;
                        if(s != $"URL:{scheme}")
                            schemeKey.SetValue("", $"URL:{scheme}");

                        var p = schemeKey.GetValue("URL Protocol") as string;
                        if(p != "")
                            schemeKey.SetValue("URL Protocol", "");

                        var exePath = Assembly.GetExecutingAssembly().Location;
                        var e = CommandKey.GetValue("") as string;
                        if(e != $"\"{exePath}\" \"%1\"")
                            CommandKey.SetValue("", $"\"{exePath}\" \"%1\"");

                        return scheme;
                    }
                }
            }
            return FirstBoot(); //レジストリに登録がなかった場合新規作成
        }
        ///<summary>自分で書いたレジストリを削除(GUIDで確認)</summary>
        private void DeleteUriScheme()
        {
            //HKCU\SOFTWARE\Classes
            using(var classesKey = Registry.CurrentUser.OpenSubKey($"Software\\Classes", true))
            {
                if(classesKey == null) return;

                var keys = classesKey.GetSubKeyNames();
                keys = keys.Where(x => x.StartsWith(SCHEME_PREFIX)).ToArray();
                foreach(var scheme in keys)
                {
                    //HKCU\SOFTWARE\Classes\tn-launcher で始まるキー
                    using(var schemeKey = classesKey.OpenSubKey(scheme, false))
                    {
                        var guid = schemeKey.GetValue("GUID") as string;
                        if(guid == GUID)
                            classesKey.DeleteSubKeyTree(scheme);
                    }
                }
            }
        }
        ///<summary>レジストリにURIスキームを登録</summary>
        private string RegisterUriScheme(string scheme)
        {
            //HKCU\SOFTWARE\Classes\tn-launcher-********
            using(var schemeKey = Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("Classes").CreateSubKey(scheme))
            //HKCU\SOFTWARE\Classes\tn-launcher-********\shell\open\command
            using(var CommandKey = schemeKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command"))
            {
                schemeKey.SetValue("", $"URL:{scheme}");
                schemeKey.SetValue("URL Protocol", "");
                schemeKey.SetValue("GUID", GUID);

                var exePath = Assembly.GetExecutingAssembly().Location;
                CommandKey.SetValue("", $"\"{exePath}\" \"%1\"");
            }
            return scheme;
        }
    }
}
