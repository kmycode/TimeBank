using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBank.Models.Data.MainContext;
using TimeBank.Models.DataContainer;
using TimeBank.Models.Stopwatch;

namespace TimeBank.ViewModels
{
    class MainViewModel
    {
        private readonly StopwatchModel stopwatch = new StopwatchModel();
        private readonly WorkCollection works = WorkCollection.Instance;

        public ReactiveProperty<bool> IsWorking => this.stopwatch.IsWorking;

        public ReactiveProperty<int> CurrentSeconds => this.stopwatch.CurrentSeconds;

        public ReactiveProperty<int> TodaySeconds => this.stopwatch.TodaySeconds;

        public ReactiveProperty<int> YesterdaySeconds => this.stopwatch.YesterdaySeconds;

        public ReactiveProperty<Work> CurrentWork => this.stopwatch.CurrentWork;

        public ReactiveProperty<bool> CanToggleIsWorking { get; }

        public ReactiveCollection<Work> Works => this.works;

        public MainViewModel()
        {
            this.CanToggleIsWorking = this.CurrentWork.Select(work => work != null).ToReactiveProperty();

            this.works.Loaded += this.stopwatch.OnWorksLoaded;

            this.works.Initialize();
        }
    }
}
