namespace Gafa.Patterns
{
	public class Lazy<U> where U : class, new()
	{
		private object _safetyLock = new object();
		private U _value = null;
		public U Value
		{
			get
			{
				if (_value == null)
				{
					lock (_safetyLock)
					{
						if (_value == null)
						{
							_value = new U();
						}
					}
				}
				return _value;
			}
		}

		protected Lazy() { }
	}


	public class Singleton<U> where U : class, new()
	{
		private static Lazy<U> _ValueLazy;

		public static U Instance
		{
			get
			{
				return _ValueLazy.Value;
			}
		}

		protected Singleton() { }
	}

	public class SingletonClassic<U> where U : class, new()
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

		protected SingletonClassic() { }

		private static object safetyLock = new object();
	}
}
