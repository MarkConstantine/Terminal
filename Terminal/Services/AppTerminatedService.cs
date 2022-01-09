using System;

namespace Terminal.Services
{
    public class AppTerminatedService
    {
        private static AppTerminatedService _instance;
        public static AppTerminatedService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppTerminatedService();
                }
                return _instance;
            }
        }

        public event EventHandler Terminated;
        
        private AppTerminatedService()
        {
        }

        public void Terminate()
        {
            Terminated?.Invoke(this, EventArgs.Empty);
        }
    }
}
