using PuyoTextEditor.Properties;
using System;
using System.Runtime.Serialization;

namespace PuyoTextEditor
{
    /// <summary>
    /// The exception that is thrown when a null value is assigned to a variable that does not accept it as a valid value.
    /// </summary>
    public class NullValueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullValueException"/> class.
        /// </summary>
        public NullValueException() : base(Resources.ValueNull)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullValueException"/> class with the name of the variable that causes this exception.
        /// </summary>
        /// <param name="memberName">The name of the variable that caused the exception.</param>
        public NullValueException(string? memberName) : base(Resources.ValueNull)
        {
            MemberName = memberName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullValueException"/> class with a specified error message and the exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for this exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public NullValueException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes an instance of the <see cref="NullValueException"/> class with a specified error message and the name of the parameter that causes this exception.
        /// </summary>
        /// <param name="memberName">The name of the variable that caused the exception.</param>
        /// <param name="message">A message that describes the error.</param>
        public NullValueException(string? memberName, string? message) : base(message)
        {
            MemberName = memberName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullValueException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">An object that describes the source or destination of the serialized data.</param>
        protected NullValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message => !string.IsNullOrEmpty(MemberName)
            ? base.Message + Environment.NewLine + string.Format(Resources.VariableName, MemberName)
            : base.Message;

        public string? MemberName { get; }
    }
}
