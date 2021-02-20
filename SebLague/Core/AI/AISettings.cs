namespace Chess {
	using System.Collections.Generic;
	using System.Collections;


	public class AISettings {

		public event System.Action requestAbortSearch;

		public int depth;
		public bool useIterativeDeepening = true;
		public bool useTranspositionTable;

		public bool useThreading;
		public bool useFixedDepthSearch;
		public int searchTimeMillis = 1000;
		public bool endlessSearchMode;
		public bool clearTTEachMove;

		//TJ: book currently not supported, it's handled by the GUI in UCI engines
		public bool useBook;
		//public TextAsset book;
		public int maxBookPly = 10;

		public MoveGenerator.PromotionMode promotionsToSearch;

		public Search.SearchDiagnostics diagnostics;

		public void RequestAbortSearch () {
			requestAbortSearch?.Invoke ();
		}
	}
}