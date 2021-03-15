using PuyoTextEditor.Properties;
using System;
using System.Xml.Linq;

namespace PuyoTextEditor.Xml
{
    internal static class XElementExtensions
    {

        /// <summary>
        /// Returns the <see cref="XAttribute"/> of this <see cref="XElement"/> that has the specified <see cref="XName"/>, 
        /// or throws a <see cref="NullValueException"/> if there is no attribute with the specified <see cref="XName"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="XAttribute"/> that has the specified <see cref="XName"/>.
        /// </returns>
        /// <exception cref="NullValueException">Thrown when there is no attribute with the specified <see cref="XName"/>.</exception>
        /// <inheritdoc cref="XElement.Attribute(XName)"/>
        public static XAttribute AttributeOrThrow(this XElement element, XName name) => element.Attribute(name)
            ?? throw new NullValueException(string.Format(Resources.ElementAttributeNotFound, element.Name, name), (Exception?)null);
    }
}