namespace LostTech.StaticallyTyped {
    using System;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using JetBrains.Annotations;

    public static class StaticallyTyped {
        public const string DynamicCodeAssemblyName = "LostTech.StaticallyTyped.DynamicCode";

        /// <summary>
        /// Assumes, that <see cref="dynamic"/> object implements interface <typeparamref name="I"/>.
        /// Returns a statically-typed wrapper of type <typeparamref name="I"/> for this object.
        /// </summary>
        /// <typeparam name="I">An interface type the input object is expected to implement</typeparam>
        /// <param name="dynamic">A dynamic object, that will be wrapped into a static interface</param>
        /// <returns>A statically-typed wrapper of type <typeparamref name="I"/> for the input object.</returns>
        [NotNull]
        // ReSharper disable once InconsistentNaming
        public static I AssumeType<I>([NotNull] this IDynamicMetaObjectProvider dynamic)
            where I: class => StaticallyTyped<I>.AssumeType(dynamic);

        internal static readonly ModuleBuilder wrappersModule;

        static StaticallyTyped() {
            var assemblyName = new AssemblyName(DynamicCodeAssemblyName);
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            wrappersModule = assembly.DefineDynamicModule(assemblyName.Name);
        }
    }

    // ReSharper disable once InconsistentNaming
    static class StaticallyTyped<I> where I: class
    {
        static readonly Func<IDynamicMetaObjectProvider, I> Constructor;

        [NotNull]
        public static I AssumeType([NotNull] IDynamicMetaObjectProvider dynamic)
            => dynamic is null ? throw new ArgumentNullException(nameof(dynamic)) : Constructor(dynamic);

        static StaticallyTyped() {
            if (typeof(I).IsInterface == false)
                throw new ArgumentException("Only interface types are supported", nameof(I));

            Type wrapper = GenerateWrapperType();
            Constructor = dyn => (I)Activator.CreateInstance(wrapper, dyn);
        }

        static Type GenerateWrapperType() {
            string typeName = $"{typeof(I).Name}";
            string ns = "LostTech.StaticallyTyped.Wrappers." + (typeof(I).Namespace ?? "");
            // TODO: resolve naming conflicts
            TypeBuilder wrapper = StaticallyTyped.wrappersModule.DefineType(
                name: ns + "." + typeName, 
                attr: TypeAttributes.Sealed | TypeAttributes.Class,
                parent: typeof(StaticallyTypedWrapperBase));

            wrapper.AddInterfaceImplementation(typeof(I));

            PropertyWrapper.CreatePropertyImplementations<I>(wrapper);

            DefineConstructor(wrapper);

            return wrapper.CreateTypeInfo();
        }

        static void DefineConstructor(TypeBuilder wrapper) {
            var ctor = wrapper.DefineConstructor(MethodAttributes.Public | MethodAttributes.Public,
                CallingConventions.Standard, new[] {typeof(IDynamicMetaObjectProvider)});
            var dynamic = ctor.DefineParameter(1, ParameterAttributes.None,
                StaticallyTypedWrapperBase.ConstructorParameterName);

            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Ldarg_1); // dynamic
            il.Emit(OpCodes.Call, StaticallyTypedWrapperBase.Constructor); // base(dynamic)
            il.Emit(OpCodes.Ret);
        }
    }
}
