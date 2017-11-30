// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace BehaviorAnalysis.Jobs
{
    internal abstract class ReaderWriterCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _cache;
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        protected ReaderWriterCache()
            : this(null)
        {

        }

        protected ReaderWriterCache(IEqualityComparer<TKey> comparer)
        {
            _cache = new Dictionary<TKey, TValue>(comparer);
        }

        protected Dictionary<TKey, TValue> Cache
        {
            get { return _cache; }
        }

        protected TValue FetchOrCreateItem(TKey key, Func<TValue> creator)
        {
            // Passing the delegate as an argument allows the inline delegate to be static
            return FetchOrCreateItem(key, (Func<TValue> innerCreator) => innerCreator(), creator);
        }

        protected TValue FetchOrCreateItem<TArgument>(TKey key, Func<TArgument, TValue> creator, TArgument state)
        {
            // first, see if the item already exists in the cache
            _readerWriterLock.EnterReadLock();
            try
            {
                TValue existingEntry;
                if (_cache.TryGetValue(key, out existingEntry))
                {
                    return existingEntry;
                }
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }

            // insert the new item into the cache
            TValue newEntry = creator(state);
            _readerWriterLock.EnterWriteLock();
            try
            {
                TValue existingEntry;
                if (_cache.TryGetValue(key, out existingEntry))
                {
                    // another thread already inserted an item, so use that one
                    return existingEntry;
                }

                _cache[key] = newEntry;
                return newEntry;
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }
    }
}
