using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SchetsEditorC;

public interface ISchetsTool
{
    public  Element.ElementTypes ElementType { get; }
    string IconName { get => Element.IcoonNaam(ElementType); }
    
    void MuisVast(SchetsControl s, Point p);
    void MuisDrag(SchetsControl s, Point p);
    void  MuisLos(SchetsControl s, Point p);
    void  Letter(SchetsControl s, char c);
}

public abstract class StartpuntTool : ISchetsTool
{
    protected Point startpunt;
    protected Brush kwast;
    protected SolidBrush gevuld;

    public abstract Element.ElementTypes ElementType { get; }
    
    public string IconName => Element.IcoonNaam(ElementType); 
    
    public override string ToString() =>Element.TypeNaam(ElementType);

    public virtual void MuisVast(SchetsControl s, Point p)
    {   startpunt = p;
    }
    public virtual void MuisLos(SchetsControl s, Point p)
    {   kwast = new SolidBrush(s.PenKleur);
        gevuld = new SolidBrush(s.VulKleur);
    }

    public abstract void MuisDrag(SchetsControl s, Point p);
    public abstract void Letter(SchetsControl s, char c);

}

public class TekstTool : StartpuntTool
{
    public override Element.ElementTypes ElementType => Element.ElementTypes.Text;
    
    public override void MuisDrag(SchetsControl s, Point p) { }

    public override void Letter(SchetsControl s, char c)
    {
        if (c >= 32)
        {
            Graphics gr = s.Graphics;
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();
            SizeF sz = gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
            gr.DrawString   (tekst, font, kwast, this.startpunt, StringFormat.GenericTypographic);
            startpunt.X += (int)sz.Width;
            s.Invalidate();
        }
    }
}

public abstract class TweepuntTool : StartpuntTool
{
    
    public static Rectangle Punten2Rechthoek(Point p1, Point p2)
    {   return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                            , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                            );
    }
    

    public static Pen MaakPen(Brush b, int dikte)
    {
        Pen pen = new Pen(b, dikte);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        return pen;
    }
    
    public override void MuisVast(SchetsControl s, Point p)
    {   base.MuisVast(s, p);
        kwast = Brushes.Gray;
        if (gevuld == null)
            gevuld = Brushes.Transparent as SolidBrush;
        if (gevuld.Color != Color.Transparent)
        {
            gevuld = Brushes.Gray as SolidBrush;
        }
        else if (gevuld.Color == Color.Transparent)
        {
            gevuld = Brushes.Transparent as SolidBrush;
        }
    }
    public override void MuisDrag(SchetsControl s, Point p)
    {   s.Refresh();
        s.Schets.Update();
        this.Bezig(s.Graphics, this.startpunt, p);
    }
    
    public override void MuisLos(SchetsControl s, Point p)
    {   base.MuisLos(s, p);
        this.Compleet(s.Graphics, this.startpunt, p);
        s.Schets.Update();
        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c)
    {
    }
    public abstract void Bezig(Graphics g, Point p1, Point p2);
        
    public virtual void Compleet(Graphics g, Point p1, Point p2)
    {   
        Bezig(g,p1,p2);
    }
}

public class RechthoekTool : TweepuntTool
{
    protected VierkantElement _element;
    public override Element.ElementTypes ElementType => Element.ElementTypes.Vierkant;

    public override void MuisVast(SchetsControl s, Point p)
    {
        base.MuisVast(s, p);
        _element = new(Rectangle.Empty, MaakPen(kwast, 3), gevuld);
        s.Schets.VoegToe(_element);
        s.Schets.UndoAdd(_element);
    }
    
    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        _element.Bounds = Punten2Rechthoek(p1, p2);
    }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        _element.Bounds = Punten2Rechthoek(p1, p2);
        _element.Pen = MaakPen(kwast, 3);
        _element.Vulling = gevuld;
    }
}

public class CirkelTool : TweepuntTool 
{
    protected CirkelElement _element;
    public override Element.ElementTypes ElementType => Element.ElementTypes.Cirkel;

    public override void MuisVast(SchetsControl s, Point p)
    {
        SchetsWin q = null;
        base.MuisVast(s, p);
        _element = new(Rectangle.Empty, MaakPen(kwast, 3), gevuld);
        s.Schets.VoegToe(_element);
        s.Schets.UndoAdd(_element);
    }
    
    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        _element.Bounds = Punten2Rechthoek(p1, p2);
    }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        _element.Bounds = Punten2Rechthoek(p1, p2);
        _element.Pen = MaakPen(kwast, 3);
        _element.Vulling = gevuld;
    }
}

public class LijnTool : TweepuntTool
{
    protected LijnElement _element;
    
    public override Element.ElementTypes ElementType => Element.ElementTypes.Lijn;

    public override void MuisVast(SchetsControl s, Point p)
    {
        base.MuisVast(s, p);
        _element = new(p,p,MaakPen(kwast, 3));
        s.Schets.VoegToe(_element);
        s.Schets.UndoAdd(_element);
    }
    
    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        _element.Pt2 = p2;
    }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        _element.Pt2 = p2;
        _element.Pen = MaakPen(kwast, 3);
    }
}

public class PenTool : LijnTool
{
    public override Element.ElementTypes ElementType => Element.ElementTypes.Pen;

    public override void MuisDrag(SchetsControl s, Point p)
    {   this.MuisLos(s, p);
        this.MuisVast(s, p);
    }
}
    
public class GumTool : StartpuntTool
{
    public override Element.ElementTypes ElementType => Element.ElementTypes.Gum;
    
    public override void MuisVast(SchetsControl s, Point p)
    {
        Elementen el = s.Schets.Elements.LastOrDefault(e => e.Bevat(p));
        if (el == null) return;
        
        s.Schets.RedoAdd(el);
        s.Schets.HaalWeg(el);
        s.Invalidate();
    }

    public override void MuisLos(SchetsControl s, Point p) { }
    public override void MuisDrag(SchetsControl s, Point p) { }
    public override void Letter(SchetsControl s, char c) { }
}