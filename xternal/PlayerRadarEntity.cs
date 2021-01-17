using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xternal
{
    class PlayerRadarEntity
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float RotationAngle { get; set; }
        public int Health { get; set; }
        public string Nickname { get; set; }
        public bool Enemy { get; set; }
        public bool HasC4 { get; set; }
    }
}
