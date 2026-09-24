using System;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Core;

namespace Tailviewer.Settings
{
	/// <summary>
	///     The configuration of an application-wide highlighter.
	/// </summary>
	public sealed class HighlighterSettings
		: ICloneable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		///     The id of this highlighter.
		/// </summary>
		public HighlighterId Id;

		/// <summary>
		///     How <see cref="Value" /> is to be interpreted.
		/// </summary>
		public FilterMatchType MatchType;

		/// <summary>
		///     The actual highlighter value, <see cref="MatchType" /> defines how it is interpreted.
		/// </summary>
		public string Value;

		/// <summary>
		///     The background color with which matching lines are painted.
		/// </summary>
		public Color BackgroundColor;

		/// <summary>
		///     When set to true, then lines matching this highlighter are colored, otherwise
		///     this highlighter has no effect.
		/// </summary>
		public bool IsActive;

		/// <summary>
		///     Initializes this highlighter.
		/// </summary>
		public HighlighterSettings()
		{
			Id = HighlighterId.CreateNew();
			IsActive = true;
			BackgroundColor = HighlighterCollection.Palette[index: 0];
		}

		/// <summary>
		///     Restores this highlighter from the given xml reader.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public bool Restore(XmlReader reader)
		{
			var count = reader.AttributeCount;
			for (var i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);

				switch (reader.Name)
				{
					case "id":
						Id = new HighlighterId(reader.ReadContentAsGuid());
						break;

					case "type":
						MatchType = reader.ReadContentAsEnum<FilterMatchType>();
						break;

					case "value":
						Value = reader.Value;
						break;

					case "backgroundcolor":
						try
						{
							BackgroundColor = (Color) ColorConverter.ConvertFromString(reader.Value);
						}
						catch (Exception e)
						{
							Log.WarnFormat("Cannot parse value '{0}' as a color, keeping the default color instead:\r\n{1}",
							               reader.Value, e);
						}
						break;

					case "isactive":
						IsActive = reader.ReadContentAsBool();
						break;
				}
			}

			if (Id == HighlighterId.Empty)
				return false;

			return true;
		}

		/// <summary>
		///     Saves the contents of this object into the given writer.
		/// </summary>
		/// <param name="writer"></param>
		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeGuid("id", Id.Value);
			writer.WriteAttributeEnum("type", MatchType);
			writer.WriteAttributeString("value", Value);
			writer.WriteAttributeColor("backgroundcolor", BackgroundColor);
			writer.WriteAttributeBool("isactive", IsActive);
		}

		/// <summary>
		///     Creates a deep clone of this object.
		/// </summary>
		/// <returns></returns>
		public HighlighterSettings Clone()
		{
			return new HighlighterSettings
			{
				Id = Id,
				MatchType = MatchType,
				Value = Value,
				BackgroundColor = BackgroundColor,
				IsActive = IsActive
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
