namespace Snow.Exceptions
{
    using System;

    public class ProcessorNotFoundException : Exception
    {
        public ProcessorNotFoundException(string message)
            : base(message)
        {
            
        }
    }
}