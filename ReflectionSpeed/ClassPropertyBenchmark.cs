using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Extensions;
using FastMember;

namespace ReflectionSpeed
{
    [RankColumn, MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class ClassPropertyBenchmark {
        private static IList<DataClass> _data;

        [Params(10000)]
        public int N;

        [GlobalSetup]
        public void Setup() {
            if (_data == null || _data.Count != N) {
                _data = Enumerable.Range(0, N).Select(nr => new DataClass {Nr = nr}).ToList();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Verify<T>(T expected, object actual) where T : IEquatable<T> {
            if (!(actual is T a) || !EqualityComparer<T>.Default.Equals(expected, a)) {
                throw new Exception($"{expected} != {actual}");
            }
        }

        [Benchmark(Baseline = true)]
        public void DirectAccess() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = _data[i].Nr;
                Verify(i, aux);
            }
        }

        private static readonly Func<object, object> DynamicGetter = (o) => ((dynamic) o).Nr;
        [Benchmark]
        public void Dynamics() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = DynamicGetter(_data[i]);
                Verify(i, aux);
            }
        }

        private static readonly Func<DataClass, object> LambdaGetter = EnumerableExtensions.GetGetFunc<DataClass>("Nr");
        [Benchmark]
        public void Lambda() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = LambdaGetter(_data[i]);
                Verify(i, aux);
            }
        }

        private static readonly Func<DataClass, int> TypedLambdaLambdaGetter = EnumerableExtensions.GetGetFunc<DataClass, int>("Nr");
        [Benchmark]
        public void TypedLambda() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = TypedLambdaLambdaGetter(_data[i]);
                Verify(i, aux);
            }
        }

        private static readonly TypeAccessor FastMemberTypeAccessor = TypeAccessor.Create(typeof(DataClass));
        [Benchmark]
        public void FastMember() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = FastMemberTypeAccessor[_data[i], "Nr"];
                Verify(i, aux);
            }
        }

        private static readonly Func<object, object> OpenDelegateGetter = typeof(DataClass).GetProperty("Nr").GetGetMethodAsOpenDelegate();
        [Benchmark]
        public void OpenDelegate() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = OpenDelegateGetter(_data[i]);
                Verify(i, aux);
            }
        }

        private static readonly Func<DataClass, int> TypedDelegateGetter =  (Func<DataClass, int>)Delegate.CreateDelegate(typeof(Func<DataClass, int>), null, typeof(DataClass).GetProperty("Nr").GetGetMethod());
        [Benchmark]
        public void TypedDelegate() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = TypedDelegateGetter(_data[i]);
                Verify(i, aux);
            }
        }

        private static readonly MemberGetter<object, object> FastReflectionGetter = typeof(DataClass).GetProperty("Nr").DelegateForGet();
        [Benchmark]
        public void FastReflection() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = FastReflectionGetter(_data[i]);
                Verify(i, aux);
            }
        }


        private static readonly Func<object, object> PropertyInfoGetter = typeof(DataClass).GetProperty("Nr").GetValue;
        [Benchmark]
        public void ReflectionPropertyInfo() {
            
            for (var i = 0; i < _data.Count; i++) {
                var aux = PropertyInfoGetter(_data[i]);
                Verify(i, aux);
            }
        }

        private static readonly Func<object, object> TypeDescriptorGetter = TypeDescriptor.GetProperties(typeof(DataClass))["Nr"].GetValue;
        [Benchmark]
        public void ReflectionTypeDescriptor() {
            for (var i = 0; i < _data.Count; i++) {
                var aux = TypeDescriptorGetter(_data[i]);
                Verify(i, aux);
            }
        }
        
    }

    public class DataClass {
        public int Nr { get; set; }
    }
}
