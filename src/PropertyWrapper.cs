namespace LostTech.StaticallyTyped {
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    static class PropertyWrapper {
        internal static void CreatePropertyImplementations<I>(TypeBuilder typeWrapper) {
            foreach (var property in typeof(I).GetProperties()) {
                var indexTypes = property.GetIndexParameters().Select(p => p.ParameterType).ToArray();
                var wrapper = typeWrapper.DefineProperty(property.Name,
                    property.Attributes,
                    property.PropertyType,
                    indexTypes);
                if (property.GetMethod != null) {
                    var getter = typeWrapper.DefineMethod("get_" + property.Name,
                        PropertyAccessorAttributes,
                        returnType: property.PropertyType,
                        parameterTypes: indexTypes);
                    var il = getter.GetILGenerator();
                    if (indexTypes.Length == 0) {
                        Label onBindFailure = il.DefineLabel();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, StaticallyTypedWrapperBase.DynamicField); // this.dynamic
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Call, Constant); // Expression.Constant(this.dynamic)
                        il.Emit(OpCodes.Callvirt, GetMetaObject); // this.dynamic.GetMetaObject(...)

                        il.Emit(OpCodes.Ldstr, property.Name); // member name
                        il.Emit(False); // ignoreCase
                        il.Emit(OpCodes.Newobj, GetMemberBinderCtor); // binder

                        il.Emit(OpCodes.Callvirt, BindGetMember); // member meta object
                        
                        il.Emit(OpCodes.Call, MetaObjectValue); // get member value

                        if (property.PropertyType.IsValueType)
                            il.Emit(OpCodes.Unbox_Any);
                        else if (property.PropertyType != typeof(object))
                            il.Emit(OpCodes.Castclass, property.PropertyType);
                        il.Emit(OpCodes.Ret);
                    } else
                        throw new NotImplementedException();
                    wrapper.SetGetMethod(getter);

                    typeWrapper.DefineMethodOverride(getter, property.GetMethod);
                }

                if (property.SetMethod != null)
                    throw new NotImplementedException();
            }
        }

        const MethodAttributes PropertyAccessorAttributes =
            MethodAttributes.Public | MethodAttributes.SpecialName |
            MethodAttributes.Virtual | MethodAttributes.HideBySig;

        static readonly OpCode False = OpCodes.Ldc_I4_0;
        static readonly OpCode True = OpCodes.Ldc_I4_1;

        static readonly MethodInfo TryGetMember =
            typeof(DynamicObject).GetMethod(nameof(DynamicObject.TryGetMember));
        static readonly MethodInfo TrySetMember =
            typeof(DynamicObject).GetMethod(nameof(DynamicObject.TrySetMember));
        static readonly MethodInfo TryGetIndex =
            typeof(DynamicObject).GetMethod(nameof(DynamicObject.TryGetIndex));
        static readonly MethodInfo TrySetIndex =
            typeof(DynamicObject).GetMethod(nameof(DynamicObject.TrySetIndex));

        static readonly MethodInfo BindGetMember = typeof(DynamicMetaObject)
            .GetMethod(nameof(DynamicMetaObject.BindGetMember), new []{typeof(GetMemberBinder)});
        static readonly MethodInfo MetaObjectValue = typeof(DynamicMetaObject)
            .GetProperty(nameof(DynamicMetaObject.Value)).GetMethod;

        static readonly ConstructorInfo GetMemberBinderCtor = typeof(GetMemberBinder)
            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: new[] {typeof(string), typeof(bool)},
                modifiers: null);
        static readonly MethodInfo GetMetaObject = typeof(IDynamicMetaObjectProvider)
            .GetMethod(nameof(IDynamicMetaObjectProvider.GetMetaObject));

        static readonly MethodInfo Constant = typeof(Expression)
            .GetMethod(nameof(Expression.Constant), new []{typeof(object)});
    }
}
