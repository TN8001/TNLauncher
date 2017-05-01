//http://palepoli.skr.jp/tips/dotnet/shfileinfo.php
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

namespace TNLauncher
{
    public class Win32
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFOW
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
        private enum ShellFileInfoFlags : uint
        {
            SHGFI_ICON = 0x000000100,
            SHGFI_DISPLAYNAME = 0x000000200,
            SHGFI_TYPENAME = 0x000000400,
            SHGFI_ATTRIBUTES = 0x000000800,
            SHGFI_ICONLOCATION = 0x000001000,
            SHGFI_EXETYPE = 0x000002000,
            SHGFI_SYSICONINDEX = 0x000004000,
            SHGFI_LINKOVERLAY = 0x000008000,
            SHGFI_SELECTED = 0x000010000,
            SHGFI_ATTR_SPECIFIED = 0x000020000,
            SHGFI_LARGEICON = 0x000000000,
            SHGFI_SMALLICON = 0x000000001,
            SHGFI_OPENICON = 0x000000002,
            SHGFI_SHELLICONSIZE = 0x000000004,
            SHGFI_PIDL = 0x000000008,
            SHGFI_USEFILEATTRIBUTES = 0x000000010
        }
        private enum FileAttributesFlags : uint
        {
            FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
            FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,
            FILE_ATTRIBUTE_HIDDEN = 0x00000002,
            FILE_ATTRIBUTE_NORMAL = 0x00000080,
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
            FILE_ATTRIBUTE_OFFLINE = 0x00001000,
            FILE_ATTRIBUTE_READONLY = 0x00000001,
            FILE_ATTRIBUTE_SYSTEM = 0x00000004,
            FILE_ATTRIBUTE_TEMPORARY = 0x00000100
        }
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern IntPtr SHGetFileInfoW(string pszPath, FileAttributesFlags dwFileAttributes, ref SHFILEINFOW psfi, uint cbSizeFileInfo, ShellFileInfoFlags uFlags);
        public static bool GetFileInfo(string fileName, ref Item item)
        {
            if(!File.Exists(fileName))
                return false;

            var info = new SHFILEINFOW();
            SHGetFileInfoW(fileName, FileAttributesFlags.FILE_ATTRIBUTE_NORMAL,
                    ref info, (uint)Marshal.SizeOf(info),
                    ShellFileInfoFlags.SHGFI_ICON | ShellFileInfoFlags.SHGFI_SMALLICON |
                    ShellFileInfoFlags.SHGFI_TYPENAME | ShellFileInfoFlags.SHGFI_DISPLAYNAME);

            item.Image = Imaging.CreateBitmapSourceFromHIcon(info.hIcon, Int32Rect.Empty,
                                                        BitmapSizeOptions.FromEmptyOptions());
            item.Type = info.szTypeName;
            item.Title = Path.GetFileNameWithoutExtension(info.szDisplayName);
            item.Path = fileName;
            item.Base64 = item.Image.ToBase64String();

            return true;
        }
    }
    static class BitmapSourceExtensions
    {
        public static string ToBase64String(this BitmapSource bitmap)
        {
            var encoder = new PngBitmapEncoder();
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using(var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }

}