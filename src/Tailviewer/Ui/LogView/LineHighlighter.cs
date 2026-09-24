using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.Highlighters;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     An immutable snapshot of a single active <see cref="Highlighter" />, ready to be used
	///     during rendering: the filter is created once and the brushes are frozen so that
	///     matching visible lines against it is cheap.
	/// </summary>
	public sealed class LineHighlighter
	{
		private readonly ILogEntryFilter _filter;

		private LineHighlighter(ILogEntryFilter filter, Color backgroundColor)
		{
			_filter = filter;

			var background = new SolidColorBrush(backgroundColor);
			background.Freeze();
			BackgroundBrush = background;
			ForegroundBrush = Brushes.Black;
		}

		public Brush BackgroundBrush { get; }

		public Brush ForegroundBrush { get; }

		[Pure]
		public bool Matches(IReadOnlyLogEntry logLine)
		{
			return _filter.PassesFilter(logLine);
		}

		/// <summary>
		///     Creates a render-ready snapshot of all those highlighters which are active and
		///     represent a valid pattern. Invalid patterns (e.g. a regular expression while it
		///     is still being typed) are simply skipped.
		/// </summary>
		/// <param name="highlighters"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyList<LineHighlighter> CreateFrom(IHighlighters highlighters)
		{
			var snapshot = new List<LineHighlighter>();
			if (highlighters == null)
				return snapshot;

			foreach (var highlighter in highlighters.Highlighters)
			{
				if (!highlighter.IsActive)
					continue;

				try
				{
					var filter = highlighter.CreateFilter();
					if (filter != null)
						snapshot.Add(new LineHighlighter(filter, highlighter.BackgroundColor));
				}
				catch (ArgumentException)
				{
					// The user is likely still typing the pattern (or made a mistake): we simply
					// don't color anything until the pattern becomes valid again.
				}
			}

			return snapshot;
		}
	}
}
