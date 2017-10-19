﻿using Reactive.Bindings;
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

        /// <summary>
        /// 今日の経過秒数
        /// </summary>
        public ReactiveProperty<int> TodaySeconds { get; } = new ReactiveProperty<int>();

        public StopwatchModel()
        {
            // １秒ごとに処理する
            Observable.Interval(new TimeSpan(0, 0, 1))
                .Where(time => this.CurrentWork.Value != null && this.IsWorking.Value)
                .Subscribe(time => this.StepSeconds());

            // １０秒ごとにデータを保存する
            this.TodaySeconds
                .Where(time => this.CurrentWork.Value != null && time % 10 == 0)
                .Subscribe(time => this.SaveSeconds());

            // ワークを切り替えたら現在の秒数も切り替える
            this.CurrentWork
                .Subscribe(this.OnCurrentWorkChanged);
        }

        /// <summary>
        /// 現在のワークが変更された時に呼び出される
        /// </summary>
        /// <param name="work">新しいワーク</param>
        private void OnCurrentWorkChanged(Work work)
        {
            this.IsWorking.Value = false;

            if (work == null)
            {
                this.CurrentSeconds.Value = 0;
            }
            else
            {
                using (var db = new MainContext())
                {
                    // 秒数を取得する
                    this.CurrentSeconds.Value = db.WorkTimes.Where(wt => wt.WorkId == this.CurrentWork.Value.Id)
                                                            .Sum(wt => wt.DoneSeconds);
                    this.TodaySeconds.Value = this.GetTodayWorkTime(db).DoneSeconds;
                }
            }
        }

        /// <summary>
        /// データを１秒分進める
        /// </summary>
        private void StepSeconds()
        {
            this.CurrentSeconds.Value++;
            this.TodaySeconds.Value++;
        }

        /// <summary>
        /// ワークで稼働した秒数を保存する
        /// </summary>
        private void SaveSeconds()
        {
            using (var db = new MainContext())
            {
                var workTime = this.GetTodayWorkTime(db);

                workTime.DoneSeconds = this.TodaySeconds.Value;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 今日の分のワークデータを取得する
        /// </summary>
        /// <param name="db">データベース</param>
        /// <returns>今日の分のワークデータ</returns>
        private WorkTime GetTodayWorkTime(MainContext db)
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
    }
}
