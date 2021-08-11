using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RavEngine {
	public abstract class Node {
		private Node? parent;
		private List<Node> children = new List<Node>();
		private List<string> tags = new List<string>();

		public abstract void VariableUpdate();
		public abstract void FixedUpdate();
		public abstract void Start();
		public abstract void Stop();

		public bool Is<T>() { return this.GetType() == typeof(T); }

		public bool Is(Type type) { return this.GetType() == type; }

		public bool Is(Node node) { return this == node; }

		public bool HasParent() { return this.parent != null; }

		public bool HasParent<T>() where T : Node { return this.HasParent() && this.parent.Is<T>(); }

		public bool HasParent(Predicate<Node> predicate) { return this.HasParent() && predicate.Invoke(this.parent); }

		public bool HasParent<T>(Predicate<Node> predicate) where T : Node { return this.HasParent<T>() && predicate.Invoke(this.parent as T); }

		public bool HasChild(Func<Node, bool> predicate) { return this.children.Any(predicate); }

		public bool HasChild<T>() where T : Node { return this.children.Any(x => x.Is<T>()); }

		public bool HasChild<T>(Func<T, bool> predicate) where T : Node { return this.children.Any(x => x.Is<T>() && predicate.Invoke(x as T)); }

		public bool HasChildren() { return !this.children.IsEmpty(); }

		public bool HasChildrenAny(Func<Node, bool> predicate) { return this.children.Any(predicate); }

		public bool HasChildrenAny<T>(Func<T, bool> predicate) where T : Node { return this.children.Any(x => x.Is<T>() && predicate.Invoke(x as T)); }

		public bool HasChildrenAll(Func<Node, bool> predicate) { return this.children.All(predicate); }

		public bool HasChildrenAll<T>(Func<T, bool> predicate) where T : Node { return this.children.All(x => x.Is<T>() && predicate.Invoke(x as T)); }

		public bool HasChildrenCount(Func<Node, bool> predicate, out int count) {
			count = this.children.Count(predicate);
			return count > 0;
		}

		public bool HasChildrenCount<T>(out int count) where T : Node {
			count = this.children.Count(x => x.Is<T>());
			return count > 0;
		}

		public bool HasChildrenCount<T>(Func<T, bool> predicate, out int count) where T : Node {
			count = this.children.Count(x => x.Is<T>() && predicate.Invoke(x as T));
			return count > 0;
		}

		public bool HasSibling(Func<Node, bool> predicate) { return this.GetSiblings().Any(predicate); }

		public bool HasSibling<T>() where T : Node { return this.GetSiblings().Any(x => x.Is<T>()); }

		public bool HasSibling<T>(Func<T, bool> predicate) where T : Node { return this.GetSiblings().Any(x => x.Is<T>() && predicate.Invoke(x as T)); }

		public bool HasSiblings() { return !this.GetSiblings().IsEmpty(); }

		public bool HasSiblingAny(Func<Node, bool> predicate) { return this.GetSiblings().Any(predicate); }

		public bool HasSiblingAny<T>(Func<T, bool> predicate) where T : Node { return this.GetSiblings().Any(x => x.Is<T>() && predicate.Invoke(x as T)); }

		public bool HasSiblingAll(Func<Node, bool> predicate) { return this.GetSiblings().All(predicate); }

		public bool HasSiblingAll<T>(Func<T, bool> predicate) where T : Node { return this.GetSiblings().All(x => x.Is<T>() && predicate.Invoke(x as T)); }

		public bool HasSiblingCount(Func<Node, bool> predicate, out int count) {
			count = this.GetSiblings().Count(predicate);
			return count > 0;
		}

		public bool HasSiblingCount<T>(out int count) where T : Node {
			count = this.GetSiblings().Count(x => x.Is<T>());
			return count > 0;
		}

		public bool HasSiblingCount<T>(Func<T, bool> predicate, out int count) where T : Node {
			count = this.GetSiblings().Count(x => x.Is<T>() && predicate.Invoke(x as T));
			return count > 0;
		}

		private Node GetParent() {
			if (!this.HasParent()) {
				throw new Exception("Node Doesnt have a Parent");
			}
			return this.parent!;
		}

		private T GetParent<T>() where T : Node {
			if (!this.HasParent()) {
				throw new Exception("Node Doesnt have a Parent");
			}
			return this.parent! as T;
		}

		public Node GetChild(Predicate<Node> predicate) { return this.children.Find(predicate); }

		public T GetChild<T>() where T : Node { return (T) this.children.Find(x => x.Is<T>()); }

		public T GetChild<T>(Predicate<T> predicate) where T : Node { return (T) this.children.Find(x => x.Is<T>() && predicate.Invoke(x as T)); }

		public List<Node> GetChildren() { return this.children; }

		public List<T> GetChildren<T>() where T : Node { return this.children.OfType<T>() as List<T>; }

		public List<Node> GetChildren(Func<Node, bool> predicate) { return this.children.Where(predicate) as List<Node>; }

		public List<T> GetChildren<T>(Func<T, bool> predicate) where T : Node { return this.children.Where(x => x.Is<T>() && predicate.Invoke(x as T)) as List<T>; }

		public Node GetSibling(Predicate<Node> predicate) { return this.GetSiblings().Find(predicate); }

		public T GetSibling<T>() where T : Node { return (T) this.GetSiblings().Find(x => x.Is<T>()); }

		public T GetSibling<T>(Predicate<T> predicate) where T : Node { return (T) this.GetSiblings().Find(x => x.Is<T>() && predicate.Invoke(x as T)); }

		public List<Node> GetSiblings() {
			List<Node> nodes = this.GetParent().children;
			nodes.Remove(nodes.Find(x => x.Is(this))!);
			return nodes;
		}

		public List<T> GetSiblings<T>() where T : Node { return this.GetSiblings().OfType<T>() as List<T>; }

		public List<Node> GetSiblings(Func<Node, bool> predicate) { return this.GetSiblings().Where(predicate) as List<Node>; }

		public List<T> GetSiblings<T>(Func<T, bool> predicate) where T : Node { return this.GetSiblings().Where(x => x.Is<T>() && predicate.Invoke(x as T)) as List<T>; }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddChild(Node child) {
			child.parent = this;
			this.children.Add(child);
			Engine.Game.Scene.AddCacheNode(child);
			child.Start();
		}

		public void AddChild<T>() where T : Node, new() { this.AddChild(new T()); }

		public void AddChildren(params Node[] children) {
			foreach (Node child in children) {
				this.AddChild(child);
			}
		}

		public void AddSibling(Node sibling) { this.GetParent().AddChild(sibling); }

		public void AddSibling<T>() where T : Node, new() { this.GetParent().AddChild(new T()); }

		public void AddSiblings(params Node[] siblings) { this.GetParent().AddChildren(siblings); }

		public void RemoveChild(Node node) { node.Remove(); }

		public void RemoveChild<T>() where T : Node { this.GetChild<T>().Remove(); }

		public void RemoveChildren(params Node[] nodes) {
			foreach (Node node in nodes) {
				node.Remove();
			}
		}

		public void RemoveChildren<T>() where T : Node {
			foreach (T child in this.GetChildren<T>()) {
				child.Remove();
			}
		}

		public void RemoveChildren(Func<Node, bool> predicate) {
			foreach (Node child in this.GetChildren(predicate)) {
				child.Remove();
			}
		}

		public void RemoveChildren<T>(Func<T, bool> predicate) where T : Node {
			foreach (T child in this.GetChildren(predicate)) {
				child.Remove();
			}
		}

		public void RemoveSibling(Node node) { node.Remove(); }

		public void RemoveSibling<T>() where T : Node { this.GetSibling<T>().Remove(); }

		public void RemoveSiblings(params Node[] nodes) {
			foreach (Node node in nodes) {
				node.Remove();
			}
		}

		public void RemoveSiblings<T>() where T : Node {
			foreach (T sibling in this.GetSiblings<T>()) {
				sibling.Remove();
			}
		}

		public void RemoveSiblings(Func<Node, bool> predicate) {
			foreach (Node sibling in this.GetSiblings(predicate)) {
				sibling.Remove();
			}
		}

		public void RemoveSiblings<T>(Func<T, bool> predicate) where T : Node {
			foreach (T sibling in this.GetSiblings(predicate)) {
				sibling.Remove();
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Remove() {
			this.Stop();
			Engine.Game.Scene.RemoveCacheNode(this);
			Engine.Game.Scene.RemoveCacheTags(this);
			if (this.HasParent()) {
				this.parent!.children.Remove(this);
			}
		}

		public IEnumerable<string> GetTags() { return this.tags.AsReadOnly(); }

		public bool HasTag(string tag) { return this.tags.Contains(tag); }

		public bool HasTagsAll(params string[] tags) { return this.tags.All(x => tags.Contains(x)); }

		public bool HasTagsAny(params string[] tags) { return this.tags.Any(x => tags.Contains(x)); }

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddTag(string tag) {
			if (this.HasTag(tag)) {
				throw new Exception("Node Already Has Tag");
			}
			Engine.Game.Scene.AddCacheTag(tag, this);
			this.tags.Add(tag);
		}

		public void AddTags(params string[] tags) {
			foreach (string tag in tags) {
				this.AddTag(tag);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void RemoveTag(string tag) {
			if (!this.HasTag(tag)) {
				throw new Exception("Node Doesnt Have Tag");
			}
			Engine.Game.Scene.RemoveCacheTag(tag, this);
			this.tags.Remove(tag);
		}

		public void RemoveTags(params string[] tags) {
			foreach (string tag in tags) {
				this.RemoveTag(tag);
			}
		}

		public void ReplaceTag(string replace, string newTag) {
			this.RemoveTag(replace);
			this.AddTag(newTag);
		}
	}
}