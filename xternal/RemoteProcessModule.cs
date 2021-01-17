using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xternal
{
    public class RemoteProcessModule
    {
        public IntPtr Entry { get; set; }
        public uint Size { get; set; }
        public string Name { get; set; }
    }
}
