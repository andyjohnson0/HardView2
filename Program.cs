using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uk.andyjohnson.HardView2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var fileOrDirPath = args.Length >= 1 ? args[0] : null;
            if (fileOrDirPath == null)
            {
                fileOrDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
            if (fileOrDirPath != null)
            {
                Application.Run(new MainForm(fileOrDirPath));
            }
        }
    }
}
