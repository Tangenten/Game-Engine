using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using RavUtilities;

namespace RavEngine {
	public class XMLResource : Resource {
		private XmlWriterSettings xmlWriterSettings;
		private XmlReaderSettings xmlReaderSettings;
		private IExtendedXmlSerializer XML;
		private FileStream fileStream;

		protected override void LoadImplementation() {
			this.XML = new ConfigurationContainer().UseOptimizedNamespaces().EnableReferences().Create();

			this.xmlWriterSettings = new XmlWriterSettings();
			this.xmlWriterSettings.CloseOutput = false;
			this.xmlWriterSettings.Encoding = Encoding.Default;
			this.xmlWriterSettings.Indent = true;
			this.xmlWriterSettings.NamespaceHandling = NamespaceHandling.Default;
			this.xmlWriterSettings.NewLineHandling = NewLineHandling.None;

			this.xmlReaderSettings = new XmlReaderSettings();
			this.xmlReaderSettings.CloseInput = false;

			#if DEBUG
			this.fileStream = FileU.LoadStreamWaitLock(this.FilePath);
			#else
			this.fileStream = new FileStream(this.FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			#endif
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public T DeSerialize<T>() {
			this.LoadIfNotLoaded();

			string serilization = this.fileStream.ReadText();
			return this.XML.Deserialize<T>(this.xmlReaderSettings, serilization);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Serialize<T>(T data) {
			this.LoadIfNotLoaded();
			this.SkipNextReload();

			string serilization = this.XML.Serialize(this.xmlWriterSettings, data);
			this.fileStream.WriteText(serilization);
		}
	}
}