using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Graphy
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string GetExeDirectory()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            exePath = new Uri(exePath).LocalPath;
            var exeDirectory = Path.GetDirectoryName(exePath);
            return exeDirectory;
        }
    }
}
