// ObservableDictionary.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Collections;

namespace Print4e.Common
{
	/// <summary>
	///    Implementation of IObservableMap that supports reentrancy for use as a default view
	///    model.
	/// </summary>
	// ReSharper disable InconsistentNaming
	public class ObservableDictionary<K, V> : IObservableMap<K, V> // ReSharper restore InconsistentNaming
	{
		private class ObservableDictionaryChangedEventArgs : IMapChangedEventArgs<K>
		{
			public ObservableDictionaryChangedEventArgs(CollectionChange change, K key)
			{
				CollectionChange = change;
				Key = key;
			}

			public CollectionChange CollectionChange { get; private set; }
			public K Key { get; private set; }
		}

		private readonly Dictionary<K, V> _dictionary = new Dictionary<K, V>();
		public event MapChangedEventHandler<K, V> MapChanged;

		private void InvokeMapChanged(CollectionChange change, K key)
		{
			var eventHandler = MapChanged;
			if (eventHandler != null) eventHandler(this, new ObservableDictionaryChangedEventArgs(change, key));
		}

		public void Add(K key, V value)
		{
			_dictionary.Add(key, value);
			InvokeMapChanged(CollectionChange.ItemInserted, key);
		}

		public void Add(KeyValuePair<K, V> item)
		{
			Add(item.Key, item.Value);
		}

		public bool Remove(K key)
		{
			if (_dictionary.Remove(key))
			{
				InvokeMapChanged(CollectionChange.ItemRemoved, key);
				return true;
			}
			return false;
		}

		public bool Remove(KeyValuePair<K, V> item)
		{
			V currentValue;
			if (_dictionary.TryGetValue(item.Key, out currentValue) && Equals(item.Value, currentValue)
				&& _dictionary.Remove(item.Key))
			{
				InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
				return true;
			}
			return false;
		}

		public V this[K key]
		{
			get { return _dictionary[key]; }
			set
			{
				_dictionary[key] = value;
				InvokeMapChanged(CollectionChange.ItemChanged, key);
			}
		}

		public void Clear()
		{
			var priorKeys = _dictionary.Keys.ToArray();
			_dictionary.Clear();
			foreach (var key in priorKeys)
			{
				InvokeMapChanged(CollectionChange.ItemRemoved, key);
			}
		}

		public ICollection<K> Keys { get { return _dictionary.Keys; } }

		public bool ContainsKey(K key)
		{
			return _dictionary.ContainsKey(key);
		}

		public bool TryGetValue(K key, out V value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public ICollection<V> Values { get { return _dictionary.Values; } }

		public bool Contains(KeyValuePair<K, V> item)
		{
			return _dictionary.Contains(item);
		}

		public int Count { get { return _dictionary.Count; } }

		public bool IsReadOnly { get { return false; } }

		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
		{
			var arraySize = array.Length;
			foreach (var pair in _dictionary)
			{
				if (arrayIndex >= arraySize) break;
				array[arrayIndex++] = pair;
			}
		}
	}
}
