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
    public class Person : Character
    {
        public int officialID;
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
        Model personModel;
        Vector3 initPosition = new Vector3(8, 0.0f, -3);
        float camRotX = 0;
        Effect effect;
        Texture2D bulletTexture;

        Weapon _weapon;
        public Weapon weapon
        {
            get { return _weapon; }
        }

        Vector3 scale = new Vector3(0.00025f, 0.00025f, 0.00025f);

        protected bool _hasFired = false;
        public bool hasFired
        {
            get { return _hasFired; }
            set { _hasFired = value; }
        }

        //Constructor
        public Person(Game game, bool ActivePlayer, int Id, Vector3 pos, bool shot, bool DEAD)
            : base(game)
        {
            activePlayer = ActivePlayer;
            ID = Id;
            officialID = Id;
            hasFired = shot;
            _alive = !DEAD;
            _Position = initPosition;

            MachineGun machine = new MachineGun(Game, _Position, this);
            machine.DrawOrder = 3;
            machine.Activate();
            _weapon = machine;
        }

        protected override void LoadContent()
        {
            lightDirection.Normalize();
            
            effect = Game.Content.Load<Effect>("effects");
            bulletTexture = Game.Content.Load<Texture2D>("bullet");
            //personModel = LoadModel("Person");
            personModel = Game.Content.Load<Model>("Person");
            
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            if (activePlayer)
            {
                float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 1000.0f * gameSpeed;
                //MoveForward(ref _Position, _Rotation, moveSpeed);
                Move(moveSpeed);
                UpdateView();
                ProcessKeyboard(gameTime);

                _BoundingSphere = new BoundingSphere(_Position + new Vector3(0, 1, 0), 0.35f);
                if (CheckCollision(_BoundingSphere) != CollisionType.None)
                {
                    _Position = initPosition;
                    _Rotation = Quaternion.Identity;
                    camRotX = 0;
                    //gameSpeed /= 1.1f;
                }


                UpdateCamera();
                UpdateBulletPositions(moveSpeed);
            }
            else
            {
                Game1 g = ((Game1)Game);
                if (!g.Client.HumanArr[ID].dead)
                {
                    _Position.X = g.Client.HumanArr[ID].X;
                    _Position.Y = g.Client.HumanArr[ID].Y;
                    _Position.Z = g.Client.HumanArr[ID].Z;

                    _alive = !g.Client.BirdArr[ID].Dead;
                }
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            //DrawModel();
            drawMddel2();
            DrawBullets();
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
            ((Game1)Game).cameraRotation = Quaternion.Lerp(((Game1)Game).cameraRotation, _cameraRotation, 0.1f);

            //Vector3 campos = new Vector3(0, 0.1f, 0.6f);
            Vector3 campos = new Vector3(0, 0.3f, 0.3f);
            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(((Game1)Game).cameraRotation));
            campos += _Position;

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(((Game1)Game).cameraRotation));

            ((Game1)Game).viewMatrix = Matrix.CreateLookAt(campos, _Position + new Vector3(0, 0.1f, 0), camup);
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

           
            return CollisionType.None;
        }

        private void ProcessKeyboard(GameTime gameTime)
        {
            float leftRightRot = 0;

            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
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
            _Rotation *= additionalRot;

            if (keys.IsKeyDown(Keys.Space))
            {
                double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
                if (currentTime - lastBulletTime > 100)
                {
                    Bullet newBullet = new Bullet();
                    newBullet.position = _Position;
                    newBullet.rotation = _Rotation;
                    bulletList.Add(newBullet);

                    lastBulletTime = currentTime;
                }
            }
        }

        private void Move(float moveSpeed)
        {
            Vector3 moveVector;
            moveVector = Vector3.Zero;
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keys.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keys.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keys.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);

            Vector3 addVector = Vector3.Transform(moveVector, _Rotation);
            _Position += addVector * moveSpeed;
        }

        private void UpdateView()
        {
            MouseState mouse = Mouse.GetState();
            GraphicsDeviceManager graphics = ((Game1)this.Game).Graphics;
            int x = graphics.PreferredBackBufferWidth / 2;
            int y = graphics.PreferredBackBufferHeight / 2;

            KeyboardState keys = Keyboard.GetState();

            float _stepSize = 5;

            if (mouse.X - x < -2)
            {
                this.ChangeLook(new Vector3(0, MathHelper.ToRadians(_stepSize), 0));
                Mouse.SetPosition(x, mouse.Y);
            }
            if (mouse.X - x > 2)
            {
                this.ChangeLook(new Vector3(0, -MathHelper.ToRadians(_stepSize), 0));
                Mouse.SetPosition(x, mouse.Y);
            }
            if (mouse.Y - y < -2)
            {
                this.ChangeLook(new Vector3(MathHelper.ToRadians(_stepSize), 0, 0));
                Mouse.SetPosition(mouse.X, y);
            }
            if (mouse.Y - y > 2)
            {
                this.ChangeLook(new Vector3(-MathHelper.ToRadians(_stepSize), 0, 0));
                Mouse.SetPosition(mouse.X, y);
            }
        }

        public void ChangeLook(Vector3 angles)
        {
            float rotX, rotY;
            rotX = angles.X;
            rotY = angles.Y;
            camRotX += rotX;
            Matrix newRotCam = Matrix.CreateRotationX(camRotX) * Matrix.CreateRotationY(rotY) * Matrix.CreateFromQuaternion(_Rotation);
            Matrix newRotPerson = Matrix.CreateRotationY(rotY) * Matrix.CreateFromQuaternion(_Rotation);
            _cameraRotation = Quaternion.Lerp(_cameraRotation, Quaternion.CreateFromRotationMatrix(newRotCam), 0.5f);
            _Rotation = Quaternion.Lerp(_Rotation, Quaternion.CreateFromRotationMatrix(newRotPerson), 0.5f);
        }

        private Model LoadModel(string assetName)
        {

            Model newModel = Game.Content.Load<Model>(assetName); foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
            return newModel;
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
            position += addVector * speed;
        }

        /*
        private void DrawModel()
        {
            //Matrix worldMatrix = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(_Rotation) * Matrix.CreateTranslation(_Position);
            Matrix worldMatrix = Matrix.CreateScale(0.000005f, 0.000005f, 0.000005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(_Rotation) * Matrix.CreateTranslation(_Position);

            Matrix[] xwingTransforms = new Matrix[personModel.Bones.Count];
            personModel.CopyAbsoluteBoneTransformsTo(xwingTransforms);
            foreach (ModelMesh mesh in personModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
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
        */

        private void drawMddel2()
        {
            this.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.Game.GraphicsDevice.BlendState = BlendState.Opaque;
            this.Game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            Matrix[] transforms = new Matrix[personModel.Bones.Count];
            personModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in personModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    //effect.World = Matrix.CreateScale(0.0025f, 0.0025f, 0.0025f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
                    effect.World = Matrix.CreateScale(scale) * Matrix.CreateRotationY(MathHelper.PiOver2 + 0.45f) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
                    effect.View = ((Game1)Game).viewMatrix;
                    effect.Projection = ((Game1)Game).projectionMatrix;
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
