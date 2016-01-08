using System;
using System.Xml.Linq;
using AutoMapper;

namespace AnywayAnyday.DataProviders.GuestBookXmlProvider
{
    /// <summary>
    /// A helper for Automapper configuration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
