using System.Windows.Media.Imaging;

namespace TNLauncher
{
    ///<summary>ブックマークアイテムモデル</summary>
    public class Item : BindableBase
    {
        ///<summary>Process.Startするファイルパス</summary>
        public string Path { get { return _Path; } set { if(SetProperty(ref _Path, value)) OnPropertyChanged(nameof(FileName)); } }
        private string _Path;
        ///<summary>ファイル名 ListBoxに表示する用</summary>
        public string FileName { get { return System.IO.Path.GetFileName(Path); } }
        ///<summary>ブックマークに表示する名前</summary>
        public string Title { get { return _Title; } set { SetProperty(ref _Title, value); } }
        private string _Title;
        ///<summary>小アイコン(16*16)をpngにしてBase64エンコード 変更してもアイコンの更新はしない</summary>
        public string Base64 { get { return _Base64; } set { SetProperty(ref _Base64, value); } }
        private string _Base64;
        ///<summary>Process.Startするファイルに渡すコマンドライン</summary>
        public string CommandLine { get { return _CommandLine; } set { SetProperty(ref _CommandLine, value); } }
        private string _CommandLine;

        ///<summary>ファイルの種類 GUIについでに表示するだけ</summary>
        public string Type { get; set; }
        ///<summary>ListBoxに表示するアイコン用</summary>
        public BitmapSource Image { get; set; }
    }
}
