namespace MoverSoft.Common.Extensions
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    public static class ExceptionExtensions
    {
        public static bool IsFatal(this Exception ex)
        {
            if (ex is AggregateException)
            {
                return ex.Cast<AggregateException>().Flatten().InnerExceptions.Any(exception => exception.IsFatal());
            }

            if (ex.InnerException != null && ex.InnerException.IsFatal())
            {
                return true;
            }

            return
                ex is TypeInitializationException ||
                ex is AppDomainUnloadedException ||
                ex is ThreadInterruptedException ||
                ex is AccessViolationException ||
                ex is InvalidProgramException ||
                ex is BadImageFormatException ||
                ex is StackOverflowException ||
                ex is ThreadAbortException ||
                ex is OutOfMemoryException ||
                ex is SecurityException ||
                ex is SEHException;
        }
    }
}
