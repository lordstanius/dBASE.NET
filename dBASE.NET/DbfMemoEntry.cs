using System.Collections.Generic;

namespace dBASE.NET
{
    public class DbfMemoEntry
    {
        internal DbfMemoEntry(int index, string value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DbfMemoEntry other))
                return false;

            return Index == other.Index && Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            var hashCode = 1376307821;
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}
