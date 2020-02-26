using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tools_XNA_dotNET_Framework;
using MultiplayerGameLibrary;

namespace MultiplayerGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Client client;

        public bool isKeyUp = true;
        public enum Direction : byte
        {
            Up, 
            Left,
            Down,
            Right
        }
        public Direction direction = Direction.Up;

        private bool connected;
        private Color _color; //For test
        private float timer; // For test
        private Texture2D _texture; //For test



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _color = Color.CornflowerBlue;
        }
        
        protected override void Initialize()
        {
            client = new Client();
            client.Initialize();
            ConfigCamera();
            connected = client.IsConnectedToServer();
            base.Initialize();
        }

        public void ConfigCamera()
        {
            // Enable multisampling
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = 600;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();

        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _texture = Content.Load<Texture2D>("white_background");
            client.Load(Content);
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                client.Disconnect("signing out");
                Exit();
            }

            client.Update();

            // Send message
            #region Keys
            if (isKeyUp)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    direction = Direction.Up;
                    isKeyUp = false;
                    client.SendDirection((byte)direction);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    direction = Direction.Left;
                    isKeyUp = false;
                    client.SendDirection((byte)direction);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    direction = Direction.Down;
                    isKeyUp = false;
                    client.SendDirection((byte)direction);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    direction = Direction.Right;
                    isKeyUp = false;
                    client.SendDirection((byte)direction);
                }
            }
            

            if (Keyboard.GetState().IsKeyUp(Keys.W) &&
            Keyboard.GetState().IsKeyUp(Keys.A) &&
            Keyboard.GetState().IsKeyUp(Keys.S) &&
            Keyboard.GetState().IsKeyUp(Keys.D))
                isKeyUp = true;

            #endregion


            if (timer <= 0f)
            {
                if(client.IsConnectedToServer())
                {
                    _color = Color.Black;
                    timer = 2f;
                }
                else
                {
                    _color = Color.Red;
                    timer = 3f;
                }
            }

            timer -= gameTime.TotalGameTime.Seconds;
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_color);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            client.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
