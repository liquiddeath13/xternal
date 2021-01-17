using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;

namespace xternal
{
    public partial class RadarForm : MetroFramework.Forms.MetroForm
    {
        PVPLogicHolder pvpCore;
        int halfX;
        int halfY;
        readonly List<PlayerRadarEntity> radarEntities = new List<PlayerRadarEntity>();
        public bool PlayerExistInList(string nickname) => radarEntities.ToArray().FirstOrDefault(x => x != null && x.Nickname == nickname) != null;
        public RadarForm()
        {
            InitializeComponent();
            pvpCore = new PVPLogicHolder();
            halfX = pictureBox2.ClientRectangle.Width / 2;
            halfY = pictureBox2.ClientRectangle.Height / 2;
            for (int i = 0; i < 16; ++i) radarEntities.Add(null);
            Timer redrawTimer = new Timer
            {
                Interval = 15,
                Enabled = true
            };
            redrawTimer.Tick += RedrawPictureBox;
            redrawTimer.Start();
        }

        private void RedrawPictureBox(object sender, EventArgs e) => pictureBox2.Invalidate();

        private void UpdateRadarEntities()
        {
            Parallel.For(0, 16, (i) =>
            {
                if (pvpCore.ValidPlayer(i, excludeMe: false, nearOnly: false))
                {
                    if (!PlayerExistInList(pvpCore.GetPlayerName(i)))
                    {
                        radarEntities[i] = new PlayerRadarEntity
                        {
                            Enemy = pvpCore.GetLocalPlayerTeam() != pvpCore.GetPlayerTeam(i),
                            Nickname = new string(pvpCore.GetPlayerName(i).Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray())
                        };
                    }
                    var playerPos = pvpCore.GetPlayerPos(i);
                    radarEntities[i].RotationAngle = Utils.GetRotationAngle(pvpCore.GetPlayerRotation(i));
                    radarEntities[i].X = playerPos.Z / 50;
                    radarEntities[i].Y = playerPos.X / 50;
                    radarEntities[i].Health = pvpCore.GetPlayerHealth(i);
                    radarEntities[i].HasC4 = pvpCore.GetPlayerHasC4(i);
                }
                else radarEntities[i] = null;
            });
        }

        private void DrawRadarEntities(PaintEventArgs e)
        {
            try
            {
                var localRotationAngle = Utils.GetRotationAngle(pvpCore.GetLocalPlayerRotation());
                var localPos = pvpCore.GetLocalPlayerPos();
                var collection = radarEntities.Where(x => x != null).ToList();
                foreach (var player in collection)
                {
                    var statestr = $"{player.Health} HP{(player.HasC4 ? "\nC4" : "")}\n{player.Nickname}";
                    Color c = player.Enemy ? Color.Red : Color.Green;

                    PointF ellipseStarts = Utils.RotatePoint(new PointF(halfX + player.X - 2, (pictureBox2.Height - (halfY - player.Y - 2))), 0, localRotationAngle);
                    PointF lineStart = Utils.RotatePoint(new PointF(ellipseStarts.X + 6, ellipseStarts.Y + 6), 0, localRotationAngle);
                    PointF lineEnd = Utils.RotatePoint(lineStart, 12, player.RotationAngle);

                    e.Graphics.DrawEllipse(new Pen(c), new RectangleF(ellipseStarts, new Size(12, 12)));
                    e.Graphics.DrawString(statestr, new Font("Consolas", 8), new SolidBrush(c), new PointF(ellipseStarts.X + 14, ellipseStarts.Y));
                    e.Graphics.DrawLine(new Pen(c), lineStart, lineEnd);
                }
            }
            catch (Exception) { }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            if (pvpCore != null && pvpCore.IsInFight())
            {
                UpdateRadarEntities();
                DrawRadarEntities(e);
            }
        }
    }
}
