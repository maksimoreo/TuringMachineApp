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

using System.Diagnostics; // Debug

using TuringMachineEmulator;

namespace TuringMachineApp
{
    /// <summary>
    /// Interaction logic for TuringMachineUIControl.xaml
    /// </summary>
    public partial class TuringMachineUIControl : UserControl
    {
        private string fileName;
        public string FileName => fileName;

        public MainWindow parentWindow;

        private bool isWorking = false;
        public bool IsWorking => isWorking;

        private TuringMachineThread tmThread;

        // UI
        private string CurrentStateLabelText { set => this.CurrentStateLabel.Content = "State: " + value; }
        private string CurrentPositionLabelText { set => this.CurrentPositionLabel.Content = "Position: " + value; }
        private string CurrentStepLabelText { set => this.StepLabel.Content = "Step: " + value; }
        private string CurrentStatusLabelText { set => this.StatusLabel.Content = "Status: " + value; }

        private void SetTape(string tape, int pos)
        {
            if (pos > 0)
                TextBeforePos.Text = tape.Substring(0, pos);
            else
                TextBeforePos.Text = "";
            TextCurrentPos.Text = tape[pos].ToString();
            if (pos < tape.Length - 1)
                TextAfterPos.Text = tape.Substring(pos + 1);
            else
                TextAfterPos.Text = "";
        }

        public TuringMachineUIControl()
        {
            InitializeComponent();
        }

        #region Start Stop
        public void Start()
        {
            if (!isWorking)
            {
                tmThread.SetWorking(true);
                isWorking = true;
                StepButton.IsEnabled = false;
                StartStopButton.Content = "Stop";
                CurrentStatusLabelText = "Running";
                parentWindow.UpdateControls();

                // Send Message to worker thread
            }
        }

        public void Stop()
        {
            if (isWorking)
            {
                tmThread.SetWorking(false);
                isWorking = false;
                StepButton.IsEnabled = true;
                StartStopButton.Content = "Start";
                CurrentStatusLabelText = "Idle (Stopped)";
                parentWindow.UpdateControls();

                // Send Message to worker thread
            }
        }
        #endregion

        public void Initialize(TuringMachine tm, string name)
        {
            this.TMNameLabel.Content = name;
            tmThread = new TuringMachineThread(tm, UpdateUI);
            UpdateUIBasic();
            CurrentStatusLabelText = "Idle (Not started)";
        }

        public void UpdateUI(TuringMachine.Status status, string tape, string currentState, int currentPosition)
        {
            SetTape(tape, currentPosition);
            CurrentStateLabelText = currentState;
            CurrentPositionLabelText = currentPosition.ToString();

            if (status == TuringMachine.Status.NoInstruction || status == TuringMachine.Status.TapeBorder)
            {
                // Block controls
                Stop();
                StepButton.IsEnabled = false;
                StartStopButton.IsEnabled = false;
                CurrentStatusLabelText = "Done (" + status.ToString() + ")";
            }
        }

        public void UpdateUIBasic()
        {
            //if (tm != null)
            //{
            //    this.TMNameLabel.Content = tm.FileName;
            //    this.TapeLabel.Content = tm.Tape;
            //    CurrentPositionLabelText = tm.CurrentPosition.ToString();
            //    CurrentStateLabelText = tm.CurrentState;
            //    CurrentStepLabelText = tm.Steps.ToString();
            //}
            //else
            //{
            //    this.TMNameLabel.Content = "<Empty>";
            //    this.TapeLabel.Content = "";
            //    CurrentPositionLabelText = "-";
            //    CurrentStateLabelText = "-";
            //    CurrentStepLabelText = "-";
            //}
        }

        public void Step()
        {
            // Send Message to worker thread
            tmThread.AddSteps();
        }

        public void Close()
        {
            tmThread.StopAndDelete();
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            Step();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.CloseTM(this);
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (isWorking)
                Stop();
            else
                Start();
        }
    }
}
