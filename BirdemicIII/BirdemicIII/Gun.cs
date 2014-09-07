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
    class Gun : Weapon
    {
        int maxAmmo;
        int curAmmo;
        public int CurAmmo
        {
            get { return curAmmo; }
            set { curAmmo = value; }
        }
        int magSize;
        int magCur;
        public int MagCur
        {
            get { return magCur; }
            set { magCur = value; }
        }
        float reloadDelta;
        bool reloading;
        float startReloadDelta;
        

        public Gun(Game game, string name, Vector3 pos, Quaternion rot, float fireDelta, float reloadDelta, float damage, float range, bool automatic, int maxAmmo, int magSize, int curAmmo, int magCur, Character owner)
            : base(game, name, pos, rot, fireDelta, damage, range, automatic, owner)
        {
            this.reloadDelta = reloadDelta;
            this.maxAmmo = maxAmmo;
            this.magSize = magSize;
            this.curAmmo = curAmmo;
            this.magCur = magCur;

            //Body.DisableBody();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float elapsedTime = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerMillisecond;


            // reload stuff
            if (magCur == 0)
                ReloadMag();
            if (reloading)
            {
                if (startReloadDelta < reloadDelta)
                    startReloadDelta += elapsedTime;
                else
                    reloading = false;
            }

        }

        protected override bool CanFire()
        {
            if (magCur > 0 && !reloading)
                return true;
            return false;
        }

        protected override void Fire()
        {
            magCur--;
            curAmmo--;

            Console.WriteLine(magCur.ToString() + "/" + curAmmo.ToString());

            float dist;
            Vector3 pos;
            Vector3 normal;

            ((Person)owner).hasFired = true;

            Random r = new Random();
            Vector3 dir = Vector3.Transform(new Vector3(0, 0, -1), Matrix.CreateFromYawPitchRoll(0, -0.3f, 0) * Matrix.CreateFromQuaternion(owner.cameraRotation));
            float maxError = 0.10f;
            float e1, e2, e3;
            e1 = ((float)r.NextDouble() - 0.5f) * MathHelper.Pi * maxError;
            e2 = ((float)r.NextDouble() - 0.5f) * MathHelper.Pi * maxError;
            e3 = ((float)r.NextDouble() - 0.5f) * MathHelper.Pi * maxError;

            Console.WriteLine("Aim Error: " + e1.ToString() + " " + e2.ToString() + " " + e3.ToString());

            dir = Vector3.Transform(dir, Matrix.CreateFromYawPitchRoll(e1, e2, e3));

            DetectCollisions(dir);
        }

        void ReloadMag()
        {
            if (curAmmo > 0)
            {
                Console.WriteLine("reloading");
                reloading = true;
                startReloadDelta = 0;
                if (curAmmo > magSize)
                    magCur = magSize;
                else
                    magCur = curAmmo;
                curAmmo -= magCur;
            }
        }

    }
}
