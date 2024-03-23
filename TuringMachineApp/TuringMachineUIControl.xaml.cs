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
        public MainWindow parentWindow;

        private bool isWorking = false;
        public bool IsWorking => isWorking;

        private TuringMachineThread tmThread;

        // UI
        private string CurrentStateLabelText { set => CurrentStateLabel.Content = "State: " + value; }
        private string CurrentPositionLabelText { set => CurrentPositionLabel.Content = "Position: " + value; }
        private string CurrentStepLabelText { set => StepLabel.Content = "Step: " + value; }
        private string CurrentStatusLabelText { set => StatusLabel.Content = "Status: " + value; }

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
            if (isWorking) return;

            tmThread.SetWorking(true);
            isWorking = true;
            StepButton.IsEnabled = false;
            StartStopButton.Content = "Stop";
            CurrentStatusLabelText = "Running";
            parentWindow.UpdateControls();
        }

        public void Stop()
        {
            if (!isWorking) return;

            tmThread.SetWorking(false);
            isWorking = false;
            StepButton.IsEnabled = true;
            StartStopButton.Content = "Start";
            CurrentStatusLabelText = "Idle (Stopped)";
            parentWindow.UpdateControls();
        }
        #endregion

        public void Initialize(TuringMachine tm, string name)
        {
            TMNameLabel.Content = name;
            tmThread = new TuringMachineThread(tm, UpdateUI);
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

        public void Step()
        {
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
