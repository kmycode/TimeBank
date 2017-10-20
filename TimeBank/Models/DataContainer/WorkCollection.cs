using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBank.Models.Data;
using TimeBank.Models.Data.MainContext;

namespace TimeBank.Models.DataContainer
{
    class WorkCollection : ReactiveCollection<WorkItem>
    {
        public static WorkCollection Instance => _instance ?? (_instance = new WorkCollection());
        private static WorkCollection _instance;

        private WorkCollection()
        {
        }

        public void Initialize() => this.LoadWorks();

        /// <summary>
        /// ワークをデータベースからロードする
        /// </summary>
        private void LoadWorks()
        {
            int initializersCount = 0;
            ref IWorkItemInitializer getInitializerRef(IWorkItemInitializer[] initializerArray)
            {
                return ref initializerArray[initializersCount++];
            };

            // DBからロード
            IEnumerable<Work> works = default;
            DbAccess.MainContext(db =>
            {
                if (!db.Works.Any())
                {
                    this.SaveDefaultWorks(db);
                }

                works = db.Works.ToArray();
            });

            // DBの内容をメモリに記録
            var initializers = new IWorkItemInitializer[works.Count()];
            var items = works.Select(w => new WorkItem(w, out getInitializerRef(initializers))).ToArray();
            this.ClearOnScheduler();
            this.AddRangeOnScheduler(items);

            // 各アイテムを初期化
            DbAccess.MainContext(db =>
            {
                foreach (var init in initializers)
                {
                    init.Initialize(db);
                }
            });

            this.Loaded?.Invoke(this, new WorksLoadedEventArgs { Works = works, WorkItems = items, });
        }

        /// <summary>
        /// デフォルト（プリセット）のワークデータを書き込む
        /// </summary>
        /// <param name="db">データベース</param>
        private void SaveDefaultWorks(MainContext db)
        {
            db.Works.Add(new Work
            {
                Name = "練習",
            });
            db.Works.Add(new Work
            {
                Name = "制作",
            });
            db.SaveChanges();
        }

        /// <summary>
        /// すべてのワークを保存する
        /// </summary>
        private void Save()
        {
            using (var db = new MainContext())
            {
                db.Works.UpdateRange(this.Select(item => item.Work.Value));
            }
        }

        public event EventHandler<WorksLoadedEventArgs> Loaded;
    }

    class WorksLoadedEventArgs : EventArgs
    {
        public IEnumerable<Work> Works { get; set; }
        public IEnumerable<WorkItem> WorkItems { get; set; }
    }
}
