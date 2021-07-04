using SsPvo.Ui.Annotations;
using SsPvo.Ui.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SsPvo.Ui.Common.Batch
{
    public partial class BatchAction : INotifyPropertyChanged
    {
        #region fields
        private SortableBindingList<Item> _items;
        private Status _batchStatus;

        #endregion

        #region ctor
        public BatchAction(
            Scenario scenario,
            IEnumerable<Item> items = null)
        {
            BatchScenario = scenario;
            BatchStatus = Status.NotStarted;
            Items = items != null 
                ? new SortableBindingList<Item>(items) 
                : new SortableBindingList<Item>();
            Handlers = new List<IBatchScenarioHandler>();
        }
        #endregion

        #region events
        public event EventHandler<string> Log;
        public event EventHandler<Status> StatusChanged;
        #endregion

        #region props
        public SortableBindingList<Item> Items
        {
            get => _items;
            private set
            {
                if (Equals(value, _items)) return;
                _items = value;
                OnPropertyChanged();
            }
        }
        public Scenario BatchScenario { get; }
        public Status BatchStatus
        {
            get => _batchStatus;
            set
            {
                if (value == _batchStatus) return;
                _batchStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AllowStart));
                OnPropertyChanged(nameof(AllowStop));
                OnPropertyChanged(nameof(AllowPause));
                OnPropertyChanged(nameof(AllowResume));
                OnPropertyChanged(nameof(AllowAddItems));
                OnPropertyChanged(nameof(AllowRemoveItems));
                StatusChanged?.Invoke(this, value);
            }
        }

        public List<IBatchScenarioHandler> Handlers { get; }

        public bool AllowStart => BatchStatus == Status.NotStarted || BatchStatus != Status.InProgress;
        public bool AllowStop => BatchStatus == Status.InProgress;
        public bool AllowPause => BatchStatus == Status.InProgress;
        public bool AllowResume => BatchStatus == Status.Paused;
        public bool AllowAddItems => BatchStatus != Status.InProgress && BatchStatus != Status.Paused;
        public bool AllowRemoveItems => BatchStatus != Status.InProgress && BatchStatus != Status.Paused;

        #endregion

        #region methods
        public async Task Run(Options options, CancellationToken token)
        {
            if (!Items.Any())
            {
                OnLog("Нет элементов для обработки. Отмена");
                BatchStatus = Status.Canceled;
                return;
            }

            BatchStatus = Status.InProgress;
            OnLog(BatchScenario.GetDescription());

            options.AddOrUpdate<Action<string>>(nameof(Options.CommonOptions.Log), OnLog);

            int i = 0;
            foreach (var item in Items)
            {
                if (token.IsCancellationRequested)
                {
                    Canceled();
                    return;
                }

                OnLog($"{i + 1}/{Items.Count}. {item.Description}");

                Status itemResult = Status.InProgress;

                try
                {
                    itemResult = await HandleItemAsync(item, options, token);
                }
                catch (Exception e)
                {
                    itemResult = item.SetResult(Status.Error, $"{e.Message}{(e.InnerException != null ? " " + e.InnerException.Message : null)}");
                }

                switch (itemResult)
                {
                    case Status.Completed:
                    case Status.Error:
                        OnLog(item.ResultDescription);
                        break;
                    case Status.Canceled:
                        OnLog(item.ResultDescription);
                        bool stopAllOnCancel =
                            options.GetValueOrDefault<bool>(nameof(Options.CommonOptions.StopAllOnCancel));
                        if (stopAllOnCancel)
                        {
                            Canceled();
                            return;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                bool stopAllOnError = options.GetValueOrDefault<bool>(nameof(Options.CommonOptions.StopAllOnError));

                if (item.Status == Status.Error && stopAllOnError) break;

                i++;
            }

            if (Items.Any(x => x.Status == Status.Error))
            {
                OnLog("Возникли ошибки в процессе обработки.");
                BatchStatus = Status.Error;
            }
            else
            {
                OnLog("Обработка завершена.");
                BatchStatus = Status.Completed;
            }
        }

        private async Task<Status> HandleItemAsync(Item item, Options options, CancellationToken token)
        {
            var handler = Handlers.FirstOrDefault(x => x.Scenario == item.Scenario);
            if (handler == null) return item.SetResult(Status.Error, "Обработчик не зарегистрирован!");

            bool sepThread =
                options.GetValueOrDefault<bool>(nameof(BatchAction.Options.CommonOptions.HandleItemsInSeparateThread));
            if (sepThread)
            {
                return await Task.Run(async () => await handler.ProcessItemAsync(item, options, token), token);
            }
            else
            {
                return await handler.ProcessItemAsync(item, options, token);
            }
        }

        private void Canceled()
        {
            OnLog("Операция отменена пользователем.");
            BatchStatus = Status.Canceled;
        }

        private void OnLog(string msg)
        {
            Log?.Invoke(this, msg);
        }

        private void Reset()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Reset();
            }
            BatchStatus = Status.NotStarted;
        }
        #endregion

        #region classes

        #endregion


        #region INPC
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
