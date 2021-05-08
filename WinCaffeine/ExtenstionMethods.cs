﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinCaffeine
{
    public static class ExtenstionMethods
    {
        public static T FirstOr<T>(this IEnumerable<T> source, T alternate)
        {
            foreach (T t in source)
                return t;
            return alternate;
        }
    }
}
