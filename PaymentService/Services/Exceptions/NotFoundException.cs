using System;

namespace PaymentService.Services.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string content) : base()
        {
            Content = content;
        }

        public string Content { get; set; }
    }
}
