using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dBASE.NET.Tests
{
    [TestClass]
    public class Demo
    {
        [TestMethod]
        public void DemoCode()
        {
            var dbf = new Dbf(DbfVersion.FoxPro2WithMemo);
            DbfField testField = dbf.AddField("TEST", DbfFieldType.Character, 12);
            DbfRecord record = dbf.CreateRecord();
            record[testField] = "HELLO";
            dbf.SaveTo("test.dbf");

            Assert.AreEqual(record.Data[0], record[testField]);
        }
    }
}
