using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;

namespace dBASE.NET.Tools
{
    /// <summary>
    /// Calculates record changes for given dBASE table.
    /// </summary>
    public class DbfDiff
    {
        private DbfDiff() {}

        public DbfDiff(Dbf original, Dbf modified)
        {
            int addedCount = modified.Records.Count - original.Records.Count;
            if (addedCount > 0)
                EnumerateInsertedRecords(original, modified);

            if (original.DeletedRecords.Count != modified.DeletedRecords.Count)
                EnumerateDeletedRecords(original, modified);

            EnumerateUpdatedRecords(original, modified);
        }

        public static DbfDiff Deserialize(List<DbfField> fields, string serializedDiff)
        {
            var diff = new DbfDiff();

            using (var reader = new StringReader(serializedDiff))
            {
                string line = reader.ReadLine(); // discard section name [INSERTED]
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("[DELETED]"))
                        break;

                    diff.Inserted.Add(new DbfRecord(fields, line));
                }

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("[UPDATED]"))
                        break;

                    diff.Deleted.Add(int.Parse(line, CultureInfo.InvariantCulture));
                }

                while ((line = reader.ReadLine()) != null)
                {
                    int index = int.Parse(line.Remove(line.IndexOf(':')));
                    string value = line.Substring(line.IndexOf(':') + 1);
                    diff.Updated.Add(index, new DbfRecord(fields, value));
                }
            }

            return diff;
        }

        public List<DbfRecord> Inserted { get; } = new List<DbfRecord>();

        public Dictionary<int, DbfRecord> Updated { get; } = new Dictionary<int, DbfRecord>();

        public List<int> Deleted { get; } = new List<int>();

        public bool HasChanges => Deleted.Count > 0 || Updated.Count > 0 || Inserted.Count > 0;

        public void ApplyTo(Dbf dbf)
        {
            foreach (var record in Inserted)
                dbf.Records.Add(record);

            foreach (int index in Deleted)
                dbf.DeleteRecord(index);

            foreach (var kvp in Updated)
            {
                dbf.Records[kvp.Key] = kvp.Value;
                if (dbf.Fields.Any(f => f.Type == DbfFieldType.Memo))
                {
                    foreach (var field in dbf.Fields)
                    {
                        if (field.Type != DbfFieldType.Memo)
                            continue;

                        var memoEntry = kvp.Value[field.Name] as DbfMemoEntry;
                        if (!dbf.Memo.ContainsEntry(memoEntry))
                        {
                            // this is a new memo
                            kvp.Value[field.Name] = dbf.CreateMemoEntry(memoEntry.Value);
                        }
                    }
                }
            }

            Inserted.Clear();
            Updated.Clear();
            Deleted.Clear();
        }

        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[INSERTED]");
            foreach (var rec in Inserted)
                sb.AppendLine(rec.Serialize());

            sb.AppendLine("[DELETED]");
            foreach (var deleted in Deleted)
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n", deleted);

            sb.AppendLine("[UPDATED]");
            foreach (var updated in Updated)
                sb.AppendFormat("{0}:{1}\r\n", updated.Key, updated.Value.Serialize());

            return sb.ToString();
        }

        public static DbfDiff Create(string dbfName, string pathToOriginal, string pathToModified, Encoding encoding)
        {
            Dbf original = new Dbf(Path.Combine(pathToOriginal, dbfName), encoding);
            Dbf modified = new Dbf(Path.Combine(pathToModified, dbfName), encoding);

            return new DbfDiff(original, modified);
        }

        private void EnumerateInsertedRecords(Dbf original, Dbf modified)
        {
            for (int i = original.Records.Count; i < modified.Records.Count; i++)
            {
                Inserted.Add(modified.Records[i]);
            }
        }

        private void EnumerateDeletedRecords(Dbf original, Dbf modified)
        {
            for (int i = 0; i < original.Records.Count; i++)
            {
                if (!original.Records[i].IsDeleted && modified.Records[i].IsDeleted)
                    Deleted.Add(i);
            }
        }

        private void EnumerateUpdatedRecords(Dbf original, Dbf modified)
        {
            for (int i = 0; i < original.Records.Count; i++)
            {
                if (!original.Records[i].Equals(modified.Records[i]))
                    Updated.Add(i, modified.Records[i]);
            }
        }
    }
}
