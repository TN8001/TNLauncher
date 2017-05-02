using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TNLauncher
{
    public class Model : BindableBase
    {
        ///<summary>ブックマークアイテムのリスト</summary>
        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();
        ///<summary>URIスキーム コロンは含んでいない</summary>
        public string UriScheme { get { return _UriScheme; } set { SetProperty(ref _UriScheme, value); } }
        private string _UriScheme;

        protected const string SCHEME_PREFIX = "tn-launcher";

        ///<summary>ランダムなスキームを新規作成</summary>
        private string NewUriScheme => $"{SCHEME_PREFIX}-{Path.GetRandomFileName().Replace(".", "")}";
        private const string GUID = "{61C94E2B-8FFD-43C9-AB3F-4213BB5D3017}";

        public Model() { }

        ///<summary>レジストリからURIスキームを取得 無ければnull</summary>
        ///<remarks>不整合があった場合上書きする 複数登録されていた場合は最初のキーを採用する</remarks>
        protected string GetUriScheme()
        {
            //HKCU\SOFTWARE\Classes
            using(var classesKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes", false))
            {
                if(classesKey == null) return null; //レジストリに登録がなかった場合

                var keys = classesKey.GetSubKeyNames();
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

                        //複数登録がある可能性があるが最初のを採用 不整合が起きた場合はGUI上での変更で対応
                        return scheme;
                    }
                }
            }
            return null; //レジストリに登録がなかった場合
        }
        ///<summary>新規でスキーム作成 レジストリ登録</summary>
        protected string CreateUriScheme() => RegisterUriScheme(NewUriScheme);
        ///<summary>レジストリにURIスキームを登録</summary>
        protected string RegisterUriScheme(string scheme)
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
        ///<summary>自分で書いたレジストリを削除(GUIDで確認)</summary>
        protected void DeleteUriScheme()
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
        ///<summary>ファイル名からブックマークアイテムを作成し追加</summary>
        protected void AddItem(string path)
        {
            var item = new Item();
            if(Win32.GetFileInfo(path, ref item))
                Items.Add(item);
        }
        ///<summary>パス?コマンドライン と来る前提でProcess.Start</summary>
        protected void ProcessStart(string str)
        {
            var unescape = Uri.UnescapeDataString(str);
            var split = unescape.Split('?');
            var path = split[0];
            var commandLine = split.ElementAtOrDefault(1); ;

            Process.Start(path, commandLine);
        }
        ///<summary>指定パスにNetscape Bookmark Formatで書き出し。</summary>
        protected void Export(string path)
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
    }
}
