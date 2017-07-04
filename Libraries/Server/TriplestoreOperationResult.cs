using System;

namespace Libraries.Server
{
    public class TriplestoreOperationResult
    {
        public bool Succeeded { get; set; } = false;
        public object OperationResult { get; set; } = null;
        public Type ResultType { get; set; } = null;
    }
}
