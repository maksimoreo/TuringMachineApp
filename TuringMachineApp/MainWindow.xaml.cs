using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using TuringMachineEmulator;

namespace TuringMachineApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<TuringMachineUIControl> tmList = [];

        public MainWindow()
        {
            InitializeComponent();
            UpdateControls();
            StatusBarText.Text = "Ready";
        }

        private void NewWinBtn_Click(object sender, RoutedEventArgs e)
        {
            // Get filename from file dialog
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            bool? result = openFileDialog.ShowDialog();
            if (result == false)
            {
                return;
            }

            string filename = openFileDialog.FileName;
            Debug.WriteLine("selected file: " + filename);

            TuringMachine tm;

            try
            {
                tm =
                    JsonSerializer
                    .Deserialize<SerializableTuringMachine>(System.IO.File.ReadAllText(filename))
                    .ToTuringMachine();

                // tm = Parser.Parse(filename);
            }
            catch (Parser.ParseException ex)
            {
                string message = $"Invalid file: {ex.Message}";
                MessageBox.Show(this, message, "Parsing Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                throw;
            }
            catch (Exception ex)
            {
                string message = $"Error while reading from file: {ex.Message}";
                MessageBox.Show(this, message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                throw;
            }

            TuringMachineUIControl newTMWindow = new()
            {
                ParentWindow = this,
            };
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

                StartAllBtn.IsEnabled = idleMachines > 0;
                StopAllBtn.IsEnabled = workingMachines > 0;
                StepAllBtn.IsEnabled = idleMachines > 0;
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
        }

        private void StopAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tm in tmList)
                tm.Stop();
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
