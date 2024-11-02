using System;
using System.Windows.Forms;
using SchetsEditorC;

namespace SchetsEditorC;
static class Program
{
    [STAThreadAttribute]
    static void Main()
    {
        Application.Run(new SchetsEditor());
    }
}