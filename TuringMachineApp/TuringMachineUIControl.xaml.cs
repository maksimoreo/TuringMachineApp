using System.Windows;
using System.Windows.Controls;

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

        private void SetTape(string tape, int cursorOffset)
        {
            TextBeforePos.Text = tape[..(cursorOffset - 1)];
            TextCurrentPos.Text = tape[cursorOffset].ToString();
            TextAfterPos.Text = tape[(cursorOffset + 1)..];
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

        public void UpdateUI(
            Command nextCommand,
            string tape,
            string currentState,
            int currentPosition,
            int cursorOffsetFromTapePartialBeginning)
        {
            SetTape(tape, cursorOffsetFromTapePartialBeginning);
            CurrentStateLabelText = currentState;
            CurrentPositionLabelText = currentPosition.ToString();

            if (nextCommand is null)
            {
                // Block controls
                Stop();
                StepButton.IsEnabled = false;
                StartStopButton.IsEnabled = false;
                CurrentStatusLabelText = "No instruction";
            }
            else
            {
                CurrentStatusLabelText = nextCommand.ToString();
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
