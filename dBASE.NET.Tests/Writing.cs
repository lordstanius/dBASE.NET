using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace dBASE.NET.Tests
{
    [TestClass]
    public class Writing
    {
        Dbf dbf;

        [TestInitialize]
        public void testInit()
        {
        }

        private byte[] ReadBytes()
        {
            // Open stream for reading.
            FileStream stream = File.Open("test.dbf", FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            byte[] data = new byte[stream.Length];
            data = reader.ReadBytes((int)stream.Length);
            reader.Close();
            stream.Close();
            return data;
        }

        [TestMethod]
        public void WriteNoData()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.SaveTo("test.dbf");

            byte[] data = ReadBytes();
            Assert.AreEqual(0x30, data[0], "Version should be 0x30.");
        }

        [TestMethod]
        public void WriteOneField()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Character, 12);
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual("TEST", dbf.Fields[0].Name, "Field name should be TEST.");
        }

        [TestMethod]
        public void WriteFieldAndRecord()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Character, 12);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = "HELLO";
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual("HELLO", dbf.Records[0][0], "Record content should be HELLO.");
        }

        [TestMethod]
        public void WriteFieldAndThreeRecords()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Character, 12);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = "HELLO";
            record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = "WORLD";
            record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = "OUT THERE";
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual("OUT THERE", dbf.Records[2][0], "Record content should be OUT THERE.");
        }

        [TestMethod]
        public void NumericField()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Numeric, 12);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = 3.14;
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual(3.14M, dbf.Records[0][0], "Record content should be 3.14.");
        }

        [TestMethod]
        public void FloatField()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Float, 12);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = 3.14;
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual((float)3.14, dbf.Records[0][0], "Record content should be 3.14.");
        }

        [TestMethod]
        public void LogicalField()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Logical, 12);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = true;
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual(true, dbf.Records[0][0], "Record content should be TRUE.");
        }

        [TestMethod]
        public void DateField()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Date, 12);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = new DateTime(2018, 8, 7);
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual(new DateTime(2018, 8, 7), dbf.Records[0][0], "Record content should be 2018-08-07.");
        }

        [TestMethod]
        public void DateTimeField()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.DateTime, 8);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = new DateTime(2018, 8, 7, 20, 15, 8);
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual(new DateTime(2018, 8, 7, 20, 15, 8), dbf.Records[0][0], "Record content should be 2018-08-07 20:15:08.");
        }

        [TestMethod]
        public void IntegerField()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Integer, 4);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = 34;
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual(34, dbf.Records[0][0], "Record content should be 34.");
        }

        [TestMethod]
        public void CurrencyField()
        {
            dbf = new Dbf(DbfVersion.VisualFoxPro);
            dbf.AddField("TEST", DbfFieldType.Currency, 8);
            DbfRecord record = new DbfRecord(dbf.Fields);
            dbf.Records.Add(record);
            record.Data[0] = 138799L;
            dbf.SaveTo("test.dbf");

            dbf = new Dbf("test.dbf");

            Assert.AreEqual(138799L, dbf.Records[0][0], "Record content should be ");
        }
    }
}
