public class X
{
	public X (out bool hello)
	{
		hello = true;
	}

	static void Main ()
	{ }
}

public class Y : X
{
	public Y (out bool hello)
		: base (out hello)
	{ }
}
