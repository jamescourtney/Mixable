// This file provides polyfills for various attributes
// needed for nullable analysis.

#if NETSTANDARD2_0

global using NotNullWhenAttribute = Mixable.Polyfills.NotNullWhenAttribute;
global using DoesNotReturnIfAttribute = Mixable.Polyfills.DoesNotReturnIfAttribute;
global using NotNullIfNotNullAttribute = Mixable.Polyfills.NotNullIfNotNullAttribute;

#endif

using System;
using System.Diagnostics.CodeAnalysis;

#if NETSTANDARD2_0

namespace Mixable.Polyfills
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    internal class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue)
        {
        }
    }

    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    internal class DoesNotReturnIfAttribute : Attribute
    {
        public DoesNotReturnIfAttribute(bool value)
        {
        }
    }

    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    internal class NotNullIfNotNullAttribute : Attribute
    {
        public NotNullIfNotNullAttribute(string parameterName)
        {
        }
    }
}
#endif

#if !NET5_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    [ExcludeFromCodeCoverage]
    internal static class IsExternalInit { }
}

#endif