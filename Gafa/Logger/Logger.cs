namespace Gafa.Logging
{
	/// <summary>
	/// Base class exposing logging capabilities
	/// </summary>
	public abstract class Logger
	{
		/// <summary>
		/// Logger
		/// </summary>
		protected NLog.Logger Log { get; private set; }

		protected Logger()
		{
			Log = NLog.LogManager.GetLogger(GetType().FullName);
		}

		protected static NLog.LogLevel Default = NLog.LogLevel.Debug;
		protected const string LogEnter = "Enter.";
		protected const string LogExit = "Exit.";
	}
}
