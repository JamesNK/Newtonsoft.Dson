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
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating DSON data.
    /// </summary>
    public class DsonWriter : JsonWriter
    {
        private readonly TextWriter _textWriter;
        private bool _hasContent;
        private char _indentChar;
        private int _indentation;

        /// <summary>
        /// Gets or sets how many IndentChars to write for each level in the hierarchy when <see cref="Formatting"/> is set to <c>Formatting.Indented</c>.
        /// </summary>
        public int Indentation
        {
            get { return _indentation; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Indentation value must be greater than 0.");

                _indentation = value;
            }
        }

        /// <summary>
        /// Gets or sets which character to use for indenting when <see cref="Formatting"/> is set to <c>Formatting.Indented</c>.
        /// </summary>
        public char IndentChar
        {
            get { return _indentChar; }
            set { _indentChar = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsonWriter"/> class.
        /// </summary>
        /// <param name="textWriter">The textWriter.</param>
        public DsonWriter(TextWriter textWriter)
        {
            if (textWriter == null)
                throw new ArgumentNullException("textWriter");

            _textWriter = textWriter;
            _indentation = 2;
            _indentChar = ' ';
        }

        private void EnsureSpace()
        {
            if (_hasContent)
                _textWriter.Write(' ');
        }

        private void WriteInternal(string s)
        {
            _hasContent = true;
            _textWriter.Write(s);
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            _textWriter.Flush();
        }

        /// <summary>
        /// Writes out a comment <code>/*...*/</code> containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            throw new JsonWriterException("Cannot write JSON comment as DSON.");
        }

        /// <summary>
        /// Writes the start of a constructor with the given name.
        /// </summary>
        /// <param name="name">The name of the constructor.</param>
        public override void WriteStartConstructor(string name)
        {
            throw new JsonWriterException("Cannot write JSON constructor as DSON.");
        }

        /// <summary>
        /// Writes raw JSON.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRaw(string json)
        {
            throw new JsonWriterException("Cannot write raw JSON as DSON.");
        }

        /// <summary>
        /// Writes raw JSON where a value is expected and updates the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRawValue(string json)
        {
            throw new JsonWriterException("Cannot write raw JSON as DSON.");
        }

        /// <summary>
        /// Writes indent characters.
        /// </summary>
        protected override void WriteIndent()
        {
            _textWriter.WriteLine();

            // levels of indentation multiplied by the indent count
            int currentIndentCount = Top * _indentation;

            while (currentIndentCount > 0)
            {
                // write up to a max of 10 characters at once to avoid creating too many new strings
                int writeCount = Math.Min(currentIndentCount, 10);

                _textWriter.Write(new string(_indentChar, writeCount));

                currentIndentCount -= writeCount;
            }

            _hasContent = false;
        }

        /// <summary>
        /// Writes the beginning of a Json array.
        /// </summary>
        public override void WriteStartArray()
        {
            base.WriteStartArray();

            EnsureSpace();
            WriteInternal("many");
        }

        /// <summary>
        /// Writes the beginning of a Json object.
        /// </summary>
        public override void WriteStartObject()
        {
            base.WriteStartObject();

            EnsureSpace();
            WriteInternal("such");
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a Json object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public override void WritePropertyName(string name)
        {
            base.WritePropertyName(name);

            EnsureSpace();
            WriteInternal(JsonConvert.ToString(name));
            WriteInternal(" is");
        }

        /// <summary>
        /// Closes this stream and the underlying stream.
        /// </summary>
        public override void Close()
        {
            base.Close();

            if (CloseOutput && _textWriter != null)
                _textWriter.Dispose();
        }

        /// <summary>
        /// Writes the specified end token.
        /// </summary>
        /// <param name="token">The end token to write.</param>
        protected override void WriteEnd(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                    EnsureSpace();
                    WriteInternal("wow");
                    break;
                case JsonToken.EndArray:
                    EnsureSpace();
                    WriteInternal("many");
                    break;
                case JsonToken.EndConstructor:
                    throw new JsonWriterException("Cannot write JSON constructor as DSON.");
                default:
                    throw new JsonWriterException("Invalid JsonToken: " + token);
            }
        }

        protected override void WriteValueDelimiter()
        {
            base.WriteValueDelimiter();
            WriteInternal(" next");
        }

        #region WriteValue methods
        /// <summary>
        /// Writes a null value.
        /// </summary>
        public override void WriteNull()
        {
            base.WriteNull();
            EnsureSpace();
            WriteInternal("nullish");
        }

        /// <summary>
        /// Writes an undefined value.
        /// </summary>
        public override void WriteUndefined()
        {
            throw new JsonWriterException("Cannot write Undefined as DSON.");
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public override void WriteValue(string value)
        {
            base.WriteValue(value);
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                EnsureSpace();
                WriteInternal(JsonConvert.ToString(value));
            }
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public override void WriteValue(int value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public override void WriteValue(long value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ulong value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public override void WriteValue(float value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public override void WriteValue(double value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to write.</param>
        public override void WriteValue(bool value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(value ? "notfalse" : "nottrue");
        }

        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public override void WriteValue(short value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Char"/> value to write.</param>
        public override void WriteValue(char value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public override void WriteValue(byte value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        public override void WriteValue(DateTime value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

#if !NET20
        /// <summary>
        /// Writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        public override void WriteValue(DateTimeOffset value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }
#endif

        /// <summary>
        /// Writes a <see cref="T:Byte[]"/> value.
        /// </summary>
        /// <param name="value">The <see cref="T:Byte[]"/> value to write.</param>
        public override void WriteValue(byte[] value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(@"""" + Convert.ToBase64String(value) + @"""");
        }

        /// <summary>
        /// Writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        public override void WriteValue(Guid value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        public override void WriteValue(TimeSpan value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        public override void WriteValue(Uri value)
        {
            base.WriteValue(value);
            EnsureSpace();
            WriteInternal(Convert.ToString(value));
        }
        #endregion
    }
}