using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SchetsEditorC;
public class SchetsControl : UserControl
{   
    private Color penkleur;
    private Color vulkleur;

    public Color PenKleur
    { get { return penkleur; }
    }
    
    public Color VulKleur
    {
        get { return vulkleur; }
    }
    
    public Graphics Graphics { 
        get 
        {
            Graphics g = Schets.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }
    }
    
    public Schets Schets
    {
        get;
    }
    
    public SchetsControl(Image img = null)
    {   this.BorderStyle = BorderStyle.FixedSingle;
        this.Schets = new Schets(img);
        this.Paint += this.teken;
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
    private void teken(object o, PaintEventArgs pea)
    {   
        Schets.Teken(pea.Graphics);
    }
    
    private void veranderAfmeting(object o, EventArgs ea)
    {   
        Schets.VeranderAfmeting(this.ClientSize);
        this.Invalidate();
    }
    
    public void Redo(object o, EventArgs ea)
    {
        Schets.Redoer();
        Schets.Update();
        this.Invalidate();
    }

    public void Undo(object o, EventArgs ea)
    {
        Schets.Undoer();
        Schets.Update();
        this.Invalidate();
    }
    
    public void Schoon(object o, EventArgs ea)
    {   
        Schets.Schoon();
        this.Invalidate();
    }
    public void Roteer(object o, EventArgs ea)
    {   
        Schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
        Schets.Roteer();
        this.Invalidate();
    }

    public void VeranderKleur(object obj, EventArgs ea) => penkleur = Custom(obj, ea);

    public void VeranderVulkleur(object obj, EventArgs ea) => vulkleur = Custom(obj, ea);

    public static Color Custom(object obj, EventArgs ea)
    {
        string kleurNaam = "Custom";
        if (obj is ComboBox ccb) kleurNaam = ccb.Text;
        if (obj is ToolStripMenuItem mm) kleurNaam = mm.Text;
        
        if (kleurNaam != "Custom") 
            return Color.FromName(kleurNaam);
        
        if (new ColorDialog() is var dia && dia.ShowDialog() == DialogResult.OK) 
            return dia.Color;
        return Color.FromName(kleurNaam);
    }
}

