using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Cairo;
using Gtk;
 
public class GtkCairo
{
    static void Main ()
    {
        Application.Init ();
        Gtk.Window w = new Gtk.Window ("Mono-Cairo Truchet Tile");
 
        DrawingArea a = new CairoGraphic ();
 
        Box box = new HBox (true, 0);
        box.Add (a);
        
        w.Add (box);
        w.Resize (500, 500);
        w.DeleteEvent += close_window;
        w.ShowAll ();
 
        Application.Run ();
    }
    
    static void close_window (object obj, DeleteEventArgs args)
    {
        Application.Quit ();
    }
}
 
public class Truchet
{
    public enum DrawingMethod
    {
        Curved = 0,
        Rounded,
        Random
    }
    
    delegate void CellDrawing (Cairo.Context gr, int i, int j, double radius, bool rotate);
    
    bool[,] field;
    double Radius;
    static CellDrawing[] DrawingMethods;
    DrawingMethod meth;
    static int qmethods = Enum.GetValues (typeof (DrawingMethod)).Length - 1;
 
    public Truchet (int x, int y, double radius)
    {
        this.field = new bool[x, y];
        this.Radius = radius;
        InitDMethods ();
        meth = DrawingMethod.Curved;
    }
 
    void InitDMethods ()
    {
        DrawingMethods = new CellDrawing[qmethods];
        DrawingMethods[0] = new CellDrawing (CurvedCell);
        DrawingMethods[1] = new CellDrawing (RoundedCell);
    }
    
    public void GenerateMap ()
    {
        Random Rand = new Random ();
        for (int i = 0; i < field.GetLength (0); i++)
            for (int j = 0; j < field.GetLength (1); j++)
                if (Rand.Next (2) == 1)
                    SetCell (i, j, true);
                else
                    SetCell (i, j, false);
    }
    
    public void SetCell (int x, int y, bool rotated)
    {
        field[x, y] = rotated;
    }
    
    public void SetDrawingMethod (DrawingMethod method)
    {
        meth = method;
    }
    
    public void RenderMap (Cairo.Context gr, double x0, double y0)
    {
        if (meth != DrawingMethod.Random) {
            for (int i = 0; i < field.GetLength (0); i++)
                for (int j = 0; j < field.GetLength (1); j++)
                    DrawingMethods[(int)meth] (gr, i, j, Radius, field[i, j]);
        } else {
            Random Rand = new Random ();
            for (int i = 0; i < field.GetLength (0); i++)
                for (int j = 0; j < field.GetLength (1); j++)
                    DrawingMethods[Rand.Next (qmethods)] (gr, i, j, Radius, field[i, j]);
        }
 
    }
    
    public static void CurvedCell (Cairo.Context gr, int i, int j, double radius, bool rotate)
    {
        double cellsize = radius * 2;
        double x = cellsize * i, y = cellsize * j;
        gr.Save ();
        
        if (rotate) {
            gr.MoveTo (x + cellsize, y + radius);
            gr.CurveTo (x + radius, y + radius, x + radius, y + radius, x + radius, y + cellsize);
            gr.MoveTo (x + radius, y);
            gr.CurveTo (x + radius, y + radius, x + radius, y + radius, x, y + radius);
        } else {
            gr.MoveTo (x + radius, y);
            gr.CurveTo (x + radius, y + radius, x + radius, y + radius, x + cellsize, y + radius);
            gr.MoveTo (x, y + radius);
            gr.CurveTo (x + radius, y + radius, x + radius, y + radius, x + radius, y + cellsize);
        }
        
        gr.Restore ();
    }
    
    public static void RoundedCell (Cairo.Context gr, int i, int j, double radius, bool rotate)
    {
        double cellsize = radius * 2;
        double x = cellsize * i, y = cellsize * j;
        gr.Save ();
        if (rotate) {
            gr.MoveTo (x + radius, y);
            gr.Arc (x, y, radius, 0, Math.PI / 2);
            gr.MoveTo (x + cellsize, y + radius);
            gr.ArcNegative (x + cellsize, y + cellsize, radius, -Math.PI / 2, Math.PI);
 
        } else {
            gr.MoveTo (x, y + radius);
            gr.Arc (x, y + cellsize, radius, -Math.PI / 2, 0);
            gr.MoveTo (x + radius, y);
            gr.ArcNegative (x + cellsize, y, radius, Math.PI, Math.PI / 2);
 
 
        }
        
        gr.Restore ();
    }
}
 
public class CairoGraphic : DrawingArea
{
    Truchet tr;
    
    public CairoGraphic ()
    {
        tr = new Truchet (2, 2, 10);
        tr.GenerateMap ();
    }
 
    protected override bool OnExposeEvent (Gdk.EventExpose args)
    {
	using (Context g = Gdk.CairoHelper.Create (args.Window)){
	    tr.RenderMap (g, 0, 0);
	    
	    g.Color = new Color (0.6, 0.7, 0.9, 1);
	    g.LineWidth = 2;
	    g.Stroke ();
	}
        return true;
    }
}
