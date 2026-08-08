using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SaltMap
{
    internal class MainGame : Game
    {
		private readonly GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private readonly Map map = new Map();

		private bool tab = false;
		private bool middleClick = false;
		private bool rightClick = false;

		private int lastMouseWheel;
		private Point lastMousePos;

		public MainGame()
		{
			_graphics = new GraphicsDeviceManager(this);
			_graphics.PreferredBackBufferWidth = Map.ScreenWidth;
			_graphics.PreferredBackBufferHeight = Map.ScreenHeight;
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			_spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			SpriteFont font = Content.Load<SpriteFont>("DefaultFont");
			map.Init(GraphicsDevice, _spriteBatch, font);

			base.LoadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			MouseState mouseState = Mouse.GetState();

			if (!IsActive)
			{
				lastMouseWheel = mouseState.ScrollWheelValue;
				lastMousePos = mouseState.Position;
				return;
			}

			int wheel = mouseState.ScrollWheelValue;
			int wheelDelta = wheel - lastMouseWheel;
			Point mousePos = mouseState.Position;
			Point mousePosDelta = mousePos - lastMousePos;

			if (wheelDelta != 0)
				map.Zoom(mousePos, wheelDelta > 0 ? 1 : -1);

			if (mouseState.LeftButton == ButtonState.Pressed)
				map.Move(mousePosDelta);

			if (mouseState.MiddleButton == ButtonState.Pressed)
			{
				if (!middleClick)
					map.ToggleNode(mousePos);

				middleClick = true;
			}
			else
				middleClick = false;

			if (mouseState.RightButton == ButtonState.Pressed)
			{
				if (!rightClick)
					map.GetNode(mousePos);

				rightClick = true;
			}
			else
				rightClick = false;

			lastMouseWheel = wheel;
			lastMousePos = mousePos;

			KeyboardState keyboardState = Keyboard.GetState();

			if (keyboardState.IsKeyDown(Keys.Tab))
			{
				if (!tab)
					map.ToggleMode();

				tab = true;
			}
			else
				tab = false;

			map.Update(IsActive);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			_spriteBatch.Begin();
			
			map.Draw();
			
			_spriteBatch.End();

			base.Draw(gameTime);
		}
    }
}
