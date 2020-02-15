using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Enum
{
    public enum CatiaToken
    {
        Open,
        Close,
        IncorrectLengthUnit,
        Refresh
    };

    public enum FontToken
    {
        FavoriteFontListChanged
    };

    public enum InputDataToken
    {
        SelectedFontChanged,
        WorkingPartDocumentChanged,
        SelectionIncorrect
    };

    public enum ProcessToken
    {
        Started,
        Refresh,
        Failed,
        Finished
    }

    public enum SettingToken
    {
        UserPreferencesChanged,
        GeneratedFontDirectoryPathChanged,
        ClassicFontDirectoryPathChanged,
        SettingFileReadingFailed,
        SettingFileWritingFailed,
        LicenceFileReadingFailed,
        MarkingDateSettingChange
    }

    public enum DesignTableToken
    {
        DesignTableLoaded,
        SelectedPartCollectionChanged
    }
}
