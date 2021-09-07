using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleYoutubeDownloader
{
    class ProgressAction : IProgress<double>
    {
        Action<double> action;

        public ProgressAction(Action<double> action)
        {
            this.action = action;
        }

        public void Report(double value)
        {
            action(value);
        }
    }
}
