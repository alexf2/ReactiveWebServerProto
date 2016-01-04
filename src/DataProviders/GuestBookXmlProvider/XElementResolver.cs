using System;
using System.Xml.Linq;
using AutoMapper;

namespace AnywayAnyday.DataProviders.GuestBookXmlProvider
{
    public sealed class XAttributeResolver<T> : ValueResolver<XAttribute, T>
    {
        protected override T ResolveCore (XAttribute source)
        {
            if (string.IsNullOrEmpty(source?.Value))
                return default(T);
            return (T) Convert.ChangeType(source.Value, typeof (T));
        }    
    }
}
