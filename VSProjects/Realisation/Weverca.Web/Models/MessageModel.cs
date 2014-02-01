
namespace Weverca.Web.Models
{
    public class MessageModel
    {
        public string Title { get; private set; }
        public string Message { get; private set; }

        public MessageModel(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}