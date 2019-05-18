using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal class MemoEncoder : Encoder
    {
        public MemoEncoder(Encoding encoding) : base(encoding) { }

        public override byte[] Encode(DbfField field, object data)
        {
            var entry = (DbfMemoEntry)data;

            if (field.Length > 4)
                return data == null
                    ? Enumerable.Repeat((byte)' ', field.Length).ToArray()
                    : Encoding.GetBytes(entry.Index.ToString().PadLeft(field.Length));

            return data == null ? (new byte[4]) : BitConverter.GetBytes(entry.Index);
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            int index;
            // Memo fields of 5+ bytes in length store their index in text, e.g. "     39394"
            // Memo fields of 4 bytes store their index as an int.
            if (bytes.Count > 4)
            {
                string text = Encoding.ASCII.GetString(bytes.Array, bytes.Offset, bytes.Count).Trim();
                if (text.Length == 0)
                    return null;

                index = Convert.ToInt32(text);
            }
            else
            {
                index = BitConverter.ToInt32(bytes.Array, bytes.Offset);
                if (index == 0) return null;
            }

            return memo.GetMemo(index);
        }

        public override object Parse(string value)
        {
            return new DbfMemoEntry(
                index: int.Parse(value.Substring(value.LastIndexOf('@') + 1), CultureInfo.InvariantCulture),
                value: value.Remove(value.LastIndexOf('@')));
        }

        public override string ToString(object value)
        {
            var memoEntry = value as DbfMemoEntry;

            return string.Format(CultureInfo.InvariantCulture, "{0}@{1}", memoEntry.Value, memoEntry.Index);
        }
    }
}
