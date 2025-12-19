using System;
using System.Windows.Forms;

namespace SistemaFarmacia
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormCaixa());
        }
    }
}