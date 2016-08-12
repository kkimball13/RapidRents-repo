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
          .factory('$importPropertyService', ImportPropertyServiceFactory);

      ImportPropertyServiceFactory.$inject = ['$baseService'];

      function ImportPropertyServiceFactory($baseService) {

          var aSabioServiceObject = sabio.services.importedProperties;

          var newService = $baseService.merge(true, {}, aSabioServiceObject, $baseService);

          return newService;
      }
  })();
  (function () {
      "use strict";

      angular.module(APPNAME)
          .controller('importController', ImportController);

      ImportController.$inject = ['$scope', '$baseController', '$importPropertyService', '$parseUtility'];

      function ImportController(
                    $scope
                    , $baseController
                    , $importPropertyService
                    , $parseUtility
                    ) {

          var vm = this;
          vm.items = null;


          $baseController.merge(vm, $baseController);

          vm.$importPropertyService = $importPropertyService;
          vm.$parseUtility = $parseUtility;
          vm.$scope = $scope;

          vm.receiveItems = _receiveItems;
          vm.onParseError = _onParseError;
          vm.parse = _parse;
          vm.postAll = _postAll;
          vm.post = _post
          vm.successfulBatches = 0;
          vm.onPostSuccess = _onPostSuccess;
          vm.onPostAllSuccess = _onPostAllSuccess;
          vm.onFinalPostAllSuccess = _onFinalPostAllSuccess;
          vm.fireAlert = _fireAlert;


          vm.notify = vm.$importPropertyService.getNotifier($scope);


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

          function _post(myData) {
              vm.$importPropertyService.post(myData, vm.onPostSuccess, vm.onParseError);
          }

          function _postAll(data) {
              var bulk = [];

              for (var index = 0; index < data.length; index++) {
                  bulk.push(data[index]);
                  if ((bulk.length % 28000) == 0) {
                      vm.$importPropertyService.postAll(bulk, vm.onPostAllSuccess, vm.onParseError);
                      bulk = [];
                  }
              }

              if (bulk.length > 0) {
                  vm.$importPropertyService.postAll(bulk, vm.onFinalPostAllSuccess, vm.onParseError);
              }

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
      }
  })();
