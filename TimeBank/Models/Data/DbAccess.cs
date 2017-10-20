using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBank.Models.Data
{
    static class DbAccess
    {
        private static readonly object _lock = new object();

        public static void MainContext(Action<MainContext.MainContext> action)
        {
            // Sqliteは複数スレッドから同時に書き込むと壊れるので、実質同時アクセス不可と定義する
            lock (_lock)
            {
                using (var db = new MainContext.MainContext())
                {
                    action(db);
                }
            }
        }
    }
}
