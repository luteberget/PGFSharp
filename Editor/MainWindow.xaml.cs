using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetText("hello world");
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (working) return;
            var doc = textBox.Document;
            string text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
            SetText(text);
        }

        private bool working = false;
        private void SetText(string text)
        {
            try
            {
                working = true;

                var caret = textBox.CaretPosition;
                var offset = -caret.GetOffsetToPosition(caret.DocumentStart);

                textBox.Document.Blocks.Clear();
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(text));
                textBox.Document.Blocks.Add(paragraph);

                textBox.CaretPosition = textBox.CaretPosition.DocumentStart.GetPositionAtOffset(offset-1);
            }
            finally
            {
                working = false;
            }
        }
    }
}
