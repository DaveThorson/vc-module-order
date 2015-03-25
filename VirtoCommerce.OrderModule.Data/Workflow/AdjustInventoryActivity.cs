﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Foundation.Frameworks;
using VirtoCommerce.Foundation.Frameworks.Extensions;

namespace VirtoCommerce.OrderModule.Data.Workflow
{
	public class AdjustInventoryActivity : IObserver<CustomerOrderStateBasedEvalContext>
	{
		private readonly IInventoryService _inventoryService;
		public AdjustInventoryActivity(IInventoryService inventoryService)
		{
			_inventoryService = inventoryService;
		}
		#region IObserver<CustomerOrder> Members

		public void OnCompleted()
		{
		}

		public void OnError(Exception error)
		{
		}

		public void OnNext(CustomerOrderStateBasedEvalContext value)
		{
			var origStockOutOperations = value.OrigOrder.ChildrenOperations.OfType<IStockOutOperation>().ToArray();
			var modifiedStockOutOperations = value.ModifiedOrder.ChildrenOperations.OfType<IStockOutOperation>().ToArray();

			var originalPositions = new ObservableCollection<KeyValuePair<string, int>>(origStockOutOperations.SelectMany(x => x.Positions).GroupBy(x => x.ProductId).Select(x => new KeyValuePair<string, int>(x.Key, x.Sum(y => y.Quantity))));
			var modifiedPositions = new ObservableCollection<KeyValuePair<string, int>>(modifiedStockOutOperations.SelectMany(x => x.Positions).GroupBy(x => x.ProductId).Select(x => new KeyValuePair<string, int>(x.Key, x.Sum(y => y.Quantity))));

			var changedInventoryInfos = new List<InventoryInfo>();

			var inventoryInfos = _inventoryService.GetProductsInventoryInfos(originalPositions.Select(x => x.Key).Concat(modifiedPositions.Select(x => x.Key)).Distinct().ToArray());
		
			originalPositions.ObserveCollection(x => { AdjustInventory(inventoryInfos, changedInventoryInfos, EntryState.Added, x); }, x => { AdjustInventory(inventoryInfos, changedInventoryInfos,  EntryState.Deleted, x); });
			var comparer = AnonymousComparer.Create((KeyValuePair<string, int> x) => x.Key);
			modifiedPositions.Patch(originalPositions, comparer, (x, y) => { AdjustInventory(inventoryInfos, changedInventoryInfos, EntryState.Modified, x, y); });

			if (changedInventoryInfos.Any())
			{
				_inventoryService.UpsertInventories(changedInventoryInfos);
			}
		}


		#endregion

		private void AdjustInventory(IEnumerable<InventoryInfo> inventories, List<InventoryInfo> changedInventories, EntryState action, KeyValuePair<string, int> pairSource, KeyValuePair<string, int>? pairTarget = null)
		{
			var inventoryInfo = inventories.FirstOrDefault(x => x.ProductId == pairSource.Key);
			if (inventoryInfo != null)
			{
				var delta = 0;
				if (action == EntryState.Added)
				{
					delta = -pairSource.Value;
				}
				else if (action == EntryState.Deleted)
				{
					delta = pairSource.Value;
				}
				else
				{
					delta = pairSource.Value - pairTarget.Value.Value;
				}
				if (delta != 0)
				{
					inventoryInfo.InStockQuantity += delta;
					changedInventories.Add(inventoryInfo);
				}
			}

		}
	}


}