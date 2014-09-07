using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BirdemicIII
{
    public class Character : DrawableGameComponent
    {

        public int ID = 21341;
        

        protected bool _alive = true;
        public bool alive
        {
            get { return _alive; }
            set { _alive = value; }
        }

        protected bool _hasKill;
        public bool hasKill
        {
            get { return _hasKill; }
            set { _hasKill = value; }
        }

        protected int _killedID;
        public int killedID
        {
            get { return _killedID; }
            set { _killedID = value; }
        }

        protected BoundingSphere _BoundingSphere;
        public BoundingSphere BoundingSphere
        {
            get { return _BoundingSphere; }
        }

        protected Weapon _weapon;
        public Weapon weapon
        {
            get { return _weapon; }
        }

        protected List<Weapon> _weapons;
        public List<Weapon> weapons
        {
            get { return _weapons; }
        }

        protected Vector3 _Position = Vector3.One;
        protected Quaternion _Rotation = Quaternion.Identity;
        protected Quaternion _cameraRotation = Quaternion.Identity;
        public Vector3 Position
        {
            get { return _Position; }
        }
        public Quaternion Rotation
        {
            get { return _Rotation; }
        }
        public Quaternion cameraRotation
        {
            get { return _cameraRotation; }
        }

        //Constructor
        public Character(Game game)
            : base(game)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            //BoundingSphereRenderer.Render(_BoundingSphere, this.GraphicsDevice, ((Game1)Game).viewMatrix, ((Game1)Game).projectionMatrix, Color.Red);
            if (_alive == false)
            {
                if (ID == ((Game1)Game).ID)
                {
                    if ((this.GetType() == typeof(Bird) && ((Game1)Game).gameState.Equals(Game1.STATE.BIRD)) || (this.GetType() == typeof(Person) && ((Game1)Game).gameState.Equals(Game1.STATE.PERSON)))
                    {
                        ((Game1)Game).gameState = (((Game1)Game).gameState.Equals(Game1.STATE.PERSON)) ? Game1.STATE.DEADPERSON : Game1.STATE.DEADBIRD;
                        ((Game1)Game).person = null;
                        ((Game1)Game).bird = null;
                    }
                }

                if (this.GetType() == typeof(Person))
                    ((Person)this).weapon.Deactivate();
                ((Game1)Game).Components.Remove(this);
                
                
            }

            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Tab))
            {
                SwitchWeapon();
            }
            
            base.Update(gameTime);
        }

        void SwitchWeapon()
        {
            _weapon.Deactivate();
            int curI = _weapons.IndexOf(_weapon);
            _weapon = _weapons[(curI + 1) % _weapons.Count];
            _weapon.Activate();
        }
    }
}
