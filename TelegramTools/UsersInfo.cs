namespace TelegramTools
{
    public class UsersInfo
    {
        private static UsersInfo? s_instance;
        private static object s_instanceLock = new object();

        private Dictionary<long, StateGroup> _userState;

        private protected UsersInfo()
        {
            _userState = new Dictionary<long, StateGroup>();
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
    }
}
