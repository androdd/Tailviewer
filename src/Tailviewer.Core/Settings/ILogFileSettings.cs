using System.Text;
using Tailviewer.Api;

// ReSharper disable once CheckNamespace
namespace Tailviewer.Core
{
	/// <summary>
	///     Responsible for storing settings related to <see cref="ILogSource" /> and its implementations.
	/// </summary>
	[Service]
	public interface ILogFileSettings
	{
		/// <summary>
		///     The encoding used to interpret text based log files.
		/// </summary>
		/// <remarks>
		///     This encoding is used for text based log files which don't tell us their encoding, i.e.
		///     which neither start with a byte order mark, nor have a format-specific encoding, nor
		///     an encoding chosen by the user for that particular file (<see cref="TextProperties.OverwrittenEncoding"/>).
		///     When no encoding is specified (=null), which is the default, then the system's default
		///     code page (<see cref="System.Text.Encoding.Default"/>) is used for those files.
		/// </remarks>
		Encoding DefaultEncoding { get; set; }
	}
}