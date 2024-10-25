using System;

namespace PaymentService.Services.Exceptions
{
    public class UniqueKeyException : Exception
    {
        public UniqueKeyException(string content) : base()
        {
            Content = content;
        }

        public string Content { get; set; }
    }
}
