using System.Threading;

using TuringMachineEmulator;

namespace TuringMachineApp
{
    internal class TuringMachineThread
    {
        public delegate void UpdateUICallback(Command nextCommand, string tape, string state, int currentPosition, int cursorOffsetFromTapePartialBeginning);

        private readonly UpdateUICallback updateUI;
        private readonly TuringMachine tm;

        // Methods
        public TuringMachineThread(TuringMachine tm, UpdateUICallback updateUIFunction)
        {
            this.tm = tm;
            updateUI = updateUIFunction;
            TryUpdateUI();
            Thread thread = new(new ThreadStart(TMThread))
            {
                IsBackground = true,
            };

            thread.Start();
        }

        #region Thread Control
#pragma warning disable SA1310 // Field names should not contain underscore
        private readonly object lock_working = new();
        private bool control_working = false;
        private readonly object lock_steps = new();
        private int control_steps = 0;
        private readonly object lock_stopAndDelete = new();
        private bool control_stopAndDelete = false;
#pragma warning restore SA1310 // Field names should not contain underscore

        public void SetWorking(bool working)
        {
            lock (lock_working)
            {
                control_working = working;
            }
        }

        public void AddSteps()
        {
            lock (lock_steps)
            {
                control_steps++;
            }
        }

        public void StopAndDelete()
        {
            lock (lock_stopAndDelete)
            {
                control_stopAndDelete = true;
            }
        }
        #endregion

        private void TMThread()
        {
            while (true)
            {
                // Read controls
                bool working;
                lock (lock_working)
                    working = control_working;

                int steps;
                lock (lock_steps)
                {
                    steps = control_steps;
                    if (control_steps > 0)
                        control_steps--;
                }

                bool stopAndDelete;
                lock (lock_stopAndDelete)
                    stopAndDelete = control_stopAndDelete;

                // Thread logic
                if (stopAndDelete)
                    break;

                if (steps > 0 || working)
                {
                    Thread.Sleep(20);
                    tm.TryStep();
                    TryUpdateUI();
                }
                else
                {
                    // Idle
                    Thread.Sleep(10);
                }
            }
        }

        private void TryUpdateUI()
        {
            int offsetAroundCursor = 10;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                updateUI(tm.FindNextCommand(), tm.ExtractTapeAroundCursor(offsetAroundCursor), tm.State, tm.Position, offsetAroundCursor));
        }
    }
}
