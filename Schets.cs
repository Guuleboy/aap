using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace SchetsEditorC;

public class Schets
{
    private Bitmap bitmap;
    private Bitmap achtergrond;
    
    public Bitmap Achtergrond {set => achtergrond = value;}
    public Graphics BitmapGraphics
    {
        get { return Graphics.FromImage(bitmap); }
    }

    private readonly List<Elementen> elements;
    private readonly List<Elementen> redoElement;
    private readonly List<Elementen> undoElement;
    
    public List<Elementen> Elements { get => elements;}
    
    public Schets(Image img = null)
    {
        bitmap = new Bitmap(1, 1);
        achtergrond = img as Bitmap;
        
        elements = new List<Elementen>();
        redoElement = new List<Elementen>();
        undoElement = new List<Elementen>();
    }

    public void VoegToe(Elementen el)
    {
        elements.Add(el);
        Update();
    }
    
    public void HaalWeg(Elementen el)
    {
        elements.Remove(el);
        Update();
    }

    public void RedoAdd(Elementen el)
    {
        redoElement.Add(el);
        Update();
    }

    public void UndoAdd(Elementen el)
    {
        undoElement.Add(el);
        Update();
    }
    
    public void Redoer()
    {
        if (redoElement.Count > 0)
        {
            Elementen el = redoElement[^1];
            
            elements.Add(el);
            
            undoElement.Add(el);
            
            redoElement.RemoveAt(redoElement.Count - 1);

            Update();
        }
    }

    public void Undoer()
    {
        if (undoElement.Count > 0)
        {
            Elementen el = undoElement[^1];
            
            redoElement.Add(el);
            
            elements.Remove(el);
            
            undoElement.RemoveAt(undoElement.Count - 1);

            Update();
        }
    }
    
    public void VeranderAfmeting(Size sz)
    {
        if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
        {
            Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                     , Math.Max(sz.Height, bitmap.Size.Height)
                                     );
            Graphics gr = Graphics.FromImage(nieuw);
            gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
            gr.DrawImage(bitmap, 0, 0);
            bitmap = nieuw;
        }
    }
    
    public void Save(string filename, ImageFormat format) => bitmap.Save(filename, format);

    public void Save(string filename, string schetsnaam)
    {
        FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
        BinaryWriter writer = new(fs);

        writer.Write(Encoding.ASCII.GetBytes("Schets+"));
        writer.Write(schetsnaam);
        writer.Write(achtergrond != null);
        if (achtergrond != null)
        {
            byte[] image = (byte[])new ImageConverter().ConvertTo(achtergrond, typeof(byte[]));
            writer.Write(image.Length);
            writer.Write(image);

        }
        
        writer.Write(elements.Count);
        foreach (Elementen el in elements)
        {
            writer.Write(el.ToBytes());
        }
        writer.Close();
    }

    public void Teken(Graphics gr)
    {
        gr.DrawImage(bitmap, 0, 0);
    }
    public void Schoon()
    {
        redoElement.Clear();
        foreach (Elementen el in elements)
        {
            redoElement.Add(el);
        }
        elements.Clear();
        Update();
    }
    public void Roteer()
    {
        foreach (Elementen el in elements)
        {
            el.Draai(90, new Point(bitmap.Size.Width / 2, bitmap.Size.Height / 2));
        }
        Update();
    }

    public void Update()
    {
        BitmapGraphics.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        if (achtergrond != null)
        {
            BitmapGraphics.DrawImage(achtergrond, 0, 0, bitmap.Width, bitmap.Height);
        }

        foreach (Elementen el in elements)
        {
            el.Teken(BitmapGraphics);
        }
    }
}