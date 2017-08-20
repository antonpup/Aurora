using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Aurora.Utils
{
    public class TimedListObject
    {
        public object item;
        Timer timer;
        List<TimedListObject> mainList;

        public TimedListObject(object item, int duration, List<TimedListObject> list)
        {
            mainList = list;
            this.item = item;

            if(duration != 0)
            {
                timer = new Timer(duration);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(mainList.Contains(this))
            {
                mainList.Remove(this);
                if (item is IDisposable)
                    ((IDisposable)item).Dispose();
                timer.Elapsed -= Timer_Elapsed;
            }
                
        }

        public void AdjustDuration(int duration)
        {
            timer.Stop();
            timer.Interval = duration;
            timer.Start();
        }
    }
}
