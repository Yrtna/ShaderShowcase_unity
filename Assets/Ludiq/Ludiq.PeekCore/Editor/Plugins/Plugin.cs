using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Ludiq.PeekCore
{
	public abstract class Plugin
	{
		protected Plugin(string id)
		{
			Ensure.That(nameof(id)).IsNotNull(id);

			this.id = id;

			dependencies = PluginContainer.pluginDependencies[id].Select(PluginContainer.GetPlugin).ToList().AsReadOnly();
		}

		public string id { get; }
		public ReadOnlyCollection<Plugin> dependencies { get; }

		public PluginManifest manifest { get; internal set; }
		public PluginConfiguration configuration { get; internal set; }
		public PluginPaths paths { get; internal set; }
		public PluginResources resources { get; internal set; }

		public virtual IEnumerable<ScriptReferenceReplacement> scriptReferenceReplacements => Enumerable.Empty<ScriptReferenceReplacement>();

		public virtual IEnumerable<object> aotStubs => Enumerable.Empty<object>();

		public virtual IEnumerable<string> tips => Enumerable.Empty<string>();

		public virtual IEnumerable<Page> SetupWizardPages(SetupWizard wizard)
		{
			return Enumerable.Empty<Page>();
		}
	}
}