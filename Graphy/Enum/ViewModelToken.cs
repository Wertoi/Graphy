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
        DirectoryNotFound,
        FileReadingFailed,
        GenerateNewFont,
        SupportedCharacterComputed
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
        GeneratedFontDirectoryPathChanged,
        ClassicFontDirectoryPathChanged,
        SettingFileReadingFailed,
        SettingFileWritingFailed,
        LicenceFileReadingFailed,
        ComputedFontCollectionChanged,
    }

    public enum DesignTableToken
    {
        DesignTableLoaded,
        SelectedPartCollectionChanged
    }
}
