using System;
using System.Windows.Forms;

namespace CoffeeShopManagement
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize database on startup
            DatabaseHelper.InitializeDatabase();

            Application.Run(new Forms.LoginForm());
        }
    }
}