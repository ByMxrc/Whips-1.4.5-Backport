using System;
using System.Reflection;
using Terraria;

class Program {
    static void Main() {
        var meth = typeof(Projectile).GetMethod("FillWhipControlPoints", BindingFlags.Public | BindingFlags.Static);
        Console.WriteLine(meth != null ? "Found" : "Not Found");
    }
}
