using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PowerConfig;
using Microsoft.CSharp;
namespace testPowerConfig
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
           

            //Config.Dynamic.Server = "localhost";  // 
            //string server = Config.Get("Server");
            Config.Dynamic.lab = "cozer";
            Config.Dynamic.port = 8008;
            Config.Root.Name.Nickname = "Hello,PowerConfig!";

            //string port =Config.Get("port");

            // Console.WriteLine(port);
            Console.WriteLine(Config.Get("name"));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFrom());
        }
    }
}
