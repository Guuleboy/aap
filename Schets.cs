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

    private readonly List<Element> elements;
    private readonly List<Element> redoElement;
    private readonly List<Element> undoElement;
    
    public List<Element> Elements { get => elements;}
    
    public Schets(Image img = null)
    {
        bitmap = new Bitmap(1, 1);
        achtergrond = img as Bitmap;
        
        elements = new List<Element>();
        redoElement = new List<Element>();
        undoElement = new List<Element>();
    }

    public void VoegToe(Element el)
    {
        elements.Add(el);
        Update();
    }
    
    public void HaalWeg(Element el)
    {
        elements.Remove(el);
        Update();
    }

    public void RedoAdd(Element el)
    {
        redoElement.Add(el);
        Update();
    }

    public void UndoAdd(Element el)
    {
        undoElement.Add(el);
        Update();
    }
    
    public void Redoer()
    {
        if (redoElement.Count > 0)
        {
            Element el = redoElement[^1];
            
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
            Element el = undoElement[^1];
            
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
        foreach (Element el in elements)
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
        elements.Clear();
        undoElement.Clear();
        Update();
    }
    public void Roteer()
    {
        foreach (Element el in elements)
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

        foreach (Element el in elements)
        {
            el.Teken(BitmapGraphics);
        }
    }
    
}