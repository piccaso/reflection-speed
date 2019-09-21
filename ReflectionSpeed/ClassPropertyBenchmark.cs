using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Extensions;
using FastMember;

namespace ReflectionSpeed
{
    [RankColumn, MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class ClassPropertyBenchmark
    {
        private static readonly IList<DataClass> Data = Enumerable.Range(0,1000).Select(nr=>new DataClass{Nr = nr}).ToList();

        private static void Verify(int expected, object actual) {
            if (!(actual is int i) || expected != i) {
                throw new Exception($"{expected} != {actual}");
            }
        }

        [Benchmark(Baseline = true)]
        public void DirectAccess() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = Data[i].Nr;
                Verify(i, aux);
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

        private static readonly Func<DataClass, object> LambdaGetter = EnumerableExtensions.CompileGetter<DataClass>("Nr");
        [Benchmark]
        public void Lambda() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = LambdaGetter(Data[i]);
                Verify(i, aux);
            }
        }

        private static readonly TypeAccessor FastMemberTypeAccessor = TypeAccessor.Create(typeof(DataClass));
        [Benchmark]
        public void FastMember() {
            for (var i = 0; i < Data.Count; i++) {
                
                var aux = FastMemberTypeAccessor[Data[i], "Nr"];
                Verify(i, aux);
            }
        }

        private static readonly Func<object, object> OpenDelegateGetter = typeof(DataClass).GetProperty("Nr").GetGetMethodAsOpenDelegate();
        [Benchmark]
        public void OpenDelegate() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = OpenDelegateGetter(Data[i]);
                Verify(i, aux);
            }
        }

        private static readonly Func<DataClass, int> TypedDelegateGetter =  (Func<DataClass, int>)Delegate.CreateDelegate(typeof(Func<DataClass, int>), null, typeof(DataClass).GetProperty("Nr").GetGetMethod());
        [Benchmark]
        public void TypedDelegate() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = TypedDelegateGetter(Data[i]);
                Verify(i, aux);
            }
        }

        private static readonly MemberGetter<object, object> FastReflectionGetter = typeof(DataClass).GetProperty("Nr").DelegateForGet();
        [Benchmark]
        public void FastReflection() {
            for (var i = 0; i < Data.Count; i++) {
                var aux = FastReflectionGetter(Data[i]);
                Verify(i, aux);
            }
        }


        private static readonly PropertyInfo PropertyInfo = typeof(DataClass).GetProperty("Nr");
        [Benchmark]
        public void Reflection() {
            
            for (var i = 0; i < Data.Count; i++) {
                var aux = PropertyInfo.GetValue(Data[i]);
                Verify(i, aux);
            }
        }
        
    }

    public class DataClass {
        public int Nr { get; set; }
    }
}
