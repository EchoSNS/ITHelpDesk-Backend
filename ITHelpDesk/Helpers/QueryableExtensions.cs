using System.Linq.Expressions;

namespace ITHelpDesk.Helpers
{

    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string columnName, bool desc)
        {
            var parameter = Expression.Parameter(typeof(T), "t");
            var property = Expression.Property(parameter, columnName);
            var lambda = Expression.Lambda(property, parameter);

            string methodName = desc ? "OrderByDescending" : "OrderBy";

            var resultExp = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExp);
        }
    }

}
