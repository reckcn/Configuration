// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.ConfigurationModel.Internal;
using Microsoft.Framework.Internal;

namespace Microsoft.Framework.ConfigurationModel
{
    public class Configuration : IConfiguration, IConfigurationSourceRoot
    {
        private readonly IList<IConfigurationSource> _sources = new List<IConfigurationSource>();

        public Configuration(params IConfigurationSource[] sources)
            : this(null, sources)
        {
        }

        public Configuration(string basePath, params IConfigurationSource[] sources)
        {
            if (sources != null)
            {
                foreach (var singleSource in sources)
                {
                    Add(singleSource);
                }
            }

            BasePath = basePath;
        }

        public string this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }

        public IEnumerable<IConfigurationSource> Sources
        {
            get
            {
                return _sources;
            }
        }

        public string BasePath
        {
            get;
        }

        public string Get([NotNull] string key)
        {
            string value;
            return TryGet(key, out value) ? value : null;
        }

        public bool TryGet([NotNull] string key, out string value)
        {
            // If a key in the newly added configuration source is identical to a key in a 
            // formerly added configuration source, the new one overrides the former one.
            // So we search in reverse order, starting with latest configuration source.
            foreach (var src in _sources.Reverse())
            {
                if (src.TryGet(key, out value))
                {
                    return true;
                }
            }
            value = null;
            return false;
        }


        public void Set([NotNull] string key, [NotNull] string value)
        {
            foreach (var src in _sources)
            {
                src.Set(key, value);
            }
        }

        public void Reload()
        {
            foreach (var src in _sources)
            {
                src.Load();
            }
        }

        public IConfiguration GetSubKey(string key)
        {
            return new ConfigurationFocus(this, key + Constants.KeyDelimiter);
        }

        public IEnumerable<KeyValuePair<string, IConfiguration>> GetSubKeys()
        {
            return GetSubKeysImplementation(string.Empty);
        }

        public IEnumerable<KeyValuePair<string, IConfiguration>> GetSubKeys([NotNull] string key)
        {
            return GetSubKeysImplementation(key + Constants.KeyDelimiter);
        }

        private IEnumerable<KeyValuePair<string, IConfiguration>> GetSubKeysImplementation(string prefix)
        {
            var segments = _sources.Aggregate(
                Enumerable.Empty<string>(),
                (seed, source) => source.ProduceSubKeys(seed, prefix, Constants.KeyDelimiter));

            var distinctSegments = segments.Distinct();

            return distinctSegments.Select(segment => CreateConfigurationFocus(prefix, segment));
        }

        private KeyValuePair<string, IConfiguration> CreateConfigurationFocus(string prefix, string segment)
        {
            return new KeyValuePair<string, IConfiguration>(
                segment,
                new ConfigurationFocus(this, prefix + segment + Constants.KeyDelimiter));
        }

        /// <summary>
        /// Adds a new configuration source.
        /// </summary>
        /// <param name="configurationSource">The configuration source to add.</param>
        /// <returns>The same configuration source.</returns>
        public IConfigurationSourceRoot Add(IConfigurationSource configurationSource)
        {
            return Add(configurationSource, load: true);
        }

        /// <summary>
        /// Adds a new configuration source.
        /// </summary>
        /// <param name="configurationSource">The configuration source to add.</param>
        /// <param name="load">If true, the configuration source's <see cref="IConfigurationSource.Load"/> method will be called.</param>
        /// <returns>The same configuration source.</returns>
        /// <remarks>This method is intended only for test scenarios.</remarks>
        public IConfigurationSourceRoot Add(IConfigurationSource configurationSource, bool load)
        {
            if (load)
            {
                configurationSource.Load();
            }
            _sources.Add(configurationSource);
            return this;
        }
    }
}
