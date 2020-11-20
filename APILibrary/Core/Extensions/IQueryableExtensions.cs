﻿using APILibrary.Core.Attributes;
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

        public static IQueryable<TModel> SelectRange<TModel>(this IQueryable<TModel> query, HttpContext httpContext, int count, int start, int range) where TModel : ModelBase
        {
            // fail

            //int acceptRange = 50;

            //var queryString = HttpUtility.ParseQueryString(httpContext.Request.QueryString.ToString());
            //var requestUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}";

            //queryString.Set("range", $"0-{range}");
            //var first = $"{requestUrl}?{queryString}; rel=\"first\", ";

            //queryString.Set("range", $"{Math.Max(0, start-range)}-{range}");
            //var prev = $"{requestUrl}?{queryString}; rel=\"prev\", ";
            
            //queryString.Set("range", $"KEKW");
            //var next = $"{requestUrl}?{queryString}; rel=\"next\", ";

            //queryString.Set("range", $"{count-range}-{count}");
            //var last = $"{requestUrl}?{queryString}; rel=\"last\",";

            //var linkString = $"{first}{prev}{next}{last}";

            //httpContext.Response.Headers.Add("Content-Range", start + "-" + range + "/" + count);
            //httpContext.Response.Headers.Add("Accept-Range", $"{acceptRange}");
            //httpContext.Response.Headers.Add("Link", $"{linkString}");

            return query.Skip(start).Take(range);
        }
    }
}