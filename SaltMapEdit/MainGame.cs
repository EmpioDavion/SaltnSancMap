using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Profiler;
using SaltMap;
using System.Text;
using WinForm = System.Windows.Forms.Form;

namespace SaltMapEdit
{
    internal class MainGame : Game
    {
		private readonly GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private readonly Map map = new Map();

		private readonly MainForm form;

		private SpriteFont font;

		public MainGame(MainForm form)
		{
			WinForm winForm = (WinForm)WinForm.FromHandle(Window.Handle);
			winForm.Activated += (sender, e) => winForm.Hide();
			winForm.VisibleChanged += (sender, e) => winForm.Hide();

			Map.ScreenWidth = form.pnlMap.Size.Width;
			Map.ScreenHeight = form.pnlMap.Size.Height;

			this.form = form;
			form.map = map;
			form.FormClosing += Form_FormClosing;
			form.pnlMap.SizeChanged += PnlMap_SizeChanged;

			_graphics = new GraphicsDeviceManager(this);
			_graphics.PreparingDeviceSettings += PreparingDeviceSettings;
			_graphics.PreferredBackBufferWidth = Map.ScreenWidth;
			_graphics.PreferredBackBufferHeight = Map.ScreenHeight;
			_graphics.ApplyChanges();
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		private void PnlMap_SizeChanged(object sender, System.EventArgs e)
		{
			Map.ScreenWidth = form.pnlMap.Size.Width;
			Map.ScreenHeight = form.pnlMap.Size.Height;
			_graphics.PreferredBackBufferWidth = form.pnlMap.Size.Width;
			_graphics.PreferredBackBufferHeight = form.pnlMap.Size.Height;
			_graphics.ApplyChanges();
		}

		private void Form_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			Exit();
		}

		private void PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
		{
			e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = form.pnlMap.Handle;
		}

		protected override void Initialize()
		{
			_spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			font = Content.Load<SpriteFont>("DefaultFont");
			map.Init(GraphicsDevice, _spriteBatch, font);

			form.spriteFont = font;
			form.MapLoaded();

			base.LoadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			Profile profile = Profiler.Profiler.Start("MainGame.Update");

			map.Update(form.pnlMap.Focused);
			base.Update(gameTime);

			Profiler.Profiler.End(profile);
		}

		private readonly StringBuilder mousePosText = new StringBuilder();
		private readonly StringBuilder fpsText = new StringBuilder();

		protected override void Draw(GameTime gameTime)
		{
			Profile profile = Profiler.Profiler.Start("MainGame.Draw");

			_spriteBatch.Begin();
			
			map.Draw(form.mousePos);

			Vector2 mousePos = map.GetMapPosition(form.mousePos);
			mousePosText.Clear();
			mousePosText.Append(mousePos);

			double milliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
			fpsText.Clear();
			fpsText.Append(1000.0 / System.Math.Max(1, milliseconds));

			_spriteBatch.Draw(map.pixel, new Vector2(14, 14), null, new Color(0,0,0,0.3f), 0.0f,
				Vector2.Zero, new Vector2(220, 20), SpriteEffects.None, 0.0f);
			_spriteBatch.DrawString(font, mousePosText, new Vector2(16, 16), Color.White);

			_spriteBatch.DrawString(font, fpsText, new Vector2(16, 80), Color.White);

			_spriteBatch.End();

			base.Draw(gameTime);

			Profiler.Profiler.End(profile);
		}
    }
}
