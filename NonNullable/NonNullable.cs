using System;
using System.Diagnostics;

namespace NonNullable {
	[DebuggerDisplay("Value = {Value}")]
	public struct NonNullable<T> : IEquatable<T>, IEquatable<NonNullable<T>> where T : class {
		public T Value { get; }

		public NonNullable(T value) {
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			this.Value = value;
		}

		public static implicit operator NonNullable<T>(T value) => new NonNullable<T>(value);
		public static implicit operator T(NonNullable<T> wrapper) => wrapper.Value;

		public override String ToString() => this.Value.ToString();

		public override Int32 GetHashCode() => this.Value.GetHashCode();

		public Boolean Equals(T other) => this.Value.Equals(other);
		public Boolean Equals(NonNullable<T> other) => this.Equals(other.Value);
		public override Boolean Equals(Object obj) {
			if (obj == null)
				return false;

			var x = obj as T;
			if (x != null)
				return this.Value.Equals(x);

			if (obj is NonNullable<T>)
				return this.Equals((NonNullable<T>)obj);

			return false;
		}

		public static Boolean operator ==(NonNullable<T> a, NonNullable<T> b) => a.Equals(b);
		public static Boolean operator !=(NonNullable<T> a, NonNullable<T> b) => !(a == b);
		public static Boolean operator ==(NonNullable<T> a, T b) => a.Equals(b);
		public static Boolean operator !=(NonNullable<T> a, T b) => !(a == b);
		public static Boolean operator ==(T a, NonNullable<T> b) => b.Equals(a);
		public static Boolean operator !=(T a, NonNullable<T> b) => !(a == b);

		public static NonNullable<T>[] ArrayFrom(T[] ts) {
			var nnts = new NonNullable<T>[ts.Length];
			for (var i = 0; i < ts.Length; i++)
				nnts[i] = ts[i];
			return nnts;
		}
	}
}
