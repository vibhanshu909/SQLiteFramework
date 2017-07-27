using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SQLiteFramework
{
    public class SQLiteModel
    {
        [Field]
        [PrimaryKey, AutoIncrement]
        public ulong ID { get; set; }

        protected class Adapter
        {
            private SQLiteConnection sql_con;
            private SQLiteCommand sql_cmd;
            private SQLiteDataAdapter DB;
            private DataSet DS = new DataSet();

            private void SetConnection()
            {
                //Data Source=DemoT.db;New=False;
                sql_con = new SQLiteConnection("Data Source=DEMO.sqlite;Compress=True;Version=3;");
            }
            public int ExecuteQuery(string txtQuery)
            {
                SetConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = txtQuery;
                int result = sql_cmd.ExecuteNonQuery();
                sql_con.Close();
                return result;
            }
            public int ExecuteMultipleQuery(List<string> QueryList, Dictionary<string, string> Params = null)
            {
                SetConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();
                DS.Reset();
                foreach (string query in QueryList)
                {
                    sql_cmd.CommandText = query;
                    if (Params != null)
                    {
                        foreach (KeyValuePair<string, string> it in Params)
                            sql_cmd.Parameters.AddWithValue(it.Key, it.Value);
                    }
                    if (sql_cmd.ExecuteNonQuery() < 1)
                        return 0;
                }
                sql_con.Close();
                return 1;
            }
            public DataSet ExecuteDataQuery(string query, Dictionary<string, string> Params = null)
            {
                SetConnection();
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = query;
                if (Params != null)
                {
                    foreach (KeyValuePair<string, string> it in Params)
                        sql_cmd.Parameters.AddWithValue(it.Key, it.Value);
                }
                DB = new SQLiteDataAdapter(sql_cmd);
                DS.Reset();
                DB.Fill(DS);
                sql_con.Close();
                return DS;
            }
        }
        public class Conditions
        {
            public string Condition { get; set; } = "";
            public Dictionary<string, string> Params = null;
            public Conditions() { }
        }
        public class SelectionBuilder
        {
            private Conditions Condition = new Conditions();
            public SelectionBuilder() { }
            public SelectionBuilder(string key)
            {
                this.Condition.Condition = key;
            }

            #region Relational Operations
            public SelectionBuilder IsCaseEqualsTo(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += "=" + val;
                return this;
            }
            public SelectionBuilder IsCaseNotEqualsTo(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += "!=" + val;
                return this;
            }
            public SelectionBuilder IsEqualsTo(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += "=" + val + " COLLATE NOCASE";
                return this;
            }
            public SelectionBuilder IsNotEqualsTo(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += "!=" + val + " COLLATE NOCASE";
                return this;
            }
            public SelectionBuilder IsLessThan(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += "<" + val;
                return this;
            }
            public SelectionBuilder IsLessThanEqualsTo(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += "<=" + val;
                return this;
            }
            public SelectionBuilder IsGreaterThan(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += ">" + val;
                return this;
            }
            public SelectionBuilder IsGreaterThanEqualsTo(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += ">=" + val;
                return this;
            }
            #endregion

            #region Logical Operations
            public SelectionBuilder Where(string key)
            {
                this.Condition.Condition = " WHERE " + key;
                return this;
            }
            public SelectionBuilder And(string key)
            {
                this.Condition.Condition += " AND " + key;
                return this;
            }
            public SelectionBuilder Or(string key)
            {
                this.Condition.Condition += " OR " + key;
                return this;
            }
            public SelectionBuilder Not(string val = null)
            {
                val = this.Filter(val);
                this.Condition.Condition += " NOT " + val == null ? "" : val;
                return this;
            }
            public SelectionBuilder Like(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += " LIKE " + val;
                return this;
            }
            public SelectionBuilder Glob(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += " GLOB " + val;
                return this;
            }
            public SelectionBuilder Between(int start, int end)
            {
                string startVal = this.Filter(Convert.ToString(start));
                string endVal = this.Filter(Convert.ToString(end));
                int size = this.Condition.Condition.LastIndexOf(' ');
                this.Condition.Condition = this.Condition.Condition.Insert(size, "(");
                this.Condition.Condition += " BETWEEN " + startVal + " AND " + endVal + ")";
                return this;
            }
            public SelectionBuilder Exists(string subQuery)
            {
                this.Condition.Condition += " EXISTS (" + subQuery + ")";
                return this;
            }
            public SelectionBuilder In(string val)
            {
                val = this.Filter(val);
                this.Condition.Condition += " IN (" + val + ")";
                return this;
            }
            public SelectionBuilder Limit(int value)
            {
                string val = this.Filter(Convert.ToString(value));
                this.Condition.Condition += " LIMIT " + val;
                return this;
            }
            #endregion

            #region Other Operations
            public SelectionBuilder OrderBy(string param, bool desc = false)
            {
                this.Condition.Condition += " ORDER BY " + param + (desc ? " desc" : "");
                return this;
            }
            public SelectionBuilder GroupBy(string key)
            {
                key = this.Filter(key);
                this.Condition.Condition += " GROUP BY " + key;
                return this;
            }
            public SelectionBuilder Having(string condition)
            {
                this.Condition.Condition += condition;
                return this;
            }
            #endregion

            public SelectionBuilder SetParams(Dictionary<string, string> Params)
            {
                this.Condition.Params = Params;
                return this;
            }
            public SelectionBuilder AddParams(string key, string val)
            {
                if (this.Condition.Params == null)
                    this.Condition.Params = new Dictionary<string, string>();
                this.Condition.Params.Add(key, val);
                return this;
            }
            public Conditions Build()
            {
                return this.Condition;
            }
            private string Filter(string obj)
            {
                if (obj.First().Equals('@'))
                {
                    return obj;
                }
                else return "'" + obj + "'";
            }
        }

        private static Adapter Adap = new Adapter();


        private static void Create(object This)
        {
            string output = "CREATE TABLE IF NOT EXISTS ";
            Type type = This.GetType();
            string[] temp = type.ToString().Split('.');
            output += temp[temp.Length - 1];

            object[] attr = type.GetCustomAttributes(true);
            foreach (object a in attr)
            {
                Console.WriteLine("Class Attributes: " + a);
            }
            output += @"(";
            IEnumerable<PropertyInfo> Prop = GetPropertiesWithAttribute<Field>(type);
            foreach (PropertyInfo p in Prop)
            {
                output += @"'" + p.Name + @"' ";
                if (p.PropertyType == typeof(bool))
                {
                    output += @"BOOLEAN";
                }
                else if (p.PropertyType == typeof(sbyte) ||
                    p.PropertyType == typeof(short) ||
                    p.PropertyType == typeof(int) ||
                    p.PropertyType == typeof(long) ||

                    p.PropertyType == typeof(byte) ||
                    p.PropertyType == typeof(ushort) ||
                    p.PropertyType == typeof(uint) ||
                    p.PropertyType == typeof(ulong))
                {
                    output += @"INTEGER";
                }
                else if (p.PropertyType == typeof(float) ||
                    p.PropertyType == typeof(double) ||
                    p.PropertyType == typeof(decimal))
                {
                    output += @"REAL";
                }
                else if (p.PropertyType == typeof(string))
                {
                    output += @"VARCHAR";
                }
                else if (p.PropertyType == typeof(DateTime))
                {
                    output += @"TEXT DEFAULT(datetime('now','localtime'))";
                }
                else
                {
                    throw new SQLiteException("Type Not Supported in the underlying Database System.");
                }
                //output+=Marshal.SizeOf(p.PropertyType);
                foreach (Attribute a in p.GetCustomAttributes())
                {
                    Console.WriteLine("Property: " + p.Name + " PropertyAttributes: " + a);
                    if (a.ToString().Equals(typeof(PrimaryKey).ToString()))
                    {
                        output += " PRIMARY KEY";
                    }
                    else if (a.ToString().Equals(typeof(Unique).ToString()))
                    {
                        output += " UNIQUE";
                    }
                    else if (a.ToString().Equals(typeof(NotNull).ToString()))
                    {
                        output += " NOT NULL";
                    }
                    else if (a.ToString().Equals(typeof(AutoIncrement).ToString()))
                    {
                        if (p.PropertyType == typeof(sbyte) ||
                            p.PropertyType == typeof(short) ||
                            p.PropertyType == typeof(int) ||
                            p.PropertyType == typeof(long) ||

                            p.PropertyType == typeof(byte) ||
                            p.PropertyType == typeof(ushort) ||
                            p.PropertyType == typeof(uint) ||
                            p.PropertyType == typeof(ulong))
                        {
                            output += " AUTOINCREMENT";
                        }
                        else
                        {
                            throw new SQLiteException("AutoIncrement can only be applied to Integer fields");
                        }
                    }
                    else if (a.ToString().Equals(typeof(Default).ToString()))
                    {
                        Default d = a as Default;
                        string value = d.Value.ToString();
                        if (p.PropertyType == typeof(bool))
                        {
                            if (((bool)d.Value) == true)
                            {
                                value = "1";
                            }
                            else
                                value = "0";
                        }
                        output += " DEFAULT '" + value + "'";
                    }
                }
                output += @", ";
            }
            output = output.Remove(output.Length - 2, 2);
            output += @")";
            Console.WriteLine(output);
            Adapter Adap = new Adapter();
            Adap.ExecuteQuery(output);
            //Adap.ExecuteQuery();
        }
        public static bool Insert(object This, bool CreateNew = false)
        {
            if (CreateNew)
            {
                Create(This);
            }

            Type type = This.GetType();
            string key = "", value = "";
            IEnumerable<PropertyInfo> Prop = GetPropertiesWithAttribute<Field>(type);
            foreach (PropertyInfo p in Prop)
            {
                bool Inserted = false;
                foreach (Attribute a in p.GetCustomAttributes())
                {
                    if (a.GetType().ToString().Equals(typeof(AutoIncrement).ToString()))
                    {
                        Inserted = true;
                        break;
                    }
                    else if (a.GetType().ToString().Equals(typeof(Default).ToString()))
                    {
                        Inserted = true;
                        object val = p.GetValue(This);
                        if (val != (a as Default).Value)
                        {
                            key += p.Name + ", ";
                            if (p.PropertyType.Equals(typeof(bool)))
                            {
                                if (val.ToString().ToLower().Equals("true"))
                                    value += "'" + 1 + "', ";
                                else
                                    value += "'" + 0 + "', ";
                            }
                            //else if(p.PropertyType.Equals(typeof(DateTime)))
                            //{
                            //    if ("1/1/0001 12:00:00 AM".Equals(p.GetValue(This).ToString()))
                            //    {
                            //        value += DateTime.Now;
                            //    }
                            //}
                            else
                            {
                                try
                                {
                                    value += "'" + val.ToString() + "', ";
                                }
                                catch (Exception)
                                {
                                    if (p.GetCustomAttribute(typeof(NotNull)) != null)
                                        value += "'" + (a as Default).Value + "', ";
                                    else
                                        throw new SQLiteException("Null Value Found on NotNULL Field");
                                }
                            }
                        }
                        break;
                    }
                    else if (a.GetType().ToString().Equals(typeof(NotNull).ToString()))
                    {
                        object val = p.GetValue(This);
                        if (val == null)
                        {
                            throw new SQLiteException("Null Value Found on NotNULL Field");
                        }
                    }
                }
                if (!Inserted)
                {
                    try
                    {
                        value += "'" + p.GetValue(This).ToString() + "', ";
                        key += p.Name + ", ";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("NULL Value Property Found: " + ex.StackTrace);
                    }
                }
            }
            try
            {
                key = key.Remove(key.Length - 2, 2);
                value = value.Remove(value.Length - 2, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            string output = "INSERT INTO ";
            string[] temp = type.ToString().Split('.');
            output += temp[temp.Length - 1];
            output += "(" + key + ") VALUES(" + value + ")";
            Console.WriteLine(output);

            Adapter Adap = new Adapter();
            try
            {
                if (Adap.ExecuteQuery(output) == 0) return false;
                Sync(This, type);
                //IEnumerable<PropertyInfo> P = GetPropertiesWithAttribute<DirtyBit>(type);
                //PropertyInfo pi = P.GetEnumerator().Current;
                //pi.SetValue(This, Cast(false, pi.PropertyType));
                return true;
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                Console.WriteLine(ex.StackTrace);
                if (!CreateNew)
                {
                    return Insert(This, true);
                }
                //throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                //Insert(This, false);
            }
            return false;
        }
        public static List<T> All<T>()
        {
            return Select<T>();
        }

        public static List<T> Select<T>(Conditions Condition = null)
        {
            string output = "SELECT * FROM ";
            Type type = typeof(T);
            string[] temp = type.ToString().Split('.');
            output += temp[temp.Length - 1];
            if (Condition != null)
            {
                output += Condition.Condition;
                return Get<T>(output, Condition.Params);
            }
            else
            {
                return Get<T>(output);
            }
        }
        public static bool UpdateMultiple<T>(object ObjectList)
        {
            Stopwatch sw = new Stopwatch();
            List<string> list = new List<string>();
            sw.Restart();
            foreach (object This in (ObjectList as List<T>))
            {
                list.Add(GenerateUpdateQuery<T>(This));
            }
            Console.WriteLine("Time taken to Generate All Queries are: " + sw.Elapsed.ToString());
            sw.Restart();
            if (Adap.ExecuteMultipleQuery(list) == 1)
            {
                Console.WriteLine("Time Taken to Executing all the queries is: " + sw.Elapsed.ToString());
                return true;
            }
            else
                return false;
        }
        public bool Update<T>()
        {
            object This = this;
            string output = GenerateUpdateQuery<T>(This);
            try
            {
                Console.WriteLine("Update Query is: " + output);
                if (Adap.ExecuteQuery(output) == 0)
                    return false;
            }
            catch (SQLiteException ex)
            {
                throw new SQLiteException("Key Constriant Violation", ex);
            }
            return true;
        }

        public static bool Delete<T>(object This)
        {
            string output = "DELETE FROM ";
            Type type = typeof(T);
            string[] temp = type.ToString().Split('.');
            output += temp[temp.Length - 1];

            IEnumerable<PropertyInfo> Prop = GetPropertiesWithAttribute<Field>(typeof(T));
            string Condition = "";
            foreach (PropertyInfo p in Prop)
            {
                if (p.GetCustomAttribute(typeof(PrimaryKey)) != null)
                {
                    Condition = " WHERE " + p.Name + "='" + p.GetValue(This) + "'";
                }
            }
            if (Condition == "")
            {
                throw new SQLiteException("Can't Delete values from a Table with no primary key.");
            }
            output += Condition;
            Console.WriteLine("Delete Query is: " + output);
            if (Adap.ExecuteQuery(output) == 0)
                return false;
            else
                return true;
        }

        #region Support Methods
        private static string GenerateUpdateQuery<T>(object This)
        {
            string output = "UPDATE ";
            Type type = typeof(T);
            string[] temp = type.ToString().Split('.');
            output += temp[temp.Length - 1];
            output += " SET ";

            IEnumerable<PropertyInfo> Prop = GetPropertiesWithAttribute<Field>(typeof(T));
            string Condition = "";
            foreach (PropertyInfo p in Prop)
            {
                output += p.Name + "='" + p.GetValue(This) + "', ";
                if (p.GetCustomAttribute(typeof(PrimaryKey)) != null)
                {
                    Condition = " WHERE " + p.Name + "='" + p.GetValue(This) + "'";
                }
            }
            output = output.Remove(output.Length - 2, 2);
            if (Condition == "")
            {
                throw new SQLiteException("Can't update values of Table with no primary key.");
            }
            output += Condition;
            return output;
        }
        private static bool Sync(object This, Type type)
        {
            string output = "";
            IEnumerable<PropertyInfo> Prop = GetPropertiesWithAttribute<Field>(type);
            foreach (PropertyInfo pi in Prop)
            {
                if (pi.GetCustomAttribute(typeof(PrimaryKey)) != null)
                {
                    output = "SELECT MAX(" + pi.Name + ") AS LENGTH ,* FROM ";
                    break;
                }
            }
            string[] temp = type.ToString().Split('.');
            output += temp[temp.Length - 1];
            DataSet DS = Adap.ExecuteDataQuery(output);
            DataTable dt = DS.Tables[0];
            int index = 0;
            foreach (DataRow Row in dt.Rows)
            {
                for (int i = 1; i < Row.ItemArray.Length; i++)
                {
                    object s = Row.ItemArray[i];
                    PropertyInfo pi = Prop.ElementAt(index++);
                    pi.SetValue(This, Cast(s, pi.PropertyType));
                }
                return true;
            }
            return false;
        }
        private static List<T> Get<T>(string query, Dictionary<string, string> Params = null)
        {
            Console.WriteLine("Query is: " + query);
            List<T> Table = new List<T>();
            DataSet DS = Adap.ExecuteDataQuery(query, Params);
            IEnumerable<PropertyInfo> Prop = GetPropertiesWithAttribute<Field>(typeof(T));
            DataTable dt = DS.Tables[0];
            foreach (DataRow Row in dt.Rows)
            {
                T This = (T)Activator.CreateInstance(typeof(T));
                int index = 0;
                foreach (object s in Row.ItemArray)
                {
                    PropertyInfo pi = null; try
                    {
                        pi = Prop.ElementAt(index++);
                        pi.SetValue(This, Cast(s, pi.PropertyType));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                        throw ex;
                    }
                }
                Table.Add(This);
            }
            return Table;
        }
        private static object Cast(object obj, Type type)
        {
            if (type.Equals(typeof(DateTime)))
            {
                return Convert.ToDateTime(obj);
            }
            else if (type.Equals(typeof(bool)))
            {
                return Convert.ToBoolean(obj);
            }

            #region Signed Integers
            else if (type.Equals(typeof(sbyte)))
            {
                return Convert.ToSByte(obj);
            }
            else if (type.Equals(typeof(short)))
            {
                return Convert.ToInt16(obj);
            }
            else if (type.Equals(typeof(int)))
            {
                return Convert.ToInt32(obj);
            }
            else if (type.Equals(typeof(long)))
            {
                return Convert.ToInt64(obj);
            }
            #endregion
            #region Unsigned Integer
            else if (type.Equals(typeof(byte)))
            {
                return Convert.ToByte(obj);
            }
            else if (type.Equals(typeof(ushort)))
            {
                return Convert.ToUInt16(obj);
            }
            else if (type.Equals(typeof(uint)))
            {
                return Convert.ToUInt32(obj);
            }
            else if (type.Equals(typeof(ulong)))
            {
                return Convert.ToUInt64(obj);
            }
            #endregion
            #region Real 
            else if (type.Equals(typeof(float)))
            {
                return Convert.ToSingle(obj);
            }
            else if (type.Equals(typeof(double)))
            {
                return Convert.ToDouble(obj);
            }
            else if (type.Equals(typeof(Decimal)))
            {
                return Convert.ToDecimal(obj);
            }
            #endregion
            else
                return Convert.ToString(obj);
        }
        private static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(Type type)
        {
            IEnumerable<PropertyInfo> Prop = type.GetProperties()
                .Where(p =>
                {
                    return p.GetCustomAttributes()
                    .Where(a =>
                    {
                        return a.GetType().ToString().Equals(typeof(T).ToString());
                    })
                    .Any();
                });
            /*
            List<PropertyInfo> tProp = new List<PropertyInfo>();
            List<PropertyInfo> rProp = new List<PropertyInfo>();
            foreach (PropertyInfo pi in Prop)
            {
                if (pi.GetCustomAttribute(typeof(PrimaryKey)) != null)
                {
                    tProp.Add(pi);
                }
                else
                    rProp.Add(pi);
            }
            tProp.AddRange(rProp);
            return tProp;*/
            return Prop;
        }

        #endregion
        public static T newInstance<T>()
        {
            T Instance = Activator.CreateInstance<T>();
            //(Instance as SQLiteModel).PropertyChanged += (s, args) =>
            //{
            //    Console.WriteLine("Property Changed");
            //    (Instance as SQLiteModel).Dirty = true;
            //};
            return Instance;
        }
    }
    #region Attributes For SQLite
    [AttributeUsage(AttributeTargets.Property)]
    public class Field : System.Attribute
    {
        public Field() { }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : System.Attribute
    {
        public PrimaryKey() { }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class Unique : System.Attribute
    {
        public Unique() { }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNull : System.Attribute
    {
        public NotNull() { }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrement : System.Attribute
    {
        public AutoIncrement() { }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class Default : System.Attribute
    {
        public object Value { get; set; }
        public Default(object value)
        {
            this.Value = value;
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class DirtyBit : System.Attribute
    {
        public DirtyBit() { }
    }
    #endregion

}
