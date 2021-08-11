using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using RavUtilities;

namespace RavEngine {
	public class SettingsE : EngineCore {
		private Dictionary<string, object> settingsDict;

		private XmlWriterSettings xmlWriterSettings;
		private XmlReaderSettings xmlReaderSettings;
		private IExtendedXmlSerializer XML;
		private FileStream fileStream;
		private DateTime lastTime;

		public SettingsE() {
			this.XML = new ConfigurationContainer().UseOptimizedNamespaces().Create();

			this.xmlWriterSettings = new XmlWriterSettings();
			this.xmlWriterSettings.CloseOutput = false;
			this.xmlWriterSettings.Encoding = Encoding.Default;
			this.xmlWriterSettings.Indent = true;
			this.xmlWriterSettings.NamespaceHandling = NamespaceHandling.Default;
			this.xmlWriterSettings.NewLineHandling = NewLineHandling.None;

			this.xmlReaderSettings = new XmlReaderSettings();
			this.xmlReaderSettings.CloseInput = false;

			#if DEBUG
			string engineSettingsPath = "";
			if (FileU.TryGetSolutionPath(out string path)) {
				engineSettingsPath = $@"{path}\Resources\Settings.xml";
			} else {
				throw new Exception("Cant Find Solution, Did you move Executable?");
			}
			#elif RELEASE
			string engineSettingsPath = $@"{Environment.CurrentDirectory}\Resources\Settings.xml";
			#endif

			this.LoadXml(engineSettingsPath);
		}

		internal override void Start() { }

		internal override void Stop() { }

		internal override void Update() {
			if (this.HasReloaded()) {
				this.DeSerialize();
			}
		}

		internal override void Reset() { }

		internal void LoadXml(string settingsPath) {
			this.settingsDict = new Dictionary<string, object>();

			#if DEBUG
			this.fileStream = FileU.LoadStreamWaitLock(settingsPath);
			#else
			this.fileStream = new FileStream(settingsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			#endif

			this.DeSerialize();
		}

		private bool HasReloaded() {
			DateTime currentTime = File.GetLastWriteTime(this.fileStream.Name);
			return currentTime != this.lastTime;
		}

		public void Set<T>(string key, T value) {
			this.settingsDict[key] = value;
			this.Serialize();
		}

		public T Get<T>(string key) => (T) this.settingsDict[key];

		public bool TryGet<T>(string key, out T value) {
			if (this.settingsDict.TryGetValue(key, out object settingsValue)) {
				value = (T) settingsValue;
				return true;
			}

			value = default;
			return false;
		}

		private void DeSerialize() {
			try {
				string serilization = this.fileStream.ReadText();
				this.settingsDict = this.XML.Deserialize<Dictionary<string, object>>(this.xmlReaderSettings, serilization);
			} catch (Exception e) {
				Engine.Editor?.Console.WriteLine(ConsoleEntry.Error("Settings Error: " + e.Message));
			}
			this.lastTime = File.GetLastWriteTime(this.fileStream.Name);
		}

		private void Serialize() {
			try {
				string serilization = this.XML.Serialize(this.xmlWriterSettings, this.settingsDict);
				this.fileStream.WriteText(serilization);
			} catch (Exception e) {
				Engine.Editor?.Console.WriteLine(ConsoleEntry.Error("Settings Error: " + e.Message));
			}
		}
	}

	public class SettingsAttribute : Attribute {

	}
}
