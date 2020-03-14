using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Enum
{
    public enum CatiaToken
    {
        SelectedPartDocumentChanged,
        Refresh
    };

    public enum FontToken
    {
        FavoriteFontListChanged
    };

    public enum InputDataToken
    {
        SelectedFontChanged,
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
        UserPreferencesChanged,
        LicenceFileReadingFailed,
        ToleranceFactorChanged,
        KeepHistoricChanged,
        CreateVolumeChanged
    }

    public enum DesignTableToken
    {
        DesignTableLoaded,
        SelectedPartCollectionChanged
    }
}
