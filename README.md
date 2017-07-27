# SQLiteFramework
Create Databases and Tables dynamically just by extending SQLiteModel class and applying proper Attributes. No query Generation Whatsoever.
It uses MVC Architecture based Approach, And provides a effective way to create persistent Models.

Example:

Step 1: Install the NuGet Package

open up NuGet Package Manager Console and Type:
Install-package SQLiteFramework

Step 2: Create a Project SQLiteFrameworkDemo
Step 3: Create a StudentModel.cs file as per your Model requirement.

--------------------------------StudentModel.cs-------------------------------------------------

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

Step 4: Create Another Class MainProgram.cs

using System;
using System.Collections.Generics;

namespace SQLiteFrameworkDemo
{
  public class MainProgram
  {
    public static void Main(String[] args)
    {
              StudentModel Someone = new StudentModel();
              Someone.Name = "Someone";
              Someone.Age = 30;
              Someone.PhoneNumber =(ulong)new Random().Next();
              bool status=StudentModel.Insert(Someone);
              this.Grid.DataContext = Someone;
              if (status)
              {
                  List<StudentModel> list = StudentModel.All<StudentModel>();
                  foreach (StudentModel item in list)
                  {
                      Console.WriteLine(item.ID+"|"+item.Name+"|"+item.Age+"|"+item.Gender+"|"+item.PhoneNumber);
                  }
              }
    }
  }
}
