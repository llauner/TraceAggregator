
using System;
using System.Globalization;

namespace TraceAggregator.Extension
{
    public static class Generic
    {
        public static T TryCast<T>(this object obj)
        {
            //will cover 99%
            if (obj is T value)
            {
                return value;
            }

            // will cover most plausible data type
            if (obj != null)
            {
                var str = obj.ToString();
                switch (true)
                {
                    case var _ when typeof(T) == typeof(string):
                        return (T)obj;

                    case var _ when typeof(T) == typeof(int) && int.TryParse(str, out var aValue):
                        return (T)(object)aValue;

                    case var _ when typeof(T) == typeof(long) && long.TryParse(str, out var aValue):
                        return (T)(object)aValue;

                    case var _ when typeof(T) == typeof(DateTime) && DateTime.TryParse(str, out var aValue):
                        return (T)(object)aValue;

                    case var _ when typeof(T) == typeof(byte) && byte.TryParse(str, out var aValue):
                        return (T)(object)aValue;

                    case var _ when typeof(T) == typeof(short) && short.TryParse(str, out var aValue):
                        return (T)(object)aValue;

                    case var _ when typeof(T) == typeof(bool) && bool.TryParse(str, out var aValue):
                        return (T)(object)aValue;

                    case var _ when typeof(T) == typeof(float) && float.TryParse(str, NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture, out var aValue):
                        return (T)(object)aValue;
                    case var _ when typeof(T) == typeof(float?) && float.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var aValue):
                        return (T)(object)aValue;
                }

            }
            return default(T);
        }


    }
}
