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
            Graphics g = Sketch.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }
    }
    
    public Schets Sketch
    {
        get;
    }
    
    public SchetsControl(Image img = null)
    {   this.BorderStyle = BorderStyle.FixedSingle;
        this.Sketch = new Schets(img);
        this.Paint += this.teken;
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
    private void teken(object o, PaintEventArgs pea)
    {   
        Sketch.Teken(pea.Graphics);
    }
    
    private void veranderAfmeting(object o, EventArgs ea)
    {   
        Sketch.VeranderAfmeting(this.ClientSize);
        this.Invalidate();
    }
    
    public void Redo(object o, EventArgs ea)
    {
        Sketch.Redoer();
        Sketch.Update();
        this.Invalidate();
    }

    public void Undo(object o, EventArgs ea)
    {
        Sketch.Undoer();
        Sketch.Update();
        this.Invalidate();
    }
    
    public void Schoon(object o, EventArgs ea)
    {   
        Sketch.Schoon();
        this.Invalidate();
    }
    public void Roteer(object o, EventArgs ea)
    {   
        Sketch.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
        Sketch.Roteer();
        this.Invalidate();
    }
    public void VeranderKleur(object obj, EventArgs ea)
    {   
        string kleurNaam = ((ComboBox)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }
    public void VeranderKleurViaMenu(object obj, EventArgs ea)
    {   
        string kleurNaam = ((ToolStripMenuItem)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }

    public void VeranderVulkleur(object obj, EventArgs ea)
    {
        string vulkleurNaam = ((ComboBox)obj).Text;
        vulkleur = Color.FromName(vulkleurNaam);
    }
    
    public void VeranderVulKleurViaMenu(object obj, EventArgs ea)
    {
        string vulkleurNaam = ((ToolStripMenuItem)obj).Text;
        vulkleur = Color.FromName(vulkleurNaam);
    }
}

