# dBASE.NET - Read and write DBF files with .NET

__dBASE.NET__ is a .NET class library used to read FoxBase, dBASE III and dBASE IV .dbf files. Data read
from a file is returned as a list of typed fields and a list of records. This library is useful to add
data import from dBASE sources to a .NET project.

This code has been tested against a number of dBASE files, including FoxBase and dBASE III/IV
files with and without memo files. A .NET unit test project is part of this repository and new test files
may be added to it over time.

There is [an article describing the dBASE file format](http://web.archive.org/web/20150323061445/http://ulisse.elettra.trieste.it/services/doc/dbase/DBFstruct.htm#C1.5).

## Opening a DBF file

```c#
using dBASE.NET;

var dbf = new Dbf("database.dbf");
```

This returns an instance of the `Dbf` class. With this, you can iterate over fields found in the table:

```c#
foreach(DbfField field in dbf.Fields) {
	Console.WriteLine(field.Name);
}
```

You can also iterate over records:

```c#
foreach(DbfRecord record in dbf.Records) {
	Console.WriteLine(record);
}
```

Count the records:

```c#
Console.WriteLine(dbf.Records.Count);
```

## Working with memo files

When memo file accompanying the `.dbf` file is found (either `.dbt` or `.fpt`), with the same base name as the table file, then 
dBASE.NET will load the memo file's contents. 

## Writing a DBF file

To write DBF data, you can either create a new instance of `Dbf`, then create fields and records, or load an existing table and modify its fields or records.

This sample code creates a new table with a single character field, then saves the .dbf file:

```c#
var dbf = new Dbf(DbfVersion.FoxPro2WithMemo);
dbf.AddField("TEST", DbfFieldType.Character, 12);
DbfRecord record = dbf.CreateRecord();
record.Data[0] = "HELLO";
dbf.SaveTo("test.dbf");
```

## Supported Field types

| Code | Field type       | .NET counterpart |
|:-----|:-------------    |:-----------------|
| `C`  | Character string | String           |
| `D`  | Date             | DateTime         |
| `I`  | Integer          | Int32            |
| `L`  | Logical          | Bool             |
| `M`  | Memo             | String           |
| `N`  | Numeric          | Decimal          |
| `T`  | DateTime         | DateTime         |
| `Y`  | Currency         | Decimal          |
| `F`  | Float            | Float            |
| `B`  | Double           | Double           |


## Class diagram

![Class diagram](http://yuml.me/1cc9f823.png)

_yuml:_

```
http://yuml.me/diagram/scruffy/class/edit/[Dbf]+->[DbfRecord], [Dbf]+->[DbfField], [DbfRecord]+->[DbfField], [Dbf]->[DbfHeader], [DbfHeader]^-[Dbf4Header]
```` 
