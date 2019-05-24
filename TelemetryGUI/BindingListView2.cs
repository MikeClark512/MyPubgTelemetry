using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Equin.ApplicationFramework;

namespace MyPubgTelemetry.GUI
{
    public class BindingListView2<T> : BindingListView<T>, IBindingListView
    {
        public BindingListView2(IList list) : base(list)
        {
        }

        public BindingListView2(IContainer container) : base(container)
        {
        }

        public new void ApplySort(ListSortDescriptionCollection sorts)
        {
            //Debug.WriteLine(">>>>>>BLV2 ApplySort(ListSortDescriptionCollection)!");
            List<ListSortDescription> sortDescList = sorts.Cast<ListSortDescription>().ToList();
            sortDescList.Add(new ListSortDescription(GetPropertyDescriptor("MatchDate"), ListSortDirection.Descending));
            ListSortDescriptionCollection newSorts = new ListSortDescriptionCollection(sortDescList.ToArray());
            base.ApplySort(newSorts);
        }

        public new void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            //Debug.WriteLine(">>>>>>BLV2 ApplySort(PropertyDescriptor,ListSortDirection)!");
            var sort = new ListSortDescription(property, direction);
            var dateSort = new ListSortDescription(GetPropertyDescriptor("MatchDate"), ListSortDirection.Descending);
            ListSortDescription[] sorts = {sort, dateSort};
            ListSortDescriptionCollection newSorts = new ListSortDescriptionCollection(sorts);
            this.ApplySort(newSorts);
        }

        private PropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, false);
        }
    }
}