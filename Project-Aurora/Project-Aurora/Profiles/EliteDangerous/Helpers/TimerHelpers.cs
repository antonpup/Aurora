using System;
using System.Timers;

namespace Aurora.Profiles.EliteDangerous.Helpers
{
    public class DelayedMethodCaller
    {
        int _delay;
        Timer _timer = new Timer();

        public DelayedMethodCaller(int delay)
        {
            _delay = delay;
        }

        public void CallMethod(Action action)
        {
            if (!_timer.Enabled)
            {
                _timer = new Timer(_delay)
                {
                    AutoReset = false
                };
                _timer.Elapsed += (object sender, ElapsedEventArgs e) =>
                {
                    action();
                };
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                _timer.Start();
            }
        }
    }
}