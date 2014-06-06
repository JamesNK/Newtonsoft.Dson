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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Newtonsoft.Dson
{
    public static class DsonConvert
    {
        #region Serialize
        /// <summary>
        /// Serializes the specified object to a DSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A DSON string representation of the object.</returns>
        public static string SerializeObject(object value)
        {
            return SerializeObject(value, null, (JsonSerializerSettings)null);
        }

        /// <summary>
        /// Serializes the specified object to a DSON string using formatting.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output is formatted.</param>
        /// <returns>
        /// A DSON string representation of the object.
        /// </returns>
        public static string SerializeObject(object value, Formatting formatting)
        {
            return SerializeObject(value, formatting, (JsonSerializerSettings)null);
        }

        /// <summary>
        /// Serializes the specified object to a DSON string using a collection of <see cref="JsonConverter"/>.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="converters">A collection converters used while serializing.</param>
        /// <returns>A DSON string representation of the object.</returns>
        public static string SerializeObject(object value, params JsonConverter[] converters)
        {
            JsonSerializerSettings settings = (converters != null && converters.Length > 0)
                ? new JsonSerializerSettings { Converters = converters }
                : null;

            return SerializeObject(value, null, settings);
        }

        /// <summary>
        /// Serializes the specified object to a DSON string using formatting and a collection of <see cref="JsonConverter"/>.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output is formatted.</param>
        /// <param name="converters">A collection converters used while serializing.</param>
        /// <returns>A DSON string representation of the object.</returns>
        public static string SerializeObject(object value, Formatting formatting, params JsonConverter[] converters)
        {
            JsonSerializerSettings settings = (converters != null && converters.Length > 0)
                ? new JsonSerializerSettings { Converters = converters }
                : null;

            return SerializeObject(value, null, formatting, settings);
        }

        /// <summary>
        /// Serializes the specified object to a DSON string using <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
        /// If this is null, default serialization settings will be used.</param>
        /// <returns>
        /// A DSON string representation of the object.
        /// </returns>
        public static string SerializeObject(object value, JsonSerializerSettings settings)
        {
            return SerializeObject(value, null, settings);
        }

        /// <summary>
        /// Serializes the specified object to a DSON string using a type, formatting and <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
        /// If this is null, default serialization settings will be used.</param>
        /// <param name="type">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="TypeNameHandling"/> is Auto to write out the type name if the type of the value does not match.
        /// Specifing the type is optional.
        /// </param>
        /// <returns>
        /// A DSON string representation of the object.
        /// </returns>
        public static string SerializeObject(object value, Type type, JsonSerializerSettings settings)
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);

            return SerializeObjectInternal(value, type, jsonSerializer);
        }

        /// <summary>
        /// Serializes the specified object to a DSON string using formatting and <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output is formatted.</param>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
        /// If this is null, default serialization settings will be used.</param>
        /// <returns>
        /// A DSON string representation of the object.
        /// </returns>
        public static string SerializeObject(object value, Formatting formatting, JsonSerializerSettings settings)
        {
            return SerializeObject(value, null, formatting, settings);
        }

        /// <summary>
        /// Serializes the specified object to a DSON string using a type, formatting and <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output is formatted.</param>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
        /// If this is null, default serialization settings will be used.</param>
        /// <param name="type">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="TypeNameHandling"/> is Auto to write out the type name if the type of the value does not match.
        /// Specifing the type is optional.
        /// </param>
        /// <returns>
        /// A DSON string representation of the object.
        /// </returns>
        public static string SerializeObject(object value, Type type, Formatting formatting, JsonSerializerSettings settings)
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
            jsonSerializer.Formatting = formatting;

            return SerializeObjectInternal(value, type, jsonSerializer);
        }

        private static string SerializeObjectInternal(object value, Type type, JsonSerializer jsonSerializer)
        {
            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (DsonWriter dsonWriter = new DsonWriter(sw))
            {
                dsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(dsonWriter, value, type);
            }

            return sw.ToString();
        }
        #endregion
    }
}
