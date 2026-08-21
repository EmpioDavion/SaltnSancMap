using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaltMap;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
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

		public Point mousePos;
		private Point lastMousePos;
		public Map map;

		public Map.Region currentRegion;
		public Map.Connection currentConnection;

		public readonly List<Map.Region> regionList = new List<Map.Region>();

		public readonly List<string> progressList = new List<string>();

		public Microsoft.Xna.Framework.Graphics.SpriteFont spriteFont;

		private Action editAction;

		private Drag regionDrag; // drag action for reordering regions

		// connections are sorted by region
		// locations are sorted by check id
		// items are sorted by name

		public MainForm()
		{
			InitializeComponent();

			pnlMap.KeyDown += pnlMap_KeyDown;
			pnlMap.MouseWheel += pnlMap_MouseWheel;
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

			currentRegion = regionList[0];

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

		private void EditRegion()
		{
			int index = lbRegions.SelectedIndex;
			Map.Region region = regionList[index];
			string oldItem = region.key;
			string newItem = tbEdit.Text;
			Track(new RenameRegionAction(this, region, oldItem, newItem, index, false));
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
				Track(new ChangeCheckRegionAction(this, check, currentRegion, false));
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

			// if the flag is already set, this is a duplicate
			// if the flag is a check, should be toggling the check instead
			if (map.HasFlag(newItem) || map.GetLocation(newItem, out _))
				return;

			Track(new RenameProgressAction(this, oldItem, newItem, false));
		}

		internal int CompareConnections(Map.Connection a, Map.Connection b)
		{
			return regionList.IndexOf(a.Region) - regionList.IndexOf(b.Region);
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

		internal void SortLocations()
		{
			lbLocations.Items.Clear();

			currentRegion.checks.Sort(CompareChecks);

			foreach (string checkName in currentRegion.checks)
			{
				map.GetLocation(checkName, out Map.Check check);
				lbLocations.Items.Add(check);
			}
		}

		internal void SortItems()
		{
			lbItems.Items.Clear();

			currentConnection.items.Sort();

			foreach (string item in currentConnection.items)
				lbItems.Items.Add(item);
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

			map.Filter(currentRegion, check);
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

		#region Events

		private void MainForm_Click(object sender, EventArgs e)
		{
			lbSelectRegion.Visible = false;
			lbSelectCheck.Visible = false;
			tbEdit.Visible = false;
		}

		#region pnlMap

		private void pnlMap_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Middle)
			{
				if (map.GetCheck(e.Location, out Map.Check check))
					Track(new ToggleCheckAction(this, check, false));
			}
			else if (e.Button == MouseButtons.Right)
			{
				if (currentRegion != null)
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

		#region lbRegions

		private void lbRegions_Click(object sender, EventArgs e)
		{
			int i = lbRegions.SelectedIndex;
			currentRegion = i >= 0 ? regionList[i] : null;
			map.Filter(currentRegion, null);
			
			lbConnections.SelectedIndex = -1;
			lbConnections.Items.Clear();
			lbLocations.Items.Clear();

			if (currentRegion != null)
			{
				foreach (Map.Connection connection in currentRegion.connections)
					lbConnections.Items.Add(connection);

				foreach (string checkName in currentRegion.checks)
				{
					map.GetLocation(checkName, out Map.Check check);
					lbLocations.Items.Add(check);
				}

				if (lbLocations.Items.Count > 0)
					FocusCheck((Map.Check)lbLocations.Items[0]);
			}
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
					Track(new AddRegionAction(this, currentRegion, index, true));
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
				if (currentConnection != null)
					Track(new AddConnectionAction(this, currentRegion, currentConnection, true));
			}
		}

		#endregion

		#region lbLocations

		// locations are auto sorted by id

		private void lbLocations_Click(object sender, EventArgs e)
		{
			Map.Check check = (Map.Check)lbLocations.SelectedValue;

			FocusCheck(check);
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
					e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete;

				validKey &= !e.Control; // control is not pressed

				e.SuppressKeyPress = !validKey;
				e.Handled = !validKey;
			}
		}

		#endregion

		#region Buttons

		private void btnClearFilter_Click(object sender, EventArgs e)
		{
			map.Filter(null, null);
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

			if (currentRegion != null)
				i = regionList.IndexOf(currentRegion);

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

		private void btnSave_Click(object sender, EventArgs e)
		{
			map.Save();
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
			form.map.RemoveFlag(check.key);
		}

		public override void Redo()
		{
			check.checkState = newCheckState;
			form.map.AddFlag(check.key);
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
				form.SortLocations();

				if (newValue == form.currentRegion)
				{
					int index = form.lbLocations.Items.IndexOf(check);
					form.lbLocations.SelectedIndex = index;

					form.ScrollToIndex(form.lbLocations, index);
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
			form.SortProgress();

			form.map.UpdateAvailable();
		}

		public override void Redo()
		{
			form.progressList.Add(progress);
			form.SortProgress();

			form.map.UpdateAvailable();
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

			form.SortProgress();

			form.map.RemoveFlag(oldValue);
			form.map.AddFlag(newValue);

			form.map.UpdateAvailable();
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
