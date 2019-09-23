using System;
using System.Reflection;
using Sigil;

namespace ReflectionSpeed {
    public class IlEmit {
        public static Func<object, object> EmitGetter(PropertyInfo propertyInfo) {
            var getterEmiter = Emit<Func<object, object>>
                              .NewDynamicMethod("GetTestClassDataProperty")
                              .LoadArgument(0)
                              .CastClass(propertyInfo.DeclaringType)
                              .Call(propertyInfo.GetGetMethod(nonPublic: true))
                              .CastClass(typeof(object))
                              .Return();
            return getterEmiter.CreateDelegate();
        }
    }
}
