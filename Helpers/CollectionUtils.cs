using System.Collections;

namespace IPrint.Helpers
{
    public static class CollectionUtils
    {
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            return enumerable == null || !enumerable.GetEnumerator().MoveNext();
        }
    }
}
