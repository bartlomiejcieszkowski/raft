using NLog;

namespace Gafa.Logging
{
	/// <summary>
	/// Base class exposing logging capabilities
	/// </summary>
	class ILogger
	{
		/// <summary>
		/// Logger
		/// </summary>
		protected Logger Log { get; private set; }

		protected ILogger()
		{
			Log = LogManager.GetLogger(GetType().FullName);
		}
	}
}
