namespace TelegramTools
{
    /// <summary>
    /// Позволяет получить информацию о пользователях, запустивших бота. 
    /// Является синглетоном в потокобезопасной реализации.
    /// </summary>
    public class UsersInfo
    {
        private static UsersInfo? s_instance = null;
        private static object s_instanceLock = new object();

        private Dictionary<long, StateGroup> _userState;
        private Dictionary<long, string> _userLastFileId;

        private protected UsersInfo()
        {
            _userState = new Dictionary<long, StateGroup>();
            _userLastFileId = new Dictionary<long, string>();
        }
        public static UsersInfo GetInstance()
        {
            // Такая реалиизация позволяет избежать проблем при использовании многопоточности.
            if (s_instance is null)
            {
                lock (s_instanceLock)
                {
                    if (s_instance is null)
                    {
                        s_instance = new UsersInfo();
                    }
                }
            }
            return s_instance;
        }
        public StateGroup GetState(long userId)
        {
            if (_userState.ContainsKey(userId))
            {
                return _userState[userId];
            }
            else
            {
                var stateGroup = new StateGroup();
                _userState[userId] = stateGroup;
                return stateGroup;
            }
        }

        public string? GetFileId(long chatId)
        {
            if (_userLastFileId.ContainsKey(chatId))
            {
                return _userLastFileId[chatId];
            }
            else
            {
                return null;
            }
        }

        public void SetFileId(long chatId, string fileId)
        {
            _userLastFileId[chatId] = fileId;
        }
    }
}
