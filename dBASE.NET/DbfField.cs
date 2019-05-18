using System;
using System.IO;
using System.Text;

namespace dBASE.NET
{
    /// <summary>
    /// Encapsulates a field descriptor in a .dbf file.
    /// </summary>
    public class DbfField
    {
        private readonly byte[] _reserved1 = new byte[4];
        private readonly byte[] _reserved2 = new byte[2];
        private readonly byte[] _reserved3 = new byte[2];
        private readonly byte[] _reserved4 = new byte[8];

        internal DbfField(string name, DbfFieldType type, int length, Encoding encoding)
        {
            Name = name;
            Type = type;
            Length = (byte)length;
            Encoder = GetEncoder(Type, encoding);
        }

        internal DbfField(BinaryReader reader, Encoding encoding)
        {
            Name = Encoding.ASCII.GetString(reader.ReadBytes(11)).TrimEnd((char)0);
            Type = (DbfFieldType)reader.ReadByte();
            _reserved1 = reader.ReadBytes(4);
            Length = reader.ReadByte();
            Precision = reader.ReadByte();
            _reserved2 = reader.ReadBytes(2); // reserved.
            WorkAreaID = reader.ReadByte();
            _reserved3 = reader.ReadBytes(2); // reserved.
            Flags = reader.ReadByte();
            _reserved4 = reader.ReadBytes(8);
            Encoder = GetEncoder(Type, encoding);
        }

        /// <summary>
        /// Field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Field type
        /// </summary>
        public DbfFieldType Type { get; }

        internal Encoders.Encoder Encoder { get; }

        /// <summary>
        /// Length of field in bytes
        /// </summary>
        public byte Length { get; }

        public byte Precision { get; set; } = 2;
        public byte WorkAreaID { get; set; }
        public byte Flags { get; set; }

        private Encoders.Encoder GetEncoder(DbfFieldType type, Encoding encoding)
        {
            switch (type)
            {
                case DbfFieldType.Character: return new Encoders.CharacterEncoder(encoding);
                case DbfFieldType.Currency: return new Encoders.CurrencyEncoder(encoding);
                case DbfFieldType.Date: return new Encoders.DateEncoder(encoding);
                case DbfFieldType.DateTime: return new Encoders.DateTimeEncoder(encoding);
                case DbfFieldType.Integer: return new Encoders.IntegerEncoder(encoding);
                case DbfFieldType.Float: return new Encoders.FloatEncoder(encoding);
                case DbfFieldType.Double: return new Encoders.DoubleEncoder(encoding);
                case DbfFieldType.Numeric: return new Encoders.NumericEncoder(encoding);
                case DbfFieldType.Logical: return new Encoders.LogicalEncoder(encoding);
                case DbfFieldType.NullFlags: return new Encoders.NullFlagsEncoder(encoding);
                case DbfFieldType.Memo: return new Encoders.MemoEncoder(encoding);
                default:
                    throw new ArgumentException($"No encoder found for dBASE type '{type}'.");
            }
        }

        internal void Write(BinaryWriter writer)
        {
            // Pad field name with 0-bytes, then save it.
            string name = this.Name;
            if (name.Length > 11) name = name.Substring(0, 11);
            while (name.Length < 11) name += '\0';
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            writer.Write(nameBytes);

            writer.Write((char)Type);
            writer.Write(_reserved1); // 4 reserved bytes.
            writer.Write(Length);
            writer.Write(Precision);
            writer.Write(_reserved2);
            writer.Write(WorkAreaID);
            writer.Write(_reserved3);
            writer.Write(Flags);
            writer.Write(_reserved4);
        }
    }
}
