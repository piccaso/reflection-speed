using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Extensions {
    public static class EnumerableExtensions {

        internal static Func<TClass, object> CompileGetter<TClass>(string propertyName) {
            var param = Expression.Parameter(typeof(TClass));
            var body = Expression.Convert(Expression.Property(param, propertyName), typeof(object));
            return Expression.Lambda<Func<TClass, object>>(body,param).Compile();
        }

        internal static Action<TClass, object> CompileSetter<TClass>(string propertyName) {
            var propertyInfo = typeof(TClass).GetProperty(propertyName);
            if (propertyInfo == null) return null;
            var targetType = propertyInfo.DeclaringType;
            if (targetType == null) return null;
            var param = Expression.Parameter(targetType);
            var value = Expression.Parameter(typeof(object));
            var body = Expression.Call(param, propertyInfo.GetSetMethod(), Expression.Convert(value, propertyInfo.PropertyType));
            return Expression.Lambda<Action<TClass, object>>(body, param, value).Compile();
        }

        public static ICollection<IDictionary<string, object>> ToDictionaryCollection<T>(this IEnumerable<T> enumerable) {
            if (enumerable is null) return null;
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (!properties.Any()) return null;
            var getters = properties.Where(p => p.CanRead).Select(p => new {
                p.Name, GetValue = CompileGetter<T>(p.Name),
            }).ToList();
            var resp = new Collection<IDictionary<string, object>>();

            foreach (var row in enumerable) {
                var rowDict = new Dictionary<string, object>();
                foreach (var getter in getters) {
                    rowDict[getter.Name] = getter.GetValue(row);
                }
                resp.Add(rowDict);
            }

            return resp;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> collection) {
            var dataTable = new DataTable();
            var properties = typeof(T)
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanRead)
                            .ToArray();

            if (properties.Length < 1) return null;
            var getters = new Func<T, object>[properties.Length];

            for (var i = 0; i < properties.Length; i++) {
                var columnType = Nullable.GetUnderlyingType(properties[i].PropertyType) ?? properties[i].PropertyType;
                dataTable.Columns.Add(properties[i].Name, columnType);
                getters[i] = CompileGetter<T>(properties[i].Name);
            }

            foreach (var row in collection) {
                var dtRow = new object[properties.Length];
                for (var i = 0; i < properties.Length; i++) {
                    dtRow[i] = getters[i].Invoke(row) ?? DBNull.Value;
                }
                dataTable.Rows.Add(dtRow);
            }

            return dataTable;
        }
    }
}