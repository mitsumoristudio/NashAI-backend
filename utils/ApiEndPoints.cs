namespace NashAI_app.utils;

public class ApiEndPoints
{
    private const string ApiBase = "api";

    public static class Chats
    {
        public const string ChatsBase = $"{ApiBase}/chats";

        public const string SEND_URL_CHATS = $"send";

        public const string? SEARCH_URL_CHATS = $"search";

        public const string SEND_SEMANTIC_SEARCH = $"semanticsend";

        //   public const string GET_CONVERSATION_HISTORY = $"history/{{sessionid}}";
    }
}