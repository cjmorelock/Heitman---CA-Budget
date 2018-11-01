using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CABudget.Model.SQL {
    public abstract class GenericRepository<T> : IRepository<T> where T : class {
        public abstract List<T> GetList(Func<T, bool> where);
        public virtual T Project (SqlDataReader reader) {
            Type t = typeof(T);
            T item = Activator.CreateInstance<T>();
            
            // column names in the reader must match property names in class T; data types for columns should also be convertable to data types of properties.
            for (int i = 0; i < reader.FieldCount; i++) {
                PropertyInfo prop = t.GetProperty(reader.GetName(i));
                if (prop != null) {
                    prop.SetValue(item, reader.Value(i, prop.PropertyType));
                }
            }

            return item;
        }
    }
}
