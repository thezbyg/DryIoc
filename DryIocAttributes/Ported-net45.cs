﻿/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace System.ComponentModel.Composition
{
    /// <summary>Can be imported by parts that wish to dynamically create instances of other parts.</summary>
    /// <typeparam name="T">The contract type of the created parts.</typeparam>
    public class ExportFactory<T>
    {
        /// <summary>Initializes a new instance of the <see cref="ExportFactory{T}"/> class.</summary>
        /// <param name="exportCreator">Action invoked upon calls to the Create() method.</param>
        public ExportFactory(Func<Collections.Generic.KeyValuePair<T, Action>> exportCreator)
        {
            if (exportCreator == null)
                throw new ArgumentNullException(nameof(exportCreator));
            _exportLifetimeContextCreator = exportCreator;
        }

        /// <summary>Create an instance of the exported part.</summary>
        /// <returns>A handle allowing the created part to be accessed then released.</returns>
        public ExportLifetimeContext<T> CreateExport()
        {
            var partAndDisposeAction = _exportLifetimeContextCreator();
            return new ExportLifetimeContext<T>(partAndDisposeAction.Key, partAndDisposeAction.Value);
        }

        private readonly Func<Collections.Generic.KeyValuePair<T, Action>> _exportLifetimeContextCreator;
    }

    /// <summary>An ExportFactory that provides metadata describing the created exports.</summary>
    /// <typeparam name="T">The contract type being created.</typeparam>
    /// <typeparam name="TMetadata">The metadata required from the export.</typeparam>
    public class ExportFactory<T, TMetadata> : ExportFactory<T>
    {
        /// <summary>
        /// Construct an ExportFactory.
        /// </summary>
        /// <param name="exportCreator">Action invoked upon calls to the Create() method.</param>
        /// <param name="metadata">The metadata associated with the export.</param>
        public ExportFactory(Func<Collections.Generic.KeyValuePair<T, Action>> exportCreator, TMetadata metadata)
            : base(exportCreator)
        {
            Metadata = metadata;
        }

        /// <summary>The metadata associated with the export.</summary>
        public TMetadata Metadata { get; }
    }

    /// <summary>A handle allowing the graph of parts associated with an exported instance to be released.</summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ExportLifetimeContext<T> : IDisposable
    {
        /// <summary>Initializes a new instance of the <see cref="ExportLifetimeContext{T}"/> class.</summary>
        /// <param name="value">The value of the export.</param>
        /// <param name="disposeAction">An action that releases resources associated with the export.</param>
        public ExportLifetimeContext(T value, Action disposeAction)
        {
            Value = value;
            _disposeAction = disposeAction;
        }

        /// <summary>The exported value.</summary>
        public T Value { get; }

        /// <summary>Release the parts associated with the exported value.</summary>
        public void Dispose() => _disposeAction?.Invoke();

        private readonly Action _disposeAction;
    }
}