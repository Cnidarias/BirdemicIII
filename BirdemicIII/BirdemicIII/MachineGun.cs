using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BirdemicIII
{
    class MachineGun : Gun
    {
        public MachineGun(Game game, Vector3 pos, Character owner)
            : base(game, "M16", pos, Quaternion.Identity, 50, 2000, 10, 10000, true, 999, 100, 1000, 100, owner)
        {
            //carryPos = new Vector3(0.1f, 0, -0.35f);
            carryPos = new Vector3(0.1f, -0.1f, -0.35f);
            carryRot = Quaternion.CreateFromYawPitchRoll(-MathHelper.PiOver2 + 0.1f, 0.1f, 0);
        }
    }
}
