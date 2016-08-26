﻿angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.shipmentDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.settings', 'virtoCommerce.orderModule.order_res_fulfilmentCenters',
    function ($scope, bladeNavigationService, dialogService, settings, order_res_fulfilmentCenters) {
        var blade = $scope.blade;

        blade.title = 'orders.blades.shipment-detail.title';
        blade.titleValues = { number: blade.currentEntity.number };
        blade.subtitle = 'orders.blades.shipment-detail.subtitle';
        blade.currentStore = _.findWhere(blade.parentBlade.stores, { id: blade.customerOrder.storeId });
        blade.realOperationsCollection = blade.customerOrder.shipments;

        blade.statuses = settings.getValues({ id: 'Shipment.Status' });
        blade.openStatusSettingManagement = function () {
            var newBlade = new DictionarySettingDetailBlade('Shipment.Status');
            newBlade.parentRefresh = function (data) {
                blade.statuses = data;
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        blade.fulfillmentCenters = order_res_fulfilmentCenters.query();
        blade.openFulfillmentCentersList = function () {
            var newBlade = {
                id: 'fulfillmentCenterList',
                controller: 'virtoCommerce.coreModule.fulfillment.fulfillmentListController',
                template: 'Modules/$(VirtoCommerce.Core)/Scripts/fulfillment/blades/fulfillment-center-list.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);