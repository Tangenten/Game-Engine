using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Serialization;

namespace RavEngine {
	public class SettingsE : EngineCore {
		private Dictionary<string, object> settingsDict;
		private XmlSerializer settingsXml;
		private Resource settingsXmlResource;

		public SettingsE() {
		}

		internal override void Start() {
			this.settingsDict = new Dictionary<string, object>();
			//this.settingsXml = new XmlSerializer(typeof(Dictionary<string, object>));

			if (!Engine.Resources.HasResource("Settings.xml")) {
				Engine.Resources.CreateEngineResource("Settings.xml");
			}
			this.settingsXmlResource = Engine.Resources.LoadResource("Settings.xml", () => this.DeSerialize());
		}

		internal override void Stop() {
		}

		internal override void Update() {
		}

		internal override void Reset() {
			this.settingsDict = new Dictionary<string, object>();
			this.settingsXml = new XmlSerializer(typeof(Dictionary<string, object>));
		}

		public void Set<T>(string key, T value) {
			this.settingsDict[key] = value;
		}

		public bool TryGet<T>(string key, out T value) {
			if (this.settingsDict.TryGetValue(key, out object settingsValue)) {
				value = (T) settingsValue;
				return true;
			}

			value = default;
			return false;
		}

		public void DeSerialize() {
			this.settingsDict = (Dictionary<string, object>) this.settingsXml.Deserialize(this.settingsXmlResource.fileStream);
		}

		public void Serialize() {
			this.settingsXml.Serialize(this.settingsXmlResource.fileStream, this.settingsDict);
		}
	}
}
