namespace Sandra.Snow.PreCompiler.Exceptions
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