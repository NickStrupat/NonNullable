using NonNullable;
using System;

namespace AssemblyToProcess {
	public class ClassToProcess {
		private static NonNullable<String> sm = new NonNullable<String>("");
		private NonNullable<String> m = new NonNullable<String>("");  

		public static void What() {
			//NonNullable<String> z;
			var a = new NonNullable<String>("");
			//var b = default(NonNullable<String>);
			//var c = new NonNullable<String>(null);
			var fds = a.Value.Length;
		}
	}
}
