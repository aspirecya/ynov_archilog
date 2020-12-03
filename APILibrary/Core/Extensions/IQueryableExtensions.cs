using APILibrary.Core.Attributes;
using APILibrary.Core.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Specialized;
using System.Web;

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

        public static IQueryable<TModel> OrderByx<TModel>(this IQueryable<TModel> query, string orderByProperty, bool desc)
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
                 
                 var type = typeof(TModel);
                 var property = type.GetProperty(orderByProperty);
                 var parameter = Expression.Parameter(type, "p");
                 var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                 var orderByExpression = Expression.Lambda(propertyAccess, parameter);
                 var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType }, query.Expression, Expression.Quote(orderByExpression));
                 return query.Provider.CreateQuery<TModel>(resultExpression);


    /*        Expression finalExpression = Expression.Constant(false);
            var type = typeof(TModel);
            var property = type.GetProperty(orderByProperty);// get type of property string datetime ect
            ParameterExpression paramExp = Expression.Parameter(typeof(TModel), "s"); // get the type of data pizza or customer
            MemberExpression memberExp = Expression.Property(paramExp, orderByProperty);// get the property to request
            var propertyAccess = Expression.MakeMemberAccess(paramExp, property);

            MethodInfo mi = typeof(Queryable).GetMethod(command, new Type[] { typeof(string) });
            Expression call = Expression.Call(memberExp, mi, query.Expression, Expression.Quote(orderByExpression));

            finalExpression = Expression.Or(finalExpression, call);*/
        }

        public static IQueryable<TModel> Filtres<TModel>(this IQueryable<TModel> query, string key, string value)
        {
            Expression finalExpression = Expression.Constant(true);
            var type = typeof(TModel);
            var property = type.GetProperty(key);// get type of property string datetime ect
            ParameterExpression pe = Expression.Parameter(typeof(TModel), "s"); // get the type of data pizza or customer
            MemberExpression me = Expression.Property(pe, key);// get the property to request


            string replacx(string replacx, char[] characs)
            {
                foreach (char charac in characs)
                {
                    replacx = replacx.Replace(charac, ' ');
                }
                return replacx;
            }


            var xx = value.Split(",");// serch if operator
            if (xx.Length > 1)
            {
                Expression expression = null;
                if (value.Contains("]") && property.PropertyType == typeof(DateTime) || value.Contains("]") && property.PropertyType == typeof(int))
                {
                    if (value.Contains("[,"))// inferior
                    {
                        value = replacx(value, new char[] { '[', ',', ']' });
                        ConstantExpression constant = Expression.Constant(Convert.ChangeType(value, property.PropertyType));
                        finalExpression = Expression.LessThanOrEqual(me, constant);
                    }
                    else if (value.Contains(",]"))// superior
                    {
                        value = replacx(value, new char[] { '[', ',', ']' });
                        ConstantExpression constant = Expression.Constant(Convert.ChangeType(value, property.PropertyType));
                        finalExpression = Expression.GreaterThanOrEqual(me, constant);
                    }
                    else // fourchette (beetween)
                    {
                        Expression expression2 = null;
                        value = replacx(value, new char[] { '[', ']' });
                        var x = value.Split(",");
                        ConstantExpression constant1 = Expression.Constant(Convert.ChangeType(x[0], property.PropertyType));
                        ConstantExpression constant2 = Expression.Constant(Convert.ChangeType(x[1], property.PropertyType));
                        expression = Expression.GreaterThanOrEqual(me, constant1);
                        expression2 = Expression.LessThanOrEqual(me, constant2);
                        finalExpression = Expression.And(finalExpression, expression);
                        finalExpression = Expression.And(finalExpression, expression2);
                    }
                }
                else // OR 
                {
                    foreach (var x in xx)
                    {
                        ConstantExpression constant = Expression.Constant(Convert.ChangeType(x, property.PropertyType));
                        expression = Expression.Equal(me, constant);
                        finalExpression = Expression.Or(finalExpression, expression);
                    }
                }

            }
            else // JUST EQUAL
            {
                var w = Convert.ChangeType(value, property.PropertyType);
                ConstantExpression constant = Expression.Constant(w); // create value to compare with type
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

        public static IQueryable<TModel> QuerySearch<TModel>(this IQueryable<TModel> query, string key, string value)
        {
            char[] charsToTrim = { '*' }; // how to trim parameter
            Expression finalExpression = Expression.Constant(false);
            var type = typeof(TModel);
            var property = type.GetProperty(key);// get type of property string datetime ect
            ParameterExpression paramExp = Expression.Parameter(typeof(TModel), "s"); // get the type of data pizza or customer
            MemberExpression memberExp = Expression.Property(paramExp, key);// get the property to request
            ConstantExpression constant = Expression.Constant(Convert.ChangeType(value, property.PropertyType));
            var propertyAccess = Expression.MakeMemberAccess(paramExp, property);

            Expression expression = null;
            if (property.PropertyType == typeof(string)) // start and end
            {
                var xx = value.Split(",");
                foreach(var x in xx)
                {
                    if (x.StartsWith("*") && !x.EndsWith("*")) // endswith 
                    {
                        ConstantExpression c = Expression.Constant(x.Trim(charsToTrim), typeof(string));
                        MethodInfo mi = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
                        Expression call = Expression.Call(memberExp, mi, c);
                        finalExpression = Expression.Or(finalExpression, call);
                    }
                    else if (x.EndsWith("*") && !x.StartsWith("*")) // startswith 
                    {
                        ConstantExpression c = Expression.Constant(x.Trim(charsToTrim), typeof(string));
                        MethodInfo mi = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
                        Expression call = Expression.Call(memberExp, mi, c);
                        finalExpression = Expression.Or(finalExpression, call);
                    }
                    else if (x.EndsWith("*") && x.StartsWith("*")) // contains
                    {
                        ConstantExpression c = Expression.Constant(x.Trim(charsToTrim), typeof(string));
                        MethodInfo mi = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
                        Expression call = Expression.Call(memberExp, mi, c);
                        finalExpression = Expression.Or(finalExpression, call);
                    }
                    else
                    {
                        ConstantExpression c = Expression.Constant(x.Trim(charsToTrim), typeof(string));
                        expression = Expression.Equal(memberExp, constant);
                        finalExpression = Expression.Or(finalExpression, expression);
                    }
                }
            }

            var ExpressionTree = Expression.Lambda<Func<TModel, bool>>(finalExpression, new[] { paramExp });
            return query.Where(ExpressionTree);
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

        public static IQueryable<TModel> SelectRange<TModel>(this IQueryable<TModel> query, HttpContext httpContext, int count, int start, int range) where TModel : ModelBase
        {
            // fail

            int acceptRange = 50;

            var queryString = HttpUtility.ParseQueryString(httpContext.Request.QueryString.ToString());
            var requestUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}";

            queryString.Set("range", $"0-{range}");
            var first = $"{requestUrl}?{queryString}; rel=\"first\", ";

            queryString.Set("range", $"{Math.Max(0, start - range)}-{range}");
            var prev = $"{requestUrl}?{queryString}; rel=\"prev\", ";

            queryString.Set("range", $"{start + range}-{range}");
            var next = $"{requestUrl}?{queryString}; rel=\"next\", ";

            queryString.Set("range", $"{count - range}-{range}");
            var last = $"{requestUrl}?{queryString}; rel=\"last\",";

            var linkString = $"{first}{prev}{next}{last}";

            httpContext.Response.Headers.Add("Content-Range", start + "-" + range + "/" + count);
            httpContext.Response.Headers.Add("Accept-Range", $"{acceptRange}");
            httpContext.Response.Headers.Add("Link", $"{linkString}");

            return query.Skip(start).Take(range);
        }
    }
}