using System;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Settings;

namespace Tailviewer.BusinessLogic.Highlighters
{
	/// <summary>
	///     Colors matching log lines in a predefined color, without ever removing any lines from view.
	/// </summary>
	public sealed class Highlighter
	{
		private readonly HighlighterSettings _settings;

		public Highlighter()
			: this(new HighlighterSettings())
		{
		}

		public Highlighter(HighlighterSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public HighlighterId Id => _settings.Id;

		public string Value
		{
			get { return _settings.Value; }
			set { _settings.Value = value; }
		}

		public FilterMatchType MatchType
		{
			get { return _settings.MatchType; }
			set { _settings.MatchType = value; }
		}

		public Color BackgroundColor
		{
			get { return _settings.BackgroundColor; }
			set { _settings.BackgroundColor = value; }
		}

		public bool IsActive
		{
			get { return _settings.IsActive; }
			set { _settings.IsActive = value; }
		}

		/// <summary>
		///     Creates a new filter which matches those log lines this highlighter is meant to color.
		///     Returns null when <see cref="Value" /> is empty; throws <see cref="System.ArgumentException" />
		///     when the value cannot be interpreted (e.g. a malformed regular expression).
		/// </summary>
		/// <returns></returns>
		[Pure]
		public ILogEntryFilter CreateFilter()
		{
			return new QuickFilterSettings
			{
				Value = Value,
				MatchType = MatchType,
				IgnoreCase = true,
				IsInverted = false
			}.CreateFilter();
		}
	}
}
