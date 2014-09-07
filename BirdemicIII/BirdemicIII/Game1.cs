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
         public GraphicsDeviceManager Graphics
         {
             get { return graphics; }
         }
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

         private Person _person;
         public Person person
         {
             get { return _person; }
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

             this.IsMouseVisible = false;

             _env = new environment(this);

             //_person = new Person(this);

             _env.DrawOrder = 1;

             //_person.DrawOrder = 2;

             //Components.Add(_person);

             //_bird.DrawOrder = 2;
             //_person.DrawOrder = 2;

             //Components.Add(_person);
             //Components.Add(_bird);

             Components.Add(_env);

             /*MachineGun machine = new MachineGun(this, _person.Position, _person);
             machine.DrawOrder = 3;
             machine.Activate();*/

             client = new Client(this);

             Components.Add(client);
 
             base.Initialize();
         }
 
         protected override void LoadContent()
         {
             Song song = Content.Load<Song>("ilikebirds");
             MediaPlayer.Play(song);
             MediaPlayer.IsRepeating = true;
             MediaPlayer.Volume = 0.25f;

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