using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Extensions;
using FastMember;

namespace ReflectionSpeed
{
    class Program
    {
        static void Main(string[] args) {

            for (var i = 0; i < 3; i++) {
                DirectCall();
                WithReflectionDelegate();
                WithLambda();
                WithFastReflection();
                WithFastMemberTypeAccessor();
                WithFastMemberObjectAccessor();
                WithReflection();
                Console.WriteLine();
                System.Threading.Thread.Sleep(100);
            }
            
        }

        static ICollection<MyClass> GetCollection() => Enumerable.Repeat(new MyClass(), 10000000).ToList();

        static void DirectCall() {

            var myClassList = GetCollection();

            var sw = new Stopwatch();
            
            object aux = 0;    
            sw.Start();
            foreach (var obj in myClassList)    
            {    
                aux = obj.Number;    
                obj.Number = 3;    
            }    
            sw.Stop();
            Console.WriteLine($"DirectCall: {sw.Elapsed.TotalMilliseconds} msec");
        }

        static void WithReflection() {
            var list = GetCollection();
            var sw = new Stopwatch();
            
            object aux = 0;
            var pi = list.First().GetType().GetProperty("Number");
            sw.Start();
            foreach (var obj in list) {
                aux = pi.GetValue(obj);
                pi.SetValue(obj, 3);
            }
            sw.Stop();
            Console.WriteLine($"WithReflection: {sw.Elapsed.TotalMilliseconds} msec");
        }

        static void WithFastMemberObjectAccessor() {
            var list = GetCollection();
            var sw = new Stopwatch();
            
            object aux = 0;
            sw.Start();
            foreach (var obj in list) {
                var oa = ObjectAccessor.Create(obj);
                aux = oa["Number"];
                oa["Number"] = 3;
            }
            sw.Stop();
            Console.WriteLine($"WithFastMemberObjectAccessor: {sw.Elapsed.TotalMilliseconds} msec");
        }

        static void WithFastMemberTypeAccessor() {
            var list = GetCollection();
            var sw = new Stopwatch();
            
            object aux = 0;
            var ta = TypeAccessor.Create(typeof(MyClass));
            sw.Start();
            foreach (var obj in list) {
                aux = ta[obj, "Number"];
                ta[obj, "Number"] = 3;
            }
            sw.Stop();
            Console.WriteLine($"WithFastMemberTypeAccessor: {sw.Elapsed.TotalMilliseconds} msec");
        }

        static void WithFastReflection() {
            var list = GetCollection();
            var sw = new Stopwatch();
            
            object aux = 0;
            var pi = list.First().GetType().GetProperty("Number");
            var getter = pi.DelegateForGet();
            var setter = pi.DelegateForSet();
            sw.Start();
            foreach (var obj in list) {
                aux = getter(obj);
                var o = (object)obj;
                setter(ref o, 3);
            }
            sw.Stop();
            Console.WriteLine($"WithFastReflection: {sw.Elapsed.TotalMilliseconds} msec");
        }


        static void WithReflectionDelegate() {
            var myClassList = GetCollection();
            var sw = new Stopwatch();
            
            object aux = 0;
            var propertyInfo = myClassList.First().GetType().GetProperty("Number");
            var setter = (Action<MyClass, int>)Delegate.CreateDelegate(typeof(Action<MyClass, int>), null, propertyInfo.GetSetMethod());
            var getter = (Func<MyClass, int>)Delegate.CreateDelegate(typeof(Func<MyClass, int>), null, propertyInfo.GetGetMethod());
           
            sw.Start();
            foreach (var obj in myClassList)  
            {  
                aux = getter(obj);  
                setter(obj, 3);  
            }  
            sw.Stop();
            Console.WriteLine($"WithReflectionDelegate: {sw.Elapsed.TotalMilliseconds} msec");
        }

        //static Func<TClass, object> CompileGetter<TClass>(string propertyName) {
        //    var param = Expression.Parameter(typeof(TClass));
        //    var body = Expression.Convert(Expression.Property(param, propertyName), typeof(object));
        //    return Expression.Lambda<Func<TClass, object>>(body,param).Compile();
        //}

        //static Action<TClass, object> CompileSetter<TClass>(string propertyName) {
        //    var propertyInfo = typeof(TClass).GetProperty(propertyName);
        //    if (propertyInfo == null) return null;
        //    var targetType = propertyInfo.DeclaringType;
        //    if (targetType == null) return null;
        //    var param = Expression.Parameter(targetType);
        //    var value = Expression.Parameter(typeof(object));
        //    var body = Expression.Call(param, propertyInfo.GetSetMethod(), Expression.Convert(value, propertyInfo.PropertyType));
        //    return Expression.Lambda<Action<TClass, object>>(body, param, value).Compile();
        //}


        static void WithLambda() {
            var myClassList = GetCollection();
            var sw = new Stopwatch();
            
            object aux = 0;
            
            var setter = EnumerableExtensions.CompileSetter<MyClass>("Number");
            var getter = EnumerableExtensions.CompileGetter<MyClass>("Number");

            sw.Start();
            foreach (var obj in myClassList) {
                
                aux = getter(obj);
                setter(obj, 3);
            }  
            sw.Stop();
            Console.WriteLine($"WithLambda: {sw.Elapsed.TotalMilliseconds} msec");
        }
    }

    public class MyClass    
    {    
        public int Number { get; set; }    
    }
}
