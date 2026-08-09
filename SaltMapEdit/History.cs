using System.Collections.Generic;

namespace SaltMapEdit
{
	public abstract class UndoAction
	{
		// reverses undo and redo actions
		public readonly bool inverted;

		public abstract void Undo();

		public abstract void Redo();

		public void RunUndo()
		{
			if (inverted)
				Redo();
			else
				Undo();
		}

		public void RunRedo()
		{
			if (inverted)
				Undo();
			else
				Redo();
		}

		public UndoAction(bool inverted)
		{
			this.inverted = inverted;
		}
	}

	internal static class History
	{
		private static readonly Stack<UndoAction> UndoActions = new Stack<UndoAction>();
		private static readonly Stack<UndoAction> RedoActions = new Stack<UndoAction>();

		public static bool CanUndo => UndoActions.Count > 0;
		public static bool CanRedo => RedoActions.Count > 0;

		public static void Add(UndoAction action, bool runAction)
		{
			RedoActions.Clear();
			UndoActions.Push(action);

			if (runAction)
				action.RunRedo();
		}

		public static bool Undo()
		{
			if (UndoActions.Count > 0)
			{
				UndoAction action = UndoActions.Pop();
				action.RunUndo();
				RedoActions.Push(action);

				return true;
			}

			return false;
		}

		public static bool Redo()
		{
			if (RedoActions.Count > 0)
			{
				UndoAction action = RedoActions.Pop();
				action.RunRedo();
				UndoActions.Push(action);

				return true;
			}

			return false;
		}
	}
}
