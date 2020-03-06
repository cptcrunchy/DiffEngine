using Microsoft.Win32;
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
using DifferenceEngine;
using System.Collections;

namespace DiffCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DiffEngineLevel diffEngineLevel;
        public MainWindow()
        {
            InitializeComponent();
        }

        public static string GetFileName()
        {

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            return (dlg.ShowDialog() == dlg.CheckFileExists) ? dlg.FileName : string.Empty;

        }
        private void ValidateFile(string fileName, TextBox input)
        {
            if(!File.Exists(fileName))
            {
                MessageBox.Show(string.Format("{0} is invalid.", fileName), "Invalid File");
                input.Focus();
                return;
            }
        }
        
        private void DisplayTextDiff(string sourceFile, string destFile)
        {
            try
            {

                DiffListTextFile sLF = new DiffListTextFile(sourceFile);
                DiffListTextFile dLF = new DiffListTextFile(destFile);
                Engine diffEngine = new Engine();

                double time = diffEngine.ProcessDiff(sLF, dLF);
                ArrayList report = diffEngine.DiffReport();

                ResultsWindow resultsWindow = new ResultsWindow(sLF, dLF, report, time);
                resultsWindow.Show();
            }
            catch (Exception ex)
            {
                string temp = string.Format("{0}{1}{1}===STACKTRACE==={1}{2}", ex.Message, Environment.NewLine, ex.StackTrace);
                MessageBox.Show(temp, "Compare Error");
                throw;
            }
            
        }

        private void BtnSourceFile_Click(object sender, RoutedEventArgs e)
        {
            txtSourceEditor.Text = GetFileName();
        }

        private void BtnDestFile_Click(object sender, RoutedEventArgs e)
        {
            txtDestEditor.Text = GetFileName();
        }


        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            string sourceFile = txtSourceEditor.Text.Trim();
            string destFile = txtDestEditor.Text.Trim();

            ValidateFile(sourceFile, txtSourceEditor);
            ValidateFile(destFile, txtDestEditor);

            var diffLevelRadioBtns = new List<RadioButton> { RbFast, RbMedium, RbSlow };

            RadioButton checkedDiffLevel = diffLevelRadioBtns.FirstOrDefault( (RadioButton rb) => rb.IsChecked == true);

            if (checkedDiffLevel.Tag.Equals("Fast")) diffEngineLevel = DiffEngineLevel.Fast;
            if (checkedDiffLevel.Tag.Equals("Medium")) diffEngineLevel = DiffEngineLevel.Medium;
            if (checkedDiffLevel.Tag.Equals("Slow")) diffEngineLevel = DiffEngineLevel.Slow;

            DisplayTextDiff(sourceFile, destFile);
        }
    }
}
