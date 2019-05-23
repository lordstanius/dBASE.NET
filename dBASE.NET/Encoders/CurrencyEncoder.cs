using System;
using System.Globalization;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal class CurrencyEncoder : Encoder
    {
        public CurrencyEncoder(Encoding encoding) : base(encoding) { }

        public override byte[] Encode(DbfField field, object data)
        {
            if (data == null)
                return null;

            System.Diagnostics.Debug.Assert(field.Length == 8);
            return BitConverter.GetBytes((long)data);
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            return BitConverter.ToInt64(bytes.Array, bytes.Offset);
        }

        public override object Parse(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}