using Microsoft.Win32;
using NfcCardDumpConverter.Models;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace NfcCardDumpFileTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private NfcCard ReadCardFile(string filePath)
        {
            if (File.ReadAllText(filePath).StartsWith("+Sector"))
            {
                return new RfidToolsDump(filePath);
            }
            else
            {
                return new RawDump(filePath);
            }
        }

        private OpenFileDialog CreateOpenDumpDialog()
        {
            return new OpenFileDialog()
            {
                Title = "Open Dump File",
                DefaultExt = ".dump",
                Filter = "Dump file (.dump)|*.dump",
            };
        }

        private SaveFileDialog CreateSaveDumpDialog()
        {
            return new SaveFileDialog()
            {
                Title = "Save Dump File",
                DefaultExt = ".dump",
                Filter = "Dump file (.dump)|*.dump",
            };
        }

        private bool CheckIsPathValid(string filePath)
        {
            try
            {
                File.Exists(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private void filePathABrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = CreateOpenDumpDialog();
            if (dialog.ShowDialog() == true)
            {
                filePathA.Text = dialog.FileName;
            }
        }

        private void filePathBBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = CreateOpenDumpDialog();
            if (dialog.ShowDialog() == true)
            {
                filePathB.Text = dialog.FileName;
            }
        }

        private void saveAToBAsRaw_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(filePathA.Text))
            {
                MessageBox.Show("File A is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!CheckIsPathValid(filePathB.Text))
            {
                MessageBox.Show("Path B is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var card = ReadCardFile(filePathA.Text);
            ((RawDump)card).Write(filePathB.Text);
            MessageBox.Show("Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void saveAToBAsRfid_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(filePathA.Text))
            {
                MessageBox.Show("File A is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!CheckIsPathValid(filePathB.Text))
            {
                MessageBox.Show("Path B is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var card = ReadCardFile(filePathA.Text);
            ((RfidToolsDump)card).Write(filePathB.Text);
            MessageBox.Show("Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void saveAAsRaw_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(filePathA.Text))
            {
                MessageBox.Show("File A is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = CreateSaveDumpDialog();
            if (dialog.ShowDialog() == true)
            {
                var card = ReadCardFile(filePathA.Text);
                ((RawDump)card).Write(dialog.FileName);
                MessageBox.Show("Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void saveAAsRfid_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(filePathA.Text))
            {
                MessageBox.Show("File A is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = CreateSaveDumpDialog();
            if (dialog.ShowDialog() == true)
            {
                var card = ReadCardFile(filePathA.Text);
                ((RfidToolsDump)card).Write(dialog.FileName);
                MessageBox.Show("Done", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void compareAAndB_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(filePathA.Text))
            {
                MessageBox.Show("File A is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(filePathB.Text))
            {
                MessageBox.Show("File B is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var fileA = ReadCardFile(filePathA.Text);
            var fileB = ReadCardFile(filePathB.Text);

            compareResult.Document.Blocks.Clear();

            if (fileA.Length != fileB.Length)
            {
                var para = new Paragraph();
                para.Inlines.Add("File length does not match");
                compareResult.Document.Blocks.Add(para);
                return;
            }

            if (fileA.Length % 64 != 0)
            {
                var para = new Paragraph();
                para.Inlines.Add("File length is not based on 64-byte sector");
                compareResult.Document.Blocks.Add(para);
                return;
            }

            var resultPara = new Paragraph();
            compareResult.Document.Blocks.Add(resultPara);
            bool result = true;
            for (int sector = 0; sector < fileA.Length / 64; sector++)
            {
                var para = new Paragraph();
                para.Inlines.Add(new Run() { Text = $"Sector {sector}" });
                compareResult.Document.Blocks.Add(para);

                for (int line = 0; line < 4; line++)
                {
                    var lineA = new Paragraph();
                    var lineB = new Paragraph();

                    lineA.Inlines.Add("A: ");
                    lineB.Inlines.Add("B: ");

                    for (int lineOffset = 0; lineOffset < 16; lineOffset++)
                    {
                        byte byteA = fileA.RawData[sector * 64 + line * 16 + lineOffset];
                        byte byteB = fileB.RawData[sector * 64 + line * 16 + lineOffset];
                        if (byteA != byteB)
                        {
                            result = false;
                        }
                        var brush = (byteA == byteB) ? new SolidColorBrush(Color.FromRgb(0, 0, 0)) : new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        lineA.Inlines.Add(new InlineUIContainer()
                        {
                            Child = new TextBlock() { Text = NfcCard.FromByteToDoubleChar(byteA) + " ", Foreground = brush }
                        });
                        lineB.Inlines.Add(new InlineUIContainer()
                        {
                            Child = new TextBlock() { Text = NfcCard.FromByteToDoubleChar(byteB) + " ", Foreground = brush }
                        });
                    }
                    compareResult.Document.Blocks.Add(lineA);
                    compareResult.Document.Blocks.Add(lineB);
                }
                para = new Paragraph();
                para.Inlines.Add(new Run() { Text = "".PadLeft(51, '=') });
                compareResult.Document.Blocks.Add(para);
            }

            resultPara.Inlines.Add("Result: ");
            resultPara.Inlines.Add(result ? "Identical" : "NOT identical");
        }

        private void filePathA_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                e.Handled = true;
                filePathA.Text = files[0];
            }
        }

        private void filePathB_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                e.Handled = true;
                filePathB.Text = files[0];
            }
        }

        private void filePathA_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void filePathB_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
    }
}
