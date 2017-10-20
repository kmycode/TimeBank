using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBank.Models.Data;
using TimeBank.Models.Data.MainContext;

namespace TimeBank.Models.DataContainer
{
    class WorkItem : IWorkItemInitializer
    {
        public ReadOnlyReactiveProperty<Work> Work { get; }
        private readonly ReactiveProperty<Work> _work = new ReactiveProperty<Work>();

        /// <summary>
        /// 現在の経過秒数
        /// </summary>
        public ReactiveProperty<int> TotalSeconds { get; } = new ReactiveProperty<int>();

        /// <summary>
        /// 今日の経過秒数
        /// </summary>
        public ReactiveProperty<int> TodaySeconds { get; } = new ReactiveProperty<int>();

        /// <summary>
        /// 昨日の経過秒数
        /// </summary>
        public ReactiveProperty<int> YesterdaySeconds { get; } = new ReactiveProperty<int>();

        public WorkItem(Work work, out IWorkItemInitializer initializer)
        {
            this._work.Value = work;
            this.Work = this._work.ToReadOnlyReactiveProperty();

            initializer = this;
        }

        /// <summary>
        /// アイテムを初期化する（通常このメソッドは、newした側からしか呼び出せないようにする）
        /// </summary>
        /// <param name="db">データベース</param>
        void IWorkItemInitializer.Initialize(MainContext db)
        {
            // 秒数を取得する
            this.TotalSeconds.Value = db.WorkTimes.Where(wt => wt.WorkId == this.Work.Value.Id)
                                                    .Sum(wt => wt.DoneSeconds);
            this.TodaySeconds.Value = this.GetTodayWorkTime(db).DoneSeconds;
            this.YesterdaySeconds.Value = this.GetWorkTime(db, DateTime.Now.AddDays(-1))?.DoneSeconds ?? 0;

            // １０秒ごとにデータを保存する
            this.TodaySeconds
                .Where(time => time != 0 && this.Work.Value != null && time % 10 == 0)
                .Subscribe(time => this.SaveSeconds());
        }

        /// <summary>
        /// データを保存する
        /// </summary>
        public void SaveSeconds()
        {
            DbAccess.MainContext(db =>
            {
                var workTime = this.GetTodayWorkTime(db);

                db.WorkTimes.Attach(workTime);
                workTime.DoneSeconds = this.TodaySeconds.Value;
                db.SaveChanges();
            });
        }

        /// <summary>
        /// 今日の分のワークデータを取得する
        /// </summary>
        /// <param name="db">データベース</param>
        /// <returns>今日の分のワークデータ</returns>
        private WorkTime GetTodayWorkTime(MainContext db)
        {
            var now = DateTime.Now;
            var workTime = this.GetWorkTime(db, now);

            if (workTime == null)
            {
                workTime = new WorkTime
                {
                    Year = now.Year,
                    Month = now.Month,
                    Day = now.Day,
                    WorkId = this.Work.Value.Id,
                };
                db.WorkTimes.Add(workTime);
                db.SaveChanges();
            }

            return workTime;
        }

        /// <summary>
        /// 指定日付のワークデータを取得する
        /// </summary>
        /// <param name="db">データベース</param>
        /// <param name="dt">日付</param>
        /// <returns>指定日付のワークデータ</returns>
        private WorkTime GetWorkTime(MainContext db, DateTime dt)
        {
            var workTime = db.WorkTimes.Where(wt => wt.Year == dt.Year &&
                                        wt.Month == dt.Month &&
                                        wt.Day == dt.Day &&
                                        wt.WorkId == this.Work.Value.Id)
                            .SingleOrDefault();
            return workTime;
        }
    }

    /// <summary>
    /// WorkItemをnewしたオブジェクトでのみ実行できる、初期化メソッド
    /// </summary>
    interface IWorkItemInitializer
    {
        void Initialize(MainContext db);
    }
}
