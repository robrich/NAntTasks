namespace NAnt.Restrict.Filters {

	#region using

	#endregion

	public enum FilterPriority {
		File = 1,
		Content = 2,
		Nested = 3
	}

	public enum SizeWhen {
		equal,
		eq,
		greater,
		gt,
		less,
		lt,
		greaterorequal,
		ge, // greater or equal
		notequal,
		ne, // not equal
		lessorequal,
		le, // less or equal
	}

	public enum TimeWhen {
		before,
		after,
		equal
	}

}
