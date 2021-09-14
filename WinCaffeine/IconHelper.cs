using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace WinCaffeine
{
    public static class IconHelper
    {
        public static Icon GetIconFromEmbeddedResource(string name, Size size)
        {
            var asm = Assembly.GetExecutingAssembly();
            var rnames = asm.GetManifestResourceNames();
            var tofind = "." + name + ".ICO";
            foreach (string rname in rnames)
            {
                if (rname.EndsWith(tofind, StringComparison.CurrentCultureIgnoreCase))
                {
                    using (var stream = asm.GetManifestResourceStream(rname))
                    {
                        return new Icon(stream, size);
                    }
                }
            }
            throw new ArgumentException("Icon not found");
        }
    }
}
