using System.Threading;

using TuringMachineEmulator;

namespace TuringMachineApp
{
    class TuringMachineThread
    {
        public delegate void UpdateUICallback(Command nextCommand, string tape, string state, int currentPosition, int cursorOffsetFromTapePartialBeginning);
        public UpdateUICallback UpdateUI;

        readonly TuringMachine tm;

        // Methods
        public TuringMachineThread(TuringMachine tm, UpdateUICallback updateUIFunction)
        {
            this.tm = tm;
            UpdateUI = updateUIFunction;
            TryUpdateUI();
            Thread thread = new Thread(new ThreadStart(TMThread));
            thread.IsBackground = true;

            thread.Start();
        }

        // Thread
        #region Thread Control
        private object lock_working = new object();
        private bool control_working = false;
        private object lock_steps = new object();
        private int control_steps = 0;
        private object lock_stopAndDelete = new object();
        private bool control_stopAndDelete = false;

        public void SetWorking(bool working)
        {
            lock (lock_working)
            {
                this.control_working = working;
            }
        }

        public void AddSteps()
        {
            lock (lock_steps)
            {
                this.control_steps++;
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
                    tm.Step();
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
                UpdateUI(tm.FindNextCommand(), tm.ExtractTapeAroundCursor(offsetAroundCursor), tm.State, tm.Position, offsetAroundCursor));
        }
    }
}
