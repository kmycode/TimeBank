using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBank.Models.Data.MainContext;

namespace TimeBank.Models.DataContainer
{
    class WorkCollection : ReactiveCollection<Work>
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
            IEnumerable<Work> getWorks(MainContext db)
            {
                var wks = db.Works.ToArray();
                db.Works.AttachRange(wks);
                return wks;
            };

            // DBからロード
            IEnumerable<Work> works;
            using (var db = new MainContext())
            {
                works = getWorks(db);
                if (!works.Any())
                {
                    this.SaveDefaultWorks(db);
                    works = getWorks(db);
                }
            }

            // DBの内容をメモリに記録
            this.ClearOnScheduler();
            this.AddRangeOnScheduler(works);

            this.Loaded?.Invoke(this, new WorksLoadedEventArgs { Works = works, });
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
                db.Works.UpdateRange(this);
            }
        }

        public event EventHandler<WorksLoadedEventArgs> Loaded;
    }

    class WorksLoadedEventArgs : EventArgs
    {
        public IEnumerable<Work> Works { get; set; }
    }
}
