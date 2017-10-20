using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBank.Models.Common;
using TimeBank.Models.Data.MainContext;
using TimeBank.Models.DataContainer;

namespace TimeBank.Models.Stopwatch
{
    class StopwatchModel
    {
        private List<IDisposable> secondsBindings = new List<IDisposable>();
        
        /// <summary>
        /// 現在動いているか
        /// </summary>
        public ReactiveProperty<bool> IsWorking { get; } = new ReactiveProperty<bool>();

        /// <summary>
        /// 現在稼働しているワーク
        /// </summary>
        public ReactiveProperty<WorkItem> CurrentWork { get; } = new ReactiveProperty<WorkItem>();

        /// <summary>
        /// 現在の経過秒数
        /// </summary>
        public ReactiveProperty<int> CurrentSeconds { get; } = new ReactiveProperty<int>();

        /// <summary>
        /// 今日の経過秒数（バインドされる）
        /// </summary>
        public ReactiveProperty<int> TodaySeconds { get; } = new ReactiveProperty<int>();

        /// <summary>
        /// 昨日の経過秒数（バインドされる）
        /// </summary>
        public ReactiveProperty<int> YesterdaySeconds { get; } = new ReactiveProperty<int>();

        public StopwatchModel()
        {
            // １秒ごとに処理する
            Observable.Interval(new TimeSpan(0, 0, 1))
                .Where(time => this.CurrentWork.Value?.Work.Value != null && this.IsWorking.Value)
                .Subscribe(time => this.StepSeconds());

            // ワークを切り替えたら現在の秒数も切り替える
            this.CurrentWork
                .Subscribe(this.OnCurrentWorkChanged);
        }

        /// <summary>
        /// ワーク一覧がロードされた時に呼び出す
        /// </summary>
        /// <param name="sender">イベント発生元</param>
        /// <param name="e">イベントパラメータ</param>
        public void OnWorksLoaded(object sender, WorksLoadedEventArgs e)
        {
            this.CurrentWork.Value = e.WorkItems.FirstOrDefault();
        }

        /// <summary>
        /// 現在のワークが変更された時に呼び出される
        /// </summary>
        /// <param name="workItem">新しいワーク</param>
        private void OnCurrentWorkChanged(WorkItem workItem)
        {
            this.IsWorking.Value = false;

            foreach (var b in this.secondsBindings)
            {
                b.Dispose();
            }
            this.secondsBindings.Clear();

            if (workItem == null)
            {
                this.CurrentSeconds.Value = this.TodaySeconds.Value = this.YesterdaySeconds.Value = 0;
            }
            else
            {
                this.CurrentSeconds.Value = workItem.TotalSeconds.Value;
                this.TodaySeconds.Value = workItem.TodaySeconds.Value;
                this.YesterdaySeconds.Value = workItem.YesterdaySeconds.Value;

                this.secondsBindings.Add(this.CurrentSeconds.Bind(workItem.TotalSeconds));
                this.secondsBindings.Add(this.TodaySeconds.Bind(workItem.TodaySeconds));
                this.secondsBindings.Add(this.YesterdaySeconds.Bind(workItem.YesterdaySeconds));
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
            this.CurrentWork.Value?.SaveSeconds();
        }
    }
}
