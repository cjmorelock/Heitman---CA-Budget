using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    public static class ExtensionMethods {
        // Maps a primitive .NET Type to an instance *method* of SqlDataReader in order to get a column value as the correct data type
        private static Dictionary<Type, Func<SqlDataReader, int, object>> _mapSqlToNetDataType =
            new Dictionary<Type, Func<SqlDataReader, int, object>> {
                { typeof(Boolean),        (reader, index) => reader.GetBoolean(index) },
                { typeof(Byte),           (reader, index) => reader.GetByte(index) },
                { typeof(DateTime),       (reader, index) => reader.GetDateTime(index) },
                { typeof(DateTimeOffset), (reader, index) => reader.GetDateTimeOffset(index) },
                { typeof(Decimal),        (reader, index) => reader.GetDecimal(index) },
                { typeof(Double),         (reader, index) => reader.GetDouble(index) },
                { typeof(Single),         (reader, index) => reader.GetFloat(index) },
                { typeof(Int16),          (reader, index) => reader.GetInt16(index) },
                { typeof(Int32),          (reader, index) => reader.GetInt32(index) },
                { typeof(Int64),          (reader, index) => reader.GetInt64(index) },
                { typeof(Guid),           (reader, index) => reader.GetGuid(index) },
                { typeof(String),         (reader, index) => Convert.ToString(reader.GetValue(index)) },
                { typeof(Byte[]),         (reader, index) => GetBytes(reader, index) }
            };

        private static Byte[] GetBytes(SqlDataReader reader, int index) {
            Byte[] buf = null;
            long len = reader.GetBytes(index, 0, buf, 0, int.MaxValue);
            return buf;
        }

        /// <summary>
        /// Read the specified column (by index) of the SqlDataReader and returns it as an object of the specified Type t.
        /// May throw an exception if the specified Type t is not compatible with the SQL data type of the column.
        /// If the specified Type t is not one of the handled Types, then the return object will be of its native SQL data type
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="index"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object Value(this SqlDataReader reader, int index, Type t) {
            if (reader.IsDBNull(index)) return null;

            if (_mapSqlToNetDataType.TryGetValue(t, out var f)) {
                return f(reader, index);  // call the function stored in the dictionary that is key'd off the specified Type t
            } else {
                return reader.GetValue(index);
            }
        }

        public static T Value<T>(this SqlDataReader reader, int index) where T : class {
            return (T)Value(reader, index, typeof(T));
        }

        public static object Value(this SqlDataReader reader, string columnName, Type t) {
            return Value(reader, reader.GetOrdinal(columnName), t);
        }

        public static T Value<T>(this SqlDataReader reader, string columnName) where T : class {
            return (T)Value(reader, reader.GetOrdinal(columnName), typeof(T));
        }
    }
}
