using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dBASE.NET
{
    /// <summary>
    /// The Dbf class encapsulated a dBASE table (.dbf) file, allowing
    /// reading from disk, writing to disk, enumerating Fields and enumerating Records.
    /// </summary>
    public class Dbf
    {
        private readonly string _path;
        private readonly Encoding _encoding = Encoding.ASCII;

        /// <summary>
        /// Creates empty dBASE table with default (ASCII) encoding.
        /// </summary>
        /// <param name="version"></param>
        public Dbf(DbfVersion version)
        {
            Header = DbfHeader.CreateHeader(version);
        }

        /// <summary>
        /// Creates empty dBASE table with specified encoding.
        /// </summary>
        /// <param name="version"></param>
        public Dbf(DbfVersion version, Encoding encoding)
        {
            _encoding = encoding;
            Header = DbfHeader.CreateHeader(version);
        }

        /// <summary>
        /// Reads existing dBASE table with default (ASCII) encoding.
        /// </summary>
        public Dbf(string dbfPath)
        {
            _path = dbfPath;
            Read();
        }

        /// <summary>
        /// Reads existing dBASE table with specified encoding.
        /// </summary>
        public Dbf(string dbfPath, Encoding encoding)
        {
            _path = dbfPath;
            _encoding = encoding;
            Read();
        }

        public List<DbfField> Fields { get; } = new List<DbfField>();

        public List<DbfRecord> Records { get; } = new List<DbfRecord>();

        public List<DbfRecord> ActiveRecords => Records.FindAll(r => !r.IsDeleted);

        public List<DbfRecord> DeletedRecords => Records.FindAll(r => r.IsDeleted);

        internal DbfHeader Header { get; private set; }

        internal DbfMemo Memo { get; private set; }

        public DbfRecord CreateRecord()
        {
            var record = new DbfRecord(Fields);
            Records.Add(record);

            return record;
        }

        public DbfField AddNewField(string name, DbfFieldType type, int length)
        {
            if (type == DbfFieldType.Memo)
            {
                if (Header.Version == DbfVersion.dBase4SQLSystemNoMemo
                    || Header.Version == DbfVersion.dBase4SQLTableNoMemo
                    || Header.Version == DbfVersion.FoxBaseDBase3NoMemo)
                    throw new InvalidOperationException("Memo fields are not supported for this database version.");

                if (Memo == null)
                    Memo = CreateMemo();
            }

            var field = new DbfField(name, type, length, _encoding);
            Fields.Add(field);

            return field;
        }

        private DbfMemo CreateMemo()
        {
            string memoPath = Path.ChangeExtension(_path, "fpt");

            return new DbfMemo(memoPath, _encoding);
        }

        public void DeleteRecord(DbfRecord record)
        {
            record.Delete();
        }

        public void DeleteRecord(int index)
        {
            Records[index].Delete();
        }

        public void UndeleteRecord(DbfRecord record)
        {
            record.Undelete();
        }

        public void UndeleteRecord(int index)
        {
            Records[index].Undelete();
        }

        public DbfMemoEntry CreateMemoEntry(string value)
        {
            return Memo.CreateEntry(value);
        }

        public void Read()
        {
            // Open stream for reading.
            using (FileStream stream = File.Open(_path, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                ReadHeader(reader);
                Memo = ReadMemo();
                ReadFields(reader);

                // After reading the Fields, we move the read pointer to the beginning
                // of the Records, as indicated by the "HeaderLength" value in the header.
                stream.Seek(Header.HeaderLength, SeekOrigin.Begin);

                ReadRecords(reader);
            }
        }

        private DbfMemo ReadMemo()
        {
            string memoPath = Path.ChangeExtension(_path, "fpt");
            if (!File.Exists(memoPath))
            {
                memoPath = Path.ChangeExtension(_path, "dbt");
                if (!File.Exists(memoPath))
                    return null;
            }

            return new DbfMemo(memoPath, _encoding);
        }

        private void ReadHeader(BinaryReader reader)
        {
            // Peek at version number, then try to read correct version header.
            byte versionByte = reader.ReadByte();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            DbfVersion version = (DbfVersion)versionByte;
            Header = DbfHeader.CreateHeader(version);
            Header.Read(reader);
        }

        private void ReadFields(BinaryReader reader)
        {
            Fields.Clear();

            // Fields are terminated by 0x0d char.
            while (reader.PeekChar() != 0x0d)
            {
                Fields.Add(new DbfField(reader, _encoding));
            }

            // Read Fields terminator.
            reader.ReadByte();
        }

        private void ReadRecords(BinaryReader reader)
        {
            Records.Clear();

            // Records are terminated by 0x1a char (officially), or EOF (also seen).
            while (reader.PeekChar() != 0x1a && reader.PeekChar() != -1)
            {
                Records.Add(new DbfRecord(this, reader));
            }
        }

        public void Save()
        {
            if (_path == null)
                throw new InvalidOperationException("Database path is not specified.");

            SaveTo(_path);
        }

        public void SaveTo(string path)
        {
            using (var stream = File.Open(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                Header.Write(writer, Fields, Records);
                WriteFields(writer);
                WriteRecords(writer);
            }

            if (Memo != null)
                Memo.Save();
        }

        private void WriteFields(BinaryWriter writer)
        {
            foreach (DbfField field in Fields)
                field.Write(writer);
            
            // Write field descriptor array terminator.
            writer.Write((byte)0x0d);
        }

        private void WriteRecords(BinaryWriter writer)
        {
            foreach (DbfRecord record in Records)
                record.Write(writer);
            
            // Write EOF character.
            writer.Write((byte)0x1a);
        }
    }
}
