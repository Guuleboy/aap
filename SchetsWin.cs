using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace SchetsEditorC;

public class SchetsWin : Form
{
    internal ToolStripItem It;
    internal new string Name;
    
    private readonly SchetsEditor parent;
    public MenuStrip menuStrip; 
    public SchetsControl schetscontrol;
    private SchetsTool huidigeTool;
    public Panel paneel;
    private bool vast;
    private OpenFileDialog open;

    public SchetsWin(SchetsEditor parent, BinaryReader br) : this(parent)
    {
        bool hasBackground = br.ReadBoolean();
        if (hasBackground)
        {
            int imageSize = br.ReadInt32();
            var img = Image.FromStream(new MemoryStream(br.ReadBytes(imageSize)));
            schetscontrol.Sketch.Achtergrond = img as Bitmap;
        }
        int elementCount = br.ReadInt32();
        for (int i = 0; i < elementCount; i++)
        {
            schetscontrol.Sketch.VoegToe(Elementen.Deserialize(br));
        }
    }
    
    public SchetsWin(SchetsEditor parent, Image img = null)
    {
        FormBorderStyle = FormBorderStyle.None;
        
        SchetsTool[] deTools = { new PenTool()         
                                , new LijnTool()
                                , new RechthoekTool()
                                , new CirkelTool()
                                , new TekstTool()
                                , new GumTool()
                                };
        String[] deKleuren = { "Transparant", "Black", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan" };

        this.ClientSize = new Size(700, 500);
        huidigeTool = deTools[0];

        schetscontrol = new SchetsControl(img)
        {
            Location = new Point(64, 10)
        };
        schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                    {   vast=true;  
                                        huidigeTool.MuisVast(schetscontrol, mea.Location); 
                                    };
        schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                    {   if (vast)
                                        huidigeTool.MuisDrag(schetscontrol, mea.Location); 
                                    };
        schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                    {   if (vast)
                                        huidigeTool.MuisLos (schetscontrol, mea.Location);
                                        vast = false; 
                                    };
        schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                    {   huidigeTool.Letter  (schetscontrol, kpea.KeyChar); 
                                    };
        this.Controls.Add(schetscontrol);

        menuStrip = new MenuStrip();
        menuStrip.Visible = false;
        this.Controls.Add(menuStrip);
        this.maakFileMenu();
        this.maakToolMenu(deTools);
        this.maakActieMenu(deKleuren);
        this.maakToolButtons(deTools);
        this.maakActieButtons(deKleuren);
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }
    
    private void veranderAfmeting(object o, EventArgs ea)
    {
        schetscontrol.Size = new Size ( this.ClientSize.Width  - 70
            , this.ClientSize.Height - 50);
        paneel.Location = new Point(64, this.ClientSize.Height - 30);
    }

    private void klikToolMenu(object obj, EventArgs ea)
    {
        this.huidigeTool = (SchetsTool)((ToolStripMenuItem)obj).Tag;
    }

    private void klikToolButton(object obj, EventArgs ea)
    {
        this.huidigeTool = (SchetsTool)((RadioButton)obj).Tag;
    }

    private void afsluiten(object obj, EventArgs ea)
    {
        Hide();
        parent.VerwijderSchets(this);
        parent.Sketch = parent.Sketches.LastOrDefault();
        if (parent.Sketch != null)
        {
            parent.Sketch.Show();
            parent.Sketch.Location = new Point(0, 0);
        }
        this.Close();
    }

    public void SelectSketch(object o, EventArgs ea)
    {
        parent.HideAll();
        Show();
        Location = new Point(0, 0);
        parent.Sketch = this;
    }
    
    private void maakFileMenu()
    {   
        ToolStripMenuItem menu = new ToolStripMenuItem("File");
        menu.MergeAction = MergeAction.MatchOnly;
        menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
        menu.DropDownItems.Add("Open", null, this.Oplsaan);
        menu.DropDownItems.Add("Opslaan", null, this.OpslaanAls);
        menuStrip.Items.Add(menu);
    }

    private void Oplsaan(object obj, EventArgs ea)
    {
        if (Text == "") OpslaanAls(obj, ea);
        else WriteToFile(Text);
    }

    private void WriteToFile(string path)
    {
        string ext = Path.GetExtension(path);
        if (ext == ".schets")
        {
            schetscontrol.Sketch.Save(path, Name);
            return;
        }

        ImageFormat format = ext switch
        {
            ".jpg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            _ => ImageFormat.Png
        };
        schetscontrol.Sketch.Save(path, format);
    }

    private void OpslaanAls(object obj, EventArgs ea)
    {
        SaveFileDialog Dialog = new ()
        {
            Filter = "PNG|*.png;|Bitmap|*.bmp;|JPG|*.jpg|SCHETS|*.schets",
            Title = "Bestand opslaan als ...",
            FileName = $"{Name}.Schets"
        };

        if (Dialog.ShowDialog() == DialogResult.OK)
        {
            Text = Dialog.FileName;
            WriteToFile(Dialog.FileName);
        }
    }


    private void maakToolMenu(ICollection<SchetsTool> tools)
    {   
        ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
        foreach (SchetsTool tool in tools)
        {   ToolStripItem item = new ToolStripMenuItem();
            item.Tag = tool;
            item.Text = tool.ToString();
            item.Image = new Bitmap($"../../../Icons/{tool.IconName}.png");
            item.Click += this.klikToolMenu;
            menu.DropDownItems.Add(item);
        }
        menuStrip.Items.Add(menu);
    }

    private void maakActieMenu(String[] kleuren)
    {   
        ToolStripMenuItem menu = new ToolStripMenuItem("Actie");
        menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
        menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
        ToolStripMenuItem submenu = new ToolStripMenuItem("Kies kleur");
        foreach (string k in kleuren.Skip(1))
            submenu.DropDownItems.Add(k, null, schetscontrol.VeranderKleurViaMenu);
        ToolStripMenuItem vulsubmenu = new ToolStripMenuItem("Kies vulkleur");
        foreach (string k in kleuren)
            submenu.DropDownItems.Add(k, null, schetscontrol.VeranderVulKleurViaMenu);
        menu.DropDownItems.Add("Redo", null, schetscontrol.Redo);
        menu.DropDownItems.Add("Undo", null, schetscontrol.Undo);
        menu.DropDownItems.Add(submenu);
        menu.DropDownItems.Add(vulsubmenu);
        menuStrip.Items.Add(menu);
    }

    private void maakToolButtons(ICollection<SchetsTool> tools)
    {
        int t = 0;
        foreach (SchetsTool tool in tools)
        {
            RadioButton b = new RadioButton();
            b.Appearance = Appearance.Button;
            b.Size = new Size(45, 62);
            b.Location = new Point(10, 10 + t * 62);
            b.Tag = tool;
            b.Text = tool.ToString();
            b.Image = new Bitmap($"../../../Icons/{tool.IconName}.png");
            b.TextAlign = ContentAlignment.TopCenter;
            b.ImageAlign = ContentAlignment.BottomCenter;
            b.Click += this.klikToolButton;
            this.Controls.Add(b);
            if (t == 0) b.Select();
            t++;
        }
    }

    private void maakActieButtons(String[] kleuren)
    {   
        paneel = new Panel(); this.Controls.Add(paneel);
        paneel.Size = new Size(600, 24);
            
        Button clear = new Button(); paneel.Controls.Add(clear);
        clear.Text = "Clear";  
        clear.Location = new Point(  0, 0); 
        clear.Click += schetscontrol.Schoon;        
            
        Button rotate = new Button(); paneel.Controls.Add(rotate);
        rotate.Text = "Rotate"; 
        rotate.Location = new Point( 80, 0); 
        rotate.Click += schetscontrol.Roteer;

        Button undo = new Button(); paneel.Controls.Add(undo);
        undo.Text = "Undo";
        undo.Location = new Point(160,0);
        undo.Click += schetscontrol.Undo;
        
        Button redo = new Button(); paneel.Controls.Add(redo);
        redo.Text = "Redo";
        redo.Location = new Point(240,0);
        redo.Click += schetscontrol.Redo;
           
        Label penkleur = new Label(); paneel.Controls.Add(penkleur);
        penkleur.Text = "Penkleur:"; 
        penkleur.Location = new Point(330, 3); 
        penkleur.AutoSize = true;               
            
        ComboBox cbb = new ComboBox(); paneel.Controls.Add(cbb);
        cbb.Location = new Point(390, 0);
        cbb.Size = new Size(70, 20);
        cbb.DropDownStyle = ComboBoxStyle.DropDownList; 
        cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
        foreach (string k in kleuren.Skip(1))
            cbb.Items.Add(k);
        cbb.SelectedIndex = 0;
        
        Label vulkleur = new Label(); paneel.Controls.Add(vulkleur);
        vulkleur.Text = "Vulkleur:"; 
        vulkleur.Location = new Point(460, 3); 
        vulkleur.AutoSize = true;     
        
        ComboBox vulcbb = new ComboBox(); paneel.Controls.Add(vulcbb);
        vulcbb.Location = new Point(520, 0);
        vulcbb.Size = new Size(70, 20);
        vulcbb.DropDownStyle = ComboBoxStyle.DropDownList;
        vulcbb.SelectedValueChanged += schetscontrol.VeranderVulkleur;
        foreach (string k in kleuren)
            vulcbb.Items.Add(k);
        vulcbb.SelectedIndex = 0;
    }
    
}
public class Tekst : Form
{
    private TextBox invoer;

    public Tekst()
    {
        invoer = new TextBox();
        this.Controls.Add(invoer);
    }

    void zoeken(object sender, EventArgs e)
    {
        ZoekDialoog d = new ZoekDialoog();
        if (d.ShowDialog() == DialogResult.OK)
        {
            string alles = invoer.Text;
            string zk = d.Zoek.Text;

            if (!string.IsNullOrEmpty(zk)) // Controleer of de zoekterm niet leeg is
            {
                int pos = alles.IndexOf(zk);
                if (pos >= 0) // Controleer of de term gevonden is
                {
                    invoer.Select(pos, zk.Length);
                }
                else
                {
                    MessageBox.Show("Zoekterm niet gevonden.");
                }
            }
        }
    }

    public void lees(string naam) //lezen van string 
    {
        StreamReader sr = new StreamReader(naam);
        this.invoer.Text = sr.ReadToEnd();
        sr.Close();
        this.Text = naam;
    }

    void Schrijf(string naam)
    {
        StreamWriter sw = new StreamWriter(naam);
        sw.Write(this.invoer.Text);
        sw.Close();
        this.Text = naam;
    }
}

class ZoekDialoog : Form
{

    public TextBox Zoek;
    private Button ok, cc;

    public ZoekDialoog()
    {
        // positionering van de knoppen bovenaan het scherm
        Zoek = new TextBox()
        {
            Dock = DockStyle.Top
        };
        ok = new Button()
        {
            Text = "OK",
            Dock = DockStyle.Right
        };

        cc = new Button()
        {
            Text = "Annuleer",
            Dock = DockStyle.Right
        };
        
        ok.Click += this.positief;
        this.CancelButton = cc;
        this.AcceptButton = ok;
        
        this.Controls.Add(Zoek);
        this.Controls.Add(ok);
        this.Controls.Add(cc);
        this.AcceptButton = ok;
    }

    void positief(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}