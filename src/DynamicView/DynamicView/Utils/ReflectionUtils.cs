using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DynamicView.Utils
{
    public sealed class ReflectionUtils
    {
        public static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static MemberInfo[] GetPublicFieldsAndProperties(Type type)
        {
            return type
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where(mi => mi.MemberType == MemberTypes.Property || mi.MemberType == MemberTypes.Field)
                .ToArray();
        }

        public static MemberInfo GetPublicFieldOrProperty(Type type, bool isProperty, string name)
        {
            if (isProperty)
                return type.GetProperty(name);
            else
                return type.GetField(name);
        }

        public static Type GetMemberType(MemberInfo mi)
        {
            if (mi is PropertyInfo)
            {
                return ((PropertyInfo)mi).PropertyType;
            }
            else if (mi is FieldInfo)
            {
                return ((FieldInfo)mi).FieldType;
            }
            else if (mi is MethodInfo)
            {
                return ((MethodInfo)mi).ReturnType;
            }
            return null;
        }

        public static bool IsCollection(Type type)
        {
            return
                type.IsArray ||
                (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) ||
                                            type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                                                type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                                                    type.GetGenericTypeDefinition() == typeof(IList<>)
                )) ||
                type == typeof(ArrayList) ||
                typeof(IList).IsAssignableFrom(type) ||
                typeof(IList<>).IsAssignableFrom(type)
                ;
        }

        public static bool IsPrimitive(Type type)
        {
            return
                type.IsPrimitive
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(double)
                || type == typeof(Guid)
                || type == typeof(float)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(short)
                || type == typeof(DateTime)
                || ReflectionUtils.IsNullable(type) && IsPrimitive(Nullable.GetUnderlyingType(type))
                || type.IsEnum;
        }

        public static Type ExtractElementType(Type collection)
        {
            if (collection.IsArray)
            {
                return collection.GetElementType();
            }
            if (collection == typeof(ArrayList))
            {
                return typeof(object);
            }
            if (collection.IsGenericType)
            {
                return collection.GetGenericArguments()[0];
            }
            return collection;
        }

        public static Guid ConvertToGuid(object value)
        {
            return Guid.Parse(value.ToString());
        }

        public static MemberExpression GetMemberInfo(Expression method)
        {
            LambdaExpression lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }


        public static Dictionary<int, int> Clone(Dictionary<int, int> values)
        {
            if (values == null)
                return null;

            var collection = new Dictionary<int, int>();

            foreach (var item in values)
            {
                collection.Add(item.Key, item.Value);
            }

            return collection;

        }

    }
}
