

namespace ShiftStoneManager
{
	public enum Hands
	{
		Both = -1,
		Left = 0,
		Right = 1
	}

	public enum ShiftStonePrefs
	{
		Special = -6,
		Invalid = -5,
		Mirror = -4,
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
		Volatile
	}

	public enum AutomationPrefs
	{
		None,
		Random,
		Auto,
		Mirror

	}
	
	public enum Scenes
	{
		Loader = 0,
		Gym = 1,
		Park = 2,
		Map0 = 3,
		Map1 = 4,
	}
}