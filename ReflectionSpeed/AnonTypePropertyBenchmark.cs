using System;
using System.Collections.Generic;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Extensions;
using FastMember;

namespace ReflectionSpeed
{
    [RankColumn, MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class AnonTypePropertyBenchmark {

        private static readonly IList<object> Data = GenerateData();

        private static IList<object> GenerateData() {
            var data = new List<object>();
            for (int i = 0; i < 50000; i++) {
                var item = new {Nr = i};
                if (i == 0) {
                    var type = item.GetType();
                    var prop = type.GetProperty("Nr");

                    void CompileGetter<TClass>(TClass obj, string propertyName) {
                        var getter = EnumerableExtensions.CompileGetter<TClass>(propertyName);
                        _lambdaGetter = o => getter((TClass)o);
                    }
                    CompileGetter(item, "Nr");

                    _fastMemberTypeAccessor = TypeAccessor.Create(type);
                    _openDelegateGetter = prop.GetGetMethodAsOpenDelegate();
                    _fastReflectionGetter = prop.DelegateForGet();
                    _propertyInfo = prop;
                }
                data.Add(item);
            }
            return data;
        }

        private static void Verify(int expected, object actual) {
            if (!(actual is int i) || expected != i) {
                throw new Exception($"{expected} != {actual}");
            }
        }

        private static readonly Func<object, object> DynamicGetter = (o) => ((dynamic) o).Nr;
        [Benchmark]
        public void Dynamics() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = DynamicGetter(Data[i]);
                Verify(i, aux);
            }
        }

        private static Func<object, object> _lambdaGetter;
        [Benchmark]
        public void Lambda() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = _lambdaGetter(Data[i]);
                Verify(i, aux);
            }
        }

        private static TypeAccessor _fastMemberTypeAccessor;
        [Benchmark]
        public void FastMember() {
            for (var i = 0; i < Data.Count; i++) {
                
                var aux = _fastMemberTypeAccessor[Data[i], "Nr"];
                Verify(i, aux);
            }
        }

        private static Func<object, object> _openDelegateGetter;
        [Benchmark]
        public void OpenDelegate() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = _openDelegateGetter(Data[i]);
                Verify(i, aux);
            }
        }

        private static MemberGetter<object, object> _fastReflectionGetter;
        [Benchmark]
        public void FastReflection() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = _fastReflectionGetter(Data[i]);
                Verify(i, aux);
            }
        }


        private static PropertyInfo _propertyInfo;
        [Benchmark]
        public void Reflection() {
            
            for (var i = 0; i < Data.Count; i++) {
                var aux = _propertyInfo.GetValue(Data[i]);
                Verify(i, aux);
            }
        }
        
    }
}
