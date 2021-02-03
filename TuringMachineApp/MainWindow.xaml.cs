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

using System.Diagnostics;

namespace TuringMachineApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<TuringMachineUIControl> tmList = new List<TuringMachineUIControl>();
        public MainWindow()
        {
            InitializeComponent();
            UpdateControls();
            this.StatusBarText.Text = "Ready";
        }

        private void NewWinBtn_Click(object sender, RoutedEventArgs e)
        {
            // Get filename from file dialog
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == false)
            {
                return;
            }

            string filename = openFileDialog.FileName;
            Debug.WriteLine("selected file: " + filename);

            TuringMachineApp.TuringMachine tm = new TuringMachineApp.TuringMachine();
            TuringMachineApp.TuringMachine.ParseError parseError = tm.ReadFromFile(filename);

            if (parseError != null)
            {
                // Display error
                MessageBox.Show(this, "Error while reading from file!", "Parsing Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);

                if (parseError != null)
                {
                    Debug.WriteLine("|  Klaida skaitant is failo " + filename + ':');

                    while (parseError != null)
                    {
                        Debug.WriteLine("|  " + parseError.message);
                        parseError = parseError.causedBy;
                    }
                    
                    return;
                }

                return;
            }

            TuringMachineUIControl newTMWindow = new TuringMachineUIControl();
            newTMWindow.parentWindow = this;
            newTMWindow.Initialize(tm, filename);

            this.TMContainer.Children.Add(newTMWindow);
            tmList.Add(newTMWindow);
            UpdateControls();
        }

        public void UpdateControls()
        {
            if (tmList.Count == 0)
            {
                StartAllBtn.IsEnabled = false;
                StopAllBtn.IsEnabled = false;
                StepAllBtn.IsEnabled = false;
                CloseAllBtn.IsEnabled = false;
            }
            else
            {
                int workingMachines = 0, idleMachines = 0;
                foreach (var tm in tmList)
                {
                    if (tm.IsWorking)
                        workingMachines++;
                    else
                        idleMachines++;
                }

                StartAllBtn.IsEnabled = idleMachines > 0 ? true : false;
                StopAllBtn.IsEnabled = workingMachines > 0 ? true : false;
                StepAllBtn.IsEnabled = idleMachines > 0 ? true : false;
                CloseAllBtn.IsEnabled = true;
            }
        }

        public void CloseTM(TuringMachineUIControl tm)
        {
            tmList.RemoveAll(x => x == tm);
            this.TMContainer.Children.Remove(tm);
            tm.Close();
            UpdateControls();
        }

        private void StartAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tm in tmList)
                tm.Start();
            //UpdateControls();
        }

        private void StopAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tm in tmList)
                tm.Stop();
            //UpdateControls();
        }

        private void StepAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tm in tmList)
                tm.Step();
            UpdateControls();
        }

        private void CloseAllBtn_Click(object sender, RoutedEventArgs e)
        {
            while (tmList.Count > 0)
            {
                CloseTM(tmList[0]);
            }
        }
    }
}
