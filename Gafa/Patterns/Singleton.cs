namespace Gafa.Patterns
{
	public class Singleton<U> where U : class, new()
	{
		private static U _instance;

		public static U Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (safetyLock)
					{
						if (_instance == null)
						{
							_instance = new U();
						}
					}
				}
				return _instance;
			}
		}

		protected Singleton() { }

		private static object safetyLock = new object();
	}
}
