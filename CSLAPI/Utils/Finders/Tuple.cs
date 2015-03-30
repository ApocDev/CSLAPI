using System;
using System.Collections.Generic;

namespace CSLAPI.Utils.Finders
{
	// Either Mono or .NET 3.5 (been many years since I touched 3.5) doesn't have a Tuple class in it. So we'll add a very simple one.
	[Serializable]
	public class Tuple<T1, T2> : IEquatable<Tuple<T1, T2>>
	{
		private readonly T1 _item1;
		private readonly T2 _item2;

		public Tuple(T1 item1, T2 item2)
		{
			_item1 = item1;
			_item2 = item2;
		}

		public T1 Item1 { get { return _item1; } }
		public T2 Item2 { get { return _item2; } }

		/// <summary>
		///     Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		///     A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Item1: {0}, Item2: {1}", Item1, Item2);
		}

		#region Equality members

		/// <summary>
		///     Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Tuple<T1, T2> other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return EqualityComparer<T1>.Default.Equals(_item1, other._item1) && EqualityComparer<T2>.Default.Equals(_item2, other._item2);
		}

		/// <summary>
		///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
		///     <see cref="T:System.Object" />.
		/// </summary>
		/// <returns>
		///     true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />;
		///     otherwise, false.
		/// </returns>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Tuple<T1, T2>) obj);
		}

		/// <summary>
		///     Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		///     A hash code for the current <see cref="T:System.Object" />.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (EqualityComparer<T1>.Default.GetHashCode(_item1) * 397) ^ EqualityComparer<T2>.Default.GetHashCode(_item2);
			}
		}

		public static bool operator ==(Tuple<T1, T2> left, Tuple<T1, T2> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Tuple<T1, T2> left, Tuple<T1, T2> right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}