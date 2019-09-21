using System;
using System.Reflection;

// https://stackoverflow.com/a/6055689/568857
// http://msmvps.com/blogs/jon_skeet/archive/2008/08/09/making-reflection-fly-and-exploring-delegates.aspx
// http://web.archive.org/web/20081025203355/http://msmvps.com/blogs/jon_skeet/archive/2008/08/09/making-reflection-fly-and-exploring-delegates.aspx

namespace Extensions {
    internal static class ReflectionUtility {
        internal static Func<object, object> GetGetMethodAsOpenDelegate(this PropertyInfo property) {
            // get the get method for the property
            var method = property.GetGetMethod(true);

            // get the generic get-method generator (ReflectionUtility.GetSetterHelper<TTarget, TValue>)
            var genericHelper = typeof(ReflectionUtility).GetMethod(
                nameof(GetGetterHelper),
                BindingFlags.Static | BindingFlags.NonPublic);

            // reflection call to the generic get-method generator to generate the type arguments
            var constructedHelper = genericHelper?.MakeGenericMethod(
                method.DeclaringType,
                method.ReturnType);

            // now call it. The null argument is because it's a static method.
            var ret = constructedHelper?.Invoke(null, new object[] {method});

            // cast the result to the action delegate and return it
            return (Func<object, object>) ret;
        }

        private static Func<object, object> GetGetterHelper<TTarget, TResult>(MethodInfo method) where TTarget : class { // target must be a class as property sets on structs need a ref param
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            var func = (Func<TTarget, TResult>) Delegate.CreateDelegate(typeof(Func<TTarget, TResult>), method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            object Ret(object target) => func((TTarget) target);
            return Ret;
        }
    }
}