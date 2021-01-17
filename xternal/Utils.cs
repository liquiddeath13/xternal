using System;
using System.Drawing;

namespace xternal
{
    public class Utils
    {
        public static float Rad2Deg(float rad) => (float)(rad * 180 / Math.PI);
        public static float Deg2Rad(float deg) => (float)(deg * Math.PI / 180);
        public static PointF RotatePoint(PointF origin, float distance, float angle)
        {
            float radians = Deg2Rad(angle);
            float sin = (float)Math.Sin(radians);
            float cos = (float)Math.Cos(radians);
            return new PointF(origin.X + distance * sin, origin.Y + distance * cos);
        }
        public static float GetRotationAngle(LTRotation rotation)
        {
            var angle = (float)Math.Acos(rotation.W) * 2;
            float fSinAngle = (float)Math.Sin(angle * 0.5);
            var axisY = rotation.Y / fSinAngle;
            if (axisY > 0) angle *= -1;
            return Rad2Deg(angle) + 90;
        }
    }
}
