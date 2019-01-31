namespace LostTech.StaticallyTyped {
    using System;
    using System.Dynamic;
    using System.Reflection;
    using JetBrains.Annotations;

    internal class StaticallyTypedWrapperBase {
        protected readonly IDynamicMetaObjectProvider dynamic;

        protected StaticallyTypedWrapperBase([NotNull] IDynamicMetaObjectProvider dynamic) {
            this.dynamic = dynamic ?? throw new ArgumentNullException(nameof(dynamic));
        }

        internal static readonly FieldInfo DynamicField = typeof(StaticallyTypedWrapperBase)
            .GetField(nameof(dynamic), BindingFlags.Instance | BindingFlags.NonPublic);

        internal static readonly ConstructorInfo Constructor = typeof(StaticallyTypedWrapperBase)
            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: new[] {typeof(IDynamicMetaObjectProvider)},
                modifiers: null);

        internal const string ConstructorParameterName = nameof(dynamic);
    }
}
