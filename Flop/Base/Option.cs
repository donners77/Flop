using System;
using System.Collections;
using System.Collections.Generic;

namespace Flop
{
    /// <summary>
    /// Option is similar to the <see cref="Nullable"/> type found in the System namespace, but has one important 
    /// difference: it can encapsulate both reference and value types while Nullable can contain only value types. 
    /// In other words, Nullable has the struct restriction on its generic type parameter. Option will work with any type. 
    /// </summary>
    /// <typeparam name="T">Type of the encapsulated value.</typeparam>
    public struct Option<T> : IOptionalValue, IEnumerable<T>
    {
        // Fields
        private readonly T _value;
        private readonly bool _isSome;

        // Constructor
        public Option(T value) : this(value, value != null) { }

        private Option(T value, bool isSome)
        {
            _isSome = isSome;
            _value = value;
        }

        // Properties
        public bool IsSome { get { return _isSome; } }
        public bool IsNone { get { return !IsSome; } }
        public int Count { get { return IsSome ? 1 : 0; } }

        public T Value { get { if (IsSome) return _value; throw new InvalidOperationException("Option has no value."); } }

        // Methods
        public override string ToString() { return IsSome ? Value == null ? "Some(null)" : String.Format("Some({0})", Value) : "None"; }
        public override int GetHashCode() { return IsSome && Value != null ? Value.GetHashCode() : 0; }
        public override bool Equals(object obj) { return IsSome && Value != null ? Value.Equals(obj) : false; }

        public bool ForAll(Func<T,bool> pred) { return !IsSome || pred(Value); }
        public S Fold<S>(S state, Func<S, T, S> folder) { return IsSome ? folder(state, Value) : state; }
        public bool Exists(Func<T, bool> pred) { return IsSome && pred(Value); }
        public bool Filter(Func<T, bool> pred) { return Exists(pred); }
        public Option<R> Bind<R>(Func<T, Option<R>> binder) { return IsSome ? binder(Value) : Option<R>.None; }

        // Implemenation of IEnumerable<T>
        public IEnumerable<T> AsEnumerable() { if (IsSome) yield return Value; }
        public IEnumerator<T> GetEnumerator() { return AsEnumerable().GetEnumerator(); }
	    IEnumerator IEnumerable.GetEnumerator() { return AsEnumerable().GetEnumerator(); }

        // Static
        public static Option<T> Some(T value) { return new Option<T>(value); }
        public static readonly Option<T> None = new Option<T>(default(T), false);

        public static implicit operator T(Option<T> option) { return option.Value; }
        public static implicit operator Option<T>(T value) { return value == null ? None : Some(value); }
        public static implicit operator Option<T>(OptionNone none) { return None; }
	}

    public interface IOptionalValue
    {
        bool IsSome { get; }
        bool IsNone { get; }
    }

    public struct OptionNone
    {
        public static OptionNone Default = new OptionNone();
    }

	/// <summary>
	/// Implement monadic extension methods for Option{T}
	/// </summary>
	public static class Option
	{
		/// <summary>
		/// Lift a value to Option monad.
		/// </summary>
		public static Option<T> ToOption<T> (this T value)
		{
			return new Option<T> (value);
		}

		/// <summary>
		/// Lift a reference value to Option monad. Null reference is interpreted as no value.
		/// </summary>
		public static Option<T> RefToOption<T> (this T value) where T: class
		{
			return value != null ? new Option<T> (value) : new Option<T> ();
		}

		/// <summary>
		/// Monadic bind.
		/// </summary>
		public static Option<U> Bind<T, U> (this Option<T> option, Func<T, Option<U>> func)
		{
			return option.IsSome ? func (option.Value) : new Option<U> ();
		}

		/// <summary>
		/// Select extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Option<U> Select<T, U> (this Option<T> option, Func<T, U> select)
		{
			return Bind (option, a => select (a).ToOption ());
		}

		/// <summary>
		/// SelectMany extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Option<V> SelectMany<T, U, V> (this Option<T> option,
			Func<T, Option<U>> project, Func<T, U, V> select)
		{
			return Bind (option, a => project (a).Bind (b => select (a, b).ToOption ()));
		}

		/// <summary>
		/// Where extension method needed to enable Linq's syntactic sugaring.
		/// </summary>
		public static Option<T> Where<T> (this Option<T> option, Func<T, bool> predicate)
		{
			return option.IsSome && predicate (option.Value) ? option : new Option<T> ();
		}
	}
}
