using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace dBASE.NET
{
    /// <summary>
    /// DbfRecord encapsulates a record in a .dbf file. It contains an array with
    /// data (as an Object) for each field.
    /// </summary>
    public class DbfRecord : IEquatable<DbfRecord>
    {
        private const char Delimiter = '·';

        private const byte DeletedRecordMarker = 0x2A;
        private const byte ActiveRecordMarker = 0x20;

        private readonly List<DbfField> _fields;
        private byte _marker = ActiveRecordMarker;

        internal DbfRecord(Dbf dbf, BinaryReader reader)
        {
            Data = new List<object>(dbf.Fields.Count);

            // Read record marker.
            _marker = reader.ReadByte();
            _fields = dbf.Fields;

            // Read entire record as sequence of bytes.
            // Note that record length includes marker.
            byte[] row = reader.ReadBytes(dbf.Header.RecordLength - 1);

            // Read data for each field.
            int offset = 0;
            foreach (DbfField field in dbf.Fields)
            {
                var bytes = new ArraySegment<byte>(row, offset, field.Length);
                Data.Add(field.Encoder.Decode(bytes, dbf.Memo));
                offset += field.Length;
            }
        }

        public DbfRecord(List<DbfField> fields, string serializedRecord)
        {
            var data = serializedRecord.Split(Delimiter);
            _fields = fields;
            _marker = byte.Parse(data[0], CultureInfo.InvariantCulture);
            Data = new List<object>(fields.Count);

            for (int i = 0; i < fields.Count; i++)
                Data.Add(fields[i].Encoder.Parse(Encoders.Encoder.UnescapeString(data[i + 1])));
        }

        /// <summary>
        /// Create an empty record.
        /// </summary>
        public DbfRecord(List<DbfField> fields)
        {
            _fields = fields;
            Data = new List<object>(fields.Count) { null };
        }

        public bool IsDeleted => _marker == DeletedRecordMarker;

        public List<object> Data { get; }

        public object this[int index] => Data[index];

        public object this[string name]
        {
            get => GetData(name);
            set
            {
                // TODO: Validate if object is compatible with field type
                Data[GetIndex(name)] = value;
            }
        }

        private int GetIndex(string fieldName) => _fields.FindIndex(x => x.Name.Equals(fieldName));

        internal void Delete()
        {
            _marker = DeletedRecordMarker;
        }

        internal void Undelete()
        {
            _marker = ActiveRecordMarker;
        }

        private object GetData(string fieldName)
        {
            int index = GetIndex(fieldName);
            if (index == -1)
                throw new IndexOutOfRangeException($"Field '{fieldName}' does not exist.");

            return Data[index];
        }

        public object this[DbfField field]
        {
            get => this[field.Name];
            set => this[field.Name] = value;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(_marker);

            int index = 0;
            foreach (DbfField field in _fields)
            {
                byte[] buffer = field.Encoder.Encode(field, Data[index]);
                writer.Write(buffer);
                index++;
            }
        }

        /// <summary>
        /// Returns culture-aware CSV string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var value in Data)
            {
                var stringValue = value?.ToString() ?? string.Empty;
                if (stringValue.Contains(","))
                    sb.AppendFormat("\"{0}\",", stringValue);
                else
                    sb.AppendFormat("{0},", stringValue);
            }

            if (IsDeleted)
                sb.Append("[deleted]");
            else
                --sb.Length;

            return sb.ToString();
        }

        public bool Equals(DbfRecord other)
        {
            if (other == null)
                return false;

            if (other.Data.Count != Data.Count)
                return false;

            var values = Data.GetEnumerator();
            var otherValues = other.Data.GetEnumerator();
            while (values.MoveNext() && otherValues.MoveNext())
                if (!Equals(values.Current, otherValues.Current))
                    return false;

            return true;
        }

        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}" + Delimiter, _marker);
            for (int i = 0; i < Data.Count; ++i)
                sb.AppendFormat(
                    "{0}" + Delimiter,
                    Encoders.Encoder.EscapeString(_fields[i].Encoder.ToString(Data[i])));

            --sb.Length;

            return sb.ToString();
        }
    }
}
