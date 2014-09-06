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
     public class Game1 : Microsoft.Xna.Framework.Game
     {
         public bool CANDRAW = false;
         GraphicsDeviceManager graphics;
         SpriteBatch spriteBatch;
         GraphicsDevice device;
         public int ID = -1;
         public Vector3 cameraPosition;
         public Vector3 cameraUpDirection;

         public Quaternion cameraRotation = Quaternion.Identity;
 
         public Matrix viewMatrix;
         public Matrix projectionMatrix;
         
         private Bird _bird;
         public Bird bird
         {
             get { return _bird; }
             set { _bird = value; }
         }
         Client client;
         public Client Client
         {
             get { return client; }
         }

         environment _env;
         public environment Env
         {
             get { return _env; }
         }
 
         public Game1()
         {
             graphics = new GraphicsDeviceManager(this);
             Content.RootDirectory = "Content";
         }
 
         protected override void Initialize()
         {
             graphics.PreferredBackBufferWidth = 800;
             graphics.PreferredBackBufferHeight = 600;
             graphics.IsFullScreen = false;
             graphics.ApplyChanges();
             Window.Title = "Birdemic III: The Birdemicing";

             _env = new environment(this);
             _env.DrawOrder = 1;
             /*for (int i = 1; i < 50; i++)
             {     
                 _bird = new Bird(this, false, i);
                 _bird.DrawOrder = 50 + i;
                 Components.Add(_bird);
             }*/
             client = new Client(this);

             //_bird = new Bird(this, true, 0);
             //_bird.DrawOrder = 2;
            // Components.Add(_bird);
    
             Components.Add(_env);
             Components.Add(client);
 
             base.Initialize();
         }
 
         protected override void LoadContent()
         {
             spriteBatch = new SpriteBatch(GraphicsDevice);
             
             device = graphics.GraphicsDevice;
         }
  

        
        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keys = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keys.IsKeyDown(Keys.Escape))
                this.Exit();
            //Console.WriteLine(bird.Position.Y.ToString());
            base.Update(gameTime);
        }   
 
         
 
         protected override void Draw(GameTime gameTime)
         {
             device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
             base.Draw(gameTime);
         }
    
        
     }
 }