using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using static SaltMap.Map;
using WinPoint = System.Drawing.Point;

namespace SaltMap
{
	public class Map
	{
		[Flags]
		public enum Rune
		{
			None,
			Vertigo		= 1 << 0,
			Dart		= 1 << 1,
			Shadowflip	= 1 << 2,
			DoubleJump	= 1 << 3,	// unused
			Redshift	= 1 << 4,
			Hardlight	= 1 << 5
		}

		[Flags]
		public enum CheckState
		{
			Available	= 1 << 0,
			Blocked		= 1 << 1,
			OutOfLogic	= 1 << 2,
			Collected	= 1 << 3
		}

		public enum CheckType
		{
			Chest,
			Sack,
			Mimic,
			Switch,
			Item,
			Sequence,
			DisabledSequence, // sequence controlled by switch
			Sanctuary,
			Boss,
			NPC,
			Dialogue
		}

		public class Check
		{
			public int id;
			public Vector2 loc;

			[JsonProperty]
			internal string region;

			[JsonIgnore]
			public CheckState checkState;

			[NonSerialized]
			public CheckType checkType;

			[NonSerialized]
			public string key;

			[NonSerialized]
			internal Region _region;

			[JsonIgnore]
			public Region Region
			{
				get => _region;
				set { _region = value; region = value.key; }
			}

			[JsonIgnore]
			public CheckGroup group;

			public override string ToString() => key;
		}

		public class CSM : Check
		{
			public string[] items;
		}

		public class Item : Check
		{
			public string item;
		}

		public class Sequence : Check
		{
			public string type;
		}

		public class Sanctuary : Check
		{
			public int shrine;
		}

		public class Boss : Check
		{
			public string[] items;
		}

		public class NPC : Check
		{
			public int seg;
			public int monster;
			public int npc;
			public string[] scripts;
		}

		public class Dialogue : Check
		{
			[JsonIgnore]
			public NPC NPC { get; internal set; }
		}

		public class CheckGroup
		{
			public Vector2 loc;
			public readonly List<Check> checks = new List<Check>();
		}

		private class GameDialogue
		{
			public class Node
			{
				public class Text
				{
					public string[] text;				// text per language
				}

				public class Option
				{
					public string[] text;				// text per language
					public string action;				// the node to go to
					public string coopAction;			// the node to go to
				}

				public int id;							// custom data added for check id
				public string name;						// label for precheckFlagGoto
				public Text[] text;						// dialogue text
				public Option[] option;					// player replies
				public string[] precheckFlagStr;		// skips to node in precheckFlagGoto if player has flag
				public string[] precheckFlagGoto;		// goes to the node if player has flag in precheckFlagStr
				public string postSetFlagStr;			// adds the flag to the player
				public string postGoto;					// goes to the node after this node
				public string[] giveScript;				// gives the player items
				public string[] storeScript;			// lists the shop items
			}

			public string name;							// internal name of the NPC
			public Node[] nodeList;						// list of dialogues for the NPC
			public int rune;							// unused value
		}

		public class Region
		{
			[NonSerialized]
			public List<string> checks = new List<string>();

			public List<Connection> connections = new List<Connection>();
			public bool updated;

			[NonSerialized]
			public string key;

			public override string ToString() => key;
		}

		public class Connection
		{
			[JsonProperty]
			internal string region;
			public List<string> items = new List<string>();

			[NonSerialized]
			internal Region _region;

			[JsonIgnore]
			public Region Region
			{
				get => _region;
				set { _region = value; region = value?.key; }
			}

			public bool CanEnter(HashSet<string> flags)
			{
				if (this.items != null)
					foreach (string flag in this.items)
						if (!flags.Contains(flag))
							return false;

				return true;
			}

			public override string ToString() => Region.ToString();

			[OnSerializing]
			internal void SetRegionString(StreamingContext context)
			{
				region = _region.key;
			}
		}

		private class SegmentData
		{
			public string file;
			public Texture2D texture;
			public bool visible = true;
			public bool loading = false;
		}

		public enum DrawMode
		{
			Zoomable,
			Full
		}

		private const int SEGMENTS_X = 12;
		private const int SEGMENTS_Y = 6;

		private const int MIN_ZOOM = 6;
		private const int MAX_ZOOM = 11;

		public const float FULL_SCALE = 0.1f;

		public static int ScreenWidth = 1178;
		public static int ScreenHeight = 570;

		private Dictionary<string, CSM> chests;
		private Dictionary<string, CSM> sacks;
		private Dictionary<string, CSM> mimics;
		private Dictionary<string, CSM> switches;
		private Dictionary<string, Item> items;
		private Dictionary<string, Sequence> sequences;
		private Dictionary<string, Sequence> disabledSequences;
		private Dictionary<string, Sanctuary> sanctuaries;
		private Dictionary<string, Boss> bosses;
		private Dictionary<string, NPC> npcs;
		private Dictionary<string, Dialogue> dialogues;

		public Dictionary<string, Region> regions;

		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;

		private SegmentData[,] segments;

		private SpriteFont font;
		private Texture2D map;
		public Texture2D pixel;
		private int zoom = 6;

		public float ZoomScale => zoom * 0.1f;

		private Vector2 mapPosition = new Vector2(-49.0f, -86.0f);
		private Matrix camera = Matrix.Identity;

		public readonly float itemScale = 0.115f;

		public readonly float blockBorderSize = 40.0f;
		public readonly float blockFillSize = 30.0f;

		public DrawMode drawMode = DrawMode.Zoomable;

		private readonly Stack<Region> updateStack = new Stack<Region>();

		private readonly HashSet<string> flags = new HashSet<string>();

		private readonly Dictionary<string, Check> locations = new Dictionary<string, Check>();

		private readonly List<CheckGroup> checkGroups = new List<CheckGroup>();

		private Region regionFilter = null;
		private Check checkFilter = null;

		private Rune runes;

		public Func<bool> saveRegions = () => true;

		private string subfolder = "";

#if !DEBUG

		private CheckGroup hoveredCheckGroup = null;

#endif

		public Map()
		{
			camera = Matrix.CreateTranslation(new Vector3(-mapPosition * ZoomScale, 0.0f));
		}

		public void Init(string subfolder, GraphicsDevice graphicsDevice,
			SpriteBatch spriteBatch, SpriteFont font, bool loadItems)
		{
			this.subfolder = subfolder;
			this.graphicsDevice = graphicsDevice;
			this.spriteBatch = spriteBatch;
			this.font = font;

			camera.M11 = camera.M22 = ZoomScale;

			string[] files = Directory.GetFiles($"{subfolder}/map_cut/");

			segments = new SegmentData[SEGMENTS_X, SEGMENTS_Y];

			int a = subfolder.Length + 18;

			foreach (string file in files)
			{
				char x1 = file[a];
				char x2 = file[a + 1];
				char y1 = file[a + 3];
				char y2 = file[a + 4];

				int x = (x1 - '0') * 10 + x2 - '0';
				int y = (y1 - '0') * 10 + y2 - '0';

				segments[x, y] = new SegmentData() { file = file };
			}

			using (FileStream fs = new FileStream($"{subfolder}/map_full_low.jpg", FileMode.Open))
				map = Texture2D.FromStream(graphicsDevice, fs);

			pixel = new Texture2D(graphicsDevice, 1, 1);
			pixel.SetData(new Color[] { Color.White });

			chests = LoadJson<Dictionary<string, CSM>>($"{subfolder}/chests.json");
			sacks = LoadJson<Dictionary<string, CSM>>($"{subfolder}/sacks.json");
			mimics = LoadJson<Dictionary<string, CSM>>($"{subfolder}/mimics.json");
			switches = LoadJson<Dictionary<string, CSM>>($"{subfolder}/switches.json");

			if (loadItems)
			{
				items = new Dictionary<string, Item>();
				AddItems(chests);
				AddItems(sacks);
				AddItems(mimics);
			}
			else
				items = null;

			sequences = LoadJson<Dictionary<string, Sequence>>($"{subfolder}/sequences.json");
			disabledSequences = LoadJson<Dictionary<string, Sequence>>($"{subfolder}/disabled_sequences.json");

			foreach (KeyValuePair<string, Sequence> kvp in disabledSequences)
			{
				kvp.Value.key = kvp.Key;
				kvp.Value.checkType = CheckType.DisabledSequence;
			}

			sanctuaries = LoadJson<Dictionary<string, Sanctuary>>($"{subfolder}/sanctuaries.json");
			bosses = LoadJson<Dictionary<string, Boss>>($"{subfolder}/bosses.json");
			npcs = LoadJson<Dictionary<string, NPC>>($"{subfolder}/npcs.json");

			if (!File.Exists($"{subfolder}/dialogues.json"))
				CreateDialogues();
			else
				dialogues = LoadJson<Dictionary<string, Dialogue>>($"{subfolder}/dialogues.json");

			regions = LoadJson<Dictionary<string, Region>>($"{subfolder}/regions.json");

			foreach (KeyValuePair<string, Region> kvp in regions)
			{
				kvp.Value.key = kvp.Key;

				foreach (Connection connection in kvp.Value.connections)
					connection._region = regions[connection.region];
			}

			locations.Clear();

			AddLocations(chests, CheckType.Chest);
			AddLocations(sacks, CheckType.Sack);
			AddLocations(mimics, CheckType.Mimic);
			AddLocations(switches, CheckType.Switch);

			if (loadItems)
				AddLocations(items, CheckType.Item);

			AddLocations(sequences, CheckType.Sequence);
			AddLocations(sanctuaries, CheckType.Sanctuary);
			AddLocations(bosses, CheckType.Boss);
			AddLocations(npcs, CheckType.NPC);
			AddLocations(dialogues, CheckType.Dialogue);

			float maxRange = 10f * 10f;

			foreach (KeyValuePair<string, Check> kvp in locations)
			{
				if (!regions.ContainsKey(kvp.Value.region))
					regions[kvp.Value.region] = new Region() { key = kvp.Value.region };

				kvp.Value._region = regions[kvp.Value.region];
				kvp.Value._region.checks.Add(kvp.Key);

				foreach (CheckGroup checkGroup in checkGroups)
				{
					if ((checkGroup.loc - kvp.Value.loc).LengthSquared() < maxRange)
					{
						kvp.Value.group = checkGroup;
						checkGroup.checks.Add(kvp.Value);
						break;
					}
				}

				if (kvp.Value.group == null)
				{
					checkGroups.Add(kvp.Value.group = new CheckGroup()
					{
						loc = kvp.Value.loc
					});
					kvp.Value.group.checks.Add(kvp.Value);
				}
			}

			UpdateAvailable();
		}

		private void CreateDialogues()
		{
			List<GameDialogue> gameDialogues = LoadJson<List<GameDialogue>>($"{subfolder}/game_dialogues.json");

			dialogues = new Dictionary<string, Dialogue>();

			string lastFail = "";

			foreach (GameDialogue gameDialogue in gameDialogues)
			{
				string npcName = gameDialogue.name;

				foreach (GameDialogue.Node node in gameDialogue.nodeList)
				{
					char spawn = node.name[0];
					string key = npcName;

					// dialogue is for the first spawn of the npc
					if (spawn <= '1' || spawn > '9')
						key += "_1";
					else
						key += $"_{spawn}";

					if (!npcs.TryGetValue(key, out NPC npc))
					{
						if (lastFail != key)
							System.Diagnostics.Debug.WriteLine($"Could not find npc {key}");

						lastFail = key;
						continue;
					}

					string dialogueName = $"{npcName}_{node.name}";

					// some npcs may have the same initial greeting
					if (!dialogues.ContainsKey(dialogueName))
					{
						dialogues.Add(dialogueName, new Dialogue()
						{
							id = node.id,
							loc = npc.loc,
							region = npc.region,
							key = dialogueName,
							NPC = npc
						});
					}
				}
			}
		}

		// pass in player.runes
		public void UpdateRunes(bool[] playerRunes)
		{
			runes = Rune.None;

			for (int i = 0; i < playerRunes.Length; i++)
				if (playerRunes[i])
					runes = (Rune)((int)runes | (1 << i));
		}

		public void Save()
		{
			SaveChecks("chests.json", chests);
			SaveChecks("sacks.json", sacks);
			SaveChecks("mimics.json", mimics);
			SaveChecks("switches.json", switches);

			if (items != null)
				SaveChecks("items.json", items);

			SaveChecks("sequences.json", sequences);
			SaveChecks("sanctuaries.json", sanctuaries);
			SaveChecks("bosses.json", bosses);
			SaveChecks("npcs.json", npcs);
			SaveChecks("dialogues.json", dialogues);

			SaveRegions();
		}

		private void SaveChecks<T>(string path, Dictionary<string, T> checks) where T : Check
		{
			foreach (KeyValuePair<string, T> kvp in checks)
				kvp.Value.region = kvp.Value.Region.key;

			SaveJson(path, checks);
		}

		private void SaveRegions()
		{
			if (saveRegions())
				SaveJson("regions.json", regions);
		}

		private void SaveJson<T>(string path, Dictionary<string, T> dict)
		{
			string json = JsonConvert.SerializeObject(dict, Formatting.Indented);
			File.WriteAllText(path, json);
		}

		private static T LoadJson<T>(string path)
		{
			string json = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<T>(json);
		}

		private void AddItems(Dictionary<string, CSM> csms)
		{
			foreach (KeyValuePair<string, CSM> kvp in csms)
			{
				for (int i = 0; i < kvp.Value.items.Length; i++)
				{
					string key = kvp.Key + $"_Item{i + 1}";

					items.Add(key, new Item()
					{
						id = kvp.Value.id + i + 1,
						loc = kvp.Value.loc,
						region = kvp.Value.region,
						key = key
					});
				}
			}
		}

		private void AddLocations<T>(Dictionary<string, T> checks, CheckType checkType) where T : Check
		{
			foreach (KeyValuePair<string, T> kvp in checks)
			{
				kvp.Value.key = kvp.Key;
				kvp.Value.checkType = checkType;
				locations.Add(kvp.Key, kvp.Value);
			}
		}

		public void Update()
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
						ThreadPool.QueueUserWorkItem(LoadSegmentO, segment);
					}
				}
			}
		}

		public void UpdateAvailable()
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
					if (!connection.Region.updated && connection.CanEnter(flags))
						updateStack.Push(connection.Region);

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

		public bool HasFlag(string flag) => flags.Contains(flag);

		public void ClearFlags() => flags.Clear();

		public void ToggleMode() => drawMode = (DrawMode)(DrawMode.Full - drawMode);

		public bool SetZoom(int newZoom, float mouseX, float mouseY)
		{
			int oldZoom = zoom;

			// need to make mouse the same world point after zoom
			Vector2 cameraPos = new Vector2(camera.Translation.X, camera.Translation.Y);
			Vector2 mouseVec = new Vector2(mouseX, mouseY);
			Vector2 oldPoint = (mouseVec - cameraPos) / ZoomScale;
			zoom = MathHelper.Clamp(newZoom, MIN_ZOOM, MAX_ZOOM);
			Vector2 newPoint = (mouseVec - cameraPos) / ZoomScale;

			cameraPos += (newPoint - oldPoint) * ZoomScale;
			camera.Translation = new Vector3(cameraPos, 0.0f);

			camera.M11 = camera.M22 = ZoomScale;

			return zoom != oldZoom;
		}

		public bool SetZoom(int newZoom)
		{
			float centreX = ScreenWidth / 2;
			float centreY = ScreenHeight / 2;

			return SetZoom(newZoom, centreX, centreY);
		}

		public bool Zoom(float mouseX, float mouseY, int dir) =>
			SetZoom(zoom + dir, mouseX, mouseY);

		public bool Zoom(Point mousePos, int dir) => Zoom(mousePos.X, mousePos.Y, dir);
		public bool Zoom(Vector2 mousePos, int dir) => Zoom(mousePos.X, mousePos.Y, dir);
		public bool Zoom(WinPoint mousePos, int dir) => Zoom(mousePos.X, mousePos.Y, dir);

		public bool ZoomIn(float mouseX, float mouseY) => Zoom(mouseX, mouseY, 1);
		public bool ZoomIn(Point mousePos) => Zoom(mousePos, 1);
		public bool ZoomIn(Vector2 mousePos) => Zoom(mousePos, 1);
		public bool ZoomIn(WinPoint mousePos) => Zoom(mousePos, 1);

		public bool ZoomOut(float mouseX, float mouseY) => Zoom(mouseX, mouseY, -1);
		public bool ZoomOut(Point mousePos) => Zoom(mousePos, -1);
		public bool ZoomOut(Vector2 mousePos) => Zoom(mousePos, -1);
		public bool ZoomOut(WinPoint mousePos) => Zoom(mousePos, -1);

		public void Move(float x, float y) => camera.Translation += new Vector3(x, y, 0.0f);
		public void Move(Point point) => camera.Translation += new Vector3(point.X, point.Y, 0.0f);
		public void Move(Vector2 vec) => camera.Translation += new Vector3(vec.X, vec.Y, 0.0f);
		public void Move(WinPoint point) => camera.Translation += new Vector3(point.X, point.Y, 0.0f);

		public void Offset(float x, float y) => mapPosition += new Vector2(x, y);
		public void Offset(Point point) => mapPosition += new Vector2(point.X, point.Y);
		public void Offset(Vector2 vec) => mapPosition += new Vector2(vec.X, vec.Y);
		public void Offset(WinPoint point) => mapPosition += new Vector2(point.X, point.Y);

		private void LoadSegmentO(object segmentData) => LoadSegment((SegmentData)segmentData);

		private void LoadSegment(SegmentData segmentData)
		{
			using (FileStream fs = new FileStream(segmentData.file, FileMode.Open))
				segmentData.texture = Texture2D.FromStream(graphicsDevice, fs);

			segmentData.loading = false;
		}

		public void Filter(Region region, Check check)
		{
			regionFilter = region;
			checkFilter = check;
		}

		public void SetCameraPosition(Vector2 worldPos)
		{
			worldPos *= -itemScale;
			worldPos += mapPosition;
			worldPos *= ZoomScale;
			worldPos += new Vector2(ScreenWidth, ScreenHeight) * 0.5f;

			camera.Translation = new Vector3(worldPos, 0.0f);
		}

		public Color GetCheckStateColor(Check check)
		{
			return check.checkState switch
			{
				CheckState.Available => Color.LightGreen,
				CheckState.Blocked => Color.IndianRed,
				CheckState.OutOfLogic => Color.LightGoldenrodYellow,
				CheckState.Collected => Color.Gray,
				_ => Color.Magenta,
			};
		}

		public Color GetCheckTypeColor(Check check)
		{
			return check.checkType switch
			{
				CheckType.Chest => Color.LightBlue,
				CheckType.Sack => Color.Cyan,
				CheckType.Mimic => Color.Orchid,
				CheckType.Item => Color.PaleGreen,
				CheckType.Sequence => Color.Violet,
				CheckType.DisabledSequence => Color.Gray,
				CheckType.Switch => Color.MediumVioletRed,
				CheckType.Sanctuary => Color.Goldenrod,
				CheckType.Boss => Color.RosyBrown,
				CheckType.NPC => Color.Chocolate,
				CheckType.Dialogue => Color.Tan,
				_ => Color.LightCoral
			};
		}

		public void GetCheckGroupColors(CheckGroup checkGroup, List<Color> colors)
		{
			Profiler.Profile profile = Profiler.Profiler.Start("Map.GetCheckGroupColors");

			if (regionFilter != null)
			{
				bool hasCheck = checkGroup.checks.Any((check) => check.Region == regionFilter);
				bool allChecks = checkGroup.checks.All((check) => check.Region == regionFilter);

				if (allChecks)
					colors.Add(Color.LightGreen);
				else if (hasCheck)
				{
					colors.Add(Color.LightGreen);
					colors.Add(Color.IndianRed);
				}
				else
					colors.Add(Color.IndianRed);
			}
			else
			{
				CheckState mixedCheckState = 0;

				foreach (Check check in checkGroup.checks)
					mixedCheckState |= check.checkState;

				if (mixedCheckState.HasFlag(CheckState.Available))
					colors.Add(Color.LightGreen);

				if (mixedCheckState.HasFlag(CheckState.OutOfLogic))
					colors.Add(Color.LightGoldenrodYellow);

				if (mixedCheckState.HasFlag(CheckState.Blocked))
					colors.Add(Color.IndianRed);

				if (colors.Count == 0)		// all checks are collected
					colors.Add(Color.Gray);
			}

			Profiler.Profiler.End(profile);
		}

		public Vector2 GetMapPosition(Vector2 pos)
		{
			if (drawMode == DrawMode.Full)
				pos = pos * 10.0f + mapPosition;
			else
			{
				Matrix inv = Matrix.Invert(camera);
				pos = Vector2.Transform(pos, inv);
			}

			return pos / itemScale;
		}

		public Vector2 GetMapPosition(float x, float y) => GetMapPosition(new Vector2(x, y));
		public Vector2 GetMapPosition(Point pos) => GetMapPosition(new Vector2(pos.X, pos.Y));
		public Vector2 GetMapPosition(WinPoint pos) => GetMapPosition(new Vector2(pos.X, pos.Y));

		public Vector2 GetScreenPosition(Vector2 pos)
		{
			pos *= itemScale;

			if (drawMode == DrawMode.Full)
				pos = (pos - mapPosition) * FULL_SCALE;
			else
				pos = Vector2.Transform(pos, camera);

			return pos;
		}

		public Vector2 GetScreenPosition(float x, float y) => GetScreenPosition(new Vector2(x, y));
		public Vector2 GetScreenPosition(Point pos) => GetScreenPosition(new Vector2(pos.X, pos.Y));
		public Vector2 GetScreenPosition(WinPoint pos) => GetScreenPosition(new Vector2(pos.X, pos.Y));

		public bool GetLocation(string location, out Check check) =>
			locations.TryGetValue(location, out check);

		public void GetChecks(Vector2 pos, IList<Check> checks)
		{
			float maxRange = 120f * 120f;

			pos = GetMapPosition(pos);

			foreach (KeyValuePair<string, Check> kvp in locations)
			{
				Vector2 diff = kvp.Value.loc - pos;

				float diffSq = diff.LengthSquared();

				if (diffSq < maxRange)
					checks.Add(kvp.Value);
			}
		}

		public void GetChecks(float x, float y, IList<Check> checks) =>
			GetChecks(new Vector2(x, y), checks);

		public void GetChecks(Point point, IList<Check> checks) =>
			GetChecks(new Vector2(point.X, point.Y), checks);

		public void GetChecks(WinPoint point, IList<Check> checks) =>
			GetChecks(new Vector2(point.X, point.Y), checks);

		public bool GetCheck(Vector2 pos, out Check check)
		{
			float sqrLength = float.MaxValue;
			Check _check = null;
			
			pos = GetMapPosition(pos);

			foreach (KeyValuePair<string, Check> kvp in locations)
			{
				Vector2 diff = kvp.Value.loc - pos;

				float diffSq = diff.LengthSquared();

				if (diffSq < sqrLength)
				{
					sqrLength = diffSq;
					_check = kvp.Value;
				}
			}

			if (_check != null && sqrLength < 120f * 120f)
			{
				check = _check;
				return true;
			}

			check = null;

			return false;
		}

		public bool GetCheck(float x, float y, out Check check) =>
			GetCheck(new Vector2(x, y), out check);

		public bool GetCheck(Point point, out Check check) =>
			GetCheck(new Vector2(point.X, point.Y), out check);

		public bool GetCheck(WinPoint point, out Check check) =>
			GetCheck(new Vector2(point.X, point.Y), out check);

		public bool GetCheckGroup(Vector2 pos, out CheckGroup checkGroup)
		{
			float sqrLength = float.MaxValue;
			CheckGroup _checkGroup = null;

			pos = GetMapPosition(pos);

			foreach (CheckGroup group in checkGroups)
			{
				Vector2 diff = group.loc - pos;

				float diffSq = diff.LengthSquared();

				if (diffSq < sqrLength)
				{
					sqrLength = diffSq;
					_checkGroup = group;
				}
			}

			if (_checkGroup != null && sqrLength < 120f * 120f)
			{
				checkGroup = _checkGroup;
				return true;
			}

			checkGroup = null;

			return false;
		}

		public bool GetCheckGroup(float x, float y, out CheckGroup checkGroup) =>
			GetCheckGroup(new Vector2(x, y), out checkGroup);

		public bool GetCheckGroup(Point point, out CheckGroup checkGroup) =>
			GetCheckGroup(new Vector2(point.X, point.Y), out checkGroup);

		public bool GetCheckGroup(WinPoint point, out CheckGroup checkGroup) =>
			GetCheckGroup(new Vector2(point.X, point.Y), out checkGroup);

		public void ToggleCheck(Vector2 pos)
		{
			if (GetCheck(pos, out Check check))
			{
				flags.Remove(check.key);

				if (check.checkState == CheckState.Collected)
					check.checkState = CheckState.Blocked;
				else
				{
					check.checkState = CheckState.Collected;
					flags.Add(check.key);
				}

				UpdateAvailable();
			}
		}

		public void ToggleCheck(float x, float y) => ToggleCheck(new Vector2(x, y));
		public void ToggleCheck(Point point) => ToggleCheck(new Vector2(point.X, point.Y));
		public void ToggleCheck(WinPoint point) => ToggleCheck(new Vector2(point.X, point.Y));

		private void DrawEntryText(string text, Vector2 loc, float scale, Color? color = null)
		{
			Vector2 loc4 = loc * itemScale;
			float sizeScale = scale * 1.2f;
			Vector2 size = font.MeasureString(text);

			if (drawMode == DrawMode.Full)
			{
				loc4 = (loc4 - mapPosition) * scale;
				sizeScale = 1.0f;
			}
			else
				loc4 = Vector2.Transform(loc4, camera);

			if (loc4.X < -100.0f || loc4.Y < -100.0f ||
				loc4.X > ScreenWidth + 100.0f || loc4.Y > ScreenHeight + 100.0f)
				return;

			color ??= Color.White;

			spriteBatch.Draw(pixel, loc4 + Vector2.UnitY * 30.0f * scale, null, new Color(0, 0, 0, 0.4f), 0.0f, Vector2.One * 0.5f, (size + Vector2.One * 4.0f) * sizeScale, SpriteEffects.None, 0.0f);
			spriteBatch.DrawString(font, text, loc4 + Vector2.UnitY * 30.0f * scale, color.Value, 0.0f, size * 0.5f, sizeScale, SpriteEffects.None, 0.0f);
		}

		private readonly List<Color> groupColors = new List<Color>();

		private void DrawCheckBlock(Check check, float scale)
		{
			Vector2 loc4 = check.loc * itemScale;

			if (drawMode == DrawMode.Full)
				loc4 = (loc4 - mapPosition) * scale;
			else
				loc4 = Vector2.Transform(loc4, camera);

			if (loc4.X < -100.0f || loc4.Y < -100.0f ||
				loc4.X > ScreenWidth + 100.0f || loc4.Y > ScreenHeight + 100.0f)
				return;

			Color colour;

			if (regionFilter != null)
			{
				if (check.Region == regionFilter)
					colour = Color.LightGreen;
				else
					colour = Color.IndianRed;
			}
			else
				colour = GetCheckStateColor(check);

			bool selected = check == checkFilter;
			Color borderColor = selected ? Color.DarkMagenta : Color.Black;

			spriteBatch.Draw(pixel, loc4, null, borderColor, 0.0f, Vector2.One * 0.5f, blockBorderSize * scale, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(pixel, loc4, null, colour, 0.0f, Vector2.One * 0.5f, blockFillSize * scale, SpriteEffects.None, 0.0f);
		}

		private void DrawCheckGroupBlock(CheckGroup checkGroup, float scale)
		{
			Vector2 loc4 = checkGroup.loc * itemScale;

			if (drawMode == DrawMode.Full)
				loc4 = (loc4 - mapPosition) * scale;
			else
				loc4 = Vector2.Transform(loc4, camera);

			if (loc4.X < -100.0f || loc4.Y < -100.0f ||
				loc4.X > ScreenWidth + 100.0f || loc4.Y > ScreenHeight + 100.0f)
				return;

			groupColors.Clear();

			if (regionFilter != null)
			{
				bool hasCheck = checkGroup.checks.Any((check) => check.Region == regionFilter);
				bool allChecks = checkGroup.checks.All((check) => check.Region == regionFilter);

				if (allChecks)
					groupColors.Add(Color.LightGreen);
				else if (hasCheck)
				{
					groupColors.Add(Color.LightGreen);
					groupColors.Add(Color.IndianRed);
				}
				else
					groupColors.Add(Color.IndianRed);
			}
			else
				GetCheckGroupColors(checkGroup, groupColors);

			bool selected = checkGroup.checks.Contains(checkFilter);
			Color borderColor = selected ? Color.DarkMagenta : Color.Black;

			spriteBatch.Draw(pixel, loc4, null, borderColor, 0.0f, Vector2.One * 0.5f, blockBorderSize * scale, SpriteEffects.None, 0.0f);

			if (groupColors.Count == 1)
				spriteBatch.Draw(pixel, loc4, null, groupColors[0], 0.0f, Vector2.One * 0.5f, blockFillSize * scale, SpriteEffects.None, 0.0f);
			else
			{
				float fillSize = blockFillSize / groupColors.Count;
				Vector2 size = new Vector2(blockFillSize, fillSize);
				loc4.Y -= (blockFillSize - fillSize) * 0.5f * scale;

				foreach (Color color in groupColors)
				{
					spriteBatch.Draw(pixel, loc4, null, color, 0.0f, Vector2.One * 0.5f, size * scale, SpriteEffects.None, 0.0f);
					loc4.Y += fillSize * scale;
				}
			}
		}

		private void DrawBlocks(float scale)
		{
			Profiler.Profile profile = Profiler.Profiler.Start("Map.DrawBlocks");

			foreach (CheckGroup checkGroup in checkGroups)
				DrawCheckGroupBlock(checkGroup, scale);

#if DEBUG

			foreach (KeyValuePair<string, Sequence> kvp in disabledSequences)
				DrawCheckBlock(kvp.Value, scale);

#endif

			Profiler.Profiler.End(profile);
		}

#if DEBUG

		private void DrawTexts(float scale)
		{
			Profiler.Profile profile = Profiler.Profiler.Start("Map.DrawTexts");

			foreach (CheckGroup checkGroup in checkGroups)
			{
				Color? colour = null;

				if (checkGroup.checks.Count == 1)
					colour = GetCheckTypeColor(checkGroup.checks[0]);

				DrawEntryText(checkGroup.checks[0].key, checkGroup.loc, scale, colour);

				if (checkGroup.checks.Count > 1)
					DrawEntryText("+", checkGroup.loc - new Vector2(0.0f, blockFillSize / itemScale), scale);
			}

#if DEBUG

			foreach (KeyValuePair<string, Sequence> kvp in disabledSequences)
			{
				Color colour = GetCheckTypeColor(kvp.Value);
				DrawEntryText(kvp.Key, kvp.Value.loc, scale, colour);
			}

#endif

			Profiler.Profiler.End(profile);
		}

#else

		private void DrawHovered(Vector2 pos, float scale)
		{
			GetCheckGroup(pos, out hoveredCheckGroup);

			if (hoveredCheckGroup == null)
				return;

			Vector2 gap = new Vector2(1800.0f, 250.0f);
			Vector2 loc = hoveredCheckGroup.loc;
			int maxY = 7;

			for (int i = 0; i < hoveredCheckGroup.checks.Count; i++)
			{
				Color color = GetCheckColor(hoveredCheckGroup.checks[i]);

				int x = i / maxY;
				int y = i % maxY;
				DrawEntryText(hoveredCheckGroup.checks[i].key, loc + gap * new Vector2(x, y), scale, color);
			}
		}

#endif

		public void Draw(Vector2 pos)
		{
			graphicsDevice.Clear(Color.Transparent);

			float scale = ZoomScale;

			if (drawMode == DrawMode.Full)
			{
				scale = FULL_SCALE;
				spriteBatch.Draw(map, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, scale * 10.0f, SpriteEffects.None, 0.0f);
			}
			else
			{
				Vector2 mapPos = Vector2.Transform(mapPosition, camera);

				// get top left map segment and bottom right map segment on screen
				Vector2 tl = -mapPos / scale / 1024;
				Vector2 wh = new Vector2(ScreenWidth, ScreenHeight);
				Vector2 br = (wh - mapPos) / scale / 1024;
				Point tli = new Point((int)tl.X, (int)tl.Y);
				Point bri = new Point((int)br.X, (int)br.Y);

				int segmentTop = MathHelper.Clamp(tli.Y, 0, SEGMENTS_Y - 1);
				int segmentBottom = MathHelper.Clamp(bri.Y, 0, SEGMENTS_Y - 1);
				int segmentLeft = MathHelper.Clamp(tli.X, 0, SEGMENTS_X - 1);
				int segmentRight = MathHelper.Clamp(bri.X, 0, SEGMENTS_X - 1);

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

#if DEBUG
			DrawTexts(scale);
#else
			DrawHovered(pos, scale);
#endif
		}

		public void Draw(float x, float y) => Draw(new Vector2(x, y));
		public void Draw(Point point) => Draw(new Vector2(point.X, point.Y));
		public void Draw(WinPoint point) => Draw(new Vector2(point.X, point.Y));
	}
}
