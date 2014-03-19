using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BztPortFilterEditor.Commands;
using Microsoft.BizTalk.ExplorerOM;

namespace BztPortFilterEditor
{
	public class MainViewModel : INotifyPropertyChanged, INotifyPropertyChanging
	{
		private BtsCatalogExplorer _catalog;

		#region Commandes
		public ICommand CopyFilterCmd { get; set; }
		public ICommand SaveChangesCmd { get; set; }
		public ICommand RevertChangesCmd { get; set; }
		#endregion

		#region Items property
		private ICollectionView _items;
		public ObservableCollection<SendPort> Items { get; private set; }
		#endregion
		
		#region ItemFilter property
		private string _itemsFilter;
		public string ItemsFilter {
			get { return this._itemsFilter; }
			set {
				if (this._itemsFilter != value) {
					this._itemsFilter = value;
					this._items.Refresh();
					this.RaisePropertyChangedEvent("ItemsFilter");
				}
			}
		}
		#endregion

		#region Notifications Property
		private string _notifications;
		public string Notifications {
			get { return this._notifications; }
			set {
				if (this._notifications != value) {
					this._notifications = value;
					this.RaisePropertyChangedEvent("Notifications");
				}
			}
		}
		#endregion

		#region MVVM
		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;

		/// <summary>
		/// Whether the view model should ignore property-change events.
		/// </summary>
		public virtual bool IgnorePropertyChangeEvents { get; set; }

		/// <summary>
		/// Raises the PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The name of the changed property.</param>
		public virtual void RaisePropertyChangedEvent(string propertyName) {
			// Exit if changes ignored
			if (IgnorePropertyChangeEvents)
				return;

			// Exit if no subscribers
			if (PropertyChanged == null)
				return;

			// Raise event
			var e = new PropertyChangedEventArgs(propertyName);
			PropertyChanged(this, e);
		}

		/// <summary>
		/// Raises the PropertyChanging event.
		/// </summary>
		/// <param name="propertyName">The name of the changing property.</param>
		public virtual void RaisePropertyChangingEvent(string propertyName) {
			// Exit if changes ignored
			if (IgnorePropertyChangeEvents)
				return;

			// Exit if no subscribers
			if (PropertyChanging == null)
				return;

			// Raise event
			var e = new PropertyChangingEventArgs(propertyName);
			PropertyChanging(this, e);
		}

		#endregion

		public MainViewModel() {
			this._catalog = new BtsCatalogExplorer();
			this._catalog.ConnectionString = "Server=.;Initial Catalog=BizTalkMgmtDb;Integrated Security=SSPI;";

			this.Items = new ObservableCollection<SendPort>(
				this._catalog.SendPorts
					.Cast<SendPort>()
					.OrderBy(x => x.Application.Name)
					.ThenBy(x => x.Name)
				);

			this._items = CollectionViewSource.GetDefaultView(this.Items);
			this._items.Filter = x => string.IsNullOrEmpty(this.ItemsFilter) ? true : (((SendPort)x).Application.Name + ((SendPort)x).Name.ToUpper()).Contains(this.ItemsFilter.ToUpper());

			// commandes
			this.CopyFilterCmd = new RelayCommand<string>(
				x => this.CopyFilter(x),
				x => !string.IsNullOrEmpty(x)
			);

			this.SaveChangesCmd = new RelayCommand<object>(this.SaveChanges);
			this.RevertChangesCmd = new RelayCommand<object>(this.RevertChanges);
		}

		private void CopyFilter(string filter) {
			Clipboard.SetText(filter);
			this.Notifications += "Contenu du filtre copié dans le presse-papier." + Environment.NewLine; ;
		}

		private void SaveChanges(object param) {
			try {
				this._catalog.SaveChanges();
				this.Notifications += "Modifications enregistrées." + Environment.NewLine; ;
			}
			catch (Exception ex) {
				this.Notifications += "Exception lors de SaveChanges : " + ex.Message + Environment.NewLine; ;
				EventLog.WriteEntry("BstPortFilterEditor", "SaveChanges : " + ex.Message, EventLogEntryType.Error);
				throw;
			}
		}

		private void RevertChanges(object param) {
			try {
				this._catalog.DiscardChanges();
				this.Notifications += "Modifications annulées." + Environment.NewLine; ;
			}
			catch (Exception ex) {
				this.Notifications += "Exception lors de RevertChanges : " + ex.Message + Environment.NewLine; ;
				EventLog.WriteEntry("BstPortFilterEditor", "RevertChanges : " + ex.Message, EventLogEntryType.Error);
				throw;
			}
		}
	}
}
