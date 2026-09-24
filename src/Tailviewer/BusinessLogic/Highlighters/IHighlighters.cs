using System;
using System.Collections.Generic;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.Highlighters
{
	/// <summary>
	///
	/// </summary>
	[Service]
	public interface IHighlighters
	{
		IEnumerable<Highlighter> Highlighters { get; }

		/// <summary>
		///     Is fired whenever a highlighter has been added, removed or modified.
		/// </summary>
		event Action OnChanged;

		/// <summary>
		///     Adds a new highlighter.
		/// </summary>
		/// <returns></returns>
		Highlighter AddHighlighter();

		void Remove(HighlighterId id);

		/// <summary>
		///     Must be called after a highlighter has been modified so listeners are notified of the change.
		/// </summary>
		/// <param name="highlighter"></param>
		void Update(Highlighter highlighter);
	}
}
