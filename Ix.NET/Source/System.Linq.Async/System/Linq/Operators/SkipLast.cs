﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerable
    {
        /// <summary>
        /// Bypasses a specified number of elements at the end of an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">Source sequence.</param>
        /// <param name="count">Number of elements to bypass at the end of the source sequence.</param>
        /// <returns>An async-enumerable sequence containing the source sequence elements except for the bypassed ones at the end.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero.</exception>
        /// <remarks>
        /// This operator accumulates a queue with a length enough to store the first <paramref name="count"/> elements. As more elements are
        /// received, elements are taken from the front of the queue and produced on the result sequence. This causes elements to be delayed.
        /// </remarks>
        public static IAsyncEnumerable<TSource> SkipLast<TSource>(this IAsyncEnumerable<TSource> source, int count)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));

            if (count <= 0)
            {
                // Return source if not actually skipping, but only if it's a type from here, to avoid
                // issues if collections are used as keys or otherwise must not be aliased.
                if (source is AsyncIteratorBase<TSource>)
                {
                    return source;
                }

                count = 0;
            }

            return Core(source, count);

            static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source, int count, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                var q = new Queue<TSource>();

                await foreach (var x in source.WithCancellation(cancellationToken))
                {
                    q.Enqueue(x);

                    if (q.Count > count)
                        yield return q.Dequeue();
                }
            }
        }
    }
}
