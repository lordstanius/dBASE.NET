using System;
using System.Text;

namespace dBASE.NET.Encoders
{
    internal class NullFlagsEncoder : Encoder
    {
        public NullFlagsEncoder(Encoding encoding) : base(encoding) { }

        public override byte[] Encode(DbfField field, object data)
        {
            return new byte[1];
        }

        public override object Parse(string value)
        {
            return new byte[1];
        }

        public override object Decode(ArraySegment<byte> bytes, DbfMemo memo)
        {
            return bytes.Array[bytes.Offset];
        }
    }
}
