using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SsPvo.Ui.Common
{
    public class SortableBindingList<T> : BindingList<T>
    {
        private bool isSorted;
        private ListSortDirection sortDirection;
        private PropertyDescriptor sortProperty;
        private readonly PropertyDescriptorCollection _propertyDescriptors = TypeDescriptor.GetProperties(typeof(T));

        public SortableBindingList(IEnumerable<T> enumerable) : base(enumerable.ToList()) { }

        public SortableBindingList() { }


        protected override bool SupportsSortingCore { get { return true; } }

        protected override bool IsSortedCore { get { return isSorted; } }

        protected override ListSortDirection SortDirectionCore { get { return sortDirection; } }

        protected override PropertyDescriptor SortPropertyCore { get { return sortProperty; } }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            isSorted = true;
            sortDirection = direction;
            sortProperty = prop;

            Func<T, object> predicate = n => n.GetType().GetProperty(prop.Name).GetValue(n, null);

            if (Items.Count > 10000)
            {
                ResetItems(sortDirection == ListSortDirection.Ascending ? Items.AsParallel().OrderBy(predicate) : Items.AsParallel().OrderByDescending(predicate));
            }
            else
            {
                ResetItems(sortDirection == ListSortDirection.Ascending ? Items.OrderBy(predicate) : Items.OrderByDescending(predicate));
            }
        }

        protected override void RemoveSortCore()
        {
            isSorted = false;
            sortDirection = base.SortDirectionCore;
            sortProperty = base.SortPropertyCore;

            ResetBindings();
        }

        private void ResetItems(IEnumerable<T> items)
        {
            RaiseListChangedEvents = false;
            var tempList = items.ToList();
            ClearItems();

            foreach (var item in tempList) Add(item);

            RaiseListChangedEvents = true;
            ResetBindings();
        }

        public SortableBindingList<T> Load(IEnumerable<T> enumeration)
        {
            ResetItems(enumeration);
            return this;
        }
    }
}
