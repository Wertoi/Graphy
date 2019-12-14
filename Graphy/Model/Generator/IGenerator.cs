using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.Generator
{
    interface IGenerator
    {
        event EventHandler<ProgressRateChangedEventArgs> ProgressRateChanged;
        event EventHandler<ProgressRateChangedEventArgs> StepProgressRateChanged;

        double ProgressRate { get; set; }
        int StepProgressRate { get; set; }
    }

    public class ProgressRateChangedEventArgs : EventArgs
    {
        public ProgressRateChangedEventArgs(double progressRate)
        {
            ProgressRate = progressRate;
        }
        public double ProgressRate { get; private set; }
    }
}
