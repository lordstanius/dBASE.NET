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
            float value = 0;
            if (data != null)
                value = (float)data;

            return BitConverter.GetBytes(value);
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            return BitConverter.ToSingle(bytes.Array, bytes.Offset);
        }

        public override object Parse(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}