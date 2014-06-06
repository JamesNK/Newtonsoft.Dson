#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Dson;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Newtonsoft.Dson.Tests
{
    [TestFixture]
    public class DsonWriterTests
    {
        [Test]
        public void SerializeObject()
        {
            string dson = DsonConvert.SerializeObject(new { hello = "world" }, Formatting.Indented);

            Assert.AreEqual(@"such
  ""hello"" is ""world""
wow", dson);
        }

        [Test]
        public void SerializeArray()
        {
            string dson = DsonConvert.SerializeObject(new { hello = "world", people = new[] { "James", "Brendon", "Amy" } });
            
            Assert.AreEqual(@"such ""hello"" is ""world"" next ""people"" is many ""James"" next ""Brendon"" next ""Amy"" many wow", dson);
        }

        [Test]
        public void SerializeArrayWithFormatting()
        {
            string dson = DsonConvert.SerializeObject(new { hello = "world", people = new[] { "James", "Brendon", "Amy" } }, Formatting.Indented);

            Assert.AreEqual(@"such
  ""hello"" is ""world"" next
  ""people"" is many
    ""James"" next
    ""Brendon"" next
    ""Amy""
  many
wow", dson);
        }

        [Test]
        public void SerializeByteArray()
        {
            string dson = DsonConvert.SerializeObject(new { hello = "world", people = Encoding.UTF8.GetBytes("how now brown cow") });

            Assert.AreEqual(@"such ""hello"" is ""world"" next ""people"" is ""aG93IG5vdyBicm93biBjb3c="" wow", dson);
        }
    }
}
