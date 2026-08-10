using System.Collections.Generic;
using System.Diagnostics;

namespace Profiler
{
	public static class Profiler
	{
		private static readonly Dictionary<string, Profile> Profiles = new Dictionary<string, Profile>();

		private static readonly Stack<Profile> ProfileStack = new Stack<Profile>();

		private static readonly Stopwatch Timer = Stopwatch.StartNew();

		public static Profile Start(string name)
		{
#if DEBUG

			if (!Profiles.TryGetValue(name, out Profile profile))
				Profiles.Add(name, profile = new Profile(name));

			profile.startTime = GetCurrentTime();
			profile.endTime = 0;

			ProfileStack.Push(profile);

#endif

			return profile;
		}

		public static void End(Profile profile)
		{
#if DEBUG

			if (ProfileStack.Count == 0)
				throw new System.Exception("Profile stack is empty");
			else if (ProfileStack.Peek() != profile)
				throw new System.ArgumentException("Profile is not on top of stack");

			ProfileStack.Pop();

			profile.endTime = GetCurrentTime();

			long time = profile.endTime - profile.startTime;
			profile.min = System.Math.Min(profile.min, time);
			profile.max = System.Math.Max(profile.max, time);

			profile.count++;
			profile.total += time;

#endif
		}

		public static long GetCurrentTime() => Timer.ElapsedMilliseconds;
	}

	public class Profile
	{
		public readonly string name;
		public long startTime;
		public long endTime;
		public long min = long.MaxValue;
		public long max = -1;

		public long total = 0;
		public long count = 0;

		public long Average => total / System.Math.Max(1, count);

		public Profile(string name) => this.name = name;

		public override string ToString() => $"{name} - {max}";
	}
}
