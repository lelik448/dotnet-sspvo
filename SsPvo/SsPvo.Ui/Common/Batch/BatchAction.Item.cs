using System.ComponentModel;
using System.Runtime.CompilerServices;
using SsPvo.Ui.Annotations;

namespace SsPvo.Ui.Common.Batch
{
    partial class BatchAction
    {
        public class Item : INotifyPropertyChanged
        {
            #region fields
            private Scenario _scenario;
            private Status _status;
            private string _description;
            private object _processedEntity;
            private string _resultDescription;
            #endregion

            #region ctor
            public Item(object processedEntity, Scenario scenario)
            {
                _processedEntity = processedEntity;
                Scenario = scenario;
                Status = Status.NotStarted;
            }
            #endregion

            #region props
            public Scenario Scenario
            {
                get => _scenario;
                set
                {
                    if (value == _scenario) return;
                    _scenario = value;
                    OnPropertyChanged();
                }
            }
            public Status Status
            {
                get => _status;
                set
                {
                    if (value == _status) return;
                    _status = value;
                    OnPropertyChanged();
                }
            }
            public string Description
            {
                get => _description;
                set
                {
                    if (value == _description) return;
                    _description = value;
                    OnPropertyChanged();
                }
            }
            public string ResultDescription
            {
                get => _resultDescription;
                set
                {
                    if (value == _resultDescription) return;
                    _resultDescription = value;
                    OnPropertyChanged();
                }
            }
            #endregion

            #region methods
            public void SetProcessedEntity<TEntity>(TEntity value, Status status)
                where TEntity : class
            {
                _processedEntity = value;
                this.Status = status;
                OnPropertyChanged("ProcessedEntity");
            }

            public TEntity GetProcessedEntity<TEntity>()
                where TEntity : class
            {
                return _processedEntity as TEntity;
            }

            public Status SetResult(Status status, string description)
            {
                Status = status;
                ResultDescription = description;
                return Status;
            }

            public void Reset()
            {
                Status = Status.NotStarted;
                ResultDescription = null;
            }
            #endregion

            #region INPC
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}
