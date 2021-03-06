using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Enum
{
    public enum CatiaToken
    {
        SelectedPartChanged,
        Refresh
    };

    public enum InputDataToken
    {
        SelectionIncorrect
    };

    public enum ProcessToken
    {
        SimpleStarted,
        ComplexStarted,
        Refresh,
        Failed,
        Finished
    }

    public enum SettingToken
    {
        IconCollectionChanged,
        LicenceFileReadingFailed,
        ToleranceFactorChanged,
        KeepHistoricChanged,
        CreateVolumeChanged
    }

    public enum IconToken
    {
        IconCollectionChanged,
        SelectedIconChanged
    }
}
