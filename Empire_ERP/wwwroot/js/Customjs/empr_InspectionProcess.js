var empr_InspectionProcess = {
    totalCount: 0,
    rowsCount: 0,
    parties: [],
    PartiesData: [],
    pickId: 0,
    PickQty: 0,
    detailGridCall: true,
    InitEvents: function () {
        $(document).ready(function () {
            console.log('GetAQLChart', GetAQLChart);


            $('button[data-bs-toggle="pill"]').on('shown.bs.tab', function (e) {
                window.dispatchEvent(new Event('resize'));
            });

            $('button[data-bs-toggle="pill"]').on('shown.bs.tab', function (e) {
                var targetId = $(e.target).attr("data-bs-target");

                if (targetId === '#pills-inspection') {
                    window.dispatchEvent(new Event('resize'));
                }
            });


            empr_InspectionProcess.InitReportTypeDDL();
            empr_InspectionProcess.ResetForm();

            window.addEventListener('message', function (event) {
                if (event.origin !== window.location.origin) {
                    return;
                }
                var data = event.data;
                if (data && data.traN_ID) {
                    $('#Code').val(data.traN_ID);
                    empr_InspectionProcess.GetMerchantPurchaseOrderDetailByCode(data.traN_ID);
                }
            });

            ajaxHelper.ajaxGetJson("/DeliveryFeeding/GetParties", function (data) {
                if (data.msgType == 1) {
                    empr_InspectionProcess.PartiesData = data.data;
                    parties = data.data;
                }
                else {
                    empr_helper.notify(data.data, data.msgType);
                }
            }, false, true);

            $('body').on('click', '#BtnQuickSearch', function () {
                
                empr_InspectionProcess.InitQuickSearchGrid();
            });

            $('body').on('click', '.elm_edit', function () {
                var id = $(this).attr("reportid");
                $('#Code').val(id);
                $('.vHide').show();
                $('.modal').modal('hide');
                empr_InspectionProcess.GetMerchantPurchaseOrderDetailByCode(id);

            });

            $('body').on('click', '.elm_copy', function () {
                var id = $(this).attr("reportid");
                var date = $(this).attr("reportdate");
                swal({
                    title: 'Are you sure you want to Copy this record?',
                    text: "",
                    type: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#0CC27E',
                    cancelButtonColor: '#FF586B',
                    confirmButtonText: 'Yes',
                    cancelButtonText: 'No',
                    confirmButtonClass: 'btn btn-success mr-5',
                    cancelButtonClass: 'btn btn-danger',
                    buttonsStyling: false
                }).then(function () {
                    $('#updatedDate').val(date);
                    empr_helper.selectedBill = id;
                    $('#CopyViewModal').modal('show');
                });
            });

            $('body').on('click', '#saveCopiedRecord', function () {
                ajaxHelper.ajaxPostJsonData({ traN_ID: empr_helper.selectedBill, v_DATE: $('#updatedDate').val() }, "/InspectionProcess/CopyRecord", function (data) {
                    empr_helper.notify(data.msg, data.msgType);
                    if (data.msgType == 1) {
                        $('.modal').modal('hide');
                        empr_InspectionProcess.GetMerchantPurchaseOrderDetailByCode(data.data.code);
                    }
                }, false, true);
            });


            $('body').on('click', '#BtnSave', function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_InspectionProcess.ValidateInfo()) {
                            empr_InspectionProcess.SaveInfo();
                        }
                    }
                } else {
                    if (empr_InspectionProcess.ValidateInfo()) {
                        empr_InspectionProcess.SaveInfo();
                    }
                }
            });

            $('body').on('click', '#BtnInsSave', function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        empr_InspectionProcess.SaveInspectionSheet();
                    }
                } else {
                    empr_InspectionProcess.SaveInspectionSheet();
                }
            });

            $('body').on('click', '#BtnNew', function () {
                empr_InspectionProcess.ResetForm();
            });

            $('body').on('click', '#BtnDelete', function () {
                empr_InspectionProcess.Delete();
            });

            $('body').on('click', '#BtnPrint, #BtnGenerateReport', function () {
                empr_helper.notify('Print is not ready', 2);
                //empr_InspectionProcess.GeneratePrintReport();
            });

            $('body').on('click', '.elm_print', function () {
                empr_helper.notify('Print is not ready', 2);
                //empr_helper.selectedBill = $(this).attr("reportid");
                //empr_InspectionProcess.GeneratePrintReport();
            });

            $('body').on('click', '#BtnBatchPick', function () {
                empr_InspectionProcess.InitBatchPickGrid();
            });

            $('body').on('click', '#BtnAddBatch', function () {
                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                if (selectedSodas.length > 0) {
                    var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();

                    empr_InspectionProcess.pickId = selectedSodas[0].picK_ID;
                    empr_InspectionProcess.PickQty = selectedSodas[0].qty;
                    //empr_InspectionProcess.InitSupplierDDL(Supplier.value.data, selectedSodas[0].spartY_CODE);

                    selectedSodas[0].__KEY__ = empr_InspectionProcess.GenerateKey(36);
                    var abc = selectedSodas[0];
                    empr_InspectionProcess.CreateGrid([selectedSodas[0]]);

                    //$('.modal').hide();
                    // Model double clicks
                    var modalEl = document.getElementById('SodaPickModal');
                    var modalInstance = bootstrap.Modal.getInstance(modalEl);
                    if (modalInstance) {
                        modalInstance.hide(); // proper close
                    }

                    //$('#CLIENT_PO').prop('disabled', true);
                    $('#V_DATE').focus();
                }
                else {
                    empr_helper.notify("Please select the items first.", 2);
                }
            });

            if (Permissions != "Admin") {
                !Permissions.r_ADD && $('#BtnNew').hide();
                !Permissions.r_VIEW && $('#BtnQuickSearch').hide();
                !Permissions.r_PRINT && $('.btn-print').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }

        });
    },
    GeneratePrintReport: function () {

        empr_InspectionProcess.InitReportTypeDDL();
        let TRAN_ID = empr_helper.selectedBill;
        let MD_ID = $('#ReportType').dxSelectBox('option', 'value');
        if (TRAN_ID == 0 || TRAN_ID == null || TRAN_ID == undefined || TRAN_ID == "") {
            empr_helper.notify("Please open the bill in edit mode.", 2);
            return;
        }
        var dataModel = {
            TRAN_ID: TRAN_ID,
            MD_ID: MD_ID,
        }
        ajaxHelper.ajaxPostJsonData(dataModel, "/InspectionProcess/GetPrintReport", function (data) {
            if (data.msgType == 1) {
                $('#ModalBody').empty();
                setTimeout(function () {
                    $('#ModalBody').html("<center><object id='objReport' data='" + window.location.origin + data.data + "' width='1100' height='600'></object></center>");
                    $('#ShowReportModal').show();
                    $('#ShowReportModal').modal('show');
                }, 100);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    InitReportTypeDDL: function (selectedValue) {
        ajaxHelper.ajaxGetJson("/InspectionProcess/GetReportTypes", function (data) {
            if (data.msgType == 1) {
                if (data.data.length > 0) {
                    selectedValue = data.data[0].mD_ID;
                }
                $('#ReportType').dxSelectBox({
                    dataSource: data.data,
                    displayExpr: 'mD_NAME',
                    valueExpr: 'mD_ID',
                    value: selectedValue,
                    searchEnabled: true,
                    width: '100%',
                    placeholder: 'Search',
                    showClearButton: true,
                    dropDownOptions: {
                        height: 'auto',
                    },
                    pagingEnabled: true,
                    searchTimeout: 500,
                    onValueChanged: function (e) {
                    },
                });
            }
            else {
                empr_helper.notify(data.data, data.msgType);
            }
        }, false, true);
    },

    findAQLSystem: function () {
        
        var footerGrid = $('#FooterSumGridContainer').dxDataGrid('instance');
        var footerRow = footerGrid.getVisibleRows()[0].data;

        var userQty = parseFloat(footerRow.qtY_INS) || 0;
        var userMajorDefects = parseFloat(footerRow.maJ_DEF) || 0;
        var userMinorDefects = parseFloat(footerRow.miN_DEF) || 0;

        var aqlList = GetAQLChart.value.data;

        if (!aqlList || aqlList.length === 0) return;

        var allKeys = Object.keys(aqlList[0]);
        var matchingColumn = "";

        for (var key of allKeys) {
            if (key.includes("_")) {
                var range = key.split("_");
                var min = parseInt(range[0]);
                var max = parseInt(range[1]);

                if (userQty >= min && userQty <= max) {
                    matchingColumn = key;
                    break;
                }
            }
        }

        if (!matchingColumn) {
            return;
        }
        debugger;
        var rowMajorDefects = aqlList.find(r => r.CODE == 2);
        var rowMinorDefects = aqlList.find(r => r.CODE == 3);

        var finalResult = "FAIL";
        var majorDefResult = false;
        var minorDefResult = false;

        if (rowMajorDefects && rowMinorDefects) {
            var majorDefCellValue = rowMajorDefects[matchingColumn];
            var minorDefCellValue = rowMinorDefects[matchingColumn];

            if (majorDefCellValue && majorDefCellValue.includes("/")) {
                var majorLimit = parseInt(majorDefCellValue.split("/")[1].trim());

                if (userMajorDefects <= majorLimit) {
                    majorDefResult = true;
                } else {
                    majorDefResult = false;
                }
            }

            if (minorDefCellValue && minorDefCellValue.includes("/")) {
                var minorLimit = parseInt(minorDefCellValue.split("/")[1].trim());

                if (userMinorDefects <= minorLimit) {
                    minorDefResult = true;
                } else {
                    minorDefResult = false;
                }
            }

            if (majorDefResult && minorDefResult) {
                finalResult = "PASS"
            }
        }

        $('#auditStatus').val(finalResult);

        if (finalResult === "FAIL") {
            $('#auditStatus').addClass('status-fail').removeClass('status-pass');
        } else {
            $('#auditStatus').addClass('status-pass').removeClass('status-fail');
        }
    },

    ResetForm: function () {

        $('#pills-planingDetail-tab, #pills-inspection-tab, #pills-detailinfo-tab').removeClass('active show');
        $('#pills-planingDetail-tab').addClass('active show');
        $('#pills-detailinfo-tab').hide();
        $('#pills-inspection-tab').hide();

        $('#pills-planingDetail, #pills-detailinfo, #pills-inspection').removeClass('active show');
        $('#pills-planingDetail').addClass('active show');

        $('#pills-warninghome-tab, #pills-warningattributes-tab, #pills-warningprofile-tab').removeClass('active');
        $('#pills-warninghome-tab').addClass('active');

        $('#pills-warningattributes-tab').hide();
        $('#pills-warningprofile-tab').hide();

        $('#pills-warningattributes, #pills-warningprofile').removeClass('active');
        $('#pills-warninghome').addClass('active show');
        
        $('#auditStatus').removeClass('status-pass status-fail');

        var cleanData = JSON.parse(JSON.stringify(QualityCheck));
        empr_InspectionProcess.CreateGrid(cleanData);

        empr_InspectionProcess.detailGridCall = true;


        if ($('#DetailContainer').data('dxDataGrid')) {
            $('#DetailContainer').dxDataGrid('instance').refresh();
        }
        empr_InspectionProcess.CreateInspectionSizeChartGrid(InspectionSizeChart);
        empr_InspectionProcess.CreateFooterSumGrid();
        empr_InspectionProcess.pickId = 0;
        $('.Record input').not('#ASTATUS, .dx-texteditor-input, #V_DATE').val('');
        $('#REMARKS').val('');

        $('#QA_NAME').val(Username);
        $('#QA_NAME').prop('disabled', true);
        $('#INS_DATE').val(todayDate);
        $('#INS_DATE').prop('disabled', true);
        $('#ORDER_QTY').prop('disabled', true);
        $('#CTN_QTY').prop('disabled', true);
        $('#CLIENT_PO').prop('disabled', true);


        if (PERFIX == 'IPGV') {
            $('#BtnBatchPick').hide();
        }
        $('#key_hidden').val('');

        $('#BtnDelete').hide();
        $('#CLIENT_PO').val('');
        $('#PARTY_CODE').val('');
        empr_InspectionProcess.InitPOJobGridDDL(null, true);
        if (Permissions != "Admin") {
            if (Permissions.r_ADD) {
                $('#BtnSave').show();
            } else {
                $('#BtnSave').hide();
            }
        } else {
            $('#BtnSave').show();
        }

        empr_InspectionProcess.InitializeLockGrid(null);

        if ($('#FinishItem').data('dxSelectBox') != null) {
            $('#FinishItem').dxSelectBox('instance').dispose();
        }
    },
    InitializeLockGrid: function (data) {
        if (data == null) {
            var obj = [
                { color: 0 },
                { color: 0 },
                { color: 0 },
                { color: 0 },
                { color: 0 },
                { color: 0 },
                { color: 0 },
                { color: 0 },
                { color: 0 },
                { color: 0 }
            ];

            empr_InspectionProcess.CreateLockGrid(obj);
        } else {
            ajaxHelper.ajaxGetJson('/InspectionProcess/GetLockGridData?code=' + data, function (responce) {
                if (responce.detail.msgType == 1) {
                    var detailData = responce.detail.data;
                    empr_InspectionProcess.CreateLockGrid(detailData);
                }
                else {
                    empr_helper.notify(responce.master.msg, responce.master.msgType);
                }
            }, false, true);
        }

    },
    CreateLockGrid: function (dataSrc) {
        console.log('Create Lock Grid', dataSrc);
        var col = [


            {
                dataField: 'intake',
                caption: 'Intake#',
                dataType: 'number',
                width: 90,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'dchanneL_CODE',
                caption: 'Dist Channel',
                lookup: {
                    dataSource: DistributionChannel,
                    displayExpr: 'value',
                    valueExpr: 'key',
                    allowClearing: true
                },
                width: 130,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'sentitY_CODE',
                caption: 'Entity#',
                lookup: {
                    dataSource: SEntity,
                    displayExpr: 'value',
                    valueExpr: 'key',
                    allowClearing: true
                },
                width: 100,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'ordeR_NO',
                caption: 'Order #',
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'iteM_CODE',
                caption: 'Item',
                lookup: {
                    dataSource: Items,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 130,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'gradE_CODE',
                caption: 'Brand',
                lookup: {
                    dataSource: Grade,
                    displayExpr: 'value',
                    valueExpr: 'key',
                    allowClearing: true
                },
                width: 130,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'color',
                caption: 'Color',
                lookup: {
                    dataSource: Colors,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 120,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'shiP_DATE',
                caption: 'Ship Date',
                dataType: 'date',
                format: 'dd-MM-yyyy',
                alignment: 'center',
                width: 100,
                setCellValue: function (newData, value) {
                    if (value) {
                        var date = new Date(value);
                        var year = date.getFullYear();
                        if (year < 100) year += 2000;

                        var shipDate = new Date(year, date.getMonth(), date.getDate());
                        newData.shiP_DATE = shipDate;

                        // ✅ Auto set Booking Date = 15 days before Ship Date
                        var bookingDate = new Date(shipDate);
                        bookingDate.setDate(shipDate.getDate() - 15);
                        newData.bookinG_DATE = bookingDate;

                    } else {
                        newData.shiP_DATE = null;
                        newData.bookinG_DATE = null;
                    }
                },
                cellTemplate: function (container, options) {
                    var $dateCell = $('<div>').appendTo(container);
                    var dateValue = options.value;
                    if (dateValue) {
                        var date = new Date(dateValue);
                        var day = ("0" + date.getDate()).slice(-2);
                        var month = ("0" + (date.getMonth() + 1)).slice(-2);
                        var year = date.getFullYear();
                        $dateCell.text(day + '-' + month + '-' + year);
                    }
                },
                allowEditing: false
            },
            {
                dataField: 'handoveR_DATE',
                caption: 'HandOver Date',
                dataType: 'date',
                format: 'dd-MM-yyyy',
                alignment: 'center',
                width: 100,
                setCellValue: function (newData, value) {
                    if (value) {
                        var date = new Date(value);
                        var year = date.getFullYear();
                        if (year < 100) year += 2000;
                        newData.handoveR_DATE = new Date(year, date.getMonth(), date.getDate());
                    } else {
                        newData.handoveR_DATE = value;
                    }
                },
                cellTemplate: function (container, options) {
                    var $dateCell = $('<div>').appendTo(container);
                    var dateValue = options.value;
                    if (dateValue) {
                        var date = new Date(dateValue);
                        var day = ("0" + date.getDate()).slice(-2);
                        var month = ("0" + (date.getMonth() + 1)).slice(-2);
                        var year = date.getFullYear()
                        var formattedDate = day + '-' + month + '-' + year;
                        $dateCell.text(formattedDate);
                    }
                },
                allowEditing: false
            },
            {
                dataField: 'bookinG_DATE',
                caption: 'Booking Date',
                dataType: 'date',
                format: 'dd-MM-yyyy',
                alignment: 'center',
                width: 100,
                setCellValue: function (newData, value) {
                    if (value) {
                        var date = new Date(value);
                        var year = date.getFullYear();
                        if (year < 100) year += 2000;
                        newData.bookinG_DATE = new Date(year, date.getMonth(), date.getDate());
                    } else {
                        newData.bookinG_DATE = null;
                    }
                },
                cellTemplate: function (container, options) {
                    var $dateCell = $('<div>').appendTo(container);
                    var dateValue = options.value;
                    if (dateValue) {
                        var date = new Date(dateValue);
                        var day = ("0" + date.getDate()).slice(-2);
                        var month = ("0" + (date.getMonth() + 1)).slice(-2);
                        var year = date.getFullYear();
                        $dateCell.text(day + '-' + month + '-' + year);
                    }
                },
                allowEditing: false
            },
            {
                dataField: 'port',
                caption: 'Port',
                lookup: {
                    dataSource: Ports,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 100,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'seasoN_CODE',
                caption: 'Season',
                lookup: {
                    dataSource: Season,
                    displayExpr: 'value',
                    valueExpr: 'key',
                    allowClearing: true
                },
                width: 130,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'dT_DESC',
                caption: 'Comment',
                allowEditing: false
            },
            {
                dataField: 'qty',
                caption: 'Ship Qty',
                dataType: 'number',
                setCellValue: function (newData, value, currentRowData) {
                    newData.qty = value;
                    var rate = parseFloat(currentRowData.rate) || 0;
                    var carton = parseFloat(currentRowData.carton) || 0;
                    newData.amt = value * rate;
                    if (carton > 0) {
                        newData.totaL_CARTON = value / carton;
                    } else {
                        newData.totaL_CARTON = 0;
                    }
                },
                width: 80,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'offeR_QTY',
                caption: 'Offer Qty',
                dataType: 'number',
                width: 80,
                alignment: 'center',
                allowEditing: true,
                setCellValue: function (newData, value, currentRowData) {
                    debugger;
                    newData.offeR_QTY = value;

                    var originalQty = parseFloat(currentRowData.qty) || 0;
                    if (originalQty > 0) {
                        var diff = originalQty - value;
                        var decreasePercentage = Number(((diff / originalQty) * 100).toFixed(2));
                        newData.diff_per = parseFloat(decreasePercentage.toFixed(2));
                    } else {
                        newData.diff_per = 0;
                    }
                    var carton = parseFloat(currentRowData.carton) || 0;
                    newData.ctN_PCS = (carton > 0) ? (value / carton) : 0;
                },

            },
            {
                dataField: 'cuttinG_QTY',
                caption: 'Cutting Qty',
                dataType: 'number',
                width: 80,
                alignment: 'center',
                allowEditing: true
            },
            {
                dataField: 'ofF_LINE',
                caption: 'Offline',
                dataType: 'number',
                width: 80,
                alignment: 'center',
            },
            {
                dataField: 'blisT_PCTN',
                caption: 'BList Per Ctn',
                width: 80,
                alignment: 'center',
            },
            {
                dataField: 'ctN_PCS',
                caption: '1 Ctn Pcs',
                width: 80,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'pacK_MODE',
                caption: 'Pack Mode',
                width: 80,
                alignment: 'center',
            },
            {
                dataField: 'carton',
                caption: 'Ctn',
                dataType: 'number',
                width: 100,
                alignment: 'center',

                allowEditing: false
            },
            {
                dataField: 'totaL_CARTON',
                caption: 'T.Ctn',
                dataType: 'number',
                allowEditing: false,
                width: 100,
                alignment: 'center'
            },
            {
                dataField: 'size',
                caption: 'Size',
                groupIndex: 0,
                lookup: {
                    dataSource: Sizes,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 150,
                alignment: 'center',
                allowEditing: false
            },

            {
                dataField: 'unit',
                caption: 'Unit',
                lookup: {
                    dataSource: Units,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 80,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'rate',
                caption: 'Rate',
                dataType: 'number',
                setCellValue: function (newData, value, currentRowData) {
                    newData.rate = value;
                    var qty = parseFloat(currentRowData.qty) || 0;
                    newData.amt = qty * value;
                },
                width: 70,
                alignment: 'center',
                visible: false,
                allowEditing: false
            },
            {
                dataField: 'amt',
                caption: 'Amount',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                allowEditing: false,
                width: 100,
                visible: false,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'comM_TYPE',
                caption: 'Comm Type',
                lookup: {
                    dataSource: empr_helper.commType,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 150,
                alignment: 'center',
                visible: false,
                allowEditing: false
            },
            {
                dataField: 'comM_RATE',
                caption: 'Comm Rate',
                visible: false,
                allowEditing: false
            },
            {
                dataField: 'comM_AMT',
                caption: 'Comm Amt',
                visible: false,
                allowEditing: false
            },
        ];
        empr_helper.dxGridLock('#LockGridContainer', col, dataSrc, '', 'single', 225);

    },
    CreateGrid: function (dataSrc) {

        var col = [
            {
                dataField: "Action ", // Space hata diya hai agar galti se tha
                width: 50,
                alignment: 'center',
                fixed: true,
                fixedPosition: "left",
                allowExporting: false,
                allowEditing: false,
                cellTemplate: function (container, options) {
                    // Total rows count nikalne ke liye
                    var totalRows = options.component.getVisibleRows().length;

                    // Check karein ki kya ye last row hai (rowIndex 0 se start hota hai isliye -1)
                    if (options.rowIndex === totalRows - 1) {
                        var html = '<div class="btn-group btn-group-sm">';
                        html += `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 3px" onclick="empr_InspectionProcess.AddRow()" title="Add Row"><i class="fa fa-plus"></i></a>`;
                        html += `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 5px" onclick="empr_InspectionProcess.DeleteRow('${options.data.__KEY__}')" title="Delete"><i class="fa fa-trash"></i></a>`;
                        html += '</div>';
                        $(html).appendTo(container);
                    }
                }
            },
            {
                dataField: 'inS_SIZE_ID',
                caption: 'Code',
                visible: false,
            },
            {
                dataField: 'fab',
                caption: 'Fabrication',
                width: 350,
                alignment: 'left',
                allowEditing: function (e) {
                    return e.row && e.row.data && e.row.data.isNewRow === true;
                }
            },
            {
                dataField: 'f_MIN',
                caption: 'F.MIN',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                //width: 100,
                minWidth: 100,
                alignment: 'center',
                customizeText: function (cellInfo) {
                    if (cellInfo.value === 0 || cellInfo.value === null) {
                        return "";
                    }
                    return cellInfo.valueText;
                }
            },
            {
                dataField: 'f_MAJ',
                caption: 'F.MAJ',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                //width: 100,
                minWidth: 100,
                alignment: 'center',
                customizeText: function (cellInfo) {
                    if (cellInfo.value === 0 || cellInfo.value === null) {
                        return "";
                    }
                    return cellInfo.valueText;
                }
            },






            {
                dataField: 'sti',
                caption: 'MAKE / STITCHING',
                width: 350,
                alignment: 'left',
                allowEditing: function (e) {
                    return e.row && e.row.data && e.row.data.isNewRow === true;
                }
            },
            {
                dataField: 's_MIN',
                caption: 'S.MIN',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                minWidth: 100,
                alignment: 'center',
                customizeText: function (cellInfo) {
                    if (cellInfo.value === 0 || cellInfo.value === null) {
                        return "";
                    }
                    return cellInfo.valueText;
                }
            },
            {
                dataField: 's_MAJ',
                caption: 'S.MAJ',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                minWidth: 100,
                alignment: 'center',
                customizeText: function (cellInfo) {
                    if (cellInfo.value === 0 || cellInfo.value === null) {
                        return "";
                    }
                    return cellInfo.valueText;
                }
            },




            {
                dataField: 'acc',
                caption: 'ACCESSORIES / FINISHING',
                width: 350,
                alignment: 'left',
                allowEditing: function (e) {
                    return e.row && e.row.data && e.row.data.isNewRow === true;
                }
            },
            {
                dataField: 'a_MIN',
                caption: 'A.MIN',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                alignment: 'center',
                customizeText: function (cellInfo) {
                    if (cellInfo.value === 0 || cellInfo.value === null) {
                        return "";
                    }
                    return cellInfo.valueText;
                }
            },
            {
                dataField: 'a_MAJ',
                caption: 'A.MAJ',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                alignment: 'center',
                customizeText: function (cellInfo) {
                    if (cellInfo.value === 0 || cellInfo.value === null) {
                        return "";
                    }
                    return cellInfo.valueText;
                }
            },
        ];
        //empr_helper.editableDxGridbindingForTransactions('#DetailContainer', col, dataSrc, "MerchantPurchaseOrderDetail", "f_MIN", '', true, 420);
        empr_helper.SimpleEditableGrid('#DetailContainer', col, dataSrc, 'single', 390);
        if (dataSrc.length == 0) {
            $('#DetailContainer').dxDataGrid('instance').addRow().done(function () {
                $('#DetailContainer').dxDataGrid('instance').saveEditData();
            });
        }

    },
    CreateInspectionSizeChartGrid: function (dataSrc) {
        console.log('CreateInspectionSizeChartGrid', dataSrc);
        var col = [

            { dataField: 'aql', caption: 'AQL', width: 150, alignment: 'center', allowEditing: false },
            { dataField: 'size_151_280', caption: '151_280', width: 150, alignment: 'center', allowEditing: false },
            { dataField: 'size_281_500', caption: '281_500', width: 150, alignment: 'center', allowEditing: false },
            { dataField: 'size_501_1200', caption: '501_1200', width: 150, alignment: 'center', allowEditing: false },
            { dataField: 'size_1201_3200', caption: '1201_3200', width: 150, alignment: 'center', allowEditing: false },
            { dataField: 'size_3201_10000', caption: '3201_10000', width: 150, alignment: 'center', allowEditing: false },
            { dataField: 'size_10001_35000', caption: '10001_35000', width: 150, alignment: 'center', allowEditing: false },
            { dataField: 'size_35000_100000', caption: '35000_100000', width: 150, alignment: 'center', allowEditing: false },

        ];

        empr_helper.SimpleEditableGrid('#InspectionGridContainer', col, dataSrc, 'single', 390);
        if (dataSrc.length == 0) {
            $('#InspectionGridContainer').dxDataGrid('instance').addRow().done(function () {
                $('#InspectionGridContainer').dxDataGrid('instance').saveEditData();
            });
        }

    },

    CreateFooterSumGrid: function (dataSrc) {

        if (dataSrc == undefined) {
            var dataSrc = [{
                qtY_INS: "",
                maJ_DEF: "",
                miN_DEF: "",
                geN_APP: "FINE",
                seL_CAR: ""
            }];
        }

        var col = [
            { dataField: 'qtY_INS', caption: 'Qty Inspected', minWidth: 150, alignment: 'center', allowEditing: false },
            {
                dataField: 'miN_DEF',
                caption: 'Minor Defects',
                minWidth: 150,
                alignment: 'center',
                allowEditing: false,
                // Decimal format yahan add kiya
                format: {
                    type: "fixedPoint",
                    precision: 2
                }
            },
            {
                dataField: 'maJ_DEF',
                caption: 'Major Defects',
                minWidth: 150,
                alignment: 'center',
                allowEditing: false,
                // Decimal format yahan bhi add kiya
                format: {
                    type: "fixedPoint",
                    precision: 2
                }
            },
            { dataField: 'geN_APP', caption: 'General Appearance', minWidth: 150, alignment: 'center', allowEditing: false },
            { dataField: 'seL_CAR', caption: 'Selected Cartons', minWidth: 150, alignment: 'center', allowEditing: false },
        ];

        empr_helper.SimpleEditableGrid('#FooterSumGridContainer', col, dataSrc, 'single', 60);
        if (dataSrc.length == 0) {
            $('#FooterSumGridContainer').dxDataGrid('instance').addRow().done(function () {
                $('#FooterSumGridContainer').dxDataGrid('instance').saveEditData();
            });
        }

    },

    updateFooterSums: function () {
        var mainGridInstance = $('#DetailContainer').dxDataGrid('instance');
        var footerGridInstance = $('#FooterSumGridContainer').dxDataGrid('instance');
        var planingDetailGridInstance = $('#LockGridContainer').dxDataGrid('instance');

        var data = mainGridInstance.getVisibleRows().map(r => r.data);
        var planingDetailData = planingDetailGridInstance.getVisibleRows().map(r => r.data);

        var totalFabMinor = 0, totalFabMajor = 0;
        var totalStiMinor = 0, totalStiMajor = 0;
        var totalAccMinor = 0, totalAccMajor = 0;
        var totalOfferQty = 0;

        data.forEach(function (item) {
            totalFabMinor += parseFloat(item.f_MIN) || 0;
            totalFabMajor += parseFloat(item.f_MAJ) || 0;

            totalStiMinor += parseFloat(item.s_MIN) || 0;
            totalStiMajor += parseFloat(item.s_MAJ) || 0;

            totalAccMinor += parseFloat(item.a_MIN) || 0;
            totalAccMajor += parseFloat(item.a_MAJ) || 0;
        });

        planingDetailData.forEach(function (item) {
            totalOfferQty += parseFloat(item.offeR_QTY) || 0;
        });

        var totalMinor = Number((totalFabMinor + totalStiMinor + totalAccMinor).toFixed(2));
        var totalMajor = Number((totalFabMajor + totalStiMajor + totalAccMajor).toFixed(2));

        var rootValue = Number(Math.sqrt(totalOfferQty).toFixed(2));
        footerGridInstance.cellValue(0, 'miN_DEF', totalMinor);
        footerGridInstance.cellValue(0, 'maJ_DEF', totalMajor);
        footerGridInstance.cellValue(0, 'qtY_INS', totalOfferQty);
        footerGridInstance.cellValue(0, 'seL_CAR', rootValue);

        

        footerGridInstance.saveEditData();

        //if (totalMajor > 0 || totalMinor > 10) {
        //    $('#auditStatus').val('FAIL');
        //} else {
        //    $('#auditStatus').val('PASS');
        //}

        empr_InspectionProcess.findAQLSystem();
    },

    CloneRow: function (uniqueKey) {
        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {

                empr_InspectionProcess.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                var dataSource = gridInstance.option("dataSource");
                if (dataSource.length > 0) {
                    let clonedRowData = $.extend(true, {}, dataSource.find(x => x.__KEY__ === uniqueKey));
                    if (clonedRowData.hasOwnProperty('dT_CODE')) {
                        delete clonedRowData.dT_CODE;
                    }
                    var key = empr_InspectionProcess.GenerateKey(36);
                    clonedRowData.__KEY__ = key;

                    clonedRowData.reV_STATUS = "N";
                    clonedRowData.reV_TOGGLE = false;

                    let newDataSource = [clonedRowData].concat(dataSource);
                    gridInstance.option("dataSource", newDataSource);
                    gridInstance.refresh();
                }
            });
        }
        else {
            empr_InspectionProcess.rowsCount += 1;
            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            var dataSource = gridInstance.option("dataSource");
            if (dataSource.length > 0) {
                //let clonedRowData = $.extend(true, {}, dataSource[index]);
                let clonedRowData = $.extend(true, {}, dataSource.find(x => x.__KEY__ === uniqueKey));
                if (clonedRowData.hasOwnProperty('dT_CODE')) {
                    delete clonedRowData.dT_CODE;
                }
                var key = empr_InspectionProcess.GenerateKey(36);
                clonedRowData.__KEY__ = key;
                clonedRowData.reV_STATUS = "N";
                clonedRowData.reV_TOGGLE = false;

                let newDataSource = [clonedRowData].concat(dataSource);
                gridInstance.option("dataSource", newDataSource);
                gridInstance.refresh();
            }
        }
    },
    AddRow: function () {
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');

        const addNewRow = function () {
            const dataSource = gridInstance.option("dataSource") || [];

            let maxId = 0;
            if (dataSource.length > 0) {
                maxId = Math.max(...dataSource.map(item => parseInt(item.inS_SIZE_ID) || 0));
            }

            dataSource.push({
                __KEY__: empr_InspectionProcess.GenerateKey(36),
                isNewRow: true,
                inS_SIZE_ID: maxId + 1,
                fab: "",
                sti: "",
                acc: ""
            });

            gridInstance.option("dataSource", dataSource);
            gridInstance.refresh();
        };

        if (gridInstance.hasEditData()) {
            gridInstance.saveEditData().done(addNewRow);
        } else {
            addNewRow();
        }
    },
    DeleteRow: function (uniqueKey) {
        debugger;
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        var dataSource = gridInstance.option("dataSource");

        if (dataSource.length > 0) {
            if (dataSource.length > 1) {

                var rowIndex = dataSource.findIndex(x => x.__KEY__ === uniqueKey);
                if (rowIndex === -1) return; // row not found

                var row = dataSource[rowIndex];
                debugger;
                if (row.inS_SIZE_ID > 15) {
                    if (!row.dT_CODE) {

                        const gridInstance = $('#DetailContainer').dxDataGrid('instance');

                        let visibleRow = gridInstance.getVisibleRows().find(r => r.data.__KEY__ === uniqueKey);

                        if (visibleRow) {
                            gridInstance.deleteRow(visibleRow.rowIndex);
                        }

                        empr_InspectionProcess.rowsCount -= 1;
                        gridInstance.saveEditData();
                    }
                    else {
                        swal({
                            title: 'Are you sure you want to remove this record?',
                            text: "You won't be able to revert this!",
                            type: 'warning',
                            showCancelButton: true,
                            confirmButtonColor: '#0CC27E',
                            cancelButtonColor: '#FF586B',
                            confirmButtonText: 'Yes, delete it!',
                            cancelButtonText: 'No, cancel!',
                            confirmButtonClass: 'btn btn-success mr-5',
                            cancelButtonClass: 'btn btn-danger',
                            buttonsStyling: false
                        }).then(function () {
                            ajaxHelper.ajaxPostJsonData({ code: row.dT_CODE }, "/InspectionProcess/Delete", function (data) {
                                empr_helper.notify(data.msg, data.msgType);
                                if (data.msgType == 1) {

                                    const grid = $('#DetailContainer').dxDataGrid('instance');

                                    let visibleRow = grid.getVisibleRows().find(r => r.data.__KEY__ === row.__KEY__);

                                    if (visibleRow) {
                                        grid.deleteRow(visibleRow.rowIndex);
                                    }

                                    grid.saveEditData();
                                }
                            }, false, true);
                        });
                    }
                }
                
            } else {
                empr_helper.notify("You are not allowed to delete the last row.", 2);
            }
        }
    },


    GenerateKey: function (keyLength) {

        var key = "";
        var characters = "abcdef0123456789";
        for (var i = 0; i < keyLength; i++) {
            if (i === 8 || i === 13 || i === 18 || i === 23) {
                key += "-";
            } else {
                key += characters.charAt(Math.floor(Math.random() * characters.length));
            }
        }
        return key;
    },
    InitFinishItemsDDL: function (selectedValue) {
        $.ajax({
            url: "DailyProduction/GetFinishItems",
            type: "GET",
            success: function (response) {
                empr_InspectionProcess.BindDxDDL("FinishItem", response.data, selectedValue, "key", "value", "Select", function (d) {
                    $('#FinishItem_Hidden').val(d.value)
                    if (d.value == null) {
                        $('#FinishItem_Hidden').val('');
                    }
                });
            }
        });
    },
    //InitSupplierDDL: function (dataSource, selectedValue) {
    //    $('#SUPPLIER').dxSelectBox({
    //        dataSource: {
    //            store: dataSource,
    //            paginate: true,
    //            pageSize: 50
    //        },
    //        paging: {
    //            enabled: true,
    //            pageSize: 50,
    //        },
    //        displayExpr: 'value',
    //        valueExpr: 'customizedKey',
    //        value: selectedValue,
    //        searchEnabled: true,
    //        width: '100%',
    //        placeholder: 'Search',
    //        showClearButton: true,
    //        dropDownOptions: {
    //            height: 'auto',
    //        },
    //        pagingEnabled: true,
    //        searchTimeout: 500,
    //        disabled: true
    //    });
    //},
    BindDxDDL: function (divId, data, selectedvalues, keyExp, dataField, placeholder, onvalueChangeFun) {
        ati_dxHelper.createDropdownSingle(divId, data, selectedvalues, keyExp, dataField, placeholder, onvalueChangeFun);
    },
    InitQuickSearchGrid: function () {
        empr_InspectionProcess.GetInspectionProcess();
    },
    GetInspectionProcess: function () {
        debugger;
        ajaxHelper.ajaxGetJson('/InspectionProcess/GetInspectionProcess', function (data) {
            if (data.msgType == 1) {
                empr_InspectionProcess.CreateQuickSearchGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    CreateQuickSearchGrid: function (dataSrc) {
        var col = [{
            dataField: "Action",
            width: 100,
            alignment: 'center',
            fixed: true,
            fixedPosition: "left",
            allowExporting: false,
            cellTemplate: function (container, options) {
                if (Permissions != "Admin" && !Permissions.r_PRINT) {
                    $(`<div class="btn-group btn-group-sm">
                           <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.traN_ID} title="Edit"><i class="fa fa-edit"></i></a>
                           </div>`).appendTo(container);
                } else {
                    $(`<div class="btn-group btn-group-sm">
                           <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.traN_ID} title="Edit"><i class="fa fa-edit"></i></a>
                           <a href="javascript:;"  class="grid-action-icon elm_print" style="margin-left: 8px" reportid=${options.data.traN_ID} title="PRINT"><i class="fa fa-print"></i></a>
                           
                           </div>`).appendTo(container);

                    /*<a href="javascript:;" class="grid-action-icon elm_copy" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.traN_ID} title="COPY"><i class="fa fa-copy"></i></a>*/
                }
            }
        },
        { dataField: 'traN_ID', caption: 'Code', alignment: 'center' },
        { dataField: 'v_DATE', caption: 'Voucher Date', dataType: 'date', format: 'dd-MM-yyy', alignment: 'center' },
        { dataField: 'astatus', caption: 'Status', alignment: 'center' },
        { dataField: 'voucheR_NO', caption: 'Voucher No', alignment: 'center', },
        { dataField: 'qA_NAME', caption: 'QA Name', alignment: 'center', },
        { dataField: 'inS_DATE', caption: 'Ins Date', dataType: 'date', format: 'dd-MM-yyy', alignment: 'center' },
        { dataField: 'ref', caption: 'Reference No', alignment: 'center' },
        { dataField: 'job', caption: 'Model# / PO#', alignment: 'center' },
        { dataField: 'ediT_USER_ID', caption: 'User', alignment: 'center' },
        { dataField: 'ediT_DATE', caption: 'Add Date', alignment: 'center' },
        { dataField: 'remarks', caption: 'Remarks', alignment: 'center' },
        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "DailyProductionQS");
    },
    GetDataToSave: function () {
        var CODE = $("#Code").val();
        var ASTATUS = $('#ASTATUS').dxSelectBox('option', 'value');
        var V_DATE = $("#V_DATE").val();
        var VOUCHER_NO = $("#VOUCHER_NO").val();
        var INS_DATE = $("#INS_DATE").val();
        var JOB_NO = $("#key_hidden").val();
        var REF = $("#REF").val();
        var REMARKS = $("#REMARKS").val();

        var masterRecord = {
            TRAN_ID: CODE,
            ASTATUS: ASTATUS,
            V_DATE: V_DATE,
            VOUCHER_NO: VOUCHER_NO,
            REF: REF,
            INS_DATE: INS_DATE,
            REMARKS: REMARKS,
            JOB_NO: JOB_NO,
        }


        //var detailRecords = [];
        //if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
        //    $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
        //        detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
        //    });
        //}
        //else {
        //    detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
        //}

        var detailRecords = [];
        if ($('#LockGridContainer').dxDataGrid('instance').hasEditData()) {
            $('#LockGridContainer').dxDataGrid('instance').saveEditData().done(function () {
                detailRecords = $('#LockGridContainer').dxDataGrid('instance').option("dataSource");
            });
        }
        else {
            detailRecords = $('#LockGridContainer').dxDataGrid('instance').option("dataSource");
        }


        if (empr_InspectionProcess.rowsCount == detailRecords.length) {
            var modelRecord = {
                Master: masterRecord,
                Detail: detailRecords
            };
            return modelRecord;
        }
        else {
            var modelRecord = {
                Master: masterRecord,
                Detail: $('#LockGridContainer').dxDataGrid('instance').option("dataSource")
            };
            return modelRecord;
        }

    },

    ValidateInfo: function () {
        var valid = true;
        var data = empr_InspectionProcess.GetDataToSave();

        if (data.Master.JOB_NO == null || data.Master.JOB_NO == '' || data.Master.JOB_NO == undefined || data.Master.JOB_NO == 0) {
            valid = false
            empr_helper.notify('Please select Job #', 2);
        }

        return valid;
    },
    SaveInfo: function () {
        var dataModel = empr_InspectionProcess.GetDataToSave();
        ajaxHelper.ajaxPostJsonData(dataModel, "/InspectionProcess/Save", function (data) {
            debugger;
            console.log('Save Return', data);
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                //empr_InspectionProcess.ResetForm();
                debugger;
                //empr_InspectionProcess.SaveInspectionSheet();
                empr_InspectionProcess.GetMerchantPurchaseOrderDetailByCode(data.data);
                //empr_InspectionProcess.GetInspectionSheetByCode();
                
                //empr_helper.selectedBill = data.data.code;
                //if (dataModel.Master.TRAN_ID == 0
                //    || dataModel.Master.TRAN_ID == null
                //    || dataModel.Master.TRAN_ID == undefined) {
                //    $('#Code').val(data.data.code);
                //    $('#VOUCHER_NO').val(data.data.voucherNo);
                //}
                //empr_InspectionProcess.GetMerchantPurchaseOrderDetailByCode(data.data.code);
                //$('#BtnDelete').show();
            }
        }, false, true);
    },

    //GetInspectionSheetDataToSave: function () {

    //    var PTRAN_ID = $("#Code").val();

    //    var JOB_NO = $("#key_hidden").val();



    //    var masterRecord = {

    //        PTRAN_ID: PTRAN_ID,

    //        JOB_NO: JOB_NO,

    //    }



    //    var detailRecords = [];



    //    if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {

    //        $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {

    //            // dataSource ki jagah items() use karein

    //            detailRecords = $('#DetailContainer').dxDataGrid('instance').getDataSource().items();

    //            console.log(detailRecords);

    //            debugger;

    //        });

    //    } else {

    //        detailRecords = $('#DetailContainer').dxDataGrid('instance').getDataSource().items();

    //    }



    //    if (empr_InspectionProcess.rowsCount == detailRecords.length) {

    //        var modelRecord = {

    //            Master: masterRecord,

    //            Detail: detailRecords

    //        };

    //        return modelRecord;

    //    }

    //    else {

    //        var modelRecord = {

    //            Master: masterRecord,

    //            Detail: $('#DetailContainer').dxDataGrid('instance').option("dataSource")

    //        };

    //        return modelRecord;

    //    }



    //},



    //SaveInspectionSheet: function () {

    //    var dataModel = empr_InspectionProcess.GetInspectionSheetDataToSave();

    //    console.log('SaveInspectionSheet', dataModel);

    //    //ajaxHelper.ajaxPostJsonData(dataModel, "/InspectionProcess/SaveInspectionSheet", function (data) {

    //    //    console.log('Save Return', data);

    //    //    empr_helper.notify(data.msg, data.msgType);

    //    //    if (data.msgType == 1) {

    //    //        //empr_InspectionProcess.GetMerchantPurchaseOrderDetailByCode(data.data);

    //    //    }

    //    //}, false, true);

    //},

    GetInspectionSheetDataToSave: async function () {
        var PTRAN_ID = $("#Code").val();
        var JOB_NO = $("#key_hidden").val();

        var masterRecord = {
            PTRAN_ID: PTRAN_ID,
            JOB_NO: JOB_NO,
        };

        var detailRecords = [];
        var gridInstance = $('#DetailContainer').dxDataGrid('instance');

        if (gridInstance.hasEditData()) {
            console.log("Saving grid data...");
            await gridInstance.saveEditData(); 
            detailRecords = gridInstance.getDataSource().items();
        } else {
            detailRecords = gridInstance.getDataSource().items();
        }

        var modelRecord = {
            Master: masterRecord,
            Detail: detailRecords
        };

        console.log('Model Prepared inside function:', modelRecord);
        return modelRecord;
    },

    SaveInspectionSheet: async function () {
        debugger;
        var dataModel = await empr_InspectionProcess.GetInspectionSheetDataToSave();

        console.log('Final dataModel for AJAX:', dataModel);

        if (dataModel.Detail && dataModel.Detail.length > 0) {

            ajaxHelper.ajaxPostJsonData(dataModel, "/InspectionProcess/SaveInspectionSheet", function (data) {
                console.log('Save Return', data);
                empr_helper.notify(data.msg, data.msgType);
            }, false, true);

        }

        
    },

    GetInspectionSheetByCode: function (code) {
        ajaxHelper.ajaxGetJson('/InspectionProcess/GetInspectionSheetByCode?code=' + code, function (data) {
            debugger;
            if (data.sheetData.data.length > 0) {
                

                empr_InspectionProcess.CreateGrid(sheetData);
            }
            else {
                cleanData = JSON.parse(JSON.stringify(QualityCheck));
                empr_InspectionProcess.CreateGrid(cleanData);
            }
            //if (data.msgType == 1) {
            //    //empr_InspectionProcess.CreateGrid(sheetData);

                
            //}
            //else {
            //    empr_helper.notify(data.msg, data.msgType);
            //}
        }, false, true);
    },

    GetMerchantPurchaseOrderDetailByCode: function (code) {
        debugger;
        ajaxHelper.ajaxGetJson('/InspectionProcess/GetInspectionProcessByCode?code=' + code, function (data) {
            console.log('Edit Data', data);
            debugger;
            if (data.master.msgType == 1) {

                $('#pills-detailinfo-tab').show();
                $('#pills-inspection-tab').show();

                $('#pills-warningattributes-tab').show();
                $('#pills-warningprofile-tab').show();

                var masterData = data.master.data[0];
                var detailData = data.detail.data;
                var sheetData = data.sheetData.data;
                console.log('Edit sheet Data', sheetData);
                var response = masterData;
                $('#Code').val(response.traN_ID);
                empr_helper.selectedBill = response.traN_ID;
                empr_InspectionProcess.pickId = detailData[0].picK_ID;
                //empr_InspectionProcess.PickQty = response.picK_QTY;
                $('#ASTATUS').dxSelectBox('instance').option('value', response.astatus);
                debugger;
                empr_InspectionProcess.detailGridCall = false;
                empr_InspectionProcess.InitPOJobGridDDL(response.joB_NO);
                $('#REF').val(response.ref);
                $('#REMARKS').val(response.remarks);
                $('#V_DATE').val(response.v_DATE);
                $('#VOUCHER_NO').val(response.voucheR_NO);


                if (Permissions != "Admin") {
                    if (Permissions.r_DLT) {
                        $('#BtnDelete').show();
                        $('#dltAllImages').show();
                        $('#dltAllImagesSpecs').show();
                    }
                    if (Permissions.r_EDIT) {
                        $('#BtnSave').show();
                    }
                    else {
                        $('#BtnSave').hide();
                    }
                } else {
                    $('#BtnSave').show();
                    $('#BtnDelete').show();
                    $('#dltAllImages').show();
                    $('#dltAllImagesSpecs').show();
                }

                empr_InspectionProcess.CreateLockGrid(detailData);
                $('.card-body').addClass('customHighlightForModifiedCells');

                if (sheetData.length > 1) {
                    sheetData.forEach(function (row) {
                        row.__KEY__ = empr_InspectionProcess.GenerateKey(36);
                    });

                    empr_InspectionProcess.CreateGrid(sheetData);
                } else {
                    var cleanData = JSON.parse(JSON.stringify(QualityCheck));
                    empr_InspectionProcess.CreateGrid(cleanData);
                }
                
                //$('.card-body').addClass('customHighlightForModifiedCells');

                setTimeout(function () {
                    empr_InspectionProcess.updateFooterSums();
                    empr_InspectionProcessImages.loadImages(response.traN_ID, response.joB_NO, 'specs');
                    empr_InspectionProcessImages.loadImages(response.traN_ID, response.joB_NO, '');

                }, 100); 

            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    
    Delete: function () {

        swal({
            title: 'Are you sure you want to remove this record?',
            text: "You won't be able to revert this!",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0CC27E',
            cancelButtonColor: '#FF586B',
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'No, cancel!',
            confirmButtonClass: 'btn btn-success mr-5',
            cancelButtonClass: 'btn btn-danger',
            buttonsStyling: false
        }).then(function () {
            ajaxHelper.ajaxPostJsonData({ code: $('#Code').val() }, "/InspectionProcess/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_InspectionProcess.ResetForm();
                    $('#BtnDelete').hide();
                }
            }, false, true);
        });
    },
    InitBatchPickGrid: function () {
        var jobId = $("#key_hidden").val();
        if (jobId == "" || jobId == null || jobId == undefined) {
            empr_helper.notify("Please select Job# first.", 2);
        }
        else {
            empr_InspectionProcess.GetBatchDetailByProcess(jobId);
        }
    },
    GetBatchDetailByProcess: function (clientPO) {
        ajaxHelper.ajaxGetJson('/InspectionProcess/GetBatchDetailByProcess?process=' + clientPO, function (data) {
            if (data.msgType == 1) {
                if (data.data.length > 0) {
                    if ($('#SodaPickGridContainer').data('dxDataGrid') != undefined) {
                        $('#SodaPickGridContainer').data('dxDataGrid').dispose();
                    }
                    empr_InspectionProcess.CreatePickGrid(data.data);
                    $('#SodaPickModal').modal('show');

                } else {
                    empr_helper.notify("No Purchase Order found for this Client PO.", 2);
                }
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    CreatePickGrid: function (dataSrc) {
        var col = [
            { dataField: 'v_DATE', caption: 'Date', dataType: 'date', allowEditing: false, format: 'dd-MM-yyy' }, // pick grid
            { dataField: 'voucheR_NO', caption: 'Voucher', allowEditing: false, },
            { dataField: 'iteM_NAME', caption: 'Item', allowEditing: false, width: 150, },
            { dataField: 'clienT_NAME', caption: 'Client', allowEditing: false, },
            { dataField: 'ref', caption: 'Ref', allowEditing: false, },
            { dataField: 'joB_NO', caption: 'Job #', allowEditing: false, },
            { dataField: 'ename', caption: 'Employee', allowEditing: false, },
            { dataField: 'deP_NAME', caption: 'Department', allowEditing: false, },
            { dataField: 'supplieR_NAME', caption: 'Supplier', allowEditing: false, width: 150, },
            { dataField: 'qty', caption: 'Quantity', allowEditing: false, },
            { dataField: 'uniT_NAME', caption: 'Unit', allowEditing: false, },
            { dataField: 'rate', caption: 'Rate', allowEditing: false, },
            { dataField: 'amt', caption: 'Amount', allowEditing: false, },
            { dataField: 'remarks', caption: 'Remarks', allowEditing: false, },
            { dataField: 'comM_TYPE', allowEditing: false, visible: false, },
            { dataField: 'comM_RATE', allowEditing: false, visible: false, },
            { dataField: 'comM_AMT', allowEditing: false, visible: false, },
            { dataField: 'status', allowEditing: false, visible: false, },
        ];
        empr_helper.dxGridbindingVouchers('#SodaPickGridContainer', col, dataSrc, "PurchaseBillPick", "single");
        setTimeout(function () {
            $('#SodaPickGridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
    CreateBatchGrid: function (dataSrc) {
        var col = [
            { dataField: 'traN_ID', caption: 'Code', visible: false, },
            { dataField: 'process', caption: 'Process', allowEditing: false, },
            { dataField: 'iteM_NAME', caption: 'Item Name', allowEditing: false, },
            { dataField: 'batch', caption: 'Batch', allowEditing: false, },
            { dataField: 'uniT_NAME', caption: 'Unit', allowEditing: false },
            { dataField: 'iteM_CODE', caption: 'Item Code', visible: false, },
            { dataField: 'qty', caption: 'Batch Qty', allowEditing: false, },
            { dataField: 'baL_QTY', caption: 'B.Qty', allowEditing: false, visible: false },
            { dataField: 'unit', caption: 'Unit', allowEditing: false, visible: false },
            { dataField: 'mfG_DATE', caption: 'MFG Date', dataType: 'date', allowEditing: false, format: 'MM-yyy' },
            { dataField: 'exP_DATE', caption: 'EXP Date', dataType: 'date', allowEditing: false, format: 'MM-yyy' },
        ];
        empr_helper.editableDxGridbindingForTransactionsVouchers('#SodaPickGridContainer', col, dataSrc, "DailyProductionPick", "v_DATE", 'multiple');
        setTimeout(function () {
            $('#SodaPickGridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
    AddBatchToDailyProduction: function () {
        if ($('#SodaPickGridContainer').dxDataGrid('instance').hasEditData()) {
            $('#SodaPickGridContainer').dxDataGrid('instance').saveEditData().done(function () {
                var data = empr_InspectionProcess.GetDataToSave();
                var IsDataAvailableInGrid = false;
                $.each(data.Detail, function (index, item) {
                    if (item.iteM_CODE != "" && item.iteM_CODE != null && item.iteM_CODE != undefined) {
                        IsDataAvailableInGrid = true;
                    }
                });

                if (IsDataAvailableInGrid) {
                    var existingData = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
                    var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                    var finalData = existingData.concat(selectedSodas);
                    $('#DetailContainer').dxDataGrid('instance').option('dataSource', finalData);
                }
                else {
                    var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                    $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedSodas);
                }
                $('.modal').hide();
                $('#V_DATE').focus();
            });
        }
        else {
            var data = empr_InspectionProcess.GetDataToSave();
            var IsDataAvailableInGrid = false;
            $.each(data.Detail, function (index, item) {
                if (item.iteM_CODE != "" && item.iteM_CODE != null && item.iteM_CODE != undefined) {
                    IsDataAvailableInGrid = true;
                }
            });

            if (IsDataAvailableInGrid) {
                var existingData = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                var finalData = existingData.concat(selectedSodas);
                $('#DetailContainer').dxDataGrid('instance').option('dataSource', finalData);
            }
            else {
                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedSodas);
            }

            $('.modal').hide();
            $('#V_DATE').focus();
        }
    },
    SetData: function (dataSource) {
        $.each(dataSource, function (index, item) {
            item.chK1 = false;
            item.chk = "0";
            if (item.unit != '') {
                var selectedUnit = Units.filter(u => u.key == item.unit);
                if (selectedUnit.length > 0) {
                    item.qtY2 = selectedUnit[0].qty;
                }
            }
            else {
                item.qtY2 = 0;
            }

            var qty = parseFloat(item.qty) || 0;
            var qtY2 = parseFloat(item.qtY2) || 1;
            var rate = parseFloat(item.rate) || 0;
            var rT_TYPE = parseFloat(item.rT_TYPE) || 0;
            if (!isNaN(qty) && !isNaN(qtY2)) {
                if (item.chK1) {
                    item.baL_QTY = qty * qtY2;
                    if (empr_InspectionProcess.Branch_RT_TYPE == 'Y') {
                        item.amt = item.baL_QTY * rate;
                        var perRate = parseFloat(rate / rT_TYPE) || 0;
                        if (perRate > -1 && perRate != 'Infinity') {
                            item.amt = (item.baL_QTY * perRate).toFixed(2);
                        }
                    }

                    if (empr_InspectionProcess.Branch_RT_TYPE == 'N') {
                        item.amt = qty * rate;
                    }
                    item.neT_AMT = item.amt;
                }
                else {
                    item.baL_QTY = qty;
                    if (empr_InspectionProcess.Branch_RT_TYPE == 'Y') {
                        var perRate = parseFloat(rate / rT_TYPE) || 0;
                        if (perRate > -1 && perRate != 'Infinity') {
                            item.amt = (item.baL_QTY * perRate).toFixed(2);
                        }
                    }

                    if (empr_InspectionProcess.Branch_RT_TYPE == 'N') {
                        item.amt = qty * rate;
                    }
                    item.neT_AMT = item.amt;
                }
            }

            item.__KEY__ = empr_InspectionProcess.GenerateKey(36);
        });

        return dataSource;
    },

    //InitFabricDDL: function (selectedValue) {

    //    $('#FABRIC').dxSelectBox({
    //        dataSource: Fabric,
    //        displayExpr: 'value',
    //        valueExpr: 'key',
    //        value: selectedValue,
    //        searchEnabled: true,
    //        width: '100%',
    //        placeholder: 'Search',
    //        showClearButton: true,
    //        dropDownOptions: {
    //            height: 'auto',
    //        },
    //        pagingEnabled: true,
    //        searchTimeout: 500,
    //    });

    //},

    //InitGSMDDL: function (selectedValue) {

    //    $('#GSM').dxSelectBox({
    //        dataSource: GSMData,
    //        displayExpr: 'value',
    //        valueExpr: 'key',
    //        value: selectedValue,
    //        searchEnabled: true,
    //        width: '100%',
    //        placeholder: 'Search',
    //        showClearButton: true,
    //        dropDownOptions: {
    //            height: 'auto',
    //        },
    //        pagingEnabled: true,
    //        searchTimeout: 500,
    //    });

    //},

    InitPOJobGridDDL: function (_selectedValue) {

        ajaxHelper.ajaxGetJson('/InspectionProcess/POJobsDropdown', function (data) {
            if (data.msgType == 1) {
                var Datasource = data.data;
                selectedObject = [];
                selectedValue = _selectedValue;

                if (_selectedValue != null) {
                    selectedObject = Datasource.filter(x => { return x.key == _selectedValue }) || [];
                    if (selectedObject.length > 0) {
                        selectedValue = selectedObject[0].key;

                        $('#key_hidden').val(selectedObject[0].key);
                        $('#CLIENT_PO').val(selectedObject[0].clientPo);
                        $('#PARTY_CODE').val(selectedObject[0].partyName);
                        $('#SUPPLIER').val(selectedObject[0].supplier);
                    }
                }

                let gridInstance;
                let currentSearchTerm = "";
                let isProgrammaticOpen = false;

                $("#JOB_NO").dxDropDownBox({
                    value: selectedValue,
                    valueExpr: "key",
                    displayExpr: "value",
                    placeholder: "Select a value...",
                    dataSource: Datasource,
                    acceptCustomValue: true,
                    showClearButton: true,
                    deferRendering: false,
                    openOnFieldClick: true,
                    onValueChanged: function (e) {
                        if (e.value && gridInstance) {
                            const selectedData = gridInstance.getDataSource().items().find(item => item.key === e.value);
                            if (selectedData) {
                                console.log('Job Id', selectedData.key);
                                if (empr_InspectionProcess.detailGridCall == true) {
                                    empr_InspectionProcess.InitializeLockGrid(selectedData.key);
                                }
                                debugger;
                                $('#key_hidden').val(selectedData.key);
                                $('#jobValue_hidden').val(selectedData.value);
                                $('#CLIENT_PO').val(selectedData.clientPo);
                                $('#PARTY_CODE').val(selectedData.partyName);
                                $('#SUPPLIER').val(selectedData.supplier);
                                $('#ORDER_QTY').val(selectedData.qty);
                                $('#CTN_QTY').val(selectedData.ctnQty);
                            }
                        } else {
                            empr_InspectionProcess.InitializeLockGrid(null);
                            $('#key_hidden').val('');
                            $('#jobValue_hidden').val();
                            $('#CLIENT_PO').val('');
                            $('#PARTY_CODE').val('');
                            $('#SUPPLIER').val('');
                            $('#ORDER_QTY').val('');
                            $('#CTN_QTY').val('');
                        }
                    },
                    onOpened: function (e) {
                        if (!currentSearchTerm) {
                            if (gridInstance) {
                                gridInstance.getDataSource().filter(null);
                                gridInstance.refresh();
                            }
                        }

                        setTimeout(() => {
                            const input = e.component._$element.find(".dx-texteditor-input").first();
                            input.focus();
                            if (input.val()) {
                                //input.select();
                            }
                        }, 50);
                    },
                    onInput: function (e) {
                        currentSearchTerm = e.event.target.value;

                        if (!e.component.option("opened")) {
                            isProgrammaticOpen = true;
                            e.component.open();
                            setTimeout(() => { isProgrammaticOpen = false; }, 100);
                        }

                        if (gridInstance) {
                            applyGridFilter(gridInstance, currentSearchTerm);
                        }
                    },
                    contentTemplate: function (e) {
                        gridInstance = $("<div>").dxDataGrid({
                            dataSource: new DevExpress.data.DataSource({
                                store: Datasource,
                                key: "key"
                            }),
                            columns: [

                                {
                                    dataField: "value",
                                    caption: "Job No",
                                    width: 200,
                                    cellTemplate: function (container, options) {
                                        highlightText(container, options.value);
                                    }
                                },
                                {
                                    dataField: "clientPo",
                                    caption: "Model# / PO#",
                                    width: 200,
                                    cellTemplate: function (container, options) {
                                        highlightText(container, options.value);
                                    }
                                },
                                {
                                    dataField: "partyName",
                                    caption: "Client",
                                    width: 370,
                                    cellTemplate: function (container, options) {
                                        highlightText(container, options.value);
                                    }
                                }
                            ],
                            selection: {
                                mode: "single",
                                showCheckBoxesMode: "always"
                            },
                            hoverStateEnabled: true,
                            height: 300,
                            keyboardNavigation: {
                                enabled: true,
                                enterKeyAction: "select",
                                editOnKeyPress: true
                            },
                            onSelectionChanged: function (selectedItems) {
                                const selected = selectedItems.selectedRowsData[0];
                                if (selected) {
                                    e.component.option("value", selected.key);

                                    e.component.close();

                                    $('#key_hidden').val(selected.key);
                                    $('#CLIENT_PO').val(selected.clientPo);
                                    $('#PARTY_CODE').val(selected.partyName);
                                }
                            },
                            onContentReady: function (e) {
                                if (currentSearchTerm) {
                                    const items = e.component.getDataSource().items();
                                    if (items.length > 0) {
                                        e.component.selectRows([items[0].key], false);
                                    }
                                }
                            }
                        }).dxDataGrid("instance");

                        gridInstance.element().on('click', function (event) {
                            event.stopPropagation();
                        });

                        return gridInstance.element();
                    }
                });

                function applyGridFilter(grid, searchTerm) {
                    const dataSource = grid.getDataSource();
                    if (searchTerm) {
                        dataSource.filter([
                            ["value", "contains", searchTerm],
                            "or",
                            ["clientPo", "contains", searchTerm],
                            "or",
                            ["partyName", "contains", searchTerm]
                        ]);
                    } else {
                        dataSource.filter(null);
                    }
                    dataSource.load();
                }

                function highlightText(container, value) {
                    if (!value) return;

                    const text = value.toString();
                    if (!currentSearchTerm || !text.toLowerCase().includes(currentSearchTerm.toLowerCase())) {
                        container.text(text);
                        return;
                    }

                    const regex = new RegExp(currentSearchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), "gi");
                    const highlighted = text.replace(regex, match =>
                        `<span style="background-color: #ffeb3b; font-weight: bold;">${match}</span>`
                    );
                    container.html(highlighted);
                }

                $(document).on("dxclick", "#JOB_NO .dx-clear-button-area", function (e) {
                    currentSearchTerm = "";
                    $('#ACT_PARENT_CODE_hidden').val('');
                    $('#displayExprAccParent').val('');
                    if (gridInstance) {
                        gridInstance.getDataSource().filter(null);
                        gridInstance.refresh();
                    }
                });

            }
            else {
                empr_helper.notify('error', 2);
            }
        }, false, true);

    },

    handleN: function (rowData, rowIndex) {

        // Get the row element
        const rowElement = $("#gridContainer").dxDataGrid("instance")
            .getRowElement(rowData);

        // Remove background from all buttons in this row
        $(document).find(".nrc_btns" + rowIndex + " button").css({
            "background-color": "",
            "color": ""
        });

        // Set background for clicked button
        $(document).find(".btn_n" + rowIndex).css({
            "background-color": "#055a87",
            "color": "white",
        });

        $(document).find("#nrc_toggle" + rowIndex).prop("checked", false);

        rowData.reV_TOGGLE = false;
    },

    handleR: function (rowData, rowIndex) {

        swal({
            title: 'Are you sure you want to Revised this record?',
            text: "",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0CC27E',
            cancelButtonColor: '#FF586B',
            confirmButtonText: 'Yes',
            cancelButtonText: 'No',
            confirmButtonClass: 'btn btn-success mr-5',
            cancelButtonClass: 'btn btn-danger',
            buttonsStyling: false
        }).then(function () {
            $(document).find(".nrc_btns" + rowIndex + " button").css({
                "background-color": "",
                "color": ""
            });

            $(document).find(".btn_r" + rowIndex).css({
                "background-color": "#51bb25",
                "color": "white",
            });

            $(document).find("#nrc_toggle" + rowIndex).prop("checked", true)

            rowData.reV_TOGGLE = true;
            rowData.reV_COLOR = "green";


            //const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            //gridInstance.refresh();
        });
    },

    handleC: function (rowData, rowIndex) {
        swal({
            title: 'Are you sure you want to Cancle this record?',
            text: "",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0CC27E',
            cancelButtonColor: '#FF586B',
            confirmButtonText: 'Yes',
            cancelButtonText: 'No',
            confirmButtonClass: 'btn btn-success mr-5',
            cancelButtonClass: 'btn btn-danger',
            buttonsStyling: false
        }).then(function () {
            $(document).find(".nrc_btns" + rowIndex + " button").css({
                "background-color": "",
                "color": ""
            });

            $(document).find(".btn_c" + rowIndex).css({
                "background-color": "##dc3545",
                "color": "white",
            });

            $(document).find("#nrc_toggle" + rowIndex).prop("checked", true)

            rowData.reV_TOGGLE = true;
            rowData.reV_COLOR = "red";
        });
    },

}