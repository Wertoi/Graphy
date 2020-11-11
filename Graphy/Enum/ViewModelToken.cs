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
        FavoriteFontCollectionChanged
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
        FavoriteFontCollectionChanged,
        IconCollectionChanged,
        LicenceFileReadingFailed,
        ToleranceFactorChanged,
        KeepHistoricChanged,
        CreateVolumeChanged,
        VerticalAlignmentChanged
    }

    public enum DesignTableToken
    {
        DesignTableLoaded,
        SelectedPartCollectionChanged
    }

    public enum IconToken
    {
        IconCollectionChanged,
        SelectedIconChanged
    }
}
