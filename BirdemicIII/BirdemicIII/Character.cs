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

        protected int _ID;
        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

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
            base.Update(gameTime);
        }
    }
}
