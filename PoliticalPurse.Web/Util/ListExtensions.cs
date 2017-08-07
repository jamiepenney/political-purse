using System.Collections.Generic;
using System.Linq;

namespace PoliticalPurse.Web.Util
{
    public static class ListExtensions
    {
        public static IEnumerable<List<T>> Batch<T>(this List<T> items, int blockSize)
        {
            return items.Select((x, index) => new {x, index})
                .GroupBy(x => x.index / blockSize, y => y.x)
                .Select(g => g.ToList());
        }
    }
}