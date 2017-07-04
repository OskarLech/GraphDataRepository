namespace Libraries.Server
{
    public class TriplestoreOperationResult
    {
        public bool Succeeded { get; set; } = true;
        public object OperationResult { get; set; } = null;
    }
}
