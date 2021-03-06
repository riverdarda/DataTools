using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UnusedMember.Global

namespace Std.Utility.Collections.Generic
{
	/// <summary>
	///     A MultiValueDictionary can be viewed as a <see cref="T:System.Collections.IDictionary" /> that allows multiple
	///     values for any given unique key. While the MultiValueDictionary API is
	///     mostly the same as that of a regular <see cref="T:System.Collections.IDictionary" />, there is a distinction
	///     in that getting the value for a key returns a <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> of
	///     values
	///     rather than a single value associated with that key. Additionally,
	///     there is functionality to allow adding or removing more than a single
	///     value at once.
	///     The MultiValueDictionary can also be viewed as a IReadOnlyDictionary&lt;TKey,IReadOnlyCollection&lt;TValue&gt;t&gt;
	///     where the <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> is abstracted from the view of the
	///     programmer.
	///     For a read-only MultiValueDictionary, see <see cref="T:System.Linq.ILookup`2" />.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class MultiValueDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>>
	{
		/// <summary>
		///     The private dictionary that this class effectively wraps around
		/// </summary>
		private readonly Dictionary<TKey, InnerCollectionView> _dictionary;

		/// <summary>
		///     The function to construct a new <see cref="T:System.Collections.Generic.ICollection`1" />
		/// </summary>
		/// <returns></returns>
		private Func<ICollection<TValue>> _newCollectionFactory;

		/// <summary>
		///     The current version of this MultiValueDictionary used to determine MultiValueDictionary modification
		///     during enumeration
		/// </summary>
		private int _version;

		/// <summary>
		///     Returns the number of <typeparamref name="TKey" />s with one or more associated <typeparamref name="TValue" />
		///     in this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		/// <value>
		///     The number of <typeparamref name="TKey" />s in this
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </value>
		public int Count
		{
			get { return _dictionary.Count; }
		}

		/// <summary>
		///     Get every <typeparamref name="TValue" /> associated with the given <typeparamref name="TKey" />. If
		///     <paramref name="key" /> is not found in this
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />, will
		///     throw a <see cref="T:System.Collections.Generic.KeyNotFoundException" />.
		/// </summary>
		/// <param name="key">The <typeparamref name="TKey" /> of the elements to retrieve.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
		/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
		///     <paramref name="key" /> does not have any associated
		///     <typeparamref name="TValue" />s in this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </exception>
		/// <value>
		///     An <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> containing every
		///     <typeparamref name="TValue" />
		///     associated with <paramref name="key" />.
		/// </value>
		/// <remarks>
		///     Note that the <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> returned will change alongside any
		///     changes
		///     to the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		/// </remarks>
		public IReadOnlyCollection<TValue> this[TKey key]
		{
			get
			{
				InnerCollectionView tKeys;
				Requires.NotNullAllowStructs(key, "key");
				if (!_dictionary.TryGetValue(key, out tKeys))
				{
					throw new KeyNotFoundException(Errors.KeyNotFound);
				}
				return tKeys;
			}
		}

		/// <summary>
		///     Gets each <typeparamref name="TKey" /> in this
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> that
		///     has one or more associated <typeparamref name="TValue" />.
		/// </summary>
		/// <value>
		///     An <see cref="T:System.Collections.Generic.IEnumerable`1" /> containing each <typeparamref name="TKey" />
		///     in this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> that has one or more associated
		///     <typeparamref name="TValue" />.
		/// </value>
		public IEnumerable<TKey> Keys
		{
			get { return _dictionary.Keys; }
		}

		/// <summary>
		///     Gets an enumerable of <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> from this
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />,
		///     where each <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> is the collection of every
		///     <typeparamref name="TValue" /> associated
		///     with a <typeparamref name="TKey" /> present in the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		/// <value>
		///     An IEnumerable of each <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> in this
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		/// </value>
		public IEnumerable<IReadOnlyCollection<TValue>> Values
		{
			get { return _dictionary.Values; }
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the default initial capacity, and uses the default
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />.
		/// </summary>
		public MultiValueDictionary()
		{
			_newCollectionFactory = () => new List<TValue>();
			_dictionary = new Dictionary<TKey, InnerCollectionView>();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that is
		///     empty, has the specified initial capacity, and uses the default
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" />
		///     for <typeparamref name="TKey" />.
		/// </summary>
		/// <param name="capacity">
		///     Initial number of keys that the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> will allocate space for
		/// </param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">capacity must be &gt;= 0</exception>
		public MultiValueDictionary(int capacity)
		{
			_newCollectionFactory = () => new List<TValue>();
			Requires.Range(capacity >= 0, "capacity", null);
			_dictionary = new Dictionary<TKey, InnerCollectionView>(capacity);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that is empty, has the default initial capacity, and uses the
		///     specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
		/// </summary>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		public MultiValueDictionary(IEqualityComparer<TKey> comparer)
		{
			_newCollectionFactory = () => new List<TValue>();
			_dictionary = new Dictionary<TKey, InnerCollectionView>(comparer);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that is empty, has the specified initial capacity, and uses the
		///     specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
		/// </summary>
		/// <param name="capacity">
		///     Initial number of keys that the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> will allocate space for
		/// </param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		public MultiValueDictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
			_newCollectionFactory = () => new List<TValue>();
			Requires.Range(capacity >= 0, "capacity", null);
			_dictionary = new Dictionary<TKey, InnerCollectionView>(capacity, comparer);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
		///     and uses the
		///     default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
		/// </summary>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		public MultiValueDictionary(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable)
			: this(enumerable, null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
		///     and uses the
		///     specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
		/// </summary>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		public MultiValueDictionary(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable,
			IEqualityComparer<TKey> comparer)
		{
			_newCollectionFactory = () => new List<TValue>();
			Requires.NotNullAllowStructs(enumerable, "enumerable");
			_dictionary = new Dictionary<TKey, InnerCollectionView>(comparer);
			foreach (var keyValuePair in enumerable)
			{
				AddRange(keyValuePair.Key, keyValuePair.Value);
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;IGrouping&lt;TKey, TValue&gt;&gt; and uses the
		///     default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the <typeparamref name="TKey" /> type.
		/// </summary>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		public MultiValueDictionary(IEnumerable<IGrouping<TKey, TValue>> enumerable)
			: this(enumerable, null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;IGrouping&lt;TKey, TValue&gt;&gt; and uses the
		///     specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.
		/// </summary>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		public MultiValueDictionary(IEnumerable<IGrouping<TKey, TValue>> enumerable, IEqualityComparer<TKey> comparer)
		{
			_newCollectionFactory = () => new List<TValue>();
			Requires.NotNullAllowStructs(enumerable, "enumerable");
			_dictionary = new Dictionary<TKey, InnerCollectionView>(comparer);
			foreach (var tKeys in enumerable)
			{
				AddRange(tKeys.Key, tKeys);
			}
		}

		/// <summary>
		///     Adds the specified <typeparamref name="TKey" /> and <typeparamref name="TValue" /> to the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		/// <param name="key">The <typeparamref name="TKey" /> of the element to add.</param>
		/// <param name="value">The <typeparamref name="TValue" /> of the element to add.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
		/// <remarks>
		///     Unlike the Add for <see cref="T:System.Collections.IDictionary" />, the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> Add will not
		///     throw any exceptions. If the given <typeparamref name="TKey" /> is already in the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />,
		///     then <typeparamref name="TValue" /> will be added to
		///     <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> associated with <paramref name="key" />
		/// </remarks>
		/// <remarks>
		///     A call to this Add method will always invalidate any currently running enumeration regardless
		///     of whether the Add method actually modified the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </remarks>
		public void Add(TKey key, TValue value)
		{
			InnerCollectionView tKeys;
			Requires.NotNullAllowStructs(key, "key");
			if (!_dictionary.TryGetValue(key, out tKeys))
			{
				tKeys = new InnerCollectionView(key, _newCollectionFactory());
				_dictionary.Add(key, tKeys);
			}
			tKeys.AddValue(value);
			var multiValueDictionary = this;
			multiValueDictionary._version = multiValueDictionary._version + 1;
		}

		/// <summary>
		///     Adds a number of key-value pairs to this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />,
		///     where
		///     the key for each value is <paramref name="key" />, and the value for a pair
		///     is an element from <paramref name="values" />
		/// </summary>
		/// <param name="key">The <typeparamref name="TKey" /> of all entries to add</param>
		/// <param name="values">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of values to add</param>
		/// <exception cref="T:System.ArgumentNullException">
		///     <paramref name="key" /> and <paramref name="values" /> must be
		///     non-null
		/// </exception>
		/// <remarks>
		///     A call to this AddRange method will always invalidate any currently running enumeration regardless
		///     of whether the AddRange method actually modified the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </remarks>
		public void AddRange(TKey key, IEnumerable<TValue> values)
		{
			InnerCollectionView tKeys;
			Requires.NotNullAllowStructs(key, "key");
			Requires.NotNullAllowStructs(values, "values");
			if (!_dictionary.TryGetValue(key, out tKeys))
			{
				tKeys = new InnerCollectionView(key, _newCollectionFactory());
				_dictionary.Add(key, tKeys);
			}
			foreach (var value in values)
			{
				tKeys.AddValue(value);
			}
			var multiValueDictionary = this;
			multiValueDictionary._version = multiValueDictionary._version + 1;
		}

		/// <summary>
		///     Gets a read-only <see cref="T:System.Linq.ILookup`2" /> view of the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     that changes as the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> changes.
		/// </summary>
		/// <value>
		///     a read-only <see cref="T:System.Linq.ILookup`2" /> view of the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		/// </value>
		public ILookup<TKey, TValue> AsLookup()
		{
			return new MultiLookup(this);
		}

		/// <summary>
		///     Removes every <typeparamref name="TKey" /> and <typeparamref name="TValue" /> from this
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		public void Clear()
		{
			_dictionary.Clear();
			var multiValueDictionary = this;
			multiValueDictionary._version = multiValueDictionary._version + 1;
		}

		/// <summary>
		///     Determines if the given <typeparamref name="TKey" />-<typeparamref name="TValue" />
		///     pair exists within this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		/// <param name="key">The <typeparamref name="TKey" /> of the element.</param>
		/// <param name="value">The <typeparamref name="TValue" /> of the element.</param>
		/// <returns><c>true</c> if found; otherwise <c>false</c></returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
		public bool Contains(TKey key, TValue value)
		{
			InnerCollectionView tKeys;
			Requires.NotNullAllowStructs(key, "key");
			if (!_dictionary.TryGetValue(key, out tKeys))
			{
				return false;
			}
			return tKeys.Contains(value);
		}

		/// <summary>
		///     Determines if the given <typeparamref name="TKey" /> exists within this
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> and has
		///     at least one <typeparamref name="TValue" /> associated with it.
		/// </summary>
		/// <param name="key">
		///     The <typeparamref name="TKey" /> to search the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> for
		/// </param>
		/// <returns>
		///     <c>true</c> if the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> contains the requested
		///     <typeparamref name="TKey" />;
		///     otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
		public bool ContainsKey(TKey key)
		{
			Requires.NotNullAllowStructs(key, "key");
			return _dictionary.ContainsKey(key);
		}

		/// <summary>
		///     Determines if the given <typeparamref name="TValue" /> exists within this
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		/// <param name="value">
		///     A <typeparamref name="TValue" /> to search the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> for
		/// </param>
		/// <returns>
		///     <c>true</c> if the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> contains the
		///     <paramref name="value" />; otherwise <c>false</c>
		/// </returns>
		public bool ContainsValue(TValue value)
		{
			var enumerator = _dictionary.Values.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.Contains(value))
					{
						continue;
					}
					return true;
				}
				return false;
			}
			finally
			{
				((IDisposable) enumerator).Dispose();
			}
		}


		/// <summary>
		///     Creates a new new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the default initial capacity, and uses the default
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The
		///     internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <typeparam name="TValueCollection">
		///     The collection type that this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     will contain in its internal dictionary.
		/// </typeparam>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <typeparamref name="TValueCollection" /> must not have
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>()
			where TValueCollection : ICollection<TValue>, new()
		{
			if (new TValueCollection().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}

			var tKeys = new MultiValueDictionary<TKey, TValue>
			{
				_newCollectionFactory = () => new TValueCollection()
			};

			return tKeys;
		}

		/// <summary>
		///     Creates a new new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the specified initial capacity, and uses the default
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The
		///     internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <typeparam name="TValueCollection">
		///     The collection type that this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     will contain in its internal dictionary.
		/// </typeparam>
		/// <param name="capacity">
		///     Initial number of keys that the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> will allocate space for
		/// </param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
		/// <exception cref="T:System.InvalidOperationException">
		///     <typeparamref name="TValueCollection" /> must not have
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(int capacity)
			where TValueCollection : ICollection<TValue>, new()
		{
			Requires.Range(capacity >= 0, "capacity");

			if (new TValueCollection().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}

			var tKeys = new MultiValueDictionary<TKey, TValue>(capacity)
			{
				_newCollectionFactory = () => new TValueCollection()
			};

			return tKeys;
		}

		/// <summary>
		///     Creates a new new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the default initial capacity, and uses the specified
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The
		///     internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <typeparam name="TValueCollection">
		///     The collection type that this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     will contain in its internal dictionary.
		/// </typeparam>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <exception cref="T:System.InvalidOperationException">
		///     <typeparamref name="TValueCollection" /> must not have
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(IEqualityComparer<TKey> comparer)
			where TValueCollection : ICollection<TValue>, new()
		{
			if (new TValueCollection().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}

			var tKeys = new MultiValueDictionary<TKey, TValue>(comparer)
			{
				_newCollectionFactory = () => new TValueCollection()
			};

			return tKeys;
		}

		/// <summary>
		///     Creates a new new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the specified initial capacity, and uses the specified
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The
		///     internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <typeparam name="TValueCollection">
		///     The collection type that this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     will contain in its internal dictionary.
		/// </typeparam>
		/// <param name="capacity">
		///     Initial number of keys that the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> will allocate space for
		/// </param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <typeparamref name="TValueCollection" /> must not have
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(int capacity,
			IEqualityComparer<TKey> comparer)
			where TValueCollection : ICollection<TValue>, new()
		{
			Requires.Range(capacity >= 0, "capacity");

			if (new TValueCollection().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}

			var tKeys = new MultiValueDictionary<TKey, TValue>(capacity, comparer)
			{
				_newCollectionFactory = () => new TValueCollection()
			};

			return tKeys;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
		///     and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the
		///     <typeparamref name="TKey" /> type.
		///     The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <typeparam name="TValueCollection">
		///     The collection type that this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     will contain in its internal dictionary.
		/// </typeparam>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <typeparamref name="TValueCollection" /> must not have
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(
			IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable)
			where TValueCollection : ICollection<TValue>, new()
		{
			Requires.NotNullAllowStructs(enumerable, "enumerable");
			if (new TValueCollection().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}

			var tKeys = new MultiValueDictionary<TKey, TValue>
			{
				_newCollectionFactory = () => new TValueCollection()
			};

			foreach (var keyValuePair in enumerable)
			{
				tKeys.AddRange(keyValuePair.Key, keyValuePair.Value);
			}
			return tKeys;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
		///     and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the
		///     <typeparamref name="TKey" /> type.
		///     The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <typeparam name="TValueCollection">
		///     The collection type that this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     will contain in its internal dictionary.
		/// </typeparam>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <typeparamref name="TValueCollection" /> must not have
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(
			IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable, IEqualityComparer<TKey> comparer)
			where TValueCollection : ICollection<TValue>, new()
		{
			Requires.NotNullAllowStructs(enumerable, "enumerable");

			if (new TValueCollection().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}

			var tKeys = new MultiValueDictionary<TKey, TValue>(comparer)
			{
				_newCollectionFactory = () => new TValueCollection()
			};

			foreach (var keyValuePair in enumerable)
			{
				tKeys.AddRange(keyValuePair.Key, keyValuePair.Value);
			}
			return tKeys;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;&lt;TKey, TValue&gt;&gt;
		///     and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the
		///     <typeparamref name="TKey" /> type.
		///     The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <typeparam name="TValueCollection">
		///     The collection type that this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     will contain in its internal dictionary.
		/// </typeparam>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <typeparamref name="TValueCollection" /> must not have
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(
			IEnumerable<IGrouping<TKey, TValue>> enumerable)
			where TValueCollection : ICollection<TValue>, new()
		{
			Requires.NotNullAllowStructs(enumerable, "enumerable");

			if (new TValueCollection().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}

			var tKeys = new MultiValueDictionary<TKey, TValue>
			{
				_newCollectionFactory = () => new TValueCollection()
			};

			foreach (var tKeys1 in enumerable)
			{
				tKeys.AddRange(tKeys1.Key, tKeys1);
			}
			return tKeys;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;IGrouping&lt;TKey, TValue&gt;&gt;
		///     and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the
		///     <typeparamref name="TKey" /> type.
		///     The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <typeparam name="TValueCollection">
		///     The collection type that this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     will contain in its internal dictionary.
		/// </typeparam>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <typeparamref name="TValueCollection" /> must not have
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create<TValueCollection>(
			IEnumerable<IGrouping<TKey, TValue>> enumerable, IEqualityComparer<TKey> comparer)
			where TValueCollection : ICollection<TValue>, new()
		{
			Requires.NotNullAllowStructs(enumerable, "enumerable");
			if (new TValueCollection().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}

			var tKeys = new MultiValueDictionary<TKey, TValue>(comparer)
			{
				_newCollectionFactory = () => new TValueCollection()
			};

			foreach (var tKeys1 in enumerable)
			{
				tKeys.AddRange(tKeys1.Key, tKeys1);
			}
			return tKeys;
		}

		/// <summary>
		///     Creates a new new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the default initial capacity, and uses the default
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The
		///     internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <param name="collectionFactory">
		///     A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
		///     in the internal dictionary store of this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <paramref name="collectionFactory" /> must create collections with
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create(Func<ICollection<TValue>> collectionFactory)
		{
			if (collectionFactory().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}
			return new MultiValueDictionary<TKey, TValue>
			{
				_newCollectionFactory = collectionFactory
			};
		}

		/// <summary>
		///     Creates a new new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the specified initial capacity, and uses the default
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The
		///     internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <param name="capacity">
		///     Initial number of keys that the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> will allocate space for
		/// </param>
		/// <param name="collectionFactory">
		///     A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
		///     in the internal dictionary store of this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
		/// <exception cref="T:System.InvalidOperationException">
		///     <paramref name="collectionFactory" /> must create collections with
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create(int capacity,
			Func<ICollection<TValue>> collectionFactory)
		{
			Requires.Range(capacity >= 0, "capacity", null);
			if (collectionFactory().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}
			return new MultiValueDictionary<TKey, TValue>(capacity)
			{
				_newCollectionFactory = collectionFactory
			};
		}

		/// <summary>
		///     Creates a new new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the default initial capacity, and uses the specified
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The
		///     internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <param name="collectionFactory">
		///     A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
		///     in the internal dictionary store of this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </param>
		/// <exception cref="T:System.InvalidOperationException">
		///     <paramref name="collectionFactory" /> must create collections with
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create(IEqualityComparer<TKey> comparer,
			Func<ICollection<TValue>> collectionFactory)
		{
			if (collectionFactory().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}
			return new MultiValueDictionary<TKey, TValue>(comparer)
			{
				_newCollectionFactory = collectionFactory
			};
		}

		/// <summary>
		///     Creates a new new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />
		///     class that is empty, has the specified initial capacity, and uses the specified
		///     <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for <typeparamref name="TKey" />. The
		///     internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <param name="capacity">
		///     Initial number of keys that the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> will allocate space for
		/// </param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <param name="collectionFactory">
		///     A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
		///     in the internal dictionary store of this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <paramref name="collectionFactory" /> must create collections with
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">Capacity must be &gt;= 0</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create(int capacity,
			IEqualityComparer<TKey> comparer, Func<ICollection<TValue>> collectionFactory)
		{
			Requires.Range(capacity >= 0, "capacity", null);
			if (collectionFactory().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}
			return new MultiValueDictionary<TKey, TValue>(capacity, comparer)
			{
				_newCollectionFactory = collectionFactory
			};
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
		///     and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the
		///     <typeparamref name="TKey" /> type.
		///     The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <param name="collectionFactory">
		///     A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
		///     in the internal dictionary store of this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <paramref name="collectionFactory" /> must create collections with
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create(
			IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable, Func<ICollection<TValue>> collectionFactory)
		{
			Requires.NotNullAllowStructs(enumerable, "enumerable");
			if (collectionFactory().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}
			var tKeys = new MultiValueDictionary<TKey, TValue>
			{
				_newCollectionFactory = collectionFactory
			};
			foreach (var keyValuePair in enumerable)
			{
				tKeys.AddRange(keyValuePair.Key, keyValuePair.Value);
			}
			return tKeys;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;KeyValuePair&lt;TKey, IReadOnlyCollection&lt;TValue&gt;&gt;&gt;
		///     and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the
		///     <typeparamref name="TKey" /> type.
		///     The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <param name="collectionFactory">
		///     A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
		///     in the internal dictionary store of this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <paramref name="collectionFactory" /> must create collections with
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create(
			IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable, IEqualityComparer<TKey> comparer,
			Func<ICollection<TValue>> collectionFactory)
		{
			Requires.NotNullAllowStructs(enumerable, "enumerable");
			if (collectionFactory().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}
			var tKeys = new MultiValueDictionary<TKey, TValue>(comparer)
			{
				_newCollectionFactory = collectionFactory
			};
			foreach (var keyValuePair in enumerable)
			{
				tKeys.AddRange(keyValuePair.Key, keyValuePair.Value);
			}
			return tKeys;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;&lt;TKey, TValue&gt;&gt;
		///     and uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the
		///     <typeparamref name="TKey" /> type.
		///     The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <param name="collectionFactory">
		///     A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
		///     in the internal dictionary store of this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <paramref name="collectionFactory" /> must create collections with
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create(
			IEnumerable<IGrouping<TKey, TValue>> enumerable, Func<ICollection<TValue>> collectionFactory)
		{
			Requires.NotNullAllowStructs(enumerable, "enumerable");
			if (collectionFactory().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}
			var tKeys = new MultiValueDictionary<TKey, TValue>
			{
				_newCollectionFactory = collectionFactory
			};
			foreach (var tKeys1 in enumerable)
			{
				tKeys.AddRange(tKeys1.Key, tKeys1);
			}
			return tKeys;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> class
		///     that contains
		///     elements copied from the specified IEnumerable&lt;IGrouping&lt;TKey, TValue&gt;&gt;
		///     and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the
		///     <typeparamref name="TKey" /> type.
		///     The internal dictionary will use instances of the <typeparamref name="TValueCollection" />
		///     class as its collection type.
		/// </summary>
		/// <param name="enumerable">IEnumerable to copy elements into this from</param>
		/// <param name="comparer">Specified comparer to use for the <typeparamref name="TKey" />s</param>
		/// <param name="collectionFactory">
		///     A function to create a new <see cref="T:System.Collections.Generic.ICollection`1" /> to use
		///     in the internal dictionary store of this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </param>
		/// <returns>
		///     A new <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> with the specified
		///     parameters.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     <paramref name="collectionFactory" /> must create collections with
		///     IsReadOnly set to true by default.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">enumerable must be non-null</exception>
		/// <remarks>
		///     If <paramref name="comparer" /> is set to null, then the default
		///     <see cref="T:System.Collections.IEqualityComparer" /> for <typeparamref name="TKey" /> is used.
		/// </remarks>
		/// <remarks>
		///     Note that <typeparamref name="TValueCollection" /> must implement
		///     <see cref="T:System.Collections.Generic.ICollection`1" />
		///     in addition to being constructable through new(). The collection returned from the constructor
		///     must also not have IsReadOnly set to True by default.
		/// </remarks>
		public static MultiValueDictionary<TKey, TValue> Create(
			IEnumerable<IGrouping<TKey, TValue>> enumerable, IEqualityComparer<TKey> comparer,
			Func<ICollection<TValue>> collectionFactory)
		{
			Requires.NotNullAllowStructs(enumerable, "enumerable");
			if (collectionFactory().IsReadOnly)
			{
				throw new InvalidOperationException(Errors.Create_TValueCollectionReadOnly);
			}
			var tKeys = new MultiValueDictionary<TKey, TValue>(comparer)
			{
				_newCollectionFactory = collectionFactory
			};
			foreach (var tKeys1 in enumerable)
			{
				tKeys.AddRange(tKeys1.Key, tKeys1);
			}
			return tKeys;
		}

		/// <summary>
		///     Get an Enumerator over the <typeparamref name="TKey" />-
		///     <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" />
		///     pairs in this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		/// <returns>
		///     an Enumerator over the <typeparamref name="TKey" />-
		///     <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" />
		///     pairs in this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		///     Removes every <typeparamref name="TValue" /> associated with the given <typeparamref name="TKey" />
		///     from the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		/// <param name="key">The <typeparamref name="TKey" /> of the elements to remove</param>
		/// <returns><c>true</c> if the removal was successful; otherwise <c>false</c></returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
		public bool Remove(TKey key)
		{
			InnerCollectionView tKeys;
			Requires.NotNullAllowStructs(key, "key");
			if (!_dictionary.TryGetValue(key, out tKeys) ||
				!_dictionary.Remove(key))
			{
				return false;
			}
			var multiValueDictionary = this;
			multiValueDictionary._version = multiValueDictionary._version + 1;
			return true;
		}

		/// <summary>
		///     Removes the first instance (if any) of the given <typeparamref name="TKey" />-<typeparamref name="TValue" />
		///     pair from this <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" />.
		/// </summary>
		/// <param name="key">The <typeparamref name="TKey" /> of the element to remove</param>
		/// <param name="value">The <typeparamref name="TValue" /> of the element to remove</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
		/// <returns><c>true</c> if the removal was successful; otherwise <c>false</c></returns>
		/// <remarks>
		///     If the <typeparamref name="TValue" /> being removed is the last one associated with its
		///     <typeparamref name="TKey" />, then that
		///     <typeparamref name="TKey" /> will be removed from the
		///     <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> and its
		///     associated <see cref="T:System.Collections.Generic.IReadOnlyCollection`1" /> will be freed as if a call to
		///     <see cref="M:Std.Utility.Collections.Generic.MultiValueDictionary`2.Remove(`0)" />
		///     had been made.
		/// </remarks>
		public bool Remove(TKey key, TValue value)
		{
			InnerCollectionView tKeys;
			Requires.NotNullAllowStructs(key, "key");
			if (!_dictionary.TryGetValue(key, out tKeys) ||
				!tKeys.RemoveValue(value))
			{
				return false;
			}
			if (tKeys.Count == 0)
			{
				_dictionary.Remove(key);
			}
			var multiValueDictionary = this;
			multiValueDictionary._version = multiValueDictionary._version + 1;
			return true;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		///     Attempts to get the <typeparamref name="TValue" /> associated with the given
		///     <typeparamref name="TKey" /> and place it into <paramref name="value" />.
		/// </summary>
		/// <param name="key">The <typeparamref name="TKey" /> of the element to retrieve</param>
		/// <param name="value">
		///     When this method returns, contains the <typeparamref name="TValue" /> associated with the specified
		///     <typeparamref name="TKey" /> if it is found; otherwise contains the default value of <typeparamref name="TValue" />
		///     .
		/// </param>
		/// <returns>
		///     <c>true</c> if the <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> contains an element
		///     with the specified
		///     <typeparamref name="TKey" />; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> must be non-null</exception>
		public bool TryGetValue(TKey key, out IReadOnlyCollection<TValue> value)
		{
			InnerCollectionView tKeys;
			Requires.NotNullAllowStructs(key, "key");
			var flag = _dictionary.TryGetValue(key, out tKeys);
			value = tKeys;
			return flag;
		}

		/// <summary>
		///     The Enumerator class for a <see cref="T:System.Collections.Generic.MultiValueDictionary`2" />
		///     that iterates over <typeparamref name="TKey" />-<see cref="T:System.Collections.Generic.IReadOnlyCollection`1" />
		///     pairs.
		/// </summary>
		private class Enumerator : IEnumerator<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>, IEnumerator, IDisposable
		{
			private readonly MultiValueDictionary<TKey, TValue> _multiValueDictionary;

			private readonly int _version;

			private KeyValuePair<TKey, IReadOnlyCollection<TValue>> _current;

			private Dictionary<TKey, InnerCollectionView>.Enumerator _enumerator;

			private EnumerationState _state;

			public KeyValuePair<TKey, IReadOnlyCollection<TValue>> Current
			{
				get { return _current; }
			}

			Object IEnumerator.Current
			{
				get
				{
					switch (_state)
					{
						case EnumerationState.BeforeFirst:
						{
							throw new InvalidOperationException(Errors.Enumerator_BeforeCurrent);
						}
						case EnumerationState.During:
						{
							return _current;
						}
						case EnumerationState.AfterLast:
						{
							throw new InvalidOperationException(Errors.Enumerator_AfterCurrent);
						}
						default:
						{
							return _current;
						}
					}
				}
			}

			internal Enumerator(MultiValueDictionary<TKey, TValue> multiValueDictionary)
			{
				_multiValueDictionary = multiValueDictionary;
				_version = multiValueDictionary._version;
				_current = new KeyValuePair<TKey, IReadOnlyCollection<TValue>>();
				_enumerator = multiValueDictionary._dictionary.GetEnumerator();
				_state = EnumerationState.BeforeFirst;
			}

			public void Dispose()
			{
				_enumerator.Dispose();
			}

			public bool MoveNext()
			{
				if (_version != _multiValueDictionary._version)
				{
					throw new InvalidOperationException(Errors.Enumerator_Modification);
				}
				if (!_enumerator.MoveNext())
				{
					_current = new KeyValuePair<TKey, IReadOnlyCollection<TValue>>();
					_state = EnumerationState.AfterLast;
					return false;
				}
				var key = _enumerator.Current.Key;
				var current = _enumerator.Current;
				_current = new KeyValuePair<TKey, IReadOnlyCollection<TValue>>(key, current.Value);
				_state = EnumerationState.During;
				return true;
			}

			public void Reset()
			{
				if (_version != _multiValueDictionary._version)
				{
					throw new InvalidOperationException(Errors.Enumerator_Modification);
				}
				_enumerator.Dispose();
				_enumerator = _multiValueDictionary._dictionary.GetEnumerator();
				_current = new KeyValuePair<TKey, IReadOnlyCollection<TValue>>();
				_state = EnumerationState.BeforeFirst;
			}

			private enum EnumerationState
			{
				BeforeFirst,
				During,
				AfterLast
			}
		}

		/// <summary>
		///     An inner class that functions as a view of an ICollection within a MultiValueDictionary
		/// </summary>
		private class InnerCollectionView : ICollection<TValue>, IReadOnlyCollection<TValue>, IGrouping<TKey, TValue>,
			IEnumerable<TValue>, IEnumerable
		{
			private readonly TKey _key;

			private readonly ICollection<TValue> _collection;

			public int Count
			{
				get { return _collection.Count; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public TKey Key
			{
				get { return _key; }
			}

			public InnerCollectionView(TKey key, ICollection<TValue> collection)
			{
				_key = key;
				_collection = collection;
			}

			public void AddValue(TValue item)
			{
				_collection.Add(item);
			}

			public bool Contains(TValue item)
			{
				return _collection.Contains(item);
			}

			public void CopyTo(TValue[] array, int arrayIndex)
			{
				Requires.NotNullAllowStructs(array, "array");
				Requires.Range(arrayIndex >= 0, "arrayIndex", null);
				Requires.Range(arrayIndex <= array.Length, "arrayIndex", null);
				Requires.Argument(array.Length - arrayIndex >= _collection.Count, "arrayIndex",
					Errors.CopyTo_ArgumentsTooSmall);
				_collection.CopyTo(array, arrayIndex);
			}

			public IEnumerator<TValue> GetEnumerator()
			{
				return _collection.GetEnumerator();
			}

			public bool RemoveValue(TValue item)
			{
				return _collection.Remove(item);
			}

			void ICollection<TValue>.Add(TValue item)
			{
				throw new NotSupportedException(Errors.ReadOnly_Modification);
			}

			void ICollection<TValue>.Clear()
			{
				throw new NotSupportedException(Errors.ReadOnly_Modification);
			}

			bool ICollection<TValue>.Remove(TValue item)
			{
				throw new NotSupportedException(Errors.ReadOnly_Modification);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		/// <summary>
		///     A view of a <see cref="T:Std.Utility.Collections.Generic.MultiValueDictionary`2" /> as a read-only
		///     <see cref="T:System.Linq.ILookup`2" /> object
		/// </summary>
		private class MultiLookup : ILookup<TKey, TValue>, IEnumerable<IGrouping<TKey, TValue>>, IEnumerable
		{
			private readonly MultiValueDictionary<TKey, TValue> _multiValueDictionary;

			public int Count
			{
				get { return _multiValueDictionary._dictionary.Count; }
			}

			public IEnumerable<TValue> this[TKey key]
			{
				get
				{
					InnerCollectionView tKeys;
					if (_multiValueDictionary._dictionary.TryGetValue(key, out tKeys))
					{
						return tKeys;
					}
					return Enumerable.Empty<TValue>();
				}
			}

			internal MultiLookup(MultiValueDictionary<TKey, TValue> multiValueDictionary)
			{
				_multiValueDictionary = multiValueDictionary;
			}

			public bool Contains(TKey key)
			{
				Requires.NotNullAllowStructs(key, "key");
				return _multiValueDictionary._dictionary.ContainsKey(key);
			}

			public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
			{
				return new Enumerator(_multiValueDictionary);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(_multiValueDictionary);
			}

			private class Enumerator : IEnumerator<IGrouping<TKey, TValue>>, IEnumerator, IDisposable
			{
				private readonly MultiValueDictionary<TKey, TValue> _multiValueDictionary;

				private IGrouping<TKey, TValue> _current;

				private readonly int _version;

				private readonly IEnumerator<KeyValuePair<TKey, InnerCollectionView>> _enumerator;

				private EnumerationState _state;

				IGrouping<TKey, TValue> IEnumerator<IGrouping<TKey, TValue>>.Current
				{
					get { return _current; }
				}

				Object IEnumerator.Current
				{
					get
					{
						switch (_state)
						{
							case EnumerationState.BeforeFirst:
							{
								throw new InvalidOperationException(Errors.Enumerator_BeforeCurrent);
							}
							case EnumerationState.During:
							{
								return _current;
							}
							case EnumerationState.AfterLast:
							{
								throw new InvalidOperationException(Errors.Enumerator_AfterCurrent);
							}
							default:
							{
								return _current;
							}
						}
					}
				}

				internal Enumerator(MultiValueDictionary<TKey, TValue> multiValueDictionary)
				{
					_multiValueDictionary = multiValueDictionary;
					_enumerator = multiValueDictionary._dictionary.GetEnumerator();
					_version = multiValueDictionary._version;
					_current = _enumerator.Current.Value;
					_state = EnumerationState.BeforeFirst;
				}

				public void Dispose()
				{
					_enumerator.Dispose();
				}

				public bool MoveNext()
				{
					if (_version != _multiValueDictionary._version)
					{
						throw new InvalidOperationException(Errors.Enumerator_Modification);
					}
					if (!_enumerator.MoveNext())
					{
						_state = EnumerationState.AfterLast;
						return false;
					}
					_current = _enumerator.Current.Value;
					_state = EnumerationState.During;
					return true;
				}

				public void Reset()
				{
					if (_version != _multiValueDictionary._version)
					{
						throw new InvalidOperationException(Errors.Enumerator_Modification);
					}
					_state = EnumerationState.BeforeFirst;
					_enumerator.Reset();
					_current = _enumerator.Current.Value;
				}

				private enum EnumerationState
				{
					BeforeFirst,
					During,
					AfterLast
				}
			}
		}
	}
}