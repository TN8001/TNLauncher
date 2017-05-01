using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace TNLauncher
{
    public partial class Welcome : Window
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
    [MarkupExtensionReturnType(typeof(string))]
    public class TextExtension : MarkupExtension
    {
        [ConstructorArgument("FileName")]
        public string FileName { get; }
        public TextExtension(string fileName)
        {
            FileName = fileName;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var uri = new Uri($"/{GetType().Namespace};component/{FileName}", UriKind.Relative);
            using(var stream = Application.GetResourceStream(uri).Stream)
            using(var reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }
    }

}
