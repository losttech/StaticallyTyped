namespace LostTech.StaticallyTyped {
    using System;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;

    public static class StaticallyTyped {
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
    }

    // ReSharper disable once InconsistentNaming
    static class StaticallyTyped<I> where I: class
    {
        static readonly Func<IDynamicMetaObjectProvider, I> Constructor;

        [NotNull]
        public static I AssumeType([NotNull] IDynamicMetaObjectProvider dynamic)
            => dynamic == null ? throw new ArgumentNullException(nameof(dynamic)) : Constructor(dynamic);

        static StaticallyTyped() {
            if (typeof(I).IsInterface == false)
                throw new ArgumentException("Only interface types are supported", nameof(I));
            Constructor = _ => throw new NotImplementedException();
        }
    }
}
