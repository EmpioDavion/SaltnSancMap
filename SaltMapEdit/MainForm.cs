using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaltMap;
using System.Collections.Generic;
using System.ComponentModel;
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

				return System.Math.Abs(dragDist.X) > SystemInformation.DragSize.Width ||
					System.Math.Abs(dragDist.Y) > SystemInformation.DragSize.Height;
			}
		}

		public Point mousePos;
		private Point lastMousePos;
		public Map map;

		public Map.Region currentRegion;
		public Map.Connection currentConnection;

		public readonly BindingList<Map.Region> regionList = new BindingList<Map.Region>();
		public readonly BindingList<Map.Connection> connectionList = new BindingList<Map.Connection>();
		public readonly BindingList<Map.Check> locationList = new BindingList<Map.Check>();
		public readonly BindingList<string> itemList = new BindingList<string>();

		public readonly BindingList<Map.Region> regionSelect = new BindingList<Map.Region>();

		public readonly List<string> progressList = new List<string>();

		private BindingList<Map.Check> stackedChecks = new BindingList<Map.Check>();

		public Microsoft.Xna.Framework.Graphics.SpriteFont spriteFont;

		private System.Action editAction;

		private Drag regionDrag; // drag action for reordering regions

		// connections are sorted by region
		// locations are sorted by check id
		// items are sorted by name

		public MainForm()
		{
			InitializeComponent();
		}

		public void MapLoaded()
		{
			foreach (Map.Region region in map.regions.Values)
			{
				regionList.Add(region);
				regionSelect.Add(region);
			}

			lbRegions.DataSource = regionList;
			lbConnections.DataSource = connectionList;
			lbLocations.DataSource = locationList;
			lbItems.DataSource = itemList;

			lbSelectRegion.DataSource = regionSelect;
			lbSelectCheck.DataSource = stackedChecks;

			map.saveRegions = SaveRegions;
		}

		private bool SaveRegions()
		{
			OrderedDictionary<string, Map.Region> regions = new OrderedDictionary<string, Map.Region>();

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

		private void ShowEdit(Rectangle rect, string text, System.Action editAction)
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
			int index = regionSelect.IndexOf(region);
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
			Map.Connection connection = (Map.Connection)lbConnections.SelectedValue;
			Map.Region newRegion = regionSelect[lbSelectRegion.SelectedIndex];
			Track(new ChangeConnectionAction(this, connection, newRegion, false));
		}

		private void EditCheck()
		{
			Map.Check check = (Map.Check)lbSelectCheck.SelectedValue;
			Track(new ChangeCheckRegionAction(this, check, currentRegion, false));
		}

		private void EditItem()
		{
			int index = lbItems.SelectedIndex;
			string oldItem = itemList[index];
			string newItem = tbEdit.Text;

			if (itemList.Contains(newItem))
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

		internal void SortConnections()
		{
			lbConnections.DataSource = null;

			connectionList.Clear();

			currentRegion.connections.Sort(CompareConnections);

			foreach (Map.Connection connection in currentRegion.connections)
				connectionList.Add(connection);

			lbConnections.DataSource = connectionList;
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
			lbLocations.DataSource = null;

			locationList.Clear();

			currentRegion.checks.Sort(CompareChecks);

			foreach (string checkName in currentRegion.checks)
			{
				map.GetLocation(checkName, out Map.Check check);
				locationList.Add(check);
			}

			lbLocations.DataSource = locationList;
		}

		internal void SortItems()
		{
			lbItems.DataSource = null;

			itemList.Clear();

			currentConnection.items.Sort();

			foreach (string item in currentConnection.items)
				itemList.Add(item);

			lbItems.DataSource = itemList;
		}

		internal void SortProgress()
		{
			progressList.Sort();

			lbProgress.Items.Clear();

			foreach (string progress in progressList)
				lbProgress.Items.Add(progress);
		}

		#region Events

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

					lbSelectCheck.DataSource = null;
					stackedChecks.Clear();

					foreach (Map.Check check in checkGroup.checks)
						if (check.Region != currentRegion)
							stackedChecks.Add(check);

					lbSelectCheck.DataSource = stackedChecks;

					if (stackedChecks.Count > 0)
					{
						if (stackedChecks.Count == 1)
						{
							Map.Check check = stackedChecks[0];
							Map.Region oldRegion = check.Region;

							if (oldRegion != currentRegion)
								Track(new ChangeCheckRegionAction(this, check, currentRegion, false));
						}
						else
						{
							Microsoft.Xna.Framework.Vector2 size = default;

							foreach (Map.Check check in stackedChecks)
							{
								Microsoft.Xna.Framework.Vector2 size2 = spriteFont.MeasureString(check.key);
								size.X = System.Math.Max(size.X, size2.X);
							}

							size.Y = lbSelectCheck.ItemHeight * stackedChecks.Count;

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
				System.Console.WriteLine(e.Delta);

			map.Zoom(mousePos, e.Delta > 0 ? 1 : -1);
		}

		private void pnlMap_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.M)
				map.ToggleMode();
		}

		#endregion

		#region lbRegions

		private void lbRegions_SelectedValueChanged(object sender, System.EventArgs e)
		{
			int i = lbRegions.SelectedIndex;
			currentRegion = i >= 0 ? regionList[i] : null;
			map.Filter(currentRegion, null);

			lbConnections.SelectedIndex = -1;
			lbConnections.DataSource = null;
			connectionList.Clear();
			lbLocations.DataSource = null;
			locationList.Clear();

			if (currentRegion != null)
			{
				foreach (Map.Connection connection in currentRegion.connections)
					connectionList.Add(connection);

				foreach (string checkName in currentRegion.checks)
				{
					map.GetLocation(checkName, out Map.Check check);
					locationList.Add(check);
				}
			}

			lbConnections.DataSource = connectionList;
			lbLocations.DataSource = locationList;
		}

		private void lbRegions_DoubleClick(object sender, System.EventArgs e)
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
			if (lbRegions.SelectedItem != null)
			{
				regionDrag.state = DragState.DragStart;
				regionDrag.startPoint = new Point(e.X, e.Y);
			}
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

		private void lbConnections_SelectedValueChanged(object sender, System.EventArgs e)
		{
			Map.Connection connection = (Map.Connection)lbConnections.SelectedValue;
			currentConnection = connection;

			lbItems.DataSource = null;
			itemList.Clear();

			if (connection != null)
				foreach (string item in connection.items)
					itemList.Add(item);

			lbItems.DataSource = itemList;
		}

		private void lbConnections_DoubleClick(object sender, System.EventArgs e)
		{
			int index = lbConnections.SelectedIndex;

			if (index >= 0)
			{
				Rectangle rect = lbConnections.GetItemRectangle(index);
				rect.Location = lbConnections.PointToScreen(rect.Location);
				rect.Location = PointToClient(rect.Location);
				ShowRegions(rect, connectionList[index].Region);
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

		private void lbLocations_SelectedValueChanged(object sender, System.EventArgs e)
		{
			Map.Check check = (Map.Check)lbLocations.SelectedValue;

			if (check != null && map.drawMode == Map.DrawMode.Zoomable)
				map.SetCameraPosition(check.loc);

			map.Filter(currentRegion, check);
		}

		#endregion

		#region lbItems

		private void lbItems_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete && currentConnection != null)
			{
				int i = lbItems.SelectedIndex;

				Track(new AddItemAction(this, currentConnection, itemList[i], true));
			}
		}

		private void lbItems_DoubleClick(object sender, System.EventArgs e)
		{
			int index = lbItems.SelectedIndex;

			if (index >= 0)
			{
				Rectangle rect = lbItems.GetItemRectangle(index);
				rect.Location = lbItems.PointToScreen(rect.Location);
				rect.Location = PointToClient(rect.Location);
				ShowEdit(rect, itemList[index], EditItem);
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

		private void lbProgress_DoubleClick(object sender, System.EventArgs e)
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

		private void lbSelectRegion_Leave(object sender, System.EventArgs e)
		{
			lbSelectRegion.Visible = false;
		}

		private void lbSelectRegion_KeyDown(object sender, KeyEventArgs e)
		{

		}

		private void lbSelectRegion_SelectedIndexChanged(object sender, System.EventArgs e)
		{

		}

		private void lbSelectRegion_Click(object sender, System.EventArgs e)
		{
			EditConnection();
			lbSelectRegion.Visible = false;
			Focus();
		}

		#endregion

		#region lbSelectCheck

		private void lbSelectCheck_Leave(object sender, System.EventArgs e)
		{
			lbSelectCheck.Visible = false;
		}

		private void lbSelectCheck_KeyDown(object sender, KeyEventArgs e)
		{

		}

		private void lbSelectCheck_SelectedIndexChanged(object sender, System.EventArgs e)
		{

		}

		private void lbSelectCheck_Click(object sender, System.EventArgs e)
		{
			EditCheck();
			lbSelectCheck.Visible = false;
			Focus();
		}

		#endregion

		#region tbEdit

		private void tbEdit_Leave(object sender, System.EventArgs e)
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

		private void btnClearFilter_Click(object sender, System.EventArgs e)
		{
			map.Filter(null, null);
		}

		private void btnAddRegion_Click(object sender, System.EventArgs e)
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

			int i = map.regions.Count;
			Track(new AddRegionAction(this, region, i, false));

			lbRegions.SelectedIndex = regionList.Count - 1;
		}

		private void btnAddConnection_Click(object sender, System.EventArgs e)
		{
			if (currentRegion != null)
			{
				Map.Connection connection = new Map.Connection()
				{
					Region = regionSelect[0]
				};

				Track(new AddConnectionAction(this, currentRegion, connection, false));
			}
		}

		private void btnAddItem_Click(object sender, System.EventArgs e)
		{
			if (currentConnection != null)
				Track(new AddItemAction(this, currentConnection, "new_item", false));
		}

		private void btnAddProgress_Click(object sender, System.EventArgs e)
		{
			Track(new AddProgressAction(this, "new_progress", false));
		}

		private void btnSave_Click(object sender, System.EventArgs e)
		{
			map.Save();
		}

		private void btnUndo_Click(object sender, System.EventArgs e)
		{
			History.Undo();

			btnUndo.Enabled = History.CanUndo;
			btnRedo.Enabled = History.CanRedo;
		}

		private void btnRedo_Click(object sender, System.EventArgs e)
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
					form.lbLocations.SelectedIndex = form.locationList.IndexOf(check);
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
			form.regionSelect.RemoveAt(index);
			form.map.regions.Remove(region.key);
		}

		public override void Redo()
		{
			form.regionList.Insert(index, region);
			form.regionSelect.Insert(index, region);
			form.map.regions.Add(region.key, region);
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
			form.regionSelect[index] = region;
		}

		public override void Redo()
		{
			region.key = newItem;
			form.map.regions.Remove(oldItem);
			form.map.regions.Add(newItem, region);
			form.regionList[index] = region;
			form.regionSelect[index] = region;
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

			form.lbRegions.DataSource = null;

			form.regionList.RemoveAt(newIndex);
			form.regionList.Insert(oldIndex, region);

			form.lbRegions.DataSource = form.regionList;
			form.lbRegions.SelectedIndex = oldIndex;
		}

		public override void Redo()
		{
			Map.Region region = form.regionList[oldIndex];

			form.lbRegions.DataSource = null;

			form.regionList.RemoveAt(oldIndex);
			form.regionList.Insert(newIndex, region);

			form.lbRegions.DataSource = form.regionList;
			form.lbRegions.SelectedIndex = newIndex;
		}
	}

	#endregion
}
