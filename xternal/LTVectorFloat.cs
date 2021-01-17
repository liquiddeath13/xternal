using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xternal
{
    public class LTVectorFloat
    {
        public float X, Y, Z;
        public LTVectorFloat() { }
        public LTVectorFloat(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public LTVectorFloat Cross(LTVectorFloat b) => new LTVectorFloat(Y * b.Z - Z * b.Y, Z * b.X - X * b.Z, X * b.Y - Y * b.X);
        public LTVectorFloat Subtract(LTVectorFloat b) => new LTVectorFloat(X - b.X, Y - b.Y, Z - b.Z);
        public LTVectorFloat Divide(float b) => new LTVectorFloat(X / b, Y / b, Z / b);
        public float Dot(LTVectorFloat b) => X * b.X + Y * b.Y + Z * b.Z;
        public float LengthSqr() => Dot(this);
        public float Length() => (float)Math.Sqrt(LengthSqr());
        public float MagSqr() => LengthSqr();
        public float Mag() => Length();
        public float Dist(LTVectorFloat b) => Subtract(b).Length();
        public float DistSqr(LTVectorFloat b) => Subtract(b).LengthSqr();
        public LTVectorFloat Unit() => Divide(Length());
        public LTVectorFloat Normalize()
        {
            var l = Length();
            X /= l;
            Y /= l;
            Z /= l;
            return this;
        }
        public override string ToString()
        {
            return $"{X}:{Y}:{Z}";
        }
    }
}
