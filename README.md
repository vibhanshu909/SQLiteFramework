# SQLiteFramework
Create Databases and Tables dynamically just by extending SQLiteModel class and applying proper Attributes. No query Generation Whatsoever.
It uses MVC Architecture based Approach, And provides a effective way to create persistent Models.
 
## Prerequisites
System.Data.SQLite
Available on NuGet.

## Requirement
Install SQLiteFramework from NuGet

## Installing
You can search and install SQLiteFramework from Manage NuGet package in visual studio.
Or
Open Package Manager Console and Type: **Install-Package SQLiteFramework**

## Getting Started

Step 1: Create a Project SQLiteFrameworkDemo

Step 2: Create a StudentModel.cs file as per your Model requirement.

---------------------------------------------------StudentModel.cs-----------------------------------------------------
```cs
using SQLiteFramework;
namespace SQLiteFrameworkDemo
{
    class StudentModel : SQLiteModel
    {
        [Field]
        [PrimaryKey, AutoIncrement]
        public ulong ID { get; set; }

        [Field]
        [NotNull]
        public string Name { get; set; } = "Unknown";// A NotNull Attributed Property can't be null else SQLiteException will be thrown. 

        [Field]
        [Default(true)]
        public bool Gender { get; set; }

        [Field]
        public int Age { get; set; }

        [Field]
        [Unique]
        public ulong PhoneNumber { get; set; }

        //Since the Oject are being created Using Reflection a Default Constructor in Required.
        public StudentModel() { }
    }
}
```
Step 3: Create Another Class MainProgram.cs

------------------------------------------------------MainProgram.cs-------------------------------------------------------
```cs
using System;
using System.Collections.Generics;

namespace SQLiteFrameworkDemo
{
    public class MainProgram
    {
        public static void Main(String[] args)
        {
            StudentModel.UseDatabase("StudentDB");
            StudentModel Someone = new StudentModel();
            Someone.Name = "Someone";
            Someone.Age = 30;
            Someone.PhoneNumber = (ulong)new Random().Next();
            bool status = StudentModel.Insert(Someone);
            this.Grid.DataContext = Someone;
            if (status)
            {
                List<StudentModel> list = StudentModel.All<StudentModel>();
                foreach (StudentModel item in list)
                {
                    Console.WriteLine(item.ID + "|" + item.Name + "|" + item.Age + "|" + item.Gender + "|" + item.PhoneNumber);//printing All the aquired rows one by one.
                }
            }
        }
    }
}
```
Step 4: Making Advance and more Complicated Selection.

SQLiteFramework provides you with a set of Classes and method to build advance selection Processes, like:-

SelectionBuilder: SelectionBuilder class provides you a set of method to build complicated and advance selection conditions.
Note: Except Build Method All Methods in SelectionBuilder returns a SelectionBuilder object, so that you can chain a long method calls.

Methods available are:-

* Where(string)
* IsLessThan(string)
* IsLessThanEqualsTo(string)
* IsEqualsTo(string)
* IsNotEqualsTo(string)
* IsCaseEqualsTo(string)      //For String Comparison
* IsCaseNotEqualsTo(string)   //For String Comparison
* IsGreaterThan(string)
* IsGreaterThanEqualsTo(string)
* And(string)
* Or(string)
* Not([Optional string])
* Like(string)
* Glob(string)
* Exists(string subQuery)
* Between(int,int)
* In(string)
* Limit(int)

**Selection Builder** also provides a way to pass Parameter to these functions, to prevent SQL injection.
example:
```cs
SelectionBuilder sb=new SelectionBuilder().Where("ID")
                                            .IsGreaterThanEqualsTo("@id")
                                            .AddParams("@id",4 //This value maybe dynamic or input from user)
                                            .Build();

List<StudentModel> List=StudentModel.Select<StudentModel>(sb);
// This will give you all the records with ID > 4 from StudentModel table.
```
You can also pass a dictionary of parameter to the SelectionBuilder object and it will automatically choose the required ones.
```cs
Dictionary<string,string> params=new Dictionary<string,string>();
params.Add("@id",4)
params.Add("@age",20);

SelectionBuilder sb=new SelectionBuilder().Where("ID")
                                            .IsGreaterThanEqualsTo("@id")
                                            .And("Age")
                                            .IsLessThan("@age")
                                            .SetParams(params)
                                            .Build();
```                                            
Note: You must set the parameters before you build the SelectionBuilder object, else you'll get a **SQLiteException** when selecting using this SelectionBuilder object.

## Version
version 1.0.2

## Authors
* vibhanshu pandey

## License
[License](https://github.com/vibhanshu-github/SQLiteFramework/edit/master/LICENSE.md)
