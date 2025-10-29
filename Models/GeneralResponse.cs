namespace ASH_Translation.Models
{
    public class GeneralResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object Error { get; set; }

        public void SetResponse(bool isSuccess, string message, object data = null,object errors=null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            Error = errors;
        }
    }
}
