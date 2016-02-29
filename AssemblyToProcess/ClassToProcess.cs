using NonNullable;
using System;

namespace AssemblyToProcess {
	public static class ClassToProcess {
		public static void What() {
			//NonNullable<String> z;
			var a = new NonNullable<String>("");
			//var b = default(NonNullable<String>);
			//var c = new NonNullable<String>(null);
			var fds = a.Value.Length;
		}
	}
}
