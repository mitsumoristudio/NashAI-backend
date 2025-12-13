using SixLabors.ImageSharp.PixelFormats;

namespace NashAI_app.utils;

public class ApiEndPoints
{
    private const string ApiBase = "api";
    
    public static class Projects
    {
        private const string Base = $"{ApiBase}/projects";

        public const string CREATE_URL_PROJECTS_CONSTANT = Base;
        
        public const string GET_URL_PROJECTS = $"{{id:guid}}";

        public const string GETALL_URL_PROJECTS_CONSTANT = Base;
        
        public const string UPDATE_URL_PROJECTS = $"{{id:guid}}";
        
        public const string GET_URL_MYPROJECT = $"{user}/{{userId:guid}}";
        
        public const string DELETE_URL_PROJECTS = $"{{id:guid}}";
        
        public const string? SEARCH_BY_NAME = $"search";

        private const string user = "user";
    }

    public static class Users
    {
        public const string Base = $"{ApiBase}/users";
        
        public const string REGISTER_URL_USER_CONSTANT = $"register";
        
        public const string LOGIN_URL_USER_CONSTANT = $"login";
        
        public const string GET_URL_USER_CONSTANT = $"{{id:guid}}";
        
        public const string DELETE_URL_USER_CONSTANT = $"{{id:guid}}";
        
        public const string UPDATE_URL_USER_CONSTANT = $"{{id:guid}}";
        
        public const string LOGOUT_URL_USER_CONSTANT = $"logout";
        
        public const string? REGISTER_URL_USER_W_EMAIL = $"registerEmail";
    }
    
    public static class Equipments
    {
        private const string Base = $"{ApiBase}/equipments";
        
        public const string CREATE_URL_EQUIPMENT_CONSTANT = Base;
        
        public const string GET_URL_EQUIPMENT = $"{{id:guid}}";
        
        public const string DELETE_URL_EQUIPMENT = $"{{id:guid}}";
        
        public const string UPDATE_URL_EQUIPMENT = $"{{id:guid}}";
        
        //  public const string GETALL_URL_EQUIPMENT = Base;
      
        public const string GET_URL_MYEQUIPMENT = $"{user}/{{userId:guid}}";

        private const string user = "user";
    }

    public static class Chats
    {
        public const string ChatsBase = $"{ApiBase}/chats";

        public const string SEND_URL_CHATS = $"send";

        public const string? SEARCH_URL_CHATS = $"search";

        public const string SEMANTIC_SEARCH_URLS = $"semantic_search";
        
        public const string SUMMARY_SEMANTIC_SEARCH_URLS = $"summary";
        
        public const string SUMMARY_SAFETY_SEARCH_URLS = $"safety_search";
        
        //   public const string GET_CONVERSATION_HISTORY = $"history/{{sessionid}}";
    }
    
    public static class Messages
    {
        public const string Base = $"{ApiBase}/message";
        
        public const string CREATE_URL_MESSAGE = $"sendEmail";
        
        public const string SEND_CONTACT_MESSAGE = $"sendContactMessage";
        
        public const string SEND_VERIFICATION_EMAIL = $"sendVerificationEmail";
    }

    
    public static class Pdfs
    {
        public const string PdfBase = $"{ApiBase}/pdfs";

        public const string INGEST_PDF = $"ingest";
        
    }
}