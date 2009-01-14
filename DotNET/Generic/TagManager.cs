public class TagManager
{
	private Dictionary<string, object> Tags = new Dictionary<string, object>();

	public TagManager()
	{
	}

	public TagManager(Dictionary<string, object> Tags)
	{
		this.Tags = Tags;
	}

	public object this[string TagName] 
	{
		get 
		{
			if (Tags.ContainsKey(TagName)) return Tags[TagName];
			else return null;
		}
		set
		{
			if (!Tags.ContainsKey(TagName)) Tags.Add(TagName, value);
			else Tags[TagName] = value;
		}
	}

	public T Get<T>(string TagName, object Default)
	{
		return (Tags[TagName] != null) ? (T)Tags[TagName] : (T)Default;
	}

	public T Get<T>(string TagName)
	{
		return (T)Tags[TagName];
	}

	public string Get(string TagName)
	{
		return Tags[TagName].ToString();
	}
}