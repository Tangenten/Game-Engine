using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RavEngine {
	public class ThreadingE : EngineCore {
		[ConsoleCommand("ASYNC_JOBS")]
		internal bool MultiThreaded { get; set; }
		[ConsoleCommand("LOG_JOBS")]
		internal bool LogJobs { get; set; }
		private List<IJob> activeJobs;

		public ThreadingE() {
			this.MultiThreaded = true;
			this.activeJobs = new List<IJob>();
		}

		internal override void Start() { }

		internal override void Stop() { }

		internal override void Update() {
			for (int i = this.activeJobs.Count - 1; i >= 0; i--) {
				this.activeJobs[i].Update();
			}
		}

		internal override void Reset() { this.MultiThreaded = true; }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Job LaunchJob(Action action) {
			Task task = new Task(action);
			if (this.MultiThreaded) {
				task.Start();
			} else {
				task.RunSynchronously();
			}
			return new Job(task);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Job LaunchJob<T>(Func<T> action) {
			Task<T> task = new Task<T>(action);
			if (this.MultiThreaded) {
				task.Start();
			} else {
				task.RunSynchronously();
			}
			return new Job(task);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Job LaunchJob(Action action, Action continueWith, bool runContinueAsync = true) {
			Task task;
			if (this.MultiThreaded) {
				if (runContinueAsync) {
					task = new Task(action, TaskCreationOptions.RunContinuationsAsynchronously);
					task.ContinueWith(conTask => { continueWith.Invoke(); }, TaskContinuationOptions.RunContinuationsAsynchronously);
					task.Start();
				} else {
					task = new Task(action, TaskCreationOptions.None);
					task.ContinueWith(conTask => { continueWith.Invoke(); }, TaskContinuationOptions.ExecuteSynchronously);
					task.Start();
				}
			} else {
				task = new Task(action, TaskCreationOptions.None);
				task.ContinueWith(conTask => { continueWith.Invoke(); }, TaskContinuationOptions.ExecuteSynchronously);
				task.RunSynchronously();
			}

			return new Job(task);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void LaunchJobsAwait(Action[] actions, Action? after = null) {
			if (this.MultiThreaded) {
				Parallel.Invoke(actions);
			} else {
				for (int i = 0; i < actions.Length; i++) {
					actions[i].Invoke();
				}
			}

			after?.Invoke();
		}

		public void For(int start, int end, Action<int> action) {
			if (this.MultiThreaded) {
				this.LaunchJob(() => { Parallel.For(start, end, action); });
			} else {
				for (int i = start; i < end; i++) {
					action.Invoke(i);
				}
			}
		}

		public void For(int start, int end, Action<Tuple<int, int>> action) {
			if (this.MultiThreaded) {
				this.LaunchJob(() => { Parallel.ForEach(Partitioner.Create(start, end), action); });
			} else {
				action.Invoke(new Tuple<int, int>(start, end));
			}
		}

		public void For(int start, int end, int range, Action<Tuple<int, int>> action) {
			if (this.MultiThreaded) {
				this.LaunchJob(() => { Parallel.ForEach(Partitioner.Create(start, end, range), action); });
			} else {
				action.Invoke(new Tuple<int, int>(start, end));
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ForAwait(int start, int end, Action<int> action) {
			if (this.MultiThreaded) {
				Parallel.For(start, end, action);
			} else {
				for (int i = start; i < end; i++) {
					action.Invoke(i);
				}
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ForAwait(int start, int end, Action<Tuple<int, int>> action) {
			if (this.MultiThreaded) {
				Parallel.ForEach(Partitioner.Create(start, end), action);
			} else {
				action.Invoke(new Tuple<int, int>(start, end));
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ForAwait(int start, int end, int range, Action<Tuple<int, int>> action) {
			if (this.MultiThreaded) {
				Parallel.ForEach(Partitioner.Create(start, end, range), action);
			} else {
				action.Invoke(new Tuple<int, int>(start, end));
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void AddJob(IJob job) {
			if (this.LogJobs) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Added Job: " + job.Name()));
			}
			this.activeJobs.Add(job);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void RemoveJob(IJob job) {
			if (this.LogJobs) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Finished Job: " + job.Name()));
			}
			this.activeJobs.Remove(job);
		}

		[ConsoleCommand("LIST_JOBS")] [MethodImpl(MethodImplOptions.Synchronized)]
		internal void ListJobs() {
			for (int i = this.activeJobs.Count - 1; i >= 0; i--) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Debug(this.activeJobs[i].Name()));
			}
		}
	}

	public interface IJob {
		internal string Name();
		internal void Update();
	}

	public class Job : IJob {
		private Task task;
		public bool Finished => this.task.IsCompleted;

		public Job(Task t) {
			this.task = t;
			Engine.Threading.AddJob(this);
		}

		void IJob.Update() {
			if (this.Finished) {
				Engine.Threading.RemoveJob(this);
			}
		}

		public void Wait() { this.task.Wait(); }

		string IJob.Name() { return this.task.ToString(); }
	}

	public class Job<T> : IJob {
		private Task<T> task;
		public bool Finished => this.task.IsCompleted;

		public Job(Task<T> t) {
			this.task = t;
			Engine.Threading.AddJob(this);
		}

		void IJob.Update() {
			if (this.Finished) {
				Engine.Threading.RemoveJob(this);
			}
		}

		public void Wait() { this.task.Wait(); }

		public bool TryGetResult(out T result) {
			if (this.Finished) {
				result = this.task.Result;
				return true;
			}

			result = default;
			return false;
		}

		string IJob.Name() { return this.task.ToString(); }
	}
}