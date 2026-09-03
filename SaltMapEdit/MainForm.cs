using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaltMap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaltMapEdit
{
	public partial class MainForm : Form
	{
		private enum DragState
		{
			None,
			DragStart,
			Dragging
		}

		private struct Drag
		{
			public DragState state;
			public Point startPoint;

			public readonly bool ShouldStartDragging(int x, int y)
			{
				if (state != DragState.DragStart)
					return false;

				Point dragDist = startPoint;
				dragDist.X -= x;
				dragDist.Y -= y;

				return Math.Abs(dragDist.X) > SystemInformation.DragSize.Width ||
					Math.Abs(dragDist.Y) > SystemInformation.DragSize.Height;
			}
		}

		internal enum Mode
		{
			Regions,
			Areas
		}

		private struct AreaData
		{
			public List<string> keys;
			public Dictionary<string, List<string>> checks;
		}

		public Point mousePos;
		private Point lastMousePos;
		public Map map;

		public Map.Region currentArea;
		public Map.Region currentRegion;
		public Map.Connection currentConnection;
		public Map.Check currentCheck;

		public readonly Dictionary<string, Map.Region> areaDict = new Dictionary<string, Map.Region>();
		public readonly List<Map.Region> areaList = new List<Map.Region>();
		public readonly List<Map.Region> regionList = new List<Map.Region>();

		public readonly List<string> progressList = new List<string>();

		private readonly Dictionary<string, int> progressDict = new Dictionary<string, int>();

		public Microsoft.Xna.Framework.Graphics.SpriteFont spriteFont;

		private Action editAction;

		private Drag areaDrag; // drag action for reordering areas
		private Drag regionDrag; // drag action for reordering regions

		internal Mode mode = Mode.Regions;

		private Task saveTask;

		private readonly Color saveBaseColor;
		private readonly Color savingStartColor = Color.Green;
		private float saveTimer;

		// connections are sorted by region
		// locations are sorted by check id
		// items are sorted by name

		public MainForm()
		{
			InitializeComponent();

			pnlMap.KeyDown += pnlMap_KeyDown;
			pnlMap.MouseWheel += pnlMap_MouseWheel;

			saveBaseColor = btnSave.BackColor;

#if !DEBUG
			pnlData.Enabled = false;
#endif
		}

		private void StartSave()
		{
			if (saveTask != null)
			{
				saveTimer = 0.0f;
				saveTask.Wait();
			}

			saveTimer = 1.0f;
			saveTask = Task.Run(Saving);
		}

		private byte LerpChannel(byte a, byte b, float inv)
		{
			return (byte)Math.Min(a * saveTimer + b * inv, 255);
		}

		private void UpdateSaving(Color c)
		{
			btnSave.BackColor = c;
			btnSave.Invalidate();
		}

		private void Saving()
		{
			while (saveTimer > 0.0f)
			{
				saveTimer = Math.Max(saveTimer - 0.01f, 0.0f);
				float inv = 1.0f - saveTimer;
				byte r = LerpChannel(savingStartColor.R, saveBaseColor.R, inv);
				byte g = LerpChannel(savingStartColor.G, saveBaseColor.G, inv);
				byte b = LerpChannel(savingStartColor.B, saveBaseColor.B, inv);
				Color c = Color.FromArgb(r, g, b);

				btnSave.BeginInvoke(() => UpdateSaving(c));
				System.Threading.Thread.Sleep(10);
			}
		}

		internal bool GetProgress(string progressItem) =>
			progressDict.ContainsKey(progressItem);

		internal void AddProgress(string progressItem)
		{
			progressDict.TryGetValue(progressItem, out int count);
			progressDict[progressItem] = count + 1;
		}

		internal void RemoveProgress(string progressItem)
		{
			if (progressDict.TryGetValue(progressItem, out int count))
			{
				if (count <= 1)
					progressDict.Remove(progressItem);
				else
					progressDict[progressItem] = count - 1;
			}
		}

		public void MapLoaded()
		{
			if (File.Exists("regions.json"))
			{
				string json = File.ReadAllText("regions.json");
				JObject saved = JObject.Parse(json);
				JEnumerable<JToken> regions = saved.Children();

				foreach (JToken region in regions)
					regionList.Add(map.regions[region.Path]);

				foreach (Map.Region region in map.regions.Values)
					if (!regionList.Contains(region))
						regionList.Add(region);
			}
			else
			{
				// the map.regions dictionary does not preserve region order
				foreach (Map.Region region in map.regions.Values)
					regionList.Add(region);
			}

			if (File.Exists("areas.json"))
			{
				string json = File.ReadAllText("areas.json");

				AreaData areaData = JsonConvert.DeserializeObject<AreaData>(json);

				for (int i = 0; i < areaData.keys.Count; i++)
				{
					Map.Region area = new Map.Region() { key = areaData.keys[i] };
					areaDict.Add(area.key, area);
					areaList.Add(area);
					area.checks.AddRange(areaData.checks[area.key]);
				}
			}

			RebuildAreas();
			RebuildRegions();
			SortConnections();
			SortRegionSelect();

			map.saveRegions = SaveRegions;
		}

		private bool SaveRegions()
		{
			OrderedDictionary regions = new OrderedDictionary();

			foreach (Map.Region region in regionList)
				regions[region.key] = region;

			string json = JsonConvert.SerializeObject(regions, Formatting.Indented);
			File.WriteAllText("regions.json", json);

			return false;
		}

		private void Track(UndoAction action)
		{
			History.Add(action, true);

			btnUndo.Enabled = History.CanUndo;
			btnRedo.Enabled = History.CanRedo;
		}

		private void ShowEdit(Rectangle rect, string text, Action editAction)
		{
			tbEdit.Text = text;
			tbEdit.Location = rect.Location;
			tbEdit.Size = rect.Size;
			tbEdit.Visible = true;
			tbEdit.Focus();
			this.editAction = editAction;
		}

		private void ShowRegions(Rectangle rect, Map.Region region)
		{
			SortRegionSelect();
			int index = lbSelectRegion.Items.IndexOf(region);
			lbSelectRegion.SelectedIndex = index;
			lbSelectRegion.Location = rect.Location;
			lbSelectRegion.Size = new Size(rect.Size.Width, lbSelectRegion.Size.Height);
			lbSelectRegion.Visible = true;
			lbSelectRegion.Focus();
		}

		private void ShowChecks(Rectangle rect)
		{
			lbSelectCheck.SelectedIndex = -1;
			lbSelectCheck.Location = rect.Location;
			lbSelectCheck.Size = rect.Size;
			lbSelectCheck.Visible = true;
			lbSelectCheck.Focus();
		}

		private void EditArea()
		{
			// don't create a duplicate name if an existing region name was entered
			if (!areaDict.ContainsKey(tbEdit.Text))
			{
				int index = lbAreas.SelectedIndex;
				Map.Region area = areaList[index];
				string oldItem = area.key;
				string newItem = tbEdit.Text;

				Track(new RenameAreaAction(this, area, oldItem, newItem, index, false));
			}
		}

		private void EditRegion()
		{
			// don't create a duplicate name if an existing region name was entered
			if (!map.regions.ContainsKey(tbEdit.Text))
			{
				int index = lbRegions.SelectedIndex;
				Map.Region region = regionList[index];
				string oldItem = region.key;
				string newItem = tbEdit.Text;

				Track(new RenameRegionAction(this, region, oldItem, newItem, index, false));
			}
		}

		private void EditConnection()
		{
			int index = lbSelectRegion.SelectedIndex;

			if (index >= 0)
			{
				Map.Region newRegion = (Map.Region)lbSelectRegion.SelectedItem;
				Track(new ChangeConnectionAction(this, currentConnection, newRegion, false));
			}
		}

		private void EditCheck()
		{
			int index = lbSelectCheck.SelectedIndex;

			if (index >= 0)
			{
				Map.Check check = (Map.Check)lbSelectCheck.SelectedItem;

				if (mode == Mode.Regions && currentRegion != null)
					Track(new ChangeCheckRegionAction(this, check, currentRegion, false));
				else if (mode == Mode.Areas && currentArea != null)
					Track(new ChangeCheckAreaAction(this, check, currentArea, false));
			}
		}

		private void EditItem()
		{
			int index = lbItems.SelectedIndex;
			string oldItem = currentConnection.items[index];
			string newItem = tbEdit.Text;

			if (currentConnection.items.Contains(newItem))
				return;

			Track(new RenameItemAction(this, currentConnection, oldItem, newItem, false));
		}

		private void EditProgress()
		{
			int index = lbProgress.SelectedIndex;
			string oldItem = progressList[index];
			string newItem = tbEdit.Text;

			Track(new RenameProgressAction(this, oldItem, newItem, false));
		}

		internal int CompareConnections(Map.Connection a, Map.Connection b)
		{
			return regionList.IndexOf(a.Region) - regionList.IndexOf(b.Region);
		}

		public void RebuildAreas()
		{
			lbAreas.SelectedIndex = -1;
			lbAreas.Items.Clear();

			foreach (Map.Region area in areaList)
				lbAreas.Items.Add(area);

			lbAreas.SelectedIndex = areaList.IndexOf(currentArea);
		}

		public void RebuildRegions()
		{
			lbRegions.SelectedIndex = -1;
			lbRegions.Items.Clear();

			foreach (Map.Region region in regionList)
				lbRegions.Items.Add(region);

			lbRegions.SelectedIndex = regionList.IndexOf(currentRegion);
		}

		internal void SortConnections()
		{
			if (currentRegion == null)
				return;

			lbConnections.Items.Clear();

			currentRegion.connections.Sort(CompareConnections);

			foreach (Map.Connection connection in currentRegion.connections)
				lbConnections.Items.Add(connection);
		}

		internal int CompareChecks(string a, string b)
		{
			//map.GetLocation(a, out Map.Check ca);
			//map.GetLocation(b, out Map.Check cb);

			//return ca.id - cb.id;
			return a.CompareTo(b);
		}

		internal void SortChecks()
		{
			lbChecks.Items.Clear();

			Map.Region current = mode == Mode.Regions ? currentRegion : currentArea;

			if (current == null)
				return;

			current.checks.Sort(CompareChecks);

			foreach (string checkName in current.checks)
			{
				map.GetLocation(checkName, out Map.Check check);
				lbChecks.Items.Add(check);
			}
		}

		internal void SortItems()
		{
			lbItems.Items.Clear();

			if (currentConnection != null)
			{
				currentConnection.items.Sort();

				foreach (string item in currentConnection.items)
					lbItems.Items.Add(item);
			}
		}

		internal void SortProgress()
		{
			progressList.Sort();

			lbProgress.Items.Clear();

			foreach (string progress in progressList)
				lbProgress.Items.Add(progress);
		}

		internal void SortRegionSelect()
		{
			lbSelectRegion.SelectedIndex = -1;

			lbSelectRegion.Items.Clear();

			foreach (Map.Region region in regionList)
				if (region != currentRegion)
					lbSelectRegion.Items.Add(region);
		}

		internal void FocusCheck(Map.Check check)
		{
			if (check != null && map.drawMode == Map.DrawMode.Zoomable)
				map.SetCameraPosition(check.loc);
		}

		private bool FilterArea(Map.Check check) => currentArea?.checks.Contains(check.key) ?? false;
		private bool FilterAreaConnection(Map.Check check) => false;

		private int GetClickedIndex(ListBox listBox, Point mousePos)
		{
			int index = listBox.TopIndex + mousePos.Y / listBox.ItemHeight;

			return index >= 0 && index < listBox.Items.Count ? index : -1;
		}

		internal void ScrollToIndex(ListBox listBox, int index)
		{
			if (index < listBox.TopIndex)
				listBox.TopIndex = index;
			else
			{
				int visibleCount = listBox.ClientSize.Height / listBox.ItemHeight;
				int bottomIndex = listBox.TopIndex + visibleCount - 1;

				if (index > bottomIndex)
					listBox.TopIndex = Math.Max(0, index - visibleCount + 1);
			}
		}

		private void SelectArea(Map.Region area)
		{
			currentArea = area;
			map.Filter(currentArea, null, null);

			lbChecks.Items.Clear();

			if (currentArea != null)
				SortChecks();

			SelectCheck(null);
		}

		private void SelectRegion(Map.Region region)
		{
			currentRegion = region;
			map.Filter(currentRegion, null, null);
			
			lbConnections.SelectedIndex = -1;
			lbConnections.Items.Clear();
			lbChecks.Items.Clear();

			if (currentRegion != null)
			{
				foreach (Map.Connection connection in currentRegion.connections)
					lbConnections.Items.Add(connection);

				SortChecks();
			}

			SelectCheck(null);
		}

		internal void SelectCheck(Map.Check check)
		{
			currentCheck = check;

			if (check != null)
			{
				tbCheckName.Enabled = true;
				tbCheckDescription.Enabled = true;
				tbCheckName.Text = check.name;
				tbCheckDescription.Text = check.description;

				if (check is Map.Sequence sequence)
					lblCheckType.Text = $"{check.checkType} ({sequence.type})";
				else if (check.checkType == Map.CheckType.Switch &&
					check is Map.Container container)
				{
					if (map.GetDisabledSequence(container.items[0], out sequence) ||
						map.GetPlatform(container.items[0], out sequence))
						lblCheckType.Text = $"{check.checkType} ({sequence.type})";
					else
						lblCheckType.Text = $"{check.checkType} (Unknown)";
				}
				else
					lblCheckType.Text = $"{check.checkType}";

				if (mode == Mode.Regions)
				{
					if (currentRegion != null)
						lbChecks.SelectedItem = check;
				}
				else if (currentArea != null)
					lbChecks.SelectedItem = check;
			}
			else
			{
				tbCheckName.Enabled = false;
				tbCheckDescription.Enabled = false;
				tbCheckName.Text = "";
				tbCheckDescription.Text = "";
				lblCheckType.Text = "";
			}

			if (mode == Mode.Regions)
				map.Filter(currentRegion, currentConnection, check);
			else
				map.Filter(currentArea, null, check);
		}

		private Map.Region GetArea(string checkName)
		{
			foreach (Map.Region area in areaList)
				if (area.checks.Contains(checkName))
					return area;

			return null;
		}

		private Map.Region GetArea(Map.Check check) => GetArea(check.key);

		#region Events

		private void MainForm_Click(object sender, EventArgs e)
		{
			lbSelectRegion.Visible = false;
			lbSelectCheck.Visible = false;
			tbEdit.Visible = false;
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			saveTimer = 0.0f;
			saveTask?.Wait();
		}

		#region pnlMap

		private void pnlMap_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (map.GetCheck(e.Location, out Map.Check check))
				{
					if (mode == Mode.Regions)
					{
						lbRegions.SelectedIndex = regionList.IndexOf(check.Region);
						SelectRegion(check.Region);
					}
					else
					{
						for (int i = 0; i < areaList.Count; i++)
						{
							if (areaList[i].checks.Contains(check.key))
							{
								lbAreas.SelectedIndex = i;
								SelectArea((Map.Region)lbAreas.SelectedItem);
								break;
							}
						}
					}

					SelectCheck(check);
				}
			}
			else if (e.Button == MouseButtons.Middle)
			{
				if (map.GetCheck(e.Location, out Map.Check check))
					Track(new ToggleCheckAction(this, check, false));
			}
			else if (e.Button == MouseButtons.Right)
			{
				if (mode == Mode.Regions && currentRegion != null)
				{
					map.GetCheckGroup(e.Location, out Map.CheckGroup checkGroup);

					if (checkGroup == null)
						return;

					lbSelectCheck.Items.Clear();

					foreach (Map.Check check in checkGroup.checks)
						if (check.Region != currentRegion)
							lbSelectCheck.Items.Add(check);

					if (lbSelectCheck.Items.Count > 0)
					{
						if (lbSelectCheck.Items.Count == 1)
						{
							Map.Check check = (Map.Check)lbSelectCheck.Items[0];
							Map.Region oldRegion = check.Region;

							if (oldRegion != currentRegion)
								Track(new ChangeCheckRegionAction(this, check, currentRegion, false));
						}
						else
						{
							Microsoft.Xna.Framework.Vector2 size = default;

							foreach (Map.Check check in lbSelectCheck.Items)
							{
								Microsoft.Xna.Framework.Vector2 size2 = spriteFont.MeasureString(check.key);
								size.X = Math.Max(size.X, size2.X);
							}

							size.Y = lbSelectCheck.ItemHeight * lbSelectCheck.Items.Count;

							Point loc = pnlMap.PointToScreen(new Point(e.X, e.Y));
							loc = PointToClient(loc);

							Rectangle rect = new Rectangle()
							{
								X = loc.X,
								Y = loc.Y,
								Width = (int)size.X + 4,
								Height = (int)size.Y + 4,
							};

							ShowChecks(rect);
						}
					}
				}
				else if (mode == Mode.Areas && currentArea != null)
				{
					map.GetCheckGroup(e.Location, out Map.CheckGroup checkGroup);

					if (checkGroup == null)
						return;

					lbSelectCheck.Items.Clear();

					foreach (Map.Check check in checkGroup.checks)
						if (check.Region != currentArea)
							lbSelectCheck.Items.Add(check);

					if (lbSelectCheck.Items.Count > 0)
					{
						if (lbSelectCheck.Items.Count == 1)
						{
							Map.Check check = (Map.Check)lbSelectCheck.Items[0];
							Map.Region oldArea = GetArea(check);

							if (oldArea != currentArea)
								Track(new ChangeCheckAreaAction(this, check, currentArea, false));
						}
						else
						{
							Microsoft.Xna.Framework.Vector2 size = default;

							foreach (Map.Check check in lbSelectCheck.Items)
							{
								Microsoft.Xna.Framework.Vector2 size2 = spriteFont.MeasureString(check.key);
								size.X = Math.Max(size.X, size2.X);
							}

							size.Y = lbSelectCheck.ItemHeight * lbSelectCheck.Items.Count;

							Point loc = pnlMap.PointToScreen(new Point(e.X, e.Y));
							loc = PointToClient(loc);

							Rectangle rect = new Rectangle()
							{
								X = loc.X,
								Y = loc.Y,
								Width = (int)size.X + 4,
								Height = (int)size.Y + 4,
							};

							ShowChecks(rect);
						}
					}
				}
			}
		}

		private void pnlMap_MouseDown(object sender, MouseEventArgs e)
		{
			pnlMap.Focus();
		}

		private void pnlMap_MouseMove(object sender, MouseEventArgs e)
		{
			mousePos = e.Location;
			Point mousePosDelta = e.Location;
			mousePosDelta.Offset(-lastMousePos.X, -lastMousePos.Y);

			if (e.Button == MouseButtons.Left)
				map.Move(mousePosDelta);

			lastMousePos = mousePos;
		}

		private void pnlMap_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta != 0)
				Console.WriteLine(e.Delta);

			map.Zoom(mousePos, e.Delta > 0 ? 1 : -1);
		}

		private void pnlMap_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.M)
				map.ToggleMode();
		}

		#endregion

		#region lbAreas

		private void lbAreas_MouseClick(object sender, MouseEventArgs e)
		{
			int index = GetClickedIndex(lbAreas, e.Location);

			if (index == -1)
				lbAreas.SelectedIndex = -1;

			SelectArea(index >= 0 ? areaList[index] : null);
		}

		private void lbAreas_DoubleClick(object sender, EventArgs e)
		{
			int index = lbAreas.SelectedIndex;

			if (index >= 0)
			{
				Rectangle rect = lbAreas.GetItemRectangle(index);
				rect.Location = lbAreas.PointToScreen(rect.Location);
				rect.Location = PointToClient(rect.Location);
				ShowEdit(rect, areaList[index].key, EditArea);
			}
		}

		private void lbAreas_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && lbAreas.SelectedItem != null)
			{
				areaDrag.state = DragState.DragStart;
				areaDrag.startPoint = new Point(e.X, e.Y);
			}
		}

		private void lbAreas_MouseUp(object sender, MouseEventArgs e)
		{
			areaDrag.state = DragState.None;
		}

		private void lbAreas_MouseMove(object sender, MouseEventArgs e)
		{
			if (areaDrag.ShouldStartDragging(e.X, e.Y))
			{
				areaDrag.state = DragState.Dragging;
				lbAreas.DoDragDrop(lbAreas.SelectedItem, DragDropEffects.Move);
			}
		}

		private void lbAreas_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		private void lbAreas_DragDrop(object sender, DragEventArgs e)
		{
			Point point = lbAreas.PointToClient(new Point(e.X, e.Y));
			Map.Region area = (Map.Region)e.Data.GetData(typeof(Map.Region));
			int oldIndex = areaList.IndexOf(area);
			int newIndex = lbAreas.IndexFromPoint(point);

			if (newIndex < 0)
				newIndex = lbAreas.Items.Count - 1;

			if (oldIndex != newIndex)
				Track(new ReorderAreaAction(this, oldIndex, newIndex, false));

			areaDrag.state = DragState.None;
		}

		private void lbAreas_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				int index = lbAreas.SelectedIndex;

				if (index >= 0)
				{
					Map.Region area = (Map.Region)lbAreas.SelectedItem;
					Track(new AddAreaAction(this, area, index, true));
				}
			}
		}

		#endregion

		#region lbRegions

		private void lbRegions_Click(object sender, EventArgs e)
		{
			int i = lbRegions.SelectedIndex;
			SelectRegion(i >= 0 ? regionList[i] : null);
		}

		private void lbRegions_DoubleClick(object sender, EventArgs e)
		{
			int index = lbRegions.SelectedIndex;

			if (index >= 0)
			{
				Rectangle rect = lbRegions.GetItemRectangle(index);
				rect.Location = lbRegions.PointToScreen(rect.Location);
				rect.Location = PointToClient(rect.Location);
				ShowEdit(rect, regionList[index].key, EditRegion);
			}
		}

		private void lbRegions_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (currentRegion != null)
				{
					int index = lbRegions.IndexFromPoint(e.Location);

					if (index >= 0 && index < lbRegions.Items.Count)
					{
						if (regionList[index] != currentRegion)
						{
							Map.Connection connection = new Map.Connection()
							{
								Region = regionList[index]
							};

							Track(new AddConnectionAction(this, currentRegion, connection, false));
						}
					}
				}
			}
			else if (e.Button == MouseButtons.Left && lbRegions.SelectedItem != null)
			{
				regionDrag.state = DragState.DragStart;
				regionDrag.startPoint = new Point(e.X, e.Y);
			}
		}

		private void lbRegions_MouseUp(object sender, MouseEventArgs e)
		{
			regionDrag.state = DragState.None;
		}

		private void lbRegions_MouseMove(object sender, MouseEventArgs e)
		{
			if (regionDrag.ShouldStartDragging(e.X, e.Y))
			{
				regionDrag.state = DragState.Dragging;
				lbRegions.DoDragDrop(lbRegions.SelectedItem, DragDropEffects.Move);
			}
		}

		private void lbRegions_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		private void lbRegions_DragDrop(object sender, DragEventArgs e)
		{
			Point point = lbRegions.PointToClient(new Point(e.X, e.Y));
			Map.Region region = (Map.Region)e.Data.GetData(typeof(Map.Region));
			int oldIndex = regionList.IndexOf(region);
			int newIndex = lbRegions.IndexFromPoint(point);

			if (newIndex < 0)
				newIndex = lbRegions.Items.Count - 1;

			if (oldIndex != newIndex)
			{
				Track(new ReorderRegionAction(this, oldIndex, newIndex, false));

				foreach (Map.Region mapRegion in regionList)
					mapRegion.connections.Sort(CompareConnections);

				SortConnections();
			}

			regionDrag.state = DragState.None;
		}

		private void lbRegions_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				int index = lbRegions.SelectedIndex;

				if (index >= 0)
				{
					Map.Region region = (Map.Region)lbRegions.SelectedItem;
					Track(new AddRegionAction(this, region, index, true));
				}
			}
		}

		#endregion

		#region lbConnections

		// connections are auto sorted by region order

		private void lbConnections_SelectedValueChanged(object sender, EventArgs e)
		{
			lbItems.Items.Clear();
			currentConnection = null;

			int index = lbConnections.SelectedIndex;

			if (index >= 0)
			{
				currentConnection = currentRegion.connections[index];

				foreach (string item in currentConnection.items)
					lbItems.Items.Add(item);
			}

			map.Filter(currentRegion, currentConnection, null);
		}

		private void lbConnections_DoubleClick(object sender, EventArgs e)
		{
			int index = lbConnections.SelectedIndex;

			if (index >= 0)
			{
				Rectangle rect = lbConnections.GetItemRectangle(index);
				rect.Location = lbConnections.PointToScreen(rect.Location);
				rect.Location = PointToClient(rect.Location);
				ShowRegions(rect, currentRegion.connections[index].Region);
			}
		}

		private void lbConnections_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				int index = lbAreas.SelectedIndex;

				if (index >= 0)
				{
					Map.Connection connection = (Map.Connection)lbConnections.SelectedItem;
					Track(new AddConnectionAction(this, currentRegion, connection, true));
				}
			}
		}

		#endregion

		#region lbChecks

		// locations are auto sorted by id

		private void lbChecks_Click(object sender, EventArgs e)
		{
			Map.Check check = (Map.Check)lbChecks.SelectedItem;

			FocusCheck(check);
			SelectCheck(check);
		}

		#endregion

		#region lbItems

		private void lbItems_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete && currentConnection != null)
			{
				int index = lbItems.SelectedIndex;
				string item = currentConnection.items[index];

				Track(new AddItemAction(this, currentConnection, item, true));

				if (lbItems.Items.Count > 0)
				{
					if (index >= lbItems.Items.Count)
						index = lbItems.Items.Count - 1;
					else if (index < 0)
						index = 0;

					lbItems.SelectedIndex = index;
				}
				else
					lbItems.SelectedIndex = -1;
			}
		}

		private void lbItems_DoubleClick(object sender, EventArgs e)
		{
			int index = lbItems.SelectedIndex;

			if (index >= 0)
			{
				Rectangle rect = lbItems.GetItemRectangle(index);
				rect.Location = lbItems.PointToScreen(rect.Location);
				rect.Location = PointToClient(rect.Location);
				ShowEdit(rect, currentConnection.items[index], EditItem);
			}
		}

		#endregion

		#region lbProgress

		private void lbProgress_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				int i = lbProgress.SelectedIndex;

				if (i >= 0)
					Track(new AddProgressAction(this, progressList[i], true));
			}
		}

		private void lbProgress_DoubleClick(object sender, EventArgs e)
		{
			int index = lbProgress.SelectedIndex;

			if (index >= 0)
			{
				Rectangle rect = lbProgress.GetItemRectangle(index);
				rect.Location = lbProgress.PointToScreen(rect.Location);
				rect.Location = PointToClient(rect.Location);
				ShowEdit(rect, progressList[index], EditProgress);
			}
		}

		#endregion

		#region lbSelectRegion

		private void lbSelectRegion_Leave(object sender, EventArgs e)
		{
			lbSelectRegion.Visible = false;
		}

		private void lbSelectRegion_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				lbSelectCheck.Visible = false;
		}

		private void lbSelectRegion_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void lbSelectRegion_Click(object sender, EventArgs e)
		{
			EditConnection();
			lbSelectRegion.Visible = false;
			Focus();
		}

		#endregion

		#region lbSelectCheck

		private void lbSelectCheck_Leave(object sender, EventArgs e)
		{
			lbSelectCheck.Visible = false;
		}

		private void lbSelectCheck_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				lbSelectCheck.Visible = false;
		}

		private void lbSelectCheck_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void lbSelectCheck_Click(object sender, EventArgs e)
		{
			EditCheck();
			lbSelectCheck.Visible = false;
			Focus();
		}

		#endregion

		#region tbEdit

		private void tbEdit_Leave(object sender, EventArgs e)
		{
			tbEdit.Visible = false;
		}

		private void tbEdit_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				tbEdit.Visible = false;
				Focus();
			}
			else if (e.KeyCode == Keys.Enter)
			{
				editAction();
				tbEdit.Visible = false;
				Focus();
			}
			else
			{
				// allowed keys are A-Z, 0-9, arrow keys, underscore, backspace, delete
				bool validKey = e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z ||
					e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9 && !e.Shift ||
					e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9 ||
					e.KeyCode >= Keys.Left && e.KeyCode <= Keys.Down ||
					e.KeyCode == Keys.OemMinus && e.Shift ||
					e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete ||
					e.KeyCode == Keys.Home || e.KeyCode == Keys.End;

				if (mode == Mode.Areas)
				{
					validKey |= e.KeyCode == Keys.Space;
					validKey |= e.KeyCode == Keys.OemQuotes && !e.Shift;
					validKey |= e.KeyCode == Keys.OemMinus;
				}

				if (e.KeyCode != Keys.A)
					validKey &= !e.Control; // control is not pressed

				e.SuppressKeyPress = !validKey;
				e.Handled = !validKey;
			}
		}

		#endregion

		#region Buttons

		private void btnClearFilter_Click(object sender, EventArgs e)
		{
			map.Filter(null, null, null);
		}

		private void btnAddArea_Click(object sender, EventArgs e)
		{
			string baseName = "new_area";
			string name = baseName;
			int count = 1;

			while (areaDict.ContainsKey(name))
				name = $"{baseName}_{++count}";

			Map.Region area = new Map.Region()
			{
				connections = new List<Map.Connection>(),
				key = name
			};

			int i = areaList.Count;

			if (lbAreas.SelectedItem != null)
				i = areaList.IndexOf((Map.Region)lbAreas.SelectedItem);

			Track(new AddAreaAction(this, area, i, false));
		}

		private void btnAddRegion_Click(object sender, EventArgs e)
		{
			string baseName = "new_region";
			string name = baseName;
			int count = 1;

			while (map.regions.ContainsKey(name))
				name = $"{baseName}_{++count}";

			Map.Region region = new Map.Region()
			{
				connections = new List<Map.Connection>(),
				key = name
			};

			int i = regionList.Count;

			if (lbRegions.SelectedItem != null)
				i = regionList.IndexOf((Map.Region)lbRegions.SelectedItem);

			Track(new AddRegionAction(this, region, i, false));
		}

		private void btnAddConnection_Click(object sender, EventArgs e)
		{
			if (currentRegion != null)
			{
				Map.Connection connection = new Map.Connection()
				{
					Region = regionList[0]
				};

				Track(new AddConnectionAction(this, currentRegion, connection, false));
			}
		}

		private void btnAddItem_Click(object sender, EventArgs e)
		{
			if (currentConnection != null)
				Track(new AddItemAction(this, currentConnection, "new_item", false));
		}

		private void btnAddProgress_Click(object sender, EventArgs e)
		{
			Track(new AddProgressAction(this, "new_progress", false));
		}

		private void btnChangeMode_Click(object sender, EventArgs e)
		{
			mode = mode == Mode.Regions ? Mode.Areas : Mode.Regions;

			if (mode == Mode.Regions)
			{
				lblAreas.Visible = false;
				lblRegions.Visible = true;
				lbAreas.Visible = false;
				lbRegions.Visible = true;
				btnAddArea.Visible = false;
				btnAddRegion.Visible = true;
				pnlConnections.Enabled = true;
				pnlItems.Enabled = true;

				map.RegionFilterLogic = null;
				map.ConnectionFilterLogic = null;

				SortConnections();
				SortChecks();
				SortItems();

				map.Filter(currentRegion, currentConnection, currentCheck);
			}
			else
			{
				lblAreas.Visible = true;
				lblRegions.Visible = false;
				lbAreas.Visible = true;
				lbRegions.Visible = false;
				btnAddArea.Visible = true;
				btnAddRegion.Visible = false;
				pnlConnections.Enabled = false;
				pnlItems.Enabled = false;

				map.RegionFilterLogic = FilterArea;
				map.ConnectionFilterLogic = FilterAreaConnection;

				lbConnections.Items.Clear();
				SortChecks();
				lbItems.Items.Clear();

				map.Filter(currentArea, null, currentCheck);
			}

			map.UpdateAvailable(GetProgress);
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			AreaData areaData = new AreaData()
			{
				keys = areaList.ConvertAll((x) => x.key),
				checks = new Dictionary<string, List<string>>()
			};

			foreach (Map.Region area in areaList)
			{
				int chestCount = 1;
				int sackCount = 1;
				int mimicCount = 1;
				int switchCount = 1;
				int liftCount = 1;
				int doorCount = 1;
				int gateCount = 1;
				int secretCount = 1;

				areaData.checks[area.key] = area.checks;

				foreach (string checkName in area.checks)
				{
					map.GetLocation(checkName, out Map.Check check);

					string obj = check.checkType switch
					{
						Map.CheckType.Chest => $"Chest {chestCount++}",
						Map.CheckType.Sack => $"Sack {sackCount++}",
						Map.CheckType.Mimic => $"Mimic {mimicCount++}",
						Map.CheckType.Switch => $"Switch {switchCount++}",
						Map.CheckType.Sequence => ((Map.Sequence)check).type switch
						{
							Map.SequenceType.Lift => $"Lift {liftCount++}",
							Map.SequenceType.Door => $"Door {doorCount++}",
							Map.SequenceType.Gate => $"Gate {gateCount++}",
							Map.SequenceType.Secret => $"Secret {secretCount++}",
							_ => null
						},
						_ => null
					};

					if (obj != null)
						check.name = $"{area.key} {obj}";
				}
			}

			string json = JsonConvert.SerializeObject(areaData, Formatting.Indented);
			File.WriteAllText("areas.json", json);

			map.Save();
			StartSave();
		}

		private void btnUndo_Click(object sender, EventArgs e)
		{
			History.Undo();

			btnUndo.Enabled = History.CanUndo;
			btnRedo.Enabled = History.CanRedo;
		}

		private void btnRedo_Click(object sender, EventArgs e)
		{
			History.Redo();

			btnUndo.Enabled = History.CanUndo;
			btnRedo.Enabled = History.CanRedo;
		}

		#endregion

		#region tbCheckName

		private void tbCheckName_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				tbCheckName.Text = currentCheck?.name;
				return;
			}

			if (e.KeyCode != Keys.Enter)
				return;

			e.Handled = true;
			e.SuppressKeyPress = true;

			string text = tbCheckName.Text;

			if (currentCheck != null)
			{
				if (text.Length > 0)
					Track(new RenameCheckAction(this, currentCheck, text, false));
				else
					tbCheckName.Text = currentCheck.name;
			}
			else if (text.Length > 0)
				tbCheckName.Text = "";
		}

		private void tbCheckName_Leave(object sender, EventArgs e)
		{
			tbCheckName.Text = currentCheck?.name;
		}

		#endregion

		#region tbCheckDescription

		private void tbCheckDescription_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				tbCheckDescription.Text = currentCheck?.name;
				return;
			}

			if (e.KeyCode != Keys.Enter)
				return;

			e.Handled = true;
			e.SuppressKeyPress = true;

			string text = tbCheckDescription.Text;

			if (currentCheck != null)
			{
				if (text.Length > 0)
					Track(new ChangeCheckDescriptionAction(this, currentCheck, text, false));
				else
					tbCheckDescription.Text = currentCheck.name;
			}
			else if (text.Length > 0)
				tbCheckDescription.Text = "";
		}

		private void tbCheckDescription_Leave(object sender, EventArgs e)
		{

		}

		#endregion

		#endregion
	}

	#region Undo

	public abstract class MainFormUndoAction : UndoAction
	{
		protected readonly MainForm form;

		public MainFormUndoAction(MainForm form, bool inverted) : base(inverted)
		{
			this.form = form;
		}
	}

	public class ToggleCheckAction : MainFormUndoAction
	{
		private readonly Map.Check check;
		private readonly Map.CheckState oldCheckState;
		private readonly Map.CheckState newCheckState;

		public ToggleCheckAction(MainForm form, Map.Check check, bool inverted) : base(form, inverted)
		{
			this.check = check;
			oldCheckState = check.checkState;
			newCheckState = check.checkState == Map.CheckState.Collected ?
				Map.CheckState.Available : Map.CheckState.Collected;
		}

		public override void Undo()
		{
			check.checkState = oldCheckState;
		}

		public override void Redo()
		{
			check.checkState = newCheckState;
		}
	}

	public class ChangeCheckAreaAction : MainFormUndoAction
	{
		private readonly Map.Check check;
		private readonly Map.Region oldArea;
		private readonly Map.Region newArea;

		public ChangeCheckAreaAction(MainForm form, Map.Check check, Map.Region newArea, bool inverted) : base(form, inverted)
		{
			this.check = check;
			oldArea = check.Region;
			this.newArea = newArea;
		}

		private void ChangeCheckArea(Map.Region oldValue, Map.Region newValue)
		{
			oldValue.checks.Remove(check.key);
			newValue.checks.Add(check.key);

			oldValue.checks.Sort(form.CompareChecks);
			newValue.checks.Sort(form.CompareChecks);

			if (oldValue == form.currentArea || newValue == form.currentArea)
			{
				form.SortChecks();

				if (newValue == form.currentArea && form.mode == MainForm.Mode.Areas)
				{
					int index = form.lbChecks.Items.IndexOf(check);
					form.lbChecks.SelectedIndex = index;

					form.ScrollToIndex(form.lbChecks, index);
				}
			}
		}

		public override void Undo()
		{
			ChangeCheckArea(newArea, oldArea);
		}

		public override void Redo()
		{
			ChangeCheckArea(oldArea, newArea);
		}
	}

	public class ChangeCheckRegionAction : MainFormUndoAction
	{
		private readonly Map.Check check;
		private readonly Map.Region oldRegion;
		private readonly Map.Region newRegion;

		public ChangeCheckRegionAction(MainForm form, Map.Check check, Map.Region newRegion, bool inverted) : base(form, inverted)
		{
			this.check = check;
			oldRegion = check.Region;
			this.newRegion = newRegion;
		}

		private void ChangeCheckRegion(Map.Region oldValue, Map.Region newValue)
		{
			check.Region = newValue;

			oldValue.checks.Remove(check.key);
			newValue.checks.Add(check.key);

			oldValue.checks.Sort(form.CompareChecks);
			newValue.checks.Sort(form.CompareChecks);

			if (oldValue == form.currentRegion || newValue == form.currentRegion)
			{
				form.SortChecks();

				if (newValue == form.currentRegion && form.mode == MainForm.Mode.Regions)
				{
					int index = form.lbChecks.Items.IndexOf(check);
					form.lbChecks.SelectedIndex = index;

					form.ScrollToIndex(form.lbChecks, index);
				}
			}
		}

		public override void Undo()
		{
			ChangeCheckRegion(newRegion, oldRegion);
		}

		public override void Redo()
		{
			ChangeCheckRegion(oldRegion, newRegion);
		}
	}

	public class AddAreaAction : MainFormUndoAction
	{
		private readonly Map.Region area;
		private readonly int index;

		public AddAreaAction(MainForm form, Map.Region area, int index, bool inverted) : base(form, inverted)
		{
			this.area = area;
			this.index = index;
		}

		public override void Undo()
		{
			form.areaList.RemoveAt(index);
			form.areaDict.Remove(area.key);

			form.RebuildAreas();
		}

		public override void Redo()
		{
			form.areaList.Insert(index, area);
			form.areaDict.Add(area.key, area);

			form.RebuildAreas();

			form.ScrollToIndex(form.lbAreas, index);
		}
	}

	public class AddRegionAction : MainFormUndoAction
	{
		private readonly Map.Region region;
		private readonly int index;

		public AddRegionAction(MainForm form, Map.Region region, int index, bool inverted) : base(form, inverted)
		{
			this.region = region;
			this.index = index;
		}

		public override void Undo()
		{
			form.regionList.RemoveAt(index);
			form.map.regions.Remove(region.key);

			foreach (Map.Region mapRegion in form.regionList)
				mapRegion.connections.Sort(form.CompareConnections);

			form.RebuildRegions();
			form.SortConnections();
			form.SortRegionSelect();
		}

		public override void Redo()
		{
			form.regionList.Insert(index, region);
			form.map.regions.Add(region.key, region);

			foreach (Map.Region mapRegion in form.regionList)
				mapRegion.connections.Sort(form.CompareConnections);

			form.RebuildRegions();
			form.SortConnections();
			form.SortRegionSelect();

			form.ScrollToIndex(form.lbRegions, index);
		}
	}

	public class AddConnectionAction : MainFormUndoAction
	{
		private readonly Map.Region region;
		private readonly Map.Connection connection;

		public AddConnectionAction(MainForm form, Map.Region region, Map.Connection connection, bool inverted) : base(form, inverted)
		{
			this.region = region;
			this.connection = connection;
		}

		public override void Undo()
		{
			region.connections.Remove(connection);
			form.SortConnections();

			if (form.currentConnection == connection)
				form.currentConnection = null;

			form.SortItems();
		}

		public override void Redo()
		{
			region.connections.Add(connection);
			form.SortConnections();
		}
	}

	public class AddItemAction : MainFormUndoAction
	{
		private readonly Map.Connection connection;
		private readonly string item;

		public AddItemAction(MainForm form, Map.Connection connection, string item, bool inverted) : base(form, inverted)
		{
			this.connection = connection;
			this.item = item;
		}

		public override void Undo()
		{
			connection.items.Remove(item);
			connection.items.Sort();

			if (form.currentConnection == connection)
				form.SortItems();
		}

		public override void Redo()
		{
			connection.items.Add(item);
			connection.items.Sort();

			if (form.currentConnection == connection)
				form.SortItems();
		}
	}

	public class AddProgressAction : MainFormUndoAction
	{
		private readonly string progress;

		public AddProgressAction(MainForm form, string progress, bool inverted) : base(form, inverted)
		{
			this.progress = progress;
		}

		public override void Undo()
		{
			form.progressList.Remove(progress);
			form.RemoveProgress(progress);
			form.SortProgress();

			form.map.UpdateAvailable(form.GetProgress);
		}

		public override void Redo()
		{
			form.progressList.Add(progress);
			form.AddProgress(progress);
			form.SortProgress();

			form.map.UpdateAvailable(form.GetProgress);
		}
	}

	public class RenameAreaAction : MainFormUndoAction
	{
		private readonly Map.Region area;
		private readonly string oldItem;
		private readonly string newItem;
		private readonly int index;

		public RenameAreaAction(MainForm form, Map.Region area, string oldItem, string newItem, int index, bool inverted) : base(form, inverted)
		{
			this.area = area;
			this.oldItem = oldItem;
			this.newItem = newItem;
			this.index = index;
		}

		public override void Undo()
		{
			area.key = oldItem;
			form.areaDict.Remove(newItem);
			form.areaDict.Add(oldItem, area);
			form.areaList[index] = area;

			form.RebuildAreas();

			form.ScrollToIndex(form.lbAreas, index);
		}

		public override void Redo()
		{
			area.key = newItem;
			form.areaDict.Remove(oldItem);
			form.areaDict.Add(newItem, area);
			form.areaList[index] = area;

			form.RebuildAreas();

			form.ScrollToIndex(form.lbAreas, index);
		}
	}

	public class RenameRegionAction : MainFormUndoAction
	{
		private readonly Map.Region region;
		private readonly string oldItem;
		private readonly string newItem;
		private readonly int index;

		public RenameRegionAction(MainForm form, Map.Region region, string oldItem, string newItem, int index, bool inverted) : base(form, inverted)
		{
			this.region = region;
			this.oldItem = oldItem;
			this.newItem = newItem;
			this.index = index;
		}

		public override void Undo()
		{
			region.key = oldItem;
			form.map.regions.Remove(newItem);
			form.map.regions.Add(oldItem, region);
			form.regionList[index] = region;

			foreach (Map.Region mapRegion in form.regionList)
				mapRegion.connections.Sort(form.CompareConnections);

			form.RebuildRegions();
			form.SortConnections();
			form.SortRegionSelect();

			form.ScrollToIndex(form.lbRegions, index);
		}

		public override void Redo()
		{
			region.key = newItem;
			form.map.regions.Remove(oldItem);
			form.map.regions.Add(newItem, region);
			form.regionList[index] = region;

			foreach (Map.Region mapRegion in form.regionList)
				mapRegion.connections.Sort(form.CompareConnections);

			form.RebuildRegions();
			form.SortConnections();
			form.SortRegionSelect();

			form.ScrollToIndex(form.lbRegions, index);
		}
	}

	public class ChangeConnectionAction : MainFormUndoAction
	{
		private readonly Map.Connection connection;
		private readonly Map.Region oldRegion;
		private readonly Map.Region newRegion;

		public ChangeConnectionAction(MainForm form, Map.Connection connection, Map.Region newRegion, bool inverted) : base(form, inverted)
		{
			this.connection = connection;
			oldRegion = connection.Region;
			this.newRegion = newRegion;
		}

		public override void Undo()
		{
			connection.Region = oldRegion;
			form.SortConnections();
		}

		public override void Redo()
		{
			connection.Region = newRegion;
			form.SortConnections();
		}
	}

	public class RenameItemAction : MainFormUndoAction
	{
		private readonly Map.Connection connection;
		private readonly string oldItem;
		private readonly string newItem;

		public RenameItemAction(MainForm form, Map.Connection connection, string oldItem, string newItem, bool inverted) : base(form, inverted)
		{
			this.connection = connection;
			this.oldItem = oldItem;
			this.newItem = newItem;
		}

		private void ChangeItem(string oldValue, string newValue)
		{
			int index = connection.items.IndexOf(oldValue);
			connection.items[index] = newValue;
			connection.items.Sort();

			if (form.currentConnection == connection)
				form.SortItems();
		}

		public override void Undo()
		{
			ChangeItem(newItem, oldItem);
		}

		public override void Redo()
		{
			ChangeItem(oldItem, newItem);
		}
	}

	public class RenameProgressAction : MainFormUndoAction
	{
		private readonly string oldItem;
		private readonly string newItem;

		public RenameProgressAction(MainForm form, string oldItem, string newItem, bool inverted) : base(form, inverted)
		{
			this.oldItem = oldItem;
			this.newItem = newItem;
		}

		private void ChangeItem(string oldValue, string newValue)
		{
			int index = form.progressList.IndexOf(oldValue);
			form.progressList[index] = newValue;

			form.RemoveProgress(oldValue);
			form.AddProgress(newValue);

			form.SortProgress();

			form.map.UpdateAvailable(form.GetProgress);
		}

		public override void Undo()
		{
			ChangeItem(newItem, oldItem);
		}

		public override void Redo()
		{
			ChangeItem(oldItem, newItem);
		}
	}

	public class RenameCheckAction : MainFormUndoAction
	{
		private readonly Map.Check check;
		private readonly string oldName;
		private readonly string newName;

		public RenameCheckAction(MainForm form, Map.Check check, string newName, bool inverted) : base(form, inverted)
		{
			this.check = check;

			if (inverted)
			{
				oldName = newName;
				this.newName = check.name;
			}
			else
			{
				oldName = check.name;
				this.newName = newName;
			}
		}

		public override void Undo()
		{
			check.name = oldName;

			form.SelectCheck(check);
		}

		public override void Redo()
		{
			check.name = newName;

			form.SelectCheck(check);
		}
	}

	public class ChangeCheckDescriptionAction : MainFormUndoAction
	{
		private readonly Map.Check check;
		private readonly string oldDescription;
		private readonly string newDescription;

		public ChangeCheckDescriptionAction(MainForm form, Map.Check check, string newDescription, bool inverted) : base(form, inverted)
		{
			this.check = check;

			if (inverted)
			{
				oldDescription = newDescription;
				this.newDescription = check.description;
			}
			else
			{
				oldDescription = check.description;
				this.newDescription = newDescription;
			}
		}

		public override void Undo()
		{
			check.description = oldDescription;

			form.SelectCheck(check);
		}

		public override void Redo()
		{
			check.description = newDescription;

			form.SelectCheck(check);
		}
	}

	// if old index is less than new index, new index will
	// need to be shifted back 1 to account for the removal
	// areas are saved in a dictionary, so does order matter?
	public class ReorderAreaAction : MainFormUndoAction
	{
		private readonly int oldIndex;
		private readonly int newIndex;

		public ReorderAreaAction(MainForm form, int oldIndex, int newIndex, bool inverted) : base(form, inverted)
		{
			this.oldIndex = oldIndex;
			this.newIndex = newIndex + (oldIndex < newIndex ? -1 : 0);
		}

		public override void Undo()
		{
			Map.Region area = form.areaList[newIndex];

			form.lbAreas.SelectedIndex = -1;
			form.areaList.RemoveAt(newIndex);
			form.areaList.Insert(oldIndex, area);

			form.RebuildAreas();

			form.lbAreas.SelectedIndex = oldIndex;
			form.ScrollToIndex(form.lbAreas, oldIndex);
		}

		public override void Redo()
		{
			Map.Region area = form.areaList[oldIndex];

			form.lbAreas.SelectedIndex = -1;
			form.areaList.RemoveAt(oldIndex);
			form.areaList.Insert(newIndex, area);

			form.RebuildAreas();

			form.lbAreas.SelectedIndex = newIndex;
			form.ScrollToIndex(form.lbAreas, newIndex);
		}
	}

	// if old index is less than new index, new index will
	// need to be shifted back 1 to account for the removal
	// regions are saved in a dictionary, so does order matter?
	public class ReorderRegionAction : MainFormUndoAction
	{
		private readonly int oldIndex;
		private readonly int newIndex;

		public ReorderRegionAction(MainForm form, int oldIndex, int newIndex, bool inverted) : base(form, inverted)
		{
			this.oldIndex = oldIndex;
			this.newIndex = newIndex + (oldIndex < newIndex ? -1 : 0);
		}

		public override void Undo()
		{
			Map.Region region = form.regionList[newIndex];

			form.lbRegions.SelectedIndex = -1;
			form.regionList.RemoveAt(newIndex);
			form.regionList.Insert(oldIndex, region);

			foreach (Map.Region mapRegion in form.regionList)
				mapRegion.connections.Sort(form.CompareConnections);

			form.RebuildRegions();
			form.SortConnections();
			form.SortRegionSelect();

			form.lbRegions.SelectedIndex = oldIndex;
			form.ScrollToIndex(form.lbRegions, oldIndex);
		}

		public override void Redo()
		{
			Map.Region region = form.regionList[oldIndex];

			form.lbRegions.SelectedIndex = -1;
			form.regionList.RemoveAt(oldIndex);
			form.regionList.Insert(newIndex, region);

			foreach (Map.Region mapRegion in form.regionList)
				mapRegion.connections.Sort(form.CompareConnections);

			form.RebuildRegions();
			form.SortConnections();
			form.SortRegionSelect();

			form.lbRegions.SelectedIndex = newIndex;
			form.ScrollToIndex(form.lbRegions, newIndex);
		}
	}

	#endregion
}
