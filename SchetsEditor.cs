using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SchetsEditorC;

public class SchetsEditor : Form
{
    private MenuStrip menuStrip;
    private ToolStripDropDownItem _schetsMenu;

    private int schetsIndex;
    private List<SchetsWin> _schetsWindows;
    
    private SchetsWin _schets;

    public SchetsEditor()
    {   
        _schetsWindows = new List<SchetsWin>();
        
        this.ClientSize = new Size(800, 600);
        menuStrip = new MenuStrip();
        this.Controls.Add(menuStrip);
        this.maakFileMenu();
        this.maakHelpMenu();
        this.Text = "Schets editor";
        this.IsMdiContainer = true;
        this.MainMenuStrip = menuStrip;
    }
    
    private void maakFileMenu()
    {   
        ToolStripDropDownItem menu = new ToolStripMenuItem("File");
        menu.DropDownItems.Add("Nieuw", null, this.NieuweSchetsManager);
        menu.DropDownItems.Add("Open", null, Open);
        menu.DropDownItems.Add("Programma Sluiten", null, this.afsluiten);
        menuStrip.Items.Add(menu);
    }
    private void maakHelpMenu()
    {   
        ToolStripDropDownItem menu = new ToolStripMenuItem("Help");
        menu.DropDownItems.Add("Over \"Schets\"", null, this.about);
        menuStrip.Items.Add(menu);
    }
    private void about(object o, EventArgs ea)
    {   
        MessageBox.Show ( "Schets versie 2.0\n(c) UU Informatica 2024"
                        , "Over \"Schets\""
                        , MessageBoxButtons.OK
                        , MessageBoxIcon.Information
                        );
    }
    
    public void HideAll()
    {
        foreach (SchetsWin window in _schetsWindows)
        {
            window.Hide();
        }
    }

    private void NieuweSchetsManager(object o, EventArgs ea) => nieuw(null);

    private void OpenFileFromImage(string path) => nieuw(Image.FromFile(path));

    private void OpenSchets(string path)
    {
        FileStream fs;
        try
        {
            fs = File.OpenRead(path);
        }
        catch (Exception e)
        {
            ErrorDialog("Error", e.Message);
            return;
        }
        BinaryReader br = new(fs);
        byte[] magic = br.ReadBytes(7);
        if (Encoding.ASCII.GetString(magic) != "Schets+")
        {
            ErrorDialog("Error", "Bestand is geen schets bestand");
            return;
        }
        string name = br.ReadString();

        HideAll();
        SchetsWin s = new(this, br)
        {
            Naam = name,
            MdiParent = this
        };
        s.Show();
        s.Location = new Point(0, 0);
        _schetsWindows.Add(s);
        _schets = s;
    }
    
    public static void ErrorDialog(string title, string caption) => MessageBox.Show (caption, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    
    private void nieuw(Image img)
    {
        HideAll();
        SchetsWin s = new(this, img)
        {
            MdiParent = this
        };
        s.Show();
        s.Location = new Point(0, 0);
        _schetsWindows.Add(s);
        _schets = s;
    }

    private void Open(object sender, EventArgs e)
    {
        if (new OpenFileDialog { Title = "Bestand openen..." } is var dia && dia.ShowDialog() == DialogResult.OK)
        {
            Text = dia.FileName;
            if (Path.HasExtension(Text) && Path.GetExtension(Text) == ".schets")
            {
                OpenSchets(Text);
                return;
            }
            OpenFileFromImage(Text);
        }
    }
    
    private void afsluiten(object sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show("Weet je zeker dat je het programma wilt sluiten?", "Confirm Close", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            this.Close();
        }
    }
}