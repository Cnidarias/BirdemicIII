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
    public class Bird : Character
    {
        public int officialID;
        bool dead = false;
        bool activePlayer = true;
        enum CollisionType { None, Building, Boundary, Target }
        float gameSpeed = 1.0f;

        struct Bullet
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        Vector3 lightDirection = new Vector3(3, -2, 5);
        List<Bullet> bulletList = new List<Bullet>();
        double lastBulletTime = 0;
        Model xwingModel;
        Vector3 xwingPosition = new Vector3(8, 1, -3);

        public Vector3 Position
        {
            set { xwingPosition = value; }
            get { return xwingPosition; }
        }

        Quaternion xwingRotation = Quaternion.Identity;
        Effect effect;
        Texture2D bulletTexture;
        SoundEffect explosionsound;

        //Constructor
        public Bird(Game game, bool ActivePlayer, int Id, Vector3 pos, bool AI, bool DEAD)
            : base(game)
        {
            dead = DEAD;
            xwingPosition = pos;
            activePlayer = ActivePlayer;
            ID = Id;
            officialID = Id;
        }

        protected override void LoadContent()
        {
            lightDirection.Normalize();
            
            effect = Game.Content.Load<Effect>("effects");
            bulletTexture = Game.Content.Load<Texture2D>("bullet");
            //xwingModel = LoadModel("Cone");
            xwingModel = Game.Content.Load<Model>("Eagle_flapping");
            explosionsound = Game.Content.Load<SoundEffect>("explosionsound");

            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            if (activePlayer)
            {
                float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 1000.0f * gameSpeed;
                MoveForward(ref xwingPosition, xwingRotation, moveSpeed);
                ProcessKeyboard(gameTime);

                BoundingSphere xwingSpere = new BoundingSphere(xwingPosition, 0.04f);
                if (CheckCollision(xwingSpere) != CollisionType.None)
                {
                    BillBoarding billy = new BillBoarding((Game1)Game, "explosiontexture", xwingPosition + new Vector3(-.05f, -0.7f, 0), new Vector2(1, 1), new Vector2(10, 1), 100.0f);
                    billy.DrawOrder = 100000;
                    ((Game1)Game).Components.Add(billy);

                    explosionsound.Play(0.15f, 0.0f, 0.0f);
                    _alive = false;
                    //xwingPosition = new Vector3(8, 1, -3);
                    //xwingRotation = Quaternion.Identity;
                    //gameSpeed /= 1.1f;
                }

                UpdateCamera();
                UpdateBulletPositions(moveSpeed);
            }
            else
            {
                
                Game1 g = ((Game1)Game);
                if (!g.Client.BirdArr[ID].Dead)
                {
                    //Console.Write("I HAPPEN");
                    xwingPosition.X = g.Client.BirdArr[ID].X;
                    xwingPosition.Y = g.Client.BirdArr[ID].Y;
                    xwingPosition.Z = g.Client.BirdArr[ID].Z;
                    
                    dead = g.Client.BirdArr[ID].Dead;
                }

            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (!((Game1)Game).Client.BirdArr[ID].Dead)
            {
                drawMddel2();
                DrawBullets();
            }
            base.Draw(gameTime);
        }

        private void UpdateBulletPositions(float moveSpeed)
        {
            for (int i = 0; i < bulletList.Count; i++)
            {
                Bullet currentBullet = bulletList[i];
                MoveForward(ref currentBullet.position, currentBullet.rotation, moveSpeed * 2.0f);
                bulletList[i] = currentBullet;

                BoundingSphere bulletSphere = new BoundingSphere(currentBullet.position, 0.05f);
                CollisionType colType = CheckCollision(bulletSphere);
                if (colType != CollisionType.None)
                {
                    bulletList.RemoveAt(i);
                    i--;

                    if (colType == CollisionType.Target)
                        gameSpeed *= 1.05f;
                }
            }
        }

        private void UpdateCamera()
        {

            ((Game1)Game).cameraRotation = Quaternion.Lerp(((Game1)Game).cameraRotation, xwingRotation, 0.1f);

            Vector3 campos = new Vector3(0, 0.1f, 0.6f);
            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(((Game1)Game).cameraRotation));
            campos += xwingPosition;

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(((Game1)Game).cameraRotation));

            ((Game1)Game).viewMatrix = Matrix.CreateLookAt(campos, xwingPosition, camup);
            ((Game1)Game).projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Game.GraphicsDevice.Viewport.AspectRatio, 0.2f, 500.0f);

            ((Game1)Game).cameraPosition = campos;
            ((Game1)Game).cameraUpDirection = camup;
        }

        private CollisionType CheckCollision(BoundingSphere sphere)
        {
            for (int i = 0; i < ((Game1)Game).Env.BuildingBoundingBoxes.Length; i++)
                if (((Game1)Game).Env.BuildingBoundingBoxes[i].Contains(sphere) != ContainmentType.Disjoint)
                    return CollisionType.Building;

            if (((Game1)Game).Env.CompleteCityBox.Contains(sphere) != ContainmentType.Contains)
                return CollisionType.Boundary;

            foreach (GameComponent gc in ((Game1)Game).Components)
            {
                if (gc.GetType() == typeof(Person))
                {
                    if (((Person)gc).BoundingSphere.Contains(sphere) != ContainmentType.Disjoint)
                    {
                        _hasKill = true;
                        _killedID = ((Person)gc).ID;
                        break;
                    }
                }
            }
           
            return CollisionType.None;
        }

        private void ProcessKeyboard(GameTime gameTime)
        {
            float leftRightRot = 0;

            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 750.0f;
            turningSpeed *= 1.6f * gameSpeed;
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Right))
                leftRightRot += turningSpeed;
            if (keys.IsKeyDown(Keys.Left))
                leftRightRot -= turningSpeed;

            float upDownRot = 0;
            if (keys.IsKeyDown(Keys.Down))
                upDownRot += turningSpeed;
            if (keys.IsKeyDown(Keys.Up))
                upDownRot -= turningSpeed;

            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRot) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot);
            xwingRotation *= additionalRot;

            if (keys.IsKeyDown(Keys.Space))
            {
                double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
                if (currentTime - lastBulletTime > 100)
                {
                    Bullet newBullet = new Bullet();
                    newBullet.position = xwingPosition;
                    newBullet.rotation = xwingRotation;
                    bulletList.Add(newBullet);

                    lastBulletTime = currentTime;
                }
            }
        }

        private Model LoadModel(string assetName)
        {

            Model newModel = Game.Content.Load<Model>(assetName); 
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
            return newModel;
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
            position += addVector * speed;
        }
        private void drawMddel2()
        {
            this.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.Game.GraphicsDevice.BlendState = BlendState.Opaque;
            this.Game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            Matrix[] transforms = new Matrix[xwingModel.Bones.Count];
            xwingModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in xwingModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = Matrix.CreateScale(0.0025f, 0.0025f, 0.0025f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(xwingRotation) * Matrix.CreateTranslation(xwingPosition);
                    effect.View = ((Game1)Game).viewMatrix;
                    effect.Projection = ((Game1)Game).projectionMatrix;
                }
                mesh.Draw();
            }
        }
        private void DrawModel()
        {
            Matrix worldMatrix = Matrix.CreateScale(0.0025f, 0.0025f, 0.0025f) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateFromQuaternion(xwingRotation) * Matrix.CreateTranslation(xwingPosition);
           // Effect e = new BasicEffect(Game.GraphicsDevice);
            Matrix[] xwingTransforms = new Matrix[xwingModel.Bones.Count];
            xwingModel.CopyAbsoluteBoneTransformsTo(xwingTransforms);
            foreach (ModelMesh mesh in xwingModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    
                    //currentEffect.CurrentTechnique = e.CurrentTechnique;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(xwingTransforms[mesh.ParentBone.Index] * worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(((Game1)Game).viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(((Game1)Game).projectionMatrix);
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xLightDirection"].SetValue(lightDirection);
                    currentEffect.Parameters["xAmbient"].SetValue(0.5f);
                }
                mesh.Draw();
            }
        }

        private void DrawBullets()
        {
            if (bulletList.Count > 0)
            {
                VertexPositionTexture[] bulletVertices = new VertexPositionTexture[bulletList.Count * 6];
                int i = 0;
                foreach (Bullet currentBullet in bulletList)
                {
                    Vector3 center = currentBullet.position;

                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 0));

                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                }

                effect.CurrentTechnique = effect.Techniques["PointSprites"];
                effect.Parameters["xWorld"].SetValue(Matrix.Identity);
                effect.Parameters["xProjection"].SetValue(((Game1)Game).projectionMatrix);
                effect.Parameters["xView"].SetValue(((Game1)Game).viewMatrix);
                effect.Parameters["xCamPos"].SetValue(((Game1)Game).cameraPosition);
                effect.Parameters["xTexture"].SetValue(bulletTexture);
                effect.Parameters["xCamUp"].SetValue(((Game1)Game).cameraUpDirection);
                effect.Parameters["xPointSpriteSize"].SetValue(0.1f);

                Game.GraphicsDevice.BlendState = BlendState.Additive;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, bulletVertices, 0, bulletList.Count * 2);
                }

                Game.GraphicsDevice.BlendState = BlendState.Opaque;
            }
        }
    }
}
