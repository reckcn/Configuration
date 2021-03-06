// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Framework.ConfigurationModel.Test;
using Xunit;

namespace Microsoft.Framework.ConfigurationModel.Json.Test
{
    public class ArrayTest
    {
        [Fact]
        public void ArraysAreConvertedToKeyValuePairs()
        {
            var json = @"{
                'ip': [
                    '1.2.3.4',
                    '7.8.9.10',
                    '11.12.13.14'
                ]
            }";

            var jsonConfigSource = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource.Load(TestStreamHelpers.StringToStream(json));
            
            Assert.Equal("1.2.3.4", jsonConfigSource.Get("ip:0"));
            Assert.Equal("7.8.9.10", jsonConfigSource.Get("ip:1"));
            Assert.Equal("11.12.13.14", jsonConfigSource.Get("ip:2"));
        }

        [Fact]
        public void ArrayOfObjects()
        {
            var json = @"{
                'ip': [
                    {
                        'address': '1.2.3.4',
                        'hidden': false
                    },
                    {
                        'address': '5.6.7.8',
                        'hidden': true
                    }
                ]
            }";

            var jsonConfigSource = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource.Load(TestStreamHelpers.StringToStream(json));

            Assert.Equal("1.2.3.4", jsonConfigSource.Get("ip:0:address"));
            Assert.Equal("False", jsonConfigSource.Get("ip:0:hidden"));
            Assert.Equal("5.6.7.8", jsonConfigSource.Get("ip:1:address"));
            Assert.Equal("True", jsonConfigSource.Get("ip:1:hidden"));
        }

        [Fact]
        public void NestedArrays()
        {
            var json = @"{
                'ip': [
                    [ 
                        '1.2.3.4',
                        '5.6.7.8'
                    ],
                    [ 
                        '9.10.11.12',
                        '13.14.15.16'
                    ],
                ]
            }";

            var jsonConfigSource = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource.Load(TestStreamHelpers.StringToStream(json));

            Assert.Equal("1.2.3.4", jsonConfigSource.Get("ip:0:0"));
            Assert.Equal("5.6.7.8", jsonConfigSource.Get("ip:0:1"));
            Assert.Equal("9.10.11.12", jsonConfigSource.Get("ip:1:0"));
            Assert.Equal("13.14.15.16", jsonConfigSource.Get("ip:1:1"));
        }

        [Fact]
        public void ImplicitArrayItemReplacement()
        {
            var json1 = @"{
                'ip': [
                    '1.2.3.4',
                    '7.8.9.10',
                    '11.12.13.14'
                ]
            }";

            var json2 = @"{
                'ip': [
                    '15.16.17.18'
                ]
            }";

            var jsonConfigSource1 = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource1.Load(TestStreamHelpers.StringToStream(json1));

            var jsonConfigSource2 = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource2.Load(TestStreamHelpers.StringToStream(json2));

            var config = new Configuration();
            config.Add(jsonConfigSource1, load: false);
            config.Add(jsonConfigSource2, load: false);

            Assert.Equal(3, config.GetSubKeys("ip").Count());
            Assert.Equal("15.16.17.18", config.Get("ip:0"));
            Assert.Equal("7.8.9.10", config.Get("ip:1"));
            Assert.Equal("11.12.13.14", config.Get("ip:2"));
        }

        [Fact]
        public void ExplicitArrayReplacement()
        {
            var json1 = @"{
                'ip': [
                    '1.2.3.4',
                    '7.8.9.10',
                    '11.12.13.14'
                ]
            }";

            var json2 = @"{
                'ip': {
                    '1': '15.16.17.18'
                }
            }";

            var jsonConfigSource1 = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource1.Load(TestStreamHelpers.StringToStream(json1));

            var jsonConfigSource2 = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource2.Load(TestStreamHelpers.StringToStream(json2));

            var config = new Configuration();
            config.Add(jsonConfigSource1, load: false);
            config.Add(jsonConfigSource2, load: false);

            Assert.Equal(3, config.GetSubKeys("ip").Count());
            Assert.Equal("1.2.3.4", config.Get("ip:0"));
            Assert.Equal("15.16.17.18", config.Get("ip:1"));
            Assert.Equal("11.12.13.14", config.Get("ip:2"));
        }

        [Fact]
        public void ArrayMerge()
        {
            var json1 = @"{
                'ip': [
                    '1.2.3.4',
                    '7.8.9.10',
                    '11.12.13.14'
                ]
            }";

            var json2 = @"{
                'ip': {
                    '3': '15.16.17.18'
                }
            }";

            var jsonConfigSource1 = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource1.Load(TestStreamHelpers.StringToStream(json1));

            var jsonConfigSource2 = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource2.Load(TestStreamHelpers.StringToStream(json2));

            var config = new Configuration();
            config.Add(jsonConfigSource1, load: false);
            config.Add(jsonConfigSource2, load: false);

            Assert.Equal(4, config.GetSubKeys("ip").Count());
            Assert.Equal("1.2.3.4", config.Get("ip:0"));
            Assert.Equal("7.8.9.10", config.Get("ip:1"));
            Assert.Equal("11.12.13.14", config.Get("ip:2"));
            Assert.Equal("15.16.17.18", config.Get("ip:3"));
        }

        [Fact]
        public void ArraysAreKeptInFileOrder()
        {
            var json = @"{
                'setting': [
                    'b',
                    'a',
                    '2'
                ]
            }";

            var jsonConfigSource = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource.Load(TestStreamHelpers.StringToStream(json));

            var config = new Configuration();
            config.Add(jsonConfigSource, load: false);

            var subkey = config.GetSubKey("setting");
            var indexSubkeys = subkey.GetSubKeys().ToArray();

            Assert.Equal(3, indexSubkeys.Count());
            Assert.Equal("b", indexSubkeys[0].Value.Get(null));
            Assert.Equal("a", indexSubkeys[1].Value.Get(null));
            Assert.Equal("2", indexSubkeys[2].Value.Get(null));
        }

        [Fact]
        public void PropertiesAreSortedByNumberOnlyFirst()
        {
            var json = @"{
                'setting': {
                    'hello': 'a',
                    'bob': 'b',
                    '42': 'c',
                    '4':'d',
                    '10': 'e',
                    '1text': 'f',
                }
            }";

            var jsonConfigSource = new JsonConfigurationSource(TestStreamHelpers.ArbitraryFilePath);
            jsonConfigSource.Load(TestStreamHelpers.StringToStream(json));

            var config = new Configuration();
            config.Add(jsonConfigSource, load: false);

            var subkey = config.GetSubKey("setting");
            var indexSubkeys = subkey.GetSubKeys().ToArray();

            Assert.Equal(6, indexSubkeys.Count());
            Assert.Equal("4", indexSubkeys[0].Key);
            Assert.Equal("10", indexSubkeys[1].Key);
            Assert.Equal("42", indexSubkeys[2].Key);
            Assert.Equal("1text", indexSubkeys[3].Key);
            Assert.Equal("bob", indexSubkeys[4].Key);
            Assert.Equal("hello", indexSubkeys[5].Key);
        }
    }
}
