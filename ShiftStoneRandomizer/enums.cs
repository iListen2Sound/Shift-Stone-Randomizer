

namespace ShiftStoneRandomizer
{
	public enum Hands
	{
		Both = -1,
		Left = 0,
		Right = 1
	}

	public enum ShiftStonePrefs
	{	
		Invalid = -5,
		Mirror = -4
		Stay = -3,
		Random = -2,
		Empty = -1,
		Adamant = 0,
		Charge,
		Flow,
		Guard,
		Stubborn,
		Surge,
		Vigor,
		Volitile
	}

	public enum AutomationPrefs
	{
		Random,
		Auto,
		Mirror,
		None
	}
}