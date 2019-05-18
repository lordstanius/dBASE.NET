using System.IO;
using System.Text;
using dBASE.NET.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dBASE.NET.Tests
{
    [TestClass]
    public class DbfDiffTests
    {
        private const string DbfName = "TRANSACT.DBF";

        private Dbf _original;
        private Dbf _modified;

        private readonly Encoding _encoding = Encoding.GetEncoding(1252);

        [TestInitialize]
        public void Init()
        {
            _original = new Dbf(Path.Combine(".\\fixtures\\_Original", DbfName), _encoding);
            _modified = new Dbf(Path.Combine(".\\fixtures\\_Modified", DbfName), _encoding);
        }

        [TestMethod]
        public void SelfTest()
        {
            Assert.IsFalse(new DbfDiff(_original, _original).HasChanges);
            Assert.IsFalse(new DbfDiff(_modified, _modified).HasChanges);
        }

        [TestMethod]
        public void InsertTest()
        {
            _modified.Records.Add(_modified.Records[0]);
            var diff = new DbfDiff(_original, _modified);

            Assert.IsTrue(diff.HasChanges);
            Assert.IsTrue(diff.Inserted.Count == 1);
            Assert.IsTrue(_modified.Records[0].Equals(_modified.Records[_modified.Records.Count - 1]));
        }

        [TestMethod]
        public void DeleteTest()
        {
            Assert.IsFalse(_modified.Records[1].IsDeleted);

            _modified.DeleteRecord(_modified.Records[1]);
            var diff = new DbfDiff(_original, _modified);

            Assert.IsTrue(diff.HasChanges);
            Assert.IsTrue(diff.Deleted.Count == 1);
        }

        [TestMethod]
        public void UpdateTest()
        {
            _modified.Records[0][_modified.Fields[1]] = "401";
            var diff = new DbfDiff(_original, _modified);

            Assert.IsTrue(diff.HasChanges);
            Assert.IsTrue(diff.Updated.Count == 1);
            Assert.IsTrue(diff.Updated.ContainsKey(0));
        }

        [TestMethod]
        public void UpdateMemoTest()
        {
            _modified.Records[2]["OPIS"] = _modified.CreateMemoEntry("This is new memo entry!");
            var diff = new DbfDiff(_original, _modified);

            Assert.IsTrue(diff.Updated.Count == 1);
            Assert.IsTrue(diff.Updated.ContainsKey(2));
        }

        [TestMethod]
        public void ApplyTest()
        {
            _modified.Records.Add(_modified.Records[0]); // add
            _modified.DeleteRecord(_modified.Records[1]); // delete
            _modified.Records[0][_modified.Fields[1]] = "401"; // update
            _modified.Records[2]["OPIS"] = _modified.CreateMemoEntry("This is a new memo entry!"); // update memo

            var diff = new DbfDiff(_original, _modified);

            // serialize to string
            string serializedDiff = diff.Serialize();

            // get instance from json
            diff = DbfDiff.Deserialize(_original.Fields, serializedDiff);

            diff.ApplyTo(_original);
            _original.Save();

            Assert.IsFalse(diff.HasChanges);

            _original = new Dbf(Path.Combine(".\\fixtures\\_Original", DbfName), _encoding);

            diff = new DbfDiff(_original, _modified);
            Assert.IsFalse(diff.HasChanges);
        }
    }
}
