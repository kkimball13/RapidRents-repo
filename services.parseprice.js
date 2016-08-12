
  (function () {
            "use strict";


            angular.module(APPNAME)
                .factory('$parseUtility', ParseUtilityFactory);

            ParseUtilityFactory.$inject = ['$baseService'];

            function ParseUtilityFactory() {

                var aSabioUtilityObject = this;
                aSabioUtilityObject.parse = _internalParse;


                function _internalParse(opts) {

                    var file = document.querySelector(opts.sabioFileId).files[0];
                    $(opts.sabioFileId).parse(opts);
                }

                return aSabioUtilityObject;
            }
  })();

(function () {
    "use strict";


    angular.module(APPNAME)
        .factory('$importRentService', ImportRentServiceFactory);

    ImportRentServiceFactory.$inject = ['$baseService'];

    function ImportRentServiceFactory($baseService) {

        var aSabioServiceObject = sabio.services.importedRents;

        var newService = $baseService.merge(true, {}, aSabioServiceObject, $baseService);

        return newService;

    }

})();

(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('parseController', ParseController);

    ParseController.$inject = ['$scope', '$baseController', '$importRentService', '$parseUtility'];

    function ParseController(
        $scope
        , $baseController
        , $importRentService
        , $parseUtility
        ) {

        var vm = this;
        vm.items = null;


        $baseController.merge(vm, $baseController);

        vm.$importRentService = $importRentService;
        vm.$parseUtility = $parseUtility;
        vm.$scope = $scope;
        vm.addresses = {};
        vm.geocoder = null;
        vm.addressId = null;
        vm.geocodeResponse = null;

        vm.receiveItems = _receiveItems;
        vm.onParseError = _onParseError;
        vm.parse = _parse;
        vm.postAll = _postAll;
        vm.updateAllAddresses = _updateAllAddresses;
        vm.post = _post
        vm.successfulBatches = 0;
        vm.getUniqueZips = _getUniqueZips;
        vm.onPostSuccess = _onPostSuccess;
        vm.onPostAllSuccess = _onPostAllSuccess;
        vm.onUpdateSuccess = _onUpdateSuccess;
        vm.onFinalPostAllSuccess = _onFinalPostAllSuccess;
        vm.fireAlert = _fireAlert;
        vm.addToArray = _addToArray;
        vm.getCityStateByZip = _getCityStateByZip;


        vm.notify = vm.$importRentService.getNotifier($scope);


        function _parse() {


            var configOpts = {

                config: {
                    complete: function (results, file) {
                        vm.receiveItems(results.data)
                    },
                    header: true,
                    worker: true
                },
                sabioFileId: "#parseFile"

            };

            vm.$parseUtility.parse(configOpts);

        }

        function _receiveItems(data) {

            vm.getUniqueZips(data);

            vm.notify(function () {

                vm.items = data;
            });

        }

        function _onPostSuccess(data, status, xhr) {
            vm.$log.log(data.item);
            vm.fireAlert('success', 'Address added to database', 'Insert successful');
        }


        function _onParseError(jqXhr, error) {
            vm.$log.log(error);
            vm.fireAlert('error', 'Try again', 'Insert failed');
        }

        function _onUpdateSuccess(status, xhr) {

            vm.fireAlert('success', 'Records Updated', 'Update successful');
        }

        //SINGLE-RECORD POST

        function _post(myData) {

            vm.$importRentService.post(myData, vm.onPostSuccess, vm.onParseError);

        }

        //BULK POST - INITIAL INSERT TO STAGING TABLE

        function _postAll(data) {
            var bulk = [];

            for (var index = 0; index < data.length; index++) {
                bulk.push(data[index]);
                if ((bulk.length % 29000) == 0) {
                    vm.$importRentService.postAll(bulk, vm.onPostAllSuccess, vm.onParseError);
                    bulk = [];
                }
            }

            if (bulk.length > 0) {
                vm.$importRentService.postAll(bulk, vm.onFinalPostAllSuccess, vm.onParseError);
            }
        }

        function _updateAllAddresses() {
            vm.$importRentService.updateAllAddresses(vm.onUpdateSuccess, vm.onParseError);
        }

        function _onPostAllSuccess(data, status, xhr) {

            vm.notify(function () {
                vm.successfulBatches++;
            });
        }

        function _onFinalPostAllSuccess(data, status, xhr) {

            vm.notify(function () {
                vm.successfulBatches++;
                vm.fireAlert('success', vm.successfulBatches + 'Batch of addresses added to database', 'Insert successful');
            });
        }



        function _fireAlert(type, message, header) {
            vm.$alertService[type](message, header);
        }


        _initialize()

        function _initialize() {
            vm.geocoder = new google.maps.Geocoder();
        }

        function _getUniqueZips(data) {
            vm.addresses = data;
            vm.uniqueZipcodes = {};


            var index;
            for (index = 0; index < vm.addresses.length; index++) {
                var currentAddress = vm.addresses[index];

                if (currentAddress.Zipcode && !vm.uniqueZipcodes[currentAddress.Zipcode]) {
                    vm.uniqueZipcodes[currentAddress.Zipcode] = [];
                }

                if (currentAddress.Zipcode) {

                    vm.uniqueZipcodes[currentAddress.Zipcode].push(currentAddress);
                }
            };

            _sendZipstoGoogle(vm.uniqueZipcodes);

        }

        function _sendZipstoGoogle(zipcodes) {

            vm.distinctCities = [];
            vm.distinctCitiesDict = {};
            vm.expectedResponses = Object.keys(zipcodes).length;
            vm.receivedResponses = 0;
            for (var distinctZip in zipcodes) {
                vm.geocoder.geocode({ 'address': distinctZip }, _onGetResults);
                vm.$log.log("Send zipcode -> ", distinctZip);
            }

        }

        function _transferData(aSingleAddress, cityStateObject) {
            aSingleAddress.City = cityStateObject.city;
            aSingleAddress.State = cityStateObject.state;
        }

        //this returns the cityState object
        function _getCityStateByZip(zipcode) {

            return vm.distinctCitiesDict[zipcode];

        }
        //this returns an array with addresses grouped by zipcode
        function _getAddressesByZip(zipcode) {

            return vm.uniqueZipcodes[zipcode];

        }

        function _processAddresses() {

            for (var distinctZip in vm.distinctCitiesDict) {

                var cityStateObject = _getCityStateByZip(distinctZip);

                var addresses = _getAddressesByZip(distinctZip);


                for (var i = 0; i < addresses.length; i++) {

                    var currentAddress = addresses[i];

                    _transferData(currentAddress, cityStateObject);

                }

            }

        }


        function _onGetResults(results, status) {

            if (status == google.maps.GeocoderStatus.OK) {

                var components = results[0].address_components;

                _setCityFromComponents(components);

            }

            else {

                alert('Geocode was not successful for the following reason: ' + status);
            }

            vm.receivedResponses++;
            if (vm.expectedResponses === vm.receivedResponses && vm.receivedResponses > 0) {
                vm.notify(function () {
                    _processAddresses();
                    vm.receivedResponses = 0;
                    vm.expectedResponses = 0;
                });

            }

        }



        function _setCityFromComponents(arrComponents) {

            var foundCity = '';
            var foundState = '';
            var foundZip = '';

            for (var i = 0; i < arrComponents.length; i++) {

                var currentComponent = arrComponents[i];
                var currentTypes = currentComponent.types;

                if (currentComponent.types[0] === "neighborhood") {
                    foundCity = currentComponent.long_name;
                }

                else if (currentComponent.types[0] === "locality" && !foundCity) {
                    foundCity = currentComponent.long_name;
                }

                else if (currentComponent.types[0] === "administrative_area_level_1") {
                    foundState = currentComponent.short_name;
                }

                else if (currentComponent.types[0] === "postal_code") {
                    foundZip = currentComponent.long_name;
                }

            }

            var cityStateZip = {
                city: foundCity,
                state: foundState,
                zipcode: foundZip
            };

            vm.addToArray(cityStateZip);

        }

        function _addToArray(cityStateZip) {
            vm.distinctCities.push(cityStateZip);

            vm.distinctCitiesDict[cityStateZip.zipcode] = cityStateZip;
            /*

            var distinctCitiesDict = {
            90230: {
                city: Culver City,
                state: CA,
                zipcode: 90230
            }, 90024: {
                city: Los Angeles,
                state: CA,
                zipcode: 90024
            } };
          */

        }

    }


})();
