using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

// TODO: controls for setting the region of checks
// maybe add list dialogs to MapTest and expose Map classes
// include null region for fixing upper and lower regions

namespace SaltMap
{
	public class Map
	{
		public enum CheckState
		{
			Available,
			Blocked,
			OutOfLogic,
			Collected
		}

		private class Check
		{
			public int id;
			public Vector2 loc;
			public string region;
			public CheckState checkState;
		}

		private class Sequence : Check
		{
			public string type;
		}

		private class CSM : Check
		{
			public string[] items;
		}

		private class Sanctuary : Check
		{
			public int shrine;
		}

		private class Boss : Check
		{
			public string[] items;
		}

		private class NPC : Check
		{
			public int seg;
			public int monster;
			public int npc;
			public string[] scripts;
		}

		private class Region
		{
			public List<string> checks = new List<string>();
			public Connection[] connections;
			public bool updated;
		}

		private class Connection
		{
			public string region;
			public string[] items;

			public Region GetRegion(Dictionary<string, Region> regions) => regions[region];

			public bool CanEnter(HashSet<string> flags)
			{
				if (this.items != null)
					foreach (string flag in this.items)
						if (!flags.Contains(flag))
							return false;

				return true;
			}
		}

		private class SegmentData
		{
			public string file;
			public Texture2D texture;
			public bool visible = true;
			public bool loading = false;
		}

		private enum DrawMode
		{
			Zoomable,
			Full
		}

		private const int SEGMENTS_X = 12;
		private const int SEGMENTS_Y = 6;

		private const int MIN_ZOOM = 5;
		private const int MAX_ZOOM = 20;

		public static int ScreenWidth = 1178;
		public static int ScreenHeight = 570;

		private Dictionary<string, CSM> chests;
		private Dictionary<string, CSM> sacks;
		private Dictionary<string, CSM> mimics;
		private Dictionary<string, Sequence> sequences;
		private Dictionary<string, Sanctuary> sanctuaries;
		private Dictionary<string, Boss> bosses;
		private Dictionary<string, NPC> npcs;

		private Dictionary<string, Region> regions;

		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;

		private SegmentData[,] segments;

		private SpriteFont font;
		private Texture2D map;
		private Texture2D pixel;
		private int zoom = 5;

		private Vector2 mapPosition = new Vector2(-49.0f, -86.0f);
		private Matrix camera = Matrix.Identity;
		private readonly float itemScale = 0.115f;

		private DrawMode drawMode = DrawMode.Zoomable;

		private readonly Stack<Region> updateStack = new Stack<Region>();

		private readonly HashSet<string> flags = new HashSet<string>();

		private readonly Dictionary<string, Check> locations = new Dictionary<string, Check>();

		public Map()
		{
			camera = Matrix.CreateTranslation(new Vector3(-mapPosition * (zoom * 0.1f), 0.0f));
		}

		private static T LoadJson<T>(string path)
		{
			string json = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<T>(json);
		}

		public void Init(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
		{
			camera.M11 = camera.M22 = zoom * 0.1f;

			this.graphicsDevice = graphicsDevice;
			this.spriteBatch = spriteBatch;

			string[] files = Directory.GetFiles("map_cut/");

			segments = new SegmentData[SEGMENTS_X, SEGMENTS_Y];

			foreach (string file in files)
			{
				int a = 17;

				char x1 = file[a];
				char x2 = file[a + 1];
				char y1 = file[a + 3];
				char y2 = file[a + 4];

				int x = (x1 - '0') * 10 + x2 - '0';
				int y = (y1 - '0') * 10 + y2 - '0';

				segments[x, y] = new SegmentData() { file = file };
			}

			this.font = font;
			map = Texture2D.FromFile(graphicsDevice, "map_full_low.jpg");
			pixel = new Texture2D(graphicsDevice, 1, 1);
			pixel.SetData(new Color[] { Color.White });

			chests = LoadJson<Dictionary<string, CSM>>("chests.json");
			sacks = LoadJson<Dictionary<string, CSM>>("sacks.json");
			mimics = LoadJson<Dictionary<string, CSM>>("mimics.json");
			sequences = LoadJson<Dictionary<string, Sequence>>("sequences.json");
			sanctuaries = LoadJson<Dictionary<string, Sanctuary>>("sanctuaries.json");
			bosses = LoadJson<Dictionary<string, Boss>>("bosses.json");
			npcs = LoadJson<Dictionary<string, NPC>>("npcs.json");

			regions = LoadJson<Dictionary<string, Region>>("regions.json");

			AddLocations(chests);
			AddLocations(sacks);
			AddLocations(mimics);
			AddLocations(sequences);
			AddLocations(sanctuaries);
			AddLocations(bosses);
			AddLocations(npcs);

			foreach (KeyValuePair<string, Check> kvp in locations)
			{
				string region = kvp.Value.region.ToLower().Replace(' ', '_').Replace('-', '_').Replace("'", "");

				if (regions.TryGetValue(region, out Region r))
					r.checks.Add(kvp.Key);
			}
		}

		private void AddLocations<T>(Dictionary<string, T> checks) where T : Check
		{
			foreach (KeyValuePair<string, T> kvp in checks)
				locations.TryAdd(kvp.Key, kvp.Value);
		}

		public void Update(bool isActive)
		{
			foreach (SegmentData segment in segments)
			{
				if (segment != null)
				{
					if (!segment.visible && segment.texture != null)
					{
						segment.texture.Dispose();
						segment.texture = null;
					}
					else if (segment.visible && segment.texture == null && !segment.loading)
					{
						segment.loading = true;
						ThreadPool.QueueUserWorkItem(LoadSegment, segment, true);
					}
				}
			}
		}

		private void UpdateAvailable()
		{
			Region menu = regions["menu"];
			updateStack.Push(menu);

			foreach (KeyValuePair<string, Check> kvp in locations)
				if (kvp.Value.checkState != CheckState.Collected)
					kvp.Value.checkState = CheckState.Blocked;

			while (updateStack.Count > 0)
			{
				Region current = updateStack.Peek();

				if (!current.updated)
				{
					foreach (string flag in current.checks)
						if (locations[flag].checkState != CheckState.Collected)
							locations[flag].checkState = CheckState.Available;

					current.updated = true;
				}

				foreach (Connection connection in current.connections)
					if (!connection.GetRegion(regions).updated && connection.CanEnter(flags))
						updateStack.Push(connection.GetRegion(regions));

				if (updateStack.Peek() == current)
					updateStack.Pop();
			}

			foreach (KeyValuePair<string, Region> kvp in regions)
				kvp.Value.updated = false;
		}

		public bool AddFlag(string flag)
		{
			if (flags.Add(flag))
			{
				UpdateAvailable();
				return true;
			}

			return false;
		}

		public bool RemoveFlag(string flag)
		{
			if (flags.Remove(flag))
			{
				UpdateAvailable();
				return true;
			}

			return false;
		}

		public void ClearFlags() => flags.Clear();

		public void ToggleMode() => drawMode = (DrawMode)(DrawMode.Full - drawMode);

		public void Zoom(float mouseX, float mouseY, int dir)
		{
			// need to make mouse the same world point after zoom
			Vector2 cameraPos = new Vector2(camera.Translation.X, camera.Translation.Y);
			Vector2 mouseVec = new Vector2(mouseX, mouseY);
			Vector2 oldPoint = (mouseVec - cameraPos) / (zoom * 0.1f);
			zoom = MathHelper.Clamp(zoom + dir, MIN_ZOOM, MAX_ZOOM);
			Vector2 newPoint = (mouseVec - cameraPos) / (zoom * 0.1f);

			cameraPos += (newPoint - oldPoint) * (zoom * 0.1f);
			camera.Translation = new Vector3(cameraPos, 0.0f);

			camera.M11 = camera.M22 = zoom * 0.1f;
		}

		public void Zoom(Point mousePos, int dir) => Zoom(mousePos.X, mousePos.Y, dir);
		public void Zoom(Vector2 mousePos, int dir) => Zoom(mousePos.X, mousePos.Y, dir);

		public void ZoomIn(float mouseX, float mouseY) => Zoom(mouseX, mouseY, 1);
		public void ZoomIn(Point mousePos) => Zoom(mousePos, 1);
		public void ZoomIn(Vector2 mousePos) => Zoom(mousePos, 1);

		public void ZoomOut(float mouseX, float mouseY) => Zoom(mouseX, mouseY, -1);
		public void ZoomOut(Point mousePos) => Zoom(mousePos, -1);
		public void ZoomOut(Vector2 mousePos) => Zoom(mousePos, -1);

		public void Move(Point point) => camera.Translation += new Vector3(point.X, point.Y, 0.0f);
		public void Move(Vector2 vec) => camera.Translation += new Vector3(vec.X, vec.Y, 0.0f);
		public void Move(float x, float y) => camera.Translation += new Vector3(x, y, 0.0f);

		public void Offset(Point point) => mapPosition += new Vector2(point.X, point.Y);
		public void Offset(Vector2 vec) => mapPosition += new Vector2(vec.X, vec.Y);
		public void Offset(float x, float y) => mapPosition += new Vector2(x, y);

		private void LoadSegment(SegmentData segmentData)
		{
			segmentData.texture = Texture2D.FromFile(graphicsDevice, segmentData.file);
			segmentData.loading = false;
		}

		public void ToggleNode(Vector2 pos)
		{
			float sqrLength = float.MaxValue;
			Check check = null;
			string name = null;

			if (drawMode == DrawMode.Full)
			{
				// find by pos
				pos = (pos - mapPosition) * zoom * 0.1f;
				pos /= itemScale;
			}
			else
			{
				Matrix inv = Matrix.Invert(camera);
				pos = Vector2.Transform(pos, inv);
				pos /= itemScale;
			}	

			foreach (KeyValuePair<string, Check> kvp in locations)
			{
				Vector2 diff = kvp.Value.loc - pos;

				float diffSq = diff.LengthSquared();

				if (diffSq < sqrLength)
				{
					sqrLength = diffSq;
					check = kvp.Value;
					name = kvp.Key;
				}
			}

			if (check != null && sqrLength < 120f * 120f)
			{
				flags.Remove(name);

				if (check.checkState == CheckState.Collected)
					check.checkState = CheckState.Blocked;
				else
				{
					check.checkState = CheckState.Collected;
					flags.Add(name);
				}

				UpdateAvailable();
			}
		}

		public void ToggleNode(Point point) => ToggleNode(new Vector2(point.X, point.Y));

		private void DrawEntryBlock(Check check, Color color, float scale)
		{
			Vector2 loc4 = check.loc * itemScale;

			if (drawMode == DrawMode.Full)
				loc4 = (loc4 - mapPosition) * scale;
			else
				loc4 = Vector2.Transform(loc4, camera);

			color = check.checkState switch
			{
				CheckState.Available => Color.Green,
				CheckState.Blocked => Color.Red,
				CheckState.OutOfLogic => Color.Yellow,
				CheckState.Collected => Color.Gray,
				_ => Color.Magenta,
			};

			spriteBatch.Draw(pixel, loc4, null, Color.Black, 0.0f, Vector2.One * 0.5f, 40.0f * scale, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(pixel, loc4, null, color, 0.0f, Vector2.One * 0.5f, 30.0f * scale, SpriteEffects.None, 0.0f);
		}

		private void DrawEntryText(string text, Vector2 loc, float scale)
		{
			Vector2 loc4 = loc * itemScale;

			if (drawMode == DrawMode.Full)
				loc4 = (loc4 - mapPosition) * scale;
			else
				loc4 = Vector2.Transform(loc4, camera);

			Vector2 size = font.MeasureString(text);
			spriteBatch.Draw(pixel, loc4 + Vector2.UnitY * 30.0f * scale, null, new Color(0, 0, 0, 0.4f), 0.0f, Vector2.One * 0.5f, (size + Vector2.One * 4.0f) * scale, SpriteEffects.None, 0.0f);
			spriteBatch.DrawString(font, text, loc4 + Vector2.UnitY * 30.0f * scale, Color.White, 0.0f, size * 0.5f, scale, SpriteEffects.None, 0.0f);
		}

		private void DrawCSMBlocks(Dictionary<string, CSM> csms, Color color, float scale)
		{
			foreach (KeyValuePair<string, CSM> kvp in csms)
				DrawEntryBlock(kvp.Value, color, scale);
		}

		private void DrawCSMTexts(Dictionary<string, CSM> csms, Color color, float scale)
		{
			foreach (KeyValuePair<string, CSM> kvp in csms)
				DrawEntryText(kvp.Key, kvp.Value.loc, scale);
		}

		private void DrawBlocks(float scale)
		{
			DrawCSMBlocks(chests, Color.Cyan, scale);
			DrawCSMBlocks(sacks, Color.Yellow, scale);
			DrawCSMBlocks(mimics, Color.Magenta, scale);

			foreach (KeyValuePair<string, Sequence> kvp in sequences)
				DrawEntryBlock(kvp.Value, Color.Green, scale);

			foreach (KeyValuePair<string, Sanctuary> kvp in sanctuaries)
				DrawEntryBlock(kvp.Value, Color.HotPink, scale);

			foreach (KeyValuePair<string, Boss> kvp in bosses)
				DrawEntryBlock(kvp.Value, Color.Purple, scale);

			foreach (KeyValuePair<string, NPC> kvp in npcs)
				DrawEntryBlock(kvp.Value, Color.White, scale);
		}

		private void DrawTexts(float scale)
		{
			DrawCSMTexts(chests, Color.Cyan, scale);
			DrawCSMTexts(sacks, Color.Yellow, scale);
			DrawCSMTexts(mimics, Color.Magenta, scale);

			foreach (KeyValuePair<string, Sequence> kvp in sequences)
				DrawEntryText(kvp.Key, kvp.Value.loc, scale);

			foreach (KeyValuePair<string, Sanctuary> kvp in sanctuaries)
				DrawEntryText(kvp.Key, kvp.Value.loc, scale);

			foreach (KeyValuePair<string, Boss> kvp in bosses)
				DrawEntryText(kvp.Key, kvp.Value.loc, scale);

			foreach (KeyValuePair<string, NPC> kvp in npcs)
				DrawEntryText(kvp.Key, kvp.Value.loc, scale);
		}

		public void Draw()
		{
			graphicsDevice.Clear(Color.Black);

			Vector2 mapPos = Vector2.Transform(mapPosition, camera);

			float scale = zoom * 0.1f;

			if (drawMode == DrawMode.Full)
			{
				scale = 0.1f;
				spriteBatch.Draw(map, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, scale * 10.0f, SpriteEffects.None, 0.0f);
			}
			else
			{
				Vector2 tl = -mapPos / scale / 1024;
				Vector2 wh = new Vector2(ScreenWidth, ScreenHeight);
				Vector2 br = (wh - mapPos) / scale / 1024;
				Point tli = new Point((int)tl.X, (int)tl.Y);
				Point bri = new Point((int)br.X, (int)br.Y);

				int segmentTop = Math.Clamp(tli.Y, 0, SEGMENTS_Y - 1);
				int segmentBottom = Math.Clamp(bri.Y, 0, SEGMENTS_Y - 1);
				int segmentLeft = Math.Clamp(tli.X, 0, SEGMENTS_X - 1);
				int segmentRight = Math.Clamp(bri.X, 0, SEGMENTS_X - 1);

				spriteBatch.Draw(map, mapPos, null, Color.White, 0.0f, Vector2.Zero, scale * 10.0f, SpriteEffects.None, 0.0f);

				foreach (SegmentData segmentData in segments)
					if (segmentData != null)
						segmentData.visible = false;

				for (int y = segmentTop; y <= segmentBottom; y++)
				{
					for (int x = segmentLeft; x <= segmentRight; x++)
					{
						SegmentData segmentData = segments[x, y];

						if (segmentData != null)
						{
							segmentData.visible = true;

							if (segmentData.texture != null)
								spriteBatch.Draw(segments[x, y].texture, mapPos + new Vector2(x, y) * 1024 * scale, null, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
						}
					}
				}
			}

			DrawBlocks(scale);
			DrawTexts(scale);
		}
	}
}
