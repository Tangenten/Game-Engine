using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RavEngine {
	public class ThreadingE : EngineCore {
		internal bool MultiThreaded { get; set; }

		public ThreadingE() {
		}

		internal override void Start() {
			this.MultiThreaded = true;
		}

		internal override void Stop() {
		}

		internal override void Update() {
		}

		internal override void Reset() {
		}

		public Task LaunchTask(Action action) {
			Task task = new Task(action);
			if (this.MultiThreaded) {
				task.Start();
			} else {
				task.RunSynchronously();
			}
			return task;
		}

		public Task[] LaunchTasks(params Action[] actions) {
			Task[] tasks = new Task[actions.Length];
			for (int i = 0; i < actions.Length; i++) {
				tasks[i] = this.LaunchTask(actions[i]);
			}

			return tasks;
		}

		public Task LaunchTask(Action action, Action continueWith, bool runContinueAsync = true) {
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
			return task;
		}

		public void LaunchTasksAwait(Action[] actions, Action? after = null) {
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
				LaunchTask(() => {
					Parallel.For(start, end, action);
				});
			} else {
				for (int i = start; i < end; i++) {
					action.Invoke(i);
				}
			}
		}

		public void For(int start, int end, Action<Tuple<int, int>> action) {
			if (this.MultiThreaded) {
				LaunchTask(() => {
					Parallel.ForEach(Partitioner.Create(start, end), action);
				});
			} else {
				action.Invoke(new Tuple<int, int>(start, end));
			}
		}

		public void For(int start, int end, int range, Action<Tuple<int, int>> action) {
			if (this.MultiThreaded) {
				LaunchTask(() => {
					Parallel.ForEach(Partitioner.Create(start, end, range), action);
				});
			} else {
				action.Invoke(new Tuple<int, int>(start, end));
			}
		}

		public void ForAwait(int start, int end, Action<int> action) {
			if (this.MultiThreaded) {
				Parallel.For(start, end, action);
			} else {
				for (int i = start; i < end; i++) {
					action.Invoke(i);
				}
			}
		}

		public void ForAwait(int start, int end, Action<Tuple<int, int>> action) {
			if (this.MultiThreaded) {
				Parallel.ForEach(Partitioner.Create(start, end), action);
			} else {
				action.Invoke(new Tuple<int, int>(start, end));
			}
		}

		public void ForAwait(int start, int end, int range, Action<Tuple<int, int>> action) {
			if (this.MultiThreaded) {
				Parallel.ForEach(Partitioner.Create(start, end, range), action);
			} else {
				action.Invoke(new Tuple<int, int>(start, end));
			}
		}
	}
}
