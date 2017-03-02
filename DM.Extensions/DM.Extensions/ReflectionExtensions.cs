using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DM.Extensions
{
    /// <summary>
    /// Represents set of extension methods for working with reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        private static readonly IDictionary<int, IEnumerable<Type>> appDomainTypes = new ConcurrentDictionary<int, IEnumerable<Type>>();

        /// <summary>
        /// Returns all types of defines application domain.
        /// </summary>
        /// <param name="appDomain">Application domain to get types from.</param>
        public static IEnumerable<Type> AllTypes(this AppDomain appDomain)
        {
            IEnumerable<Type> domainTypes;

            // ReSharper disable once InconsistentlySynchronizedField
            if (appDomainTypes.TryGetValue(appDomain.Id, out domainTypes))
            {
                return domainTypes;
            }

            var foundDomainTypes = new HashSet<Type>();
            lock (appDomainTypes)
            {
                var assemblies = appDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foundDomainTypes.Add(type);
                    }
                }

                appDomainTypes[appDomain.Id] = foundDomainTypes;
            }

            return foundDomainTypes;
        }

        /// <summary>
        /// Returns of all types in domain from which T can be assigned.
        /// </summary>
        /// <typeparam name="T">type to check.</typeparam>
        /// <param name="appDomain">Application domain.</param>
        /// <param name="isIncludeAbstract">Defines if abstract types should be included into result.</param>
        public static IEnumerable<Type> AllTypes<T>(this AppDomain appDomain, bool isIncludeAbstract = false)
        {
            return appDomain.AllTypes().OfType<T>(isIncludeAbstract);
        }

        /// <summary>
        /// Returns of types from which T can be assigned.
        /// </summary>
        /// <typeparam name="T">type to check.</typeparam>
        /// <param name="types">List of types to check.</param>
        /// <param name="isIncludeAbstract">Defines if abstract types should be included into result.</param>
        public static IEnumerable<Type> OfType<T>(this IEnumerable<Type> types, bool isIncludeAbstract = false)
        {
            var expectedType = typeof(T);

            foreach (var scannedType in types)
            {
                if (scannedType.IsAbstract && !isIncludeAbstract)
                {
                    continue;
                }

                if (!expectedType.IsAssignableFrom(scannedType))
                {
                    continue;
                }

                yield return scannedType;
            }
        }

        /// <summary>
        /// Returns a property getter delegate.
        /// </summary>
        /// <param name="propertyInfo">Property involved for getting.</param>
        /// <returns>The compiled delegate to get value to property.</returns>
        public static Delegate GetGetterDelegate(this PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                return null;
            }

            Type propertyType = propertyInfo.PropertyType;
            Type objectType = propertyInfo.DeclaringType;

            ParameterExpression paramExpression = Expression.Parameter(objectType, "value");
            Expression propertyGetterExpression = Expression.Property(paramExpression, propertyInfo);

            var funcType = typeof(Func<,>).MakeGenericType(objectType, propertyType);
            return Expression.Lambda(funcType, propertyGetterExpression, paramExpression).Compile();
        }

        /// <summary>
        /// Returns a property setter delegate.
        /// </summary>
        /// <param name="propertyInfo">The property info to get setter.</param>
        /// <returns>The compiled delegate to set value to property.</returns>
        public static Delegate GetSetterDelegate(this PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
            {
                return null;
            }

            Type propertyType = propertyInfo.PropertyType;
            Type objectType = propertyInfo.DeclaringType;

            ParameterExpression paramExpression = Expression.Parameter(objectType);
            ParameterExpression paramExpression2 = Expression.Parameter(propertyInfo.PropertyType, propertyInfo.Name);
            MemberExpression propertyGetterExpression = Expression.Property(paramExpression, propertyInfo.Name);

            var actionType = typeof(Action<,>).MakeGenericType(objectType, propertyType);

            return Expression.Lambda(
                                actionType,
                                Expression.Assign(propertyGetterExpression, paramExpression2),
                                paramExpression,
                                paramExpression2).Compile();
        }

        /// <summary>
        /// Returns list of properties with attributes from object.
        /// </summary>
        /// <param name="obj">Object to get properties from.</param>
        /// <param name="bindingFlags">Binding Flags.</param>
        /// <typeparam name="T">Type of attribute.</typeparam>
        /// <returns>List of pairs attribute-property info.</returns>
        public static IEnumerable<KeyValuePair<T, PropertyInfo>> GetAttributedProperties<T>(this object obj, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
            where T : Attribute
        {
            return obj.GetType().GetAttributedProperties<T>(bindingFlags);
        }

        /// <summary>
        /// Returns list of properties with attributes from type.
        /// </summary>
        /// <param name="objectType">Type to get properties from.</param>
        /// <param name="bindingFlags">Binding Flags.</param>
        /// <typeparam name="T">Type of attribute.</typeparam>
        /// <returns>List of pairs attribute-property info.</returns>
        public static IEnumerable<KeyValuePair<T, PropertyInfo>> GetAttributedProperties<T>(this Type objectType, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
            where T : Attribute
        {
            var attributeType = typeof(T);

            var result = new List<KeyValuePair<T, PropertyInfo>>();

            var properties = objectType.GetProperties(bindingFlags);

            foreach (var proeprtyInfo in properties)
            {
                var customAttribute = proeprtyInfo.GetCustomAttributes(attributeType).FirstOrDefault() as T;

                if (customAttribute == null)
                {
                    continue;
                }

                result.Add(new KeyValuePair<T, PropertyInfo>(customAttribute, proeprtyInfo));
            }

            return result;
        }

        /// <summary>
        /// Returns list of custom attributes for type.
        /// </summary>
        /// <typeparam name="T">Type of attribute to return.</typeparam>
        /// <param name="type">Input type.</param>
        /// <param name="isInherit">Defines if type of attribute can be derived from input type.</param>
        public static T GetTypeAttribute<T>(this MemberInfo type, bool isInherit = true)
            where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), isInherit).OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Checks if type is nullable.
        /// </summary>
        /// <param name="type">Type to check.</param>
        public static bool IsNullable(this Type type)
        {
            return
                type != null &&
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Check if type is numeric.
        /// </summary>
        /// <param name="type">Type to check.</param>
        public static bool IsNumeric(this Type type)
        {
            if (type == null || type.IsEnum)
            {
                return false;
            }

            if (IsNullable(type))
            {
                return IsNumeric(Nullable.GetUnderlyingType(type));
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns Property info from instance and expression
        /// </summary>
        /// <typeparam name="T">The type of instance</typeparam>
        /// <param name="instance">The instance</param>
        /// <param name="expression">The expression with property info</param>
        public static PropertyInfo GetPropertyInfo<T>(this T instance, Expression<Func<T, object>> expression)
        {
            return expression.GetPropertyInfo();
        }

        /// <summary>
        /// Gets property from expression
        /// </summary>
        /// <param name="expression">Property expression</param>
        /// <returns>Property</returns>
        public static PropertyInfo GetPropertyInfo(this LambdaExpression expression)
        {
            MemberExpression memberExpression;

            if (expression.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)expression.Body;
                if (unaryExpression.Operand is MemberExpression)
                {
                    memberExpression = (MemberExpression)unaryExpression.Operand;
                }
                else
                {
                    throw new ArgumentException("Couldn't get property from expression.", nameof(expression));
                }
            }
            else if (expression.Body is MemberExpression)
            {
                memberExpression = (MemberExpression)expression.Body;
            }
            else
            {
                throw new ArgumentException("Couldn't get property from expression.", nameof(expression));
            }

            return (PropertyInfo)memberExpression.Member;
        }
    }
}