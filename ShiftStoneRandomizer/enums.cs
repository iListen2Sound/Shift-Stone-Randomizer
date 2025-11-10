using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftStoneRandomizer
{
	public enum RandomedHand
	{
		Both = -1,
		Left = 0,
		Right = 1
	}

	public enum ShiftStonePrefs
	{
		Empty = -2,
		Random = -1,
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
		None
	}
}