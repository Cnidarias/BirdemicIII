﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RoundLineCode;

namespace BirdemicIII
{
    public abstract class Weapon : DrawableGameComponent
    {
        protected float fireDelta;
        protected float damage;
        protected float range;
        protected bool automatic;
        protected float lastFireDelta;

        protected Vector3 carryPos;
        protected Quaternion carryRot;

        protected ButtonState lastLeftState;

        protected Vector3 pos;
        protected Quaternion rot;

        protected Character owner;
        

        protected Vector3 scale;
        protected Model model;
        protected string _modelName;


        public Weapon(Game game, string name, Vector3 pos, Quaternion rot, float fireDelta, float damage, float range, bool automatic, Character owner)
            : base(game)
        {

            Deactivate();
            
            this.fireDelta = fireDelta;
            this.damage = damage;
            this.range = range;
            this.automatic = automatic;
            lastFireDelta = 0;

            this.pos = pos;
            this.rot = rot;
            this.owner = owner;

            this.scale = 0.1f * Vector3.One;

            this._modelName = name;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            //Vector3 thirdPRef = new Vector3(0, 0, 0.25f) + carryPos;
            Vector3 thirdPRef = carryPos;
            Matrix rotMat = Matrix.CreateFromQuaternion(owner.cameraRotation);
            Vector3 transRef = Vector3.Transform(thirdPRef, rotMat);
            rot = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromQuaternion(carryRot) * Matrix.CreateFromQuaternion(owner.cameraRotation));
            //rot = owner.cameraRotation;
            pos = transRef + owner.Position;
            
            float elapsedTime = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerMillisecond;

            lastFireDelta += elapsedTime;

            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if ((automatic || lastLeftState == ButtonState.Released) && lastFireDelta > fireDelta && CanFire())
                {
                    if(((Game1)Game).person != null)
                        ((Game1)Game).person.haveFired = true;
                    lastFireDelta = 0;
                    Fire();
                }
                //Console.WriteLine("Fire");
            }

            lastLeftState = mouse.LeftButton;
        }

        public void Activate()
        {
            Enabled = true;
            Game.Components.Add(this);
        }

        public void Deactivate()
        {
            Enabled = false;
            Game.Components.Remove(this);
        }

        protected abstract bool CanFire();

        protected abstract void Fire();


        private BasicEffect basicEffect;
        private Vector3 startPoint = new Vector3(0, 0, 0);
        private Vector3 endPoint = new Vector3(0, 0, -50);


        protected void DetectCollisions(Vector3 dir)
        {
            //Ray r = new Ray(pos, dir);
            Ray r = new Ray(owner.Position + new Vector3(0, 0.1f, 0), dir);
            Bird closest = null;
            float? dist = null;
            float closestDist = -1;

            startPoint = pos;
            endPoint = startPoint + 500 * dir;
            
            /* find the closest building distance if there is one */
            foreach (BoundingBox b in ((Game1)Game).Env.BuildingBoundingBoxes)
            {
                dist = r.Intersects(b);
                if (dist != null)
                {
                    Console.WriteLine("intersect: " + ((float)dist).ToString());
                    if (closestDist == -1 || (float)dist < closestDist)
                    {
                        closestDist = (float)dist;
                    }
                }
            }

            /*if (closestDist != -1)
                Console.WriteLine("Hit a building");
            else
                Console.WriteLine("No hits");
             * */

            /* find the closest character if there is one closer than the closest building */

            foreach (GameComponent gc in ((Game1)Game).Components)
            {
                if (gc.GetType() == typeof(Bird))
                {
                    dist = r.Intersects(((Bird)gc).BoundingSphere);
                    if (dist != null)
                    {
                        if (closestDist == -1 || (float)dist < closestDist)
                        {
                            closestDist = (float)dist;
                            closest = (Bird)gc;
                        }
                    }
                }
            }
            
            if (closest != null)
            {
                if (((Game1)Game).person != null)
                {
                    owner.hasKill = true;
                    owner.killedID = closest.ID;
                    ((Game1)Game).person.haveKill = true;
                    ((Game1)Game).person.haveKilledID = closest.officialID;
                }
            }
            
        }


        protected override void LoadContent()
        {
            model = Game.Content.Load<Model>(_modelName);

            basicEffect = new BasicEffect(GraphicsDevice);
            //basicEffect.View = Matrix.CreateLookAt(new Vector3(50, 50, 50), new Vector3(0, 0, 0), Vector3.Up);
            //basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), GraphicsDevice.Viewport.AspectRatio, 1f, 1000f);
        }

        private Matrix GetWorldMatrix()
        {
            return
                Matrix.CreateScale(scale) *
                Matrix.CreateFromQuaternion(rot) *
                Matrix.CreateTranslation(pos);
        }

        public override void Draw(GameTime gameTime)
        {
            Game1 game = (Game1)Game;
            this.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            this.Game.GraphicsDevice.BlendState = BlendState.Opaque;
            this.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.Game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix worldMatrix = GetWorldMatrix();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = game.viewMatrix;
                    effect.Projection = game.projectionMatrix;
                }
                mesh.Draw();
            }

            basicEffect.View = ((Game1)Game).viewMatrix;
            basicEffect.Projection = ((Game1)Game).projectionMatrix;
            
            basicEffect.CurrentTechnique.Passes[0].Apply();
            var vertices = new[] { new VertexPositionColor(startPoint, Color.Red), new VertexPositionColor(endPoint, Color.Red) };
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
            }


            base.Draw(gameTime);
        }

    }
}
