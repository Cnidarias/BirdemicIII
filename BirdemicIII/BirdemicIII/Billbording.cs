using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace BirdemicIII
{
    class BillBoarding : DrawableGameComponent
    {
        Vector3 _pos;
        String _texName;
        Texture2D _tex;
        VertexBuffer billBuf;
        VertexDeclaration billDec;
        Effect bbEffect;
        VertexPositionTexture[] _billVerticies;
        Vector2 _size;
        Texture2D[] _texA;
        Vector2 _xy;
        int w, h, ele, index = 0;
        float _aSp;
        float _deltaT;
        Vector3 rot = Vector3.Up;


        public BillBoarding(Game game, String texName, Vector3 pos, Vector2 size)
            : base(game)
        {
            _pos = pos;
            _texName = texName;
            _size = size;
            _xy = Vector2.Zero;
        }

        public BillBoarding(Game game, String texName, Vector3 pos, Vector2 size, Vector3 rot)
            : this(game, texName, pos, size)
        {
            this.rot = rot;
        }

        public BillBoarding(Game game, String texName, Vector3 pos, Vector2 size, Vector2 xy, float time)
            : base(game)
        {
            _pos = pos;
            _texName = texName;
            _size = size;
            _xy = xy;
            _aSp = time;
        }

        protected override void LoadContent()
        {
            _tex = Game.Content.Load<Texture2D>(_texName);
            
            RenderTarget2D renderTarget = new RenderTarget2D(((Game1)Game).GraphicsDevice, (int)(_tex.Width * _size.X), (int)(_tex.Height * _size.Y));
            Rectangle rect = new Rectangle(0, 0, (int)(_tex.Width * _size.X), (int)(_tex.Height * _size.Y));
            ((Game1)Game).GraphicsDevice.SetRenderTarget(renderTarget);
            SpriteBatch batch = new SpriteBatch(((Game1)Game).GraphicsDevice);
            batch.Begin();
            batch.Draw(_tex, rect, Color.White);
            batch.End();
            ((Game1)Game).GraphicsDevice.SetRenderTarget(null);
            
            if (_xy != Vector2.Zero)
            {                
                ele = (int)(_xy.X * _xy.Y);
                _texA = new Texture2D[ele];
                int c = 0;
                w = _tex.Width / (int)_xy.X;
                h = _tex.Height / (int)_xy.Y;

                Color[] s = new Color[w * h];
                for (int i = 0; i < _xy.Y; i++)
                {
                    for (int j = 0; j < _xy.X; j++)
                    {
                        //_tex.GetData<Color>(0, new Microsoft.Xna.Framework.Rectangle(j * w, i * h, w, h), s, 0, w * h);
                        renderTarget.GetData<Color>(0, new Microsoft.Xna.Framework.Rectangle(j * w, i * h, w, h), s, 0, w * h);
                        _texA[c] = new Texture2D(Game.GraphicsDevice, w, h);
                        _texA[c++].SetData<Color>(s);
                    }
                }
            }

            bbEffect = Game.Content.Load<Effect>("bbe");
            createBillBoardVerticies();
            base.LoadContent();

        }

        public override void Update(GameTime gameTime)
        {
            if (_xy == Vector2.Zero)
                return;
            if ((_deltaT += (_deltaT >= _aSp) ? -_deltaT : (gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerMillisecond)) == 0) //you ARE awesome
            {
                if (index == ele - 1)
                {
                    index = 0;
                    ((Game1)Game).Components.Remove(this);
                }
                else
                    index++;
                _tex = _texA[index];
            }
            base.Update(gameTime);
        }

        private void createBillBoardVerticies()
        {
            _billVerticies = new VertexPositionTexture[6];

            int i = 0;
            _billVerticies[i++] = new VertexPositionTexture(_pos, new Vector2(0, 0));
            _billVerticies[i++] = new VertexPositionTexture(_pos, new Vector2(_size.X, 0));
            _billVerticies[i++] = new VertexPositionTexture(_pos, new Vector2(_size.X, _size.Y));

            _billVerticies[i++] = new VertexPositionTexture(_pos, new Vector2(0, 0));
            _billVerticies[i++] = new VertexPositionTexture(_pos, new Vector2(_size.X, _size.Y));
            _billVerticies[i++] = new VertexPositionTexture(_pos, new Vector2(0, _size.Y));

            billBuf = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), 6, BufferUsage.WriteOnly);
            billBuf.SetData(_billVerticies);

            billDec = VertexPositionTexture.VertexDeclaration;
        }

        public override void Draw(GameTime gameTime)
        {
            bbEffect.CurrentTechnique = bbEffect.Techniques["CylBillboard"];
            bbEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            bbEffect.Parameters["xView"].SetValue(((Game1)Game).viewMatrix);
            bbEffect.Parameters["xProjection"].SetValue(((Game1)Game).projectionMatrix);
            bbEffect.Parameters["xCamPos"].SetValue(((Game1)Game).cameraPosition);
            bbEffect.Parameters["xAllowedRotDir"].SetValue(rot);
            bbEffect.Parameters["xBillboardTexture"].SetValue(_tex);



            foreach (EffectPass pass in bbEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                
                Game.GraphicsDevice.SetVertexBuffer(billBuf);

                Game.GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, _billVerticies, 0, 2, VertexPositionTexture.VertexDeclaration);
            }

            base.Draw(gameTime);
        }
    }
}