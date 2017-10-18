using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBank.Models.Data.MainContext
{
    class WorkTime
    {
        public int Id { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        /// <summary>
        /// ワークのID
        /// </summary>
        public int WorkId { get; set; }

        /// <summary>
        /// 実績秒数
        /// </summary>
        public int DoneSeconds { get; set; }
    }
}
