using МаршрутСборки.Models;

namespace МаршрутСборки.Helpers
{
    //хранит текущего пользователя на всё время сессии
    public static class SessionContext
    {
        public static User? CurrentUser { get; set; }

        public static bool IsAdmin => CurrentUser?.Role?.RoleName == "Технический директор";
        public static bool IsDispatcher => CurrentUser?.Role?.RoleName == "Диспетчер";
        public static bool IsAssembler => CurrentUser?.Role?.RoleName == "Сборщик";
        public static bool IsStorekeeper => CurrentUser?.Role?.RoleName == "Кладовщик";
        public static bool IsTester => CurrentUser?.Role?.RoleName == "Тестировщик";
        public static bool IsWarrantyEngineer => CurrentUser?.Role?.RoleName == "Инженер сервисного центра";

        public static void Clear() => CurrentUser = null;
    }
}

