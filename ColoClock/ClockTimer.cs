using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;

namespace ColoClock
{
    class ClockTimer:Timer
    {
        public DateTime Now_Time { get; set; }
        public TimeSpan Add_Time { get; set; }
        public ClockTimer() : this(DateTime.Now,TimeSpan.FromSeconds(1)) { }
        public ClockTimer(DateTime date):this(date, TimeSpan.FromSeconds(1)) { }
        public ClockTimer(DateTime date, TimeSpan add_time)
        {
            Now_Time = date;
            this.AutoReset = true;
            this.Interval = 1000;
            this.Elapsed += Time_Add;
            this.Add_Time = add_time;
            this.Enabled = true;
            this.Start();
        }

        private void Time_Add(object sender, ElapsedEventArgs e)
        {
            lock(this)
            {
                Now_Time += this.Add_Time;
            }
        }

        public void Add_Elapsed_Event(object control, ClockElapsedTask_Handle clockTask)
        {
            try
            {
                Control c = control as Control;
                this.Elapsed += (o, e) => c.Dispatcher.Invoke(() => clockTask());
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        ~ClockTimer()
        {
        }
    }
}
