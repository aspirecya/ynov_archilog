using APILibrary.Core.Attributes;
using APILibrary.Core.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace APILibrary.Core.Extensions
{
    public static class IQueryableExtensions
    {
        public static object SelectObject(object value, string[] fields)
        {
            var expo = new ExpandoObject() as IDictionary<string, object>;
            var collectionType = value.GetType();

            foreach (var field in fields)
            {
                var prop = collectionType.GetProperty(field, BindingFlags.Public |
                    BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (prop != null)
                {
                    var isPresentAttribute = prop.CustomAttributes
                         .Any(x => x.AttributeType == typeof(NotJsonAttribute));
                    if (!isPresentAttribute)
                        expo.Add(prop.Name, prop.GetValue(value));
                }
                else
                {
                    throw new Exception($"Property {field} does not exist.");
                }
            }
            return expo;
        }
        public static IQueryable<TModel> OrderByx<TModel>(this IQueryable<TModel> query, string orderByProperty,bool desc) 
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
                var type = typeof(TModel);
               var property = type.GetProperty(orderByProperty);
               var parameter = Expression.Parameter(type, "p");
               var propertyAccess = Expression.MakeMemberAccess(parameter, property);
               var orderByExpression = Expression.Lambda(propertyAccess, parameter);

               var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                             query.Expression, Expression.Quote(orderByExpression));
               return query.Provider.CreateQuery<TModel>(resultExpression);
        }
        public static IQueryable<TModel> Filtres<TModel>(this IQueryable<TModel> query, string key, string value)
        {
            Expression finalExpression = Expression.Constant(true);
            var type = typeof(TModel);
            var property = type.GetProperty(key);// get type of property string datetime ect
            ParameterExpression pe = Expression.Parameter(typeof(TModel), "s"); // get the type of data pizza or customer
            MemberExpression me = Expression.Property(pe, key);// get the property to request

            var xx = value.Split(",");// serch if operator
            if (xx.Length > 1)
            {
                Expression expression = null;
                if (value.Contains("]") && property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(int))
                {
                    /*if (value.Contains("[,"))// inferior
                    {
                            value = value.TrimEnd('[', ',', ']');
                            ConstantExpression constant = Expression.Constant(value, property.PropertyType);
                            finalExpression = Expression.LessThanOrEqual(me, constant);
                    }
                    else if (value.Contains(",]"))// superior
                    {
                        value = value.TrimEnd('[', ',', ']');
                        ConstantExpression constant = Expression.Constant(value, property.PropertyType);
                        finalExpression = Expression.GreaterThanOrEqual(me, constant);
                    }
                    else // fourchette (beetween)
                    {
                        Expression expression2 = null;
                        value = value.TrimEnd('[',']');
                        var x = value.Split(",");
                        ConstantExpression constant1 = Expression.Constant(x[0], property.PropertyType);
                        ConstantExpression constant2 = Expression.Constant(x[1], property.PropertyType);
                        expression = Expression.GreaterThanOrEqual(me, constant1);
                        expression2 = Expression.LessThanOrEqual(me, constant2);
                        finalExpression = Expression.And(finalExpression, expression);
                        finalExpression = Expression.And(finalExpression, expression2);
                    }*/
                }
                else // OR 
                {
                    foreach(var x in xx)
                    {
                        var concerted = Convert.ChangeType(x, typeof(object));
                        ConstantExpression constant = Expression.Constant(concerted, property.PropertyType);
                        expression = Expression.Equal(me, constant);
                        finalExpression = Expression.Or(finalExpression, expression);
                    }
                }

            }
            else // JUST EQUAL
            {
                ConstantExpression constant = Expression.Constant(value, property.PropertyType); // create value to compare with type
                finalExpression = Expression.Equal(me, constant); // EQUAL EX: EMAIL==POKEMON@POKEMON.COM
            }
          

            var ExpressionTree = Expression.Lambda<Func<TModel, bool>>(finalExpression, new[] { pe });

            return query.Where(ExpressionTree);

        }
        public static IQueryable<dynamic> SelectDynamic<TModel>(this IQueryable<TModel> query, string[] fields) where TModel : ModelBase
        {
            var parameter = Expression.Parameter(typeof(TModel), "x");

            var membersExpression = fields.Select(y => Expression.Property(parameter, y));

            var membersAssignment = membersExpression.Select(z => Expression.Bind(z.Member, z));

            var body = Expression.MemberInit(Expression.New(typeof(TModel)), membersAssignment);

            var lambda = Expression.Lambda<Func<TModel, dynamic>>(body, parameter);

            return query.Select(lambda);
        }

        public static IQueryable<TModel> SelectModel<TModel>(this IQueryable<TModel> query, string[] fields) where TModel : ModelBase
        {
            var parameter = Expression.Parameter(typeof(TModel), "x");

            var membersExpression = fields.Select(y => Expression.Property(parameter, y));

            var membersAssignment = membersExpression.Select(z => Expression.Bind(z.Member, z));

            var body = Expression.MemberInit(Expression.New(typeof(TModel)), membersAssignment);

            var lambda = Expression.Lambda<Func<TModel, TModel>>(body, parameter);

            return query.Select(lambda);
        }

        public static IQueryable<TModel> SelectRange<TModel>(this IQueryable<TModel> query, int start, int range) where TModel : ModelBase
        {
            return query.Skip(start).Take(range);
        }
    }
}