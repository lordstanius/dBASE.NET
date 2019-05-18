using System;
using System.IO;
using System.Text;
using dBASE.NET.Conversion;

namespace dBASE.NET
{
    public class DbfMemo
    {
        protected int BlockHeaderSize = 8;
        protected int NextBlockOffset;
        protected int BlockSize = 64;
        protected byte[] Data;
        protected string Path;
        protected Encoding Encoding;

        internal DbfMemo(string path, Encoding encoding)
        {
            Path = path;
            Encoding = encoding;

            if (File.Exists(Path))
            {
                // load from existing memo file
                Data = File.ReadAllBytes(Path);
                NextBlockOffset = (int)EndianBitConverter.Big.ToUInt32(Data, 0);
                BlockSize = (int)EndianBitConverter.Big.ToUInt32(Data, 4);
            }
            else
            {
                // create empty memo
                Data = new byte[BlockSize];
                NextBlockOffset = 1;
                Buffer.BlockCopy(EndianBitConverter.Big.GetBytes((uint)NextBlockOffset), 0, Data, 0, 4);
                Buffer.BlockCopy(EndianBitConverter.Big.GetBytes((uint)BlockSize), 0, Data, 4, 4);
                File.WriteAllBytes(Path, Data);
            }
        }

        public DbfMemoEntry GetMemo(int index)
        {
            if (!ContainsEntry(index))
                return null;

            //// The index is measured from the start of the file, even though the memo file header blocks takes
            //// up the first few index positions.
            int offset = index * BlockSize;

            int type = (int)EndianBitConverter.Big.ToUInt32(Data, offset);
            int length = (int)EndianBitConverter.Big.ToUInt32(Data, offset + 4);
            string value = Encoding.GetString(Data, offset + BlockHeaderSize, length);
            return new DbfMemoEntry(index, value);
        }

        public bool ContainsEntry(DbfMemoEntry entry) => ContainsEntry(entry.Index);

        internal bool ContainsEntry(int index) => index >= 0 && index < NextBlockOffset;

        public DbfMemoEntry CreateEntry(string value)
        {
            int requiredBlocks = (value.Length + BlockHeaderSize) / BlockSize + 1;
            var newData = new byte[Data.Length + requiredBlocks * BlockSize];
            Buffer.BlockCopy(Data, 0, newData, 0, Data.Length);
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(1U), 0, newData, Data.Length, 4);
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes((uint)value.Length), 0, newData, Data.Length + 4, 4);
            Buffer.BlockCopy(Encoding.GetBytes(value), 0, newData, Data.Length + BlockHeaderSize, value.Length);
            int nextBlockOffset = NextBlockOffset + requiredBlocks;
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes((uint)nextBlockOffset), 0, newData, 0, 4);
            var newEntry = new DbfMemoEntry(NextBlockOffset, value);
            Data = newData;
            NextBlockOffset = nextBlockOffset;

            return newEntry;
        }

        public void Save()
        {
            File.WriteAllBytes(Path, Data);
        }
    }
}