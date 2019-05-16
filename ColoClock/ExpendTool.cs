using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoClock
{
    public static class ExpendTool
    {
        public static TimeSpan TimeSpan_Now()
        {
            return new TimeSpan(0,DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, 0);
        }
    }
}
