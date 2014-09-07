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

using System.Threading;

using Lidgren.Network;

namespace BirdemicIII
{
    public class Client : GameComponent
    {
        enum PacketType
        {
            LOGIN,
            BIRD,
            PERSON
        }
        public struct BIRD
        {
            public int ID;
            public float X, Y, Z;
            public bool AI, Dead;
            public System.Net.IPEndPoint IP;
        }
        public struct HUMAN
        {
            public int ID;
            public float X, Y, Z;
            public bool shot, dead;
        }

        NetClient client;
        bool logedIn = false;

        BIRD[] birdArr = new BIRD[50];
        public BIRD[] BirdArr
        {
            get { return birdArr; }
        }
        HUMAN[] humanArr = new HUMAN[25];
        public HUMAN[] HumanArr
        {
            get { return humanArr; }
        }


        public Client(Game game)
            : base(game)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("bird");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            
            client = new NetClient(config);
            client.Start();

            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)PacketType.LOGIN);
            //client.Connect("162.222.179.157", 8412, om);
            
            client.Connect("localhost", 8412, om);
            bool canStart = false;
            NetIncomingMessage inc;
            while (!canStart)
            {
                if ((inc = client.ReadMessage()) != null)
                {
                    switch (inc.MessageType)
                    {
                        case NetIncomingMessageType.ConnectionApproval:
                        case NetIncomingMessageType.ConnectionLatencyUpdated:
                        case NetIncomingMessageType.Data:
                            Console.WriteLine("data hapens");
                            if (inc.ReadByte() == (byte)PacketType.LOGIN)
                            {
                                ((Game1)Game).ID = inc.ReadInt32();
                                Console.WriteLine(((Game1)Game).ID.ToString());
                                canStart = true;
                            }
                            break;
                        default:
                            Console.WriteLine(client.ConnectionStatus.ToString());
                            Console.WriteLine(inc.ReadString());
                            canStart = true;
                            break;
                    }
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            if (((Game1)Game).gameState.Equals(Game1.STATE.BIRD))
            {
                NetOutgoingMessage om = client.CreateMessage();
                om.Write((byte)PacketType.BIRD);
                om.Write(((Game1)Game).ID);
                om.Write(((Game1)Game).bird.Position.X);
                om.Write(((Game1)Game).bird.Position.Y);
                om.Write(((Game1)Game).bird.Position.Z);
                om.Write(((Game1)Game).bird.alive);
                om.Write(((Game1)Game).bird.hasKill);
                om.Write(((Game1)Game).bird.killedID);
                client.SendMessage(om, NetDeliveryMethod.Unreliable);
            }
            else if (((Game1)Game).gameState.Equals(Game1.STATE.PERSON))
            {
                NetOutgoingMessage om = client.CreateMessage();
                om.Write((byte)PacketType.PERSON);
                om.Write(((Game1)Game).ID);
                om.Write(((Game1)Game).person.Position.X);
                om.Write(((Game1)Game).person.Position.Y);
                om.Write(((Game1)Game).person.Position.Z);
                om.Write(((Game1)Game).person.alive);
                om.Write(((Game1)Game).person.hasFired);
                om.Write(((Game1)Game).person.hasKill);
                om.Write(((Game1)Game).person.killedID);
                client.SendMessage(om, NetDeliveryMethod.Unreliable);
            }
            else
            {
                NetOutgoingMessage om = client.CreateMessage();
                om.Write((byte)PacketType.BIRD);
                om.Write(((Game1)Game).ID);
                om.Write(0);
                om.Write(0);
                om.Write(0);
                client.SendMessage(om, NetDeliveryMethod.Unreliable);
                Thread.Sleep(5000);
            }
            
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        byte type = msg.ReadByte();
                        if (type == (byte)PacketType.LOGIN && logedIn == false)
                        {
                            byte classy = msg.ReadByte();
                            ((Game1)Game).gameState = (classy == (byte)PacketType.PERSON) ? Game1.STATE.PERSON : Game1.STATE.BIRD;
                            Console.WriteLine(((Game1)Game).gameState.ToString());
                            logedIn = true;
                            Console.WriteLine("iawd");
                            int lID = msg.ReadInt32();
                            ((Game1)Game).ID = lID;
                            int s = msg.ReadInt32();
                            Console.WriteLine("size = " + s.ToString());
                            Console.WriteLine(((Game1)Game).gameState.Equals(Game1.STATE.PERSON).ToString());

                            for (int i = 0; i < s; i++)
                            {
                                Bird bird = new Bird((Game1)Game, (lID == msg.ReadInt32() && ((Game1)Game).gameState.Equals(Game1.STATE.BIRD)) ? true : false, i, new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()), msg.ReadBoolean(), msg.ReadBoolean());
                                bird.DrawOrder = 20 + i;
                                //Console.WriteLine(lID.ToString());
                                if (i == ((Game1)Game).ID && ((Game1)Game).gameState.Equals(Game1.STATE.BIRD))
                                {
                                    Console.WriteLine("i set game1 buird ID = " + lID.ToString());
                                    ((Game1)Game).bird = bird;
                                }
                                ((Game1)Game).Components.Add(bird);
                                
                            }
                            s = msg.ReadInt32();
                            Console.WriteLine("size = " + s.ToString());
                            for (int i = 0; i < s; i++)
                            {
                                Person person = new Person((Game1)Game, (lID == msg.ReadInt32() && ((Game1)Game).gameState.Equals(Game1.STATE.PERSON)) ? true : false, i, new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat()), msg.ReadBoolean(), msg.ReadBoolean());
                                person.DrawOrder = 100 + 1;
                                //Console.WriteLine(lID.ToString());
                                if (i == ((Game1)Game).ID && ((Game1)Game).gameState.Equals(Game1.STATE.PERSON))
                                {
                                    Console.WriteLine("i happen too");
                                    ((Game1)Game).person = person;
                                }
                                ((Game1)Game).Components.Add(person);
                            }
                            ((Game1)Game).CANDRAW = true;
                        }
                        if (type == (byte)PacketType.BIRD)
                        {
                            int ID = msg.ReadInt32();
                            
                            float X = msg.ReadFloat();
                            float Y = msg.ReadFloat();
                            float Z = msg.ReadFloat();
                            bool ai = msg.ReadBoolean();
                            bool dead = msg.ReadBoolean();
                            if (ID == ((Game1)Game).ID)
                                break;
                            birdArr[ID].X = X;
                            birdArr[ID].Y = Y;
                            birdArr[ID].Z = Z;
                            birdArr[ID].Dead = dead;
                            birdArr[ID].AI = ai;
                        }
                                            
                        break;
                }
            }
           
            base.Update(gameTime);
        }
        
    }
}
