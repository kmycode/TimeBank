using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBank.Models.Data.MainContext;

namespace TimeBank.Models.Stopwatch
{
    class StopwatchModel
    {
        /// <summary>
        /// 現在動いているか
        /// </summary>
        public ReactiveProperty<bool> IsWorking { get; } = new ReactiveProperty<bool>();

        /// <summary>
        /// 現在稼働しているワーク
        /// </summary>
        public ReactiveProperty<Work> CurrentWork { get; } = new ReactiveProperty<Work>();

        /// <summary>
        /// 現在の経過秒数
        /// </summary>
        public ReactiveProperty<int> CurrentSeconds { get; } = new ReactiveProperty<int>();

        public StopwatchModel()
        {
            // １秒ごとに処理する
            Observable.Interval(new TimeSpan(0, 0, 1))
                .Where(time => this.CurrentWork.Value != null)
                .Where(time => this.IsWorking.Value)
                .Subscribe(time => this.CurrentSeconds.Value++);

            // １０秒ごとにデータを保存する
            this.CurrentSeconds
                .Where(time => time % 10 == 0)
                .Where(time => this.CurrentWork.Value != null)
                .Subscribe(time => this.Save());

            // ワークを切り替えたら現在の秒数も切り替える
            this.CurrentWork
                .Subscribe(this.OnCurrentWorkChanged);
        }

        private void OnCurrentWorkChanged(Work work)
        {
            if (work == null)
            {
                this.IsWorking.Value = false;
                this.CurrentSeconds.Value = 0;
            }
            else
            {
                using (var db = new MainContext())
                {
                    this.CurrentSeconds.Value = this.GetCurrentSeconds(db);
                }
            }
        }

        private void Save()
        {
            using (var db = new MainContext())
            {
                var workTime = this.GetCurrentWorkTime(db);

                workTime.DoneSeconds = this.CurrentSeconds.Value;
                db.SaveChanges();
            }
        }

        private WorkTime GetCurrentWorkTime(MainContext db)
        {
            var now = DateTime.Now;
            var workTime = db.WorkTimes.Where(wt => wt.Year == now.Year &&
                                                    wt.Month == now.Month &&
                                                    wt.Day == now.Day &&
                                                    wt.WorkId == this.CurrentWork.Value.Id)
                                        .SingleOrDefault();

            if (workTime == null)
            {
                workTime = new WorkTime
                {
                    Year = now.Year,
                    Month = now.Month,
                    Day = now.Day,
                    WorkId = this.CurrentWork.Value.Id,
                };
                db.WorkTimes.Add(workTime);
            }

            return workTime;
        }

        private int GetCurrentSeconds(MainContext db)
        {
            var workTimes = db.WorkTimes.Where(wt => wt.WorkId == this.CurrentWork.Value.Id)
                                        .Sum(wt => wt.DoneSeconds);
            return workTimes;
        }
    }
}
