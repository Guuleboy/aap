using System;
using System.Drawing;
using System.IO;
using static SchetsEditorC.Element;

namespace SchetsEditorC;

public static class Element
{
    public enum ElementTypes : byte
    {
        Gum,
        Text,
        Vierkant,
        Cirkel,
        Lijn,
        Pen,
        Plaatje
    }

    public static string TypeNaam(ElementTypes type) => type switch
    {
        ElementTypes.Text => "Text",
        ElementTypes.Vierkant => "Vierkant",
        ElementTypes.Cirkel => "Cirkel",
        ElementTypes.Lijn => "Lijn",
        ElementTypes.Pen => "Pen",
        ElementTypes.Plaatje => "Plaatje",
        ElementTypes.Gum => "Gum"
    };

    public static string IcoonNaam(ElementTypes type) => type switch
    {
	    ElementTypes.Text => "tekst",
	    ElementTypes.Vierkant => "kader",
	    ElementTypes.Cirkel => "cirkel",
	    ElementTypes.Lijn => "lijn",
	    ElementTypes.Pen => "pen",
	    ElementTypes.Plaatje => "plaatje",
	    ElementTypes.Gum => "gum"
    };
}

public abstract class Elementen
{
    public abstract ElementTypes ElementType { get; }
    
    public override string ToString() => TypeNaam(ElementType);
    
    public abstract void Teken(Graphics g);

    public abstract bool Contains(Point point);

    public abstract void Draai(double degrees, Point origin);

    public abstract byte[] ToBytes();
    
    internal abstract void FromBytes(BinaryReader reader);
    
    public static Elementen Deserialize(BinaryReader reader)
    {
        Elementen el = EmptyFromType((ElementTypes)reader.ReadByte());
        el.FromBytes(reader);
        return el;
    }
    
    private static Elementen EmptyFromType(Element.ElementTypes type) => type switch
    {
        Element.ElementTypes.Vierkant => new VierkantElement(),
        Element.ElementTypes.Cirkel => new CirkelElement(),
        Element.ElementTypes.Lijn => new LijnElement(),
        Element.ElementTypes.Gum => null
    };
}

public class LijnElement : Elementen
{
	public override ElementTypes ElementType => ElementTypes.Lijn;

	public Point Pt1, Pt2;
	public Pen Pen;

	public LijnElement() { }

	public LijnElement(Point p1, Point p2, Pen pen)
	{
		Pt1 = p1;
		Pt2 = p2;
		Pen = pen;
	}

	public override void Teken(Graphics s) => s.DrawLine(Pen, Pt1, Pt2);

	public override bool Contains(Point point)
	{
		Math.Sqrt(Math.Pow(Pt1.X - Pt2.X, 2) + Math.Pow(Pt1.Y - Pt2.Y, 2));
		double a = Math.Sqrt(Math.Pow(Pt1.X - Pt2.X, 2) + Math.Pow(Pt1.Y - Pt2.Y, 2));
		double b = Math.Sqrt(Math.Pow(Pt1.X - point.X, 2) + Math.Pow(Pt1.Y - point.Y, 2));
		double c = Math.Sqrt(Math.Pow(Pt2.X - point.X, 2) + Math.Pow(Pt2.Y - point.Y, 2));
		double s = (a + b + c) / 2;
		return 2 * Math.Sqrt(s * (s - a) * (s - b) * (s - c)) / a < 5;
	}

	public override byte[] ToBytes()
	{
		MemoryStream stream = new();
		BinaryWriter writer = new(stream);

		writer.Write((byte)ElementType);
		writer.Write(Pt1.X);
		writer.Write(Pt1.Y);
		writer.Write(Pt2.X);
		writer.Write(Pt2.Y);
		writer.Write(Pen.Color.ToArgb());
		writer.Write(Pen.Width);

		return stream.ToArray();
	}

	internal override void FromBytes(BinaryReader reader)
	{
		Pt1 = new Point(reader.ReadInt32(), reader.ReadInt32());
		Pt1 = new Point(reader.ReadInt32(), reader.ReadInt32());
		Pen = new Pen(Color.FromArgb(reader.ReadInt32()), reader.ReadSingle());
	}

	public override void Draai(double degrees, Point origin)
	{
		double angleRad = degrees * (Math.PI / 180);
		double cos = Math.Cos(angleRad);
		double sin = Math.Sin(angleRad);
		Point newPt1 = new()
		{
			X = (int)(cos * (Pt1.X - origin.X) - sin * (Pt1.Y - origin.Y) + origin.X),
			Y = (int)(sin * (Pt1.X - origin.X) + cos * (Pt1.Y - origin.Y) + origin.Y)
		};
		Point newPt2 = new()
		{
			X = (int)(cos * (Pt2.X - origin.X) - sin * (Pt2.Y - origin.Y) + origin.X),
			Y = (int)(sin * (Pt2.X - origin.X) + cos * (Pt2.Y - origin.Y) + origin.Y)
		};
		Pt1 = newPt1;
		Pt2 = newPt2;
	}
}

public abstract class BasisVierkantElement : Elementen
{
	public Rectangle Bounds;
	public Pen Pen;
	public SolidBrush Vulling;

	public BasisVierkantElement() { }

	public BasisVierkantElement(Rectangle vierkant, Pen pen, SolidBrush vulling)
	{
		Bounds = vierkant;
		Pen = pen;
		Vulling = vulling;
	}

	public override bool Contains(Point point) => Bounds.Contains(point);

	public override byte[] ToBytes()
	{
		MemoryStream stream = new();
		BinaryWriter writer = new(stream);

		writer.Write((byte)ElementType);
		writer.Write(Bounds.X);
		writer.Write(Bounds.Y);
		writer.Write(Bounds.Width);
		writer.Write(Bounds.Height);
		writer.Write(Pen.Color.ToArgb());
		writer.Write(Pen.Width);
		writer.Write(Vulling.Color.ToArgb());

		return stream.ToArray();
	}

	public override void Draai(double degrees, Point origin)
	{
		double angleRad = degrees * (Math.PI / 180);
		double cos = Math.Cos(angleRad);
		double sin = Math.Sin(angleRad);

		int X = (int)(cos * (Bounds.X - origin.X) - sin * (Bounds.Y - origin.Y) + origin.X);
		int Y = (int)(sin * (Bounds.X - origin.X) + cos * (Bounds.Y - origin.Y) + origin.Y);

		Bounds.X = X;
		Bounds.Y = Y;
	}

	internal override void FromBytes(BinaryReader reader)
	{
		Bounds = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
		Pen = new Pen(Color.FromArgb(reader.ReadInt32()), reader.ReadSingle());
		Vulling = new SolidBrush(Color.FromArgb(reader.ReadInt32()));
	}

}

public class VierkantElement : BasisVierkantElement
{
	public override ElementTypes ElementType => ElementTypes.Vierkant;

	public VierkantElement() { }

	public VierkantElement(Rectangle vierkant, Pen pen, SolidBrush vulling) : base(vierkant, pen, vulling) { }

	public override void Teken(Graphics s)
	{
		if (Vulling.Color != Color.Transparent) s.FillRectangle(Vulling, Bounds);
		
		s.DrawRectangle(Pen, Bounds);
	}
}

public class CirkelElement : BasisVierkantElement
{
	public override ElementTypes ElementType => ElementTypes.Cirkel;

	public CirkelElement() { }

	public CirkelElement(Rectangle vierkant, Pen pen, SolidBrush vulling) : base(vierkant, pen, vulling) { }

	public override void Teken(Graphics s)
	{
		if (Vulling.Color != Color.Transparent) s.FillEllipse(Vulling, Bounds);
		s.DrawEllipse(Pen, Bounds);
	}

	public override bool Contains(Point point)
	{
		Point midden = new(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
		double xRadius = Bounds.Width / 2;
		double yRadius = Bounds.Height / 2;
		Point normalized = new(point.X - midden.X, point.Y - midden.Y);

		return (normalized.X * normalized.X / (xRadius * xRadius)) + (normalized.Y * normalized.Y / (yRadius * yRadius)) <= 1.0;
	}
}

