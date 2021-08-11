using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSScriptLib;

namespace RavEngine {
	public class Scene {
		internal SceneNode TopNode { get; }
		private Dictionary<Type, List<Node>> cachedNodes;
		private Dictionary<string, List<Node>> cachedTags;
		private List<Server> servers;

		public Scene() {
			this.servers = new List<Server>();
			this.cachedNodes = new Dictionary<Type, List<Node>>();
			this.cachedTags = new Dictionary<string, List<Node>>();
			CSScript.EvaluatorConfig.DebugBuild = true;

			Assembly assembly = CSScript.Evaluator.ReferenceDomainAssemblies().CompileCode(@"
				using System;
				using RavEngine;
				using RavUtilities;
					public class SceneNode : Node {
						public float x = 1f;
						public float y = 5f;
						public float y2 = 5f;
						public float y3 = 5f;
						public float y4 = 5f;
						public float y5 = 5f;
						public float y6 = 5f;
						public float y7 = 5f;
						public float y8 = 5f;
						public float y9 = 5f;

						public override void VariableUpdate() {
							Console.WriteLine(""What "" + this.x + "" "" + this.y);
							this.Testa();
						}

						public override void FixedUpdate() {
						}

						public override void Start() {
						}

						public override void Stop() {
						}

						private unsafe void Testa(){
							this.x++;
							this.y++;
							this.y2++;
							this.y3++;
							this.y4++;
							this.y5++;
							this.y6++;
							this.y7++;
							this.y8++;
							this.y9++;
						}
					}");

			SceneNode i = (SceneNode) assembly.CreateInstance("SceneNode");

			//evaluator.CompileAssemblyFromFile()
			object instance = CSScript.Evaluator.ReferenceDomainAssemblies().LoadCode(@"
				using System;
				using RavEngine;
				using RavUtilities;
					public class SceneNode2 : SceneNode {
						public float y = 5f;
						public float y2 = 5f;
						public float y3 = 5f;
						public float y4 = 5f;
						public float y5 = 5f;
						public float y6 = 5f;
						public float y7 = 5f;
						public float y8 = 5f;
						public float y9 = 5f;

						public override void VariableUpdate() {
							Console.WriteLine(""What "" + base.x + "" "" + this.y);
							this.Testa();
						}

						public override void FixedUpdate() {
						}

						public override void Start() {
						}

						public override void Stop() {
						}

						private unsafe void Testa(){
							base.x++;
							this.y++;
							this.y2++;
							this.y3++;
							this.y4++;
							this.y5++;
							this.y6++;
							this.y7++;
							this.y8++;
							this.y9++;
						}
					}");

			this.TopNode = (SceneNode?) instance;
			//ReflectionU.OverrideMethod(this.TopNode.GetType().GetMethod("VariableUpdate"), instance.GetType().GetMethod("VariableUpdate"));
			//this.TopNode.TransmuteTo(instance);
			//this.TopNode.GetType().GetField("y").SetValue(this.TopNode, 2);

			if (this.TopNode == instance) {
				Console.WriteLine("Same");
			}

			if (this.TopNode.GetType() == instance.GetType()) {
				Console.WriteLine("Same");
			}

			if (typeof(SceneNode) == this.TopNode.GetType()) {
				Console.WriteLine("Same");
			}

			if (this.Compare<SceneNode>(this.TopNode.GetType())) {
				Console.WriteLine("Same");
			}
		}

		public bool Compare<A>(Type type) { return typeof(A) == type; }

		internal void Update() {
			this.TopNode.VariableUpdate();

			for (int i = this.servers.Count - 1; i >= 0; i--) {
				this.servers[i].Before();
			}

			for (int i = 0; i < Engine.Time.FixedTimeStepUpdates; i++) {
				foreach (List<Node> nodes in this.cachedNodes.Values) {
					for (int j = nodes.Count - 1; j >= 0; j--) {
						nodes[j].FixedUpdate();
					}
				}
			}

			foreach (List<Node> nodes in this.cachedNodes.Values) {
				for (int i = nodes.Count - 1; i >= 0; i--) {
					nodes[i].VariableUpdate();
				}
			}

			for (int i = this.servers.Count - 1; i >= 0; i--) {
				this.servers[i].After();
			}
		}

		internal void Reset() { }

		public List<Node> QueryNodes(Func<Node, bool> predicate) {
			List<Node> queriedNodes = new List<Node>();
			foreach (List<Node> nodes in this.cachedNodes.Values) {
				queriedNodes.AddRange(nodes.Where(predicate) as List<Node>);
			}

			return queriedNodes;
		}

		public List<T> QueryNodes<T>(Func<T, bool> predicate) where T : Node {
			List<T> queriedNodes = new List<T>();
			List<T> typedNodes = this.cachedNodes[typeof(T)] as List<T>;
			queriedNodes.AddRange(typedNodes.Where(predicate) as List<T>);

			return queriedNodes;
		}

		public List<Node> QueryNodes(string tag, Func<Node, bool> predicate) {
			List<Node> queriedNodes = new List<Node>();
			queriedNodes.AddRange(this.cachedTags[tag].Where(predicate) as List<Node>);

			return queriedNodes;
		}

		public List<T> QueryNodes<T>(string tag, Func<T, bool> predicate) where T : Node {
			List<T> queriedNodes = new List<T>();
			List<T> typedNodes = this.cachedNodes[typeof(T)] as List<T>;
			queriedNodes.AddRange(typedNodes.Where(x => x.HasTag(tag) && predicate.Invoke(x)) as List<T>);

			return queriedNodes;
		}

		internal void AddCacheNode(Node node) {
			if (!this.cachedNodes.ContainsKey(node.GetType())) {
				this.cachedNodes[node.GetType()] = new List<Node>();
			}
			this.cachedNodes[node.GetType()].Add(node);
		}

		internal void RemoveCacheNode(Node node) {
			if (!this.cachedNodes.ContainsKey(node.GetType())) {
				return;
			}
			this.cachedNodes[node.GetType()].Remove(node);
		}

		internal void AddCacheTag(string tag, Node node) {
			if (!this.cachedTags.ContainsKey(tag)) {
				this.cachedTags[tag] = new List<Node>();
			}
			this.cachedTags[tag].Add(node);
		}

		internal void RemoveCacheTag(string tag, Node node) {
			if (!this.cachedTags.ContainsKey(tag)) {
				return;
			}
			if (this.cachedTags[tag].Contains(node)) {
				this.cachedTags[tag].Remove(node);
			}
		}

		public void RemoveCacheTags(Node node) {
			foreach (string tag in node.GetTags()) {
				this.RemoveCacheTag(tag, node);
			}
		}

		public Server GetServer(Type type) {
			if (!this.HasServer(type)) {
				throw new Exception("Server Doesnt Exist");
			}
			return this.servers.Find(x => x.GetType() == type) ?? throw new InvalidOperationException();
		}

		public T GetServer<T>() where T : Server {
			if (!this.HasServer<T>()) {
				throw new Exception("Server Doesnt Exist");
			}
			return (T) this.servers.Find(x => x.GetType() == typeof(T))!;
		}

		public bool HasServer(Type type) { return this.servers.Exists(x => x.GetType() == type); }

		public bool HasServer<T>() where T : Server { return this.servers.Exists(x => x.GetType() == typeof(T)); }

		public void AddServer(Server server) {
			if (this.HasServer(server.GetType())) {
				throw new Exception("Server Already Added");
			}
			this.servers.Add(server);
		}

		public void AddServer<T>() where T : Server, new() {
			if (this.HasServer<T>()) {
				throw new Exception("Server Already Added");
			}
			this.servers.Add(new T());
		}

		public void RemoveServer(Server server) {
			if (!this.HasServer(server.GetType())) {
				throw new Exception("Server Doesnt Exist");
			}
			this.servers.Remove(server);
		}

		public void RemoveServer<T>() where T : Server, new() {
			if (!this.HasServer<T>()) {
				throw new Exception("Server Doesnt Exist");
			}
			int index = this.servers.FindIndex(x => x.GetType() == typeof(T));
			this.servers.RemoveAt(index);
		}
	}

	public class SceneNode : Node {
		public float x = 1f;

		public override void VariableUpdate() { }

		public override void FixedUpdate() { }

		public override void Start() { }

		public override void Stop() { }
	}

	public class SceneNode2 : SceneNode {
		public new float x = 1f;

		public override void VariableUpdate() { }

		public override void FixedUpdate() { }

		public override void Start() { }

		public override void Stop() { }
	}
}