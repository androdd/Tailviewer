using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.Highlighters
{
	/// <summary>
	///     Responsible for maintaining the list of available highlighters a user has created.
	///     The highlighters are backed by <see cref="HighlightersSettings" /> and therefore
	///     persisted between sessions (as long as the application settings are saved).
	/// </summary>
	public sealed class HighlighterCollection
		: IHighlighters
	{
		/// <summary>
		///     The predefined list of background colors given to newly created highlighters, in order.
		///     All of them are light enough for black text to remain readable.
		/// </summary>
		public static readonly IReadOnlyList<Color> Palette = new[]
		{
			Color.FromRgb(0xFF, 0xE0, 0x82), //< amber
			Color.FromRgb(0xA5, 0xD6, 0xA7), //< green
			Color.FromRgb(0x90, 0xCA, 0xF9), //< blue
			Color.FromRgb(0xF4, 0x8F, 0xB1), //< pink
			Color.FromRgb(0xCE, 0x93, 0xD8), //< purple
			Color.FromRgb(0xFF, 0xCC, 0x80), //< orange
			Color.FromRgb(0x80, 0xDE, 0xEA), //< cyan
			Color.FromRgb(0xE6, 0xEE, 0x9C)  //< lime
		};

		private readonly HighlightersSettings _settings;
		private readonly List<Highlighter> _highlighters;
		private readonly object _syncRoot;
		private int _numCreated;

		public HighlighterCollection(HighlightersSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_syncRoot = new object();
			_highlighters = new List<Highlighter>();
			foreach (var setting in settings)
			{
				_highlighters.Add(new Highlighter(setting));
			}

			_numCreated = _highlighters.Count;
		}

		#region Implementation of IHighlighters

		public IEnumerable<Highlighter> Highlighters
		{
			get
			{
				lock (_syncRoot)
				{
					return _highlighters.ToList();
				}
			}
		}

		public event Action OnChanged;

		public Highlighter AddHighlighter()
		{
			Highlighter highlighter;
			lock (_syncRoot)
			{
				var settings = new HighlighterSettings
				{
					BackgroundColor = Palette[_numCreated % Palette.Count]
				};
				++_numCreated;

				highlighter = new Highlighter(settings);
				_highlighters.Add(highlighter);
				_settings.Add(settings);
			}

			EmitOnChanged();
			return highlighter;
		}

		public void Remove(HighlighterId id)
		{
			lock (_syncRoot)
			{
				_highlighters.RemoveAll(x => x.Id == id);
				var idx = _settings.FindIndex(x => Equals(x.Id, id));
				if (idx != -1)
				{
					_settings.RemoveAt(idx);
				}
			}

			EmitOnChanged();
		}

		public void Update(Highlighter highlighter)
		{
			EmitOnChanged();
		}

		#endregion

		private void EmitOnChanged()
		{
			OnChanged?.Invoke();
		}
	}
}
