using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xternal
{
    public class LTRotation
    {
        public float X, Y, Z, W;
        public float Length() => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        public void Normalize()
        {
            var a = Length();
            if (a == 0)
            {
                Z = Y = X = 0;
                W = 1;
            }
            else
            {
                a = 1 / a;
                X *= a;
                Y *= a;
                Z *= a;
                W *= a;
            }
        }
    };
}
