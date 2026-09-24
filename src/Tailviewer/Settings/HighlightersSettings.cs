using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Tailviewer.Settings
{
	/// <summary>
	///     The list of all application-wide highlighters.
	/// </summary>
	public sealed class HighlightersSettings
		: List<HighlighterSettings>
		, ICloneable
	{
		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Restores the values of this object from the given xml document.
		/// </summary>
		/// <param name="reader"></param>
		public void Restore(XmlReader reader)
		{
			var highlighters = new List<HighlighterSettings>();
			var subtree = reader.ReadSubtree();
			while (subtree.Read())
				switch (subtree.Name)
				{
					case "highlighter":
						var highlighter = new HighlighterSettings();
						if (highlighter.Restore(subtree))
							highlighters.Add(highlighter);
						break;
				}

			Clear();
			AddRange(highlighters);
		}

		/// <summary>
		///     Stores the values of this object in the given xml document.
		/// </summary>
		/// <param name="writer"></param>
		public void Save(XmlWriter writer)
		{
			foreach (var highlighter in this)
			{
				writer.WriteStartElement("highlighter");
				highlighter.Save(writer);
				writer.WriteEndElement();
			}
		}

		/// <summary>
		///     Returns a deep clone of this object.
		/// </summary>
		/// <returns></returns>
		public HighlightersSettings Clone()
		{
			var highlighters = new HighlightersSettings();
			highlighters.AddRange(this.Select(x => x.Clone()));
			return highlighters;
		}
	}
}
