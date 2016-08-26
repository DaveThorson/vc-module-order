﻿angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.paymentDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.settings',
    function ($scope, bladeNavigationService, dialogService, settings) {
        var blade = $scope.blade;

        blade.title = 'orders.blades.payment-detail.title';
        blade.titleValues = { number: blade.currentEntity.number };
        blade.subtitle = 'orders.blades.payment-detail.subtitle';
        blade.currentStore = _.findWhere(blade.parentBlade.stores, { id: blade.customerOrder.storeId });
        blade.realOperationsCollection = blade.customerOrder.inPayments;

        blade.statuses = settings.getValues({ id: 'PaymentIn.Status' });
        blade.openStatusSettingManagement = function () {
            var newBlade = new DictionarySettingDetailBlade('PaymentIn.Status');
            newBlade.parentRefresh = function (data) {
                blade.statuses = data;
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);