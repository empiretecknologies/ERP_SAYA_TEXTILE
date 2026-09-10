var empr_SalesSamplingReports = {
    reportDataSrc: [],
    InitEvents: function () {
        console.log('Supplier', Supplier);
        empr_SalesSamplingReports.GetReportTypes();
        //empr_SalesSamplingReports.InitControlDDL();
        //empr_SalesSamplingReports.InitAccountDDL();
        empr_SalesSamplingReports.InitPartyCodeDDL();
        empr_SalesSamplingReports.InitSupplierCodeDDL();
        //empr_SalesSamplingReports.InitPOJobGridDDL(null);
        empr_SalesSamplingReports.InitItemDDL();
        //empr_SalesSamplingReports.InitSeasonDDL();
        $('.subTab').hide();

        $('body').on('click', '#BtnGenerate', function () {
            $("#Loader").show();
            $("#Loader").css('display', 'flex');
            setTimeout(function () {
                if (empr_SalesSamplingReports.ValidateInfo()) {
                    empr_SalesSamplingReports.GenerateReport();
                    setTimeout(function () {
                        $("#Loader").hide();
                    }, 500);
                }
            }, 200);
        });

    },


    GetReportTypes: function () {
        ajaxHelper.ajaxGetJson("/SalesSamplingReports/GetReportTypes", function (data) {
            if (data.msgType == 1) {
                empr_SalesSamplingReports.InitReportTypeGrid(data.data);
            }
            else {
                empr_helper.notify(data.data, data.msgType);
            }
        }, false, true);
    },

    InitReportTypeGrid: function (dataSrc) {
        console.log('dataSrc', dataSrc);
        reportDataSrc = dataSrc;
        var html = '';

        $.each(dataSrc, function (index, item) {
            html += `
        <div>
            <label>
                ${item.sno} — 
                <input type="radio" name="reportRadio" data-index="${index}">
                <span style="font-size:12px;">${item.reporT_NAME}</span>
            </label>
        </div>
        `;
        });

        $('#GridContainer').html(html);
    },

    GetDataToSave: function () {
        var selectedIndex = $('input[name="reportRadio"]:checked').data('index');
        var selectedRowKey = [];
        //if (selectedIndex !== undefined) {
        //    selectedRowKey = reportDataSrc[selectedIndex];
        //}
        if (selectedIndex !== undefined) {
            selectedRowKey.push(reportDataSrc[selectedIndex]);
        }
        var reportID = '0';
        if (selectedRowKey.length > 0) {
            reportID = selectedRowKey[0].r_ID;
            console.log('reportID', reportID);
            empr_helper.reportName = selectedRowKey[0].reporT_NAME;
        }
        var FROM_DATE = $("#FROM_DATE").val();
        var HIDDEN_FROM_DATE = $("#HIDDEN_FROM_DATE").val();
        var TO_DATE = $("#TO_DATE").val();
        var HIDDEN_TO_DATE = $("#HIDDEN_TO_DATE").val();
        var REPORT_ID = reportID;
        var ITEM_CODE = $('#ITEM_CODE').dxSelectBox('option', 'value');
        //var SEASON = $('#SEASON').dxSelectBox('option', 'value');
        var JOB_NO = $('#job_key_hidden').val();

        // 'instance' letay waqt hamesha check karen
        var partyInstance = $('#PARTY_CODE').dxSelectBox('instance');
        var selectedParty = partyInstance.option('selectedItem');

        var suppInstance = $('#SPARTY_CODE').dxSelectBox('instance');
        var selectedSupp = suppInstance.option('selectedItem');

        var record = {
            FROMDATE: FROM_DATE,
            TODATE: TO_DATE,
            HIDDENFROMDATE: HIDDEN_FROM_DATE,
            HIDDENTODATE: HIDDEN_TO_DATE,
            REPORTID: REPORT_ID,
            ITEM_CODE: ITEM_CODE,
            //SEASON: SEASON,

            PARTY_CODE: selectedParty ? selectedParty.key : null,
            ACT_CODE: selectedParty ? selectedParty.accountCode : null,

            SPARTY_CODE: selectedSupp ? selectedSupp.key : null,
            SACT_CODE: selectedSupp ? selectedSupp.accountCode : null,

            JOB_NO: JOB_NO,
        };
        debugger;
        return record;
    },

    GenerateReport: function () {
        var dataModel = empr_SalesSamplingReports.GetDataToSave();
        console.log(dataModel);
        ajaxHelper.ajaxPostJsonData(dataModel, "/SalesSamplingReports/GenerateReport", function (data) {
            if (data.msgType == 1) {
                empr_SalesSamplingReports.InitReportGrid(data.data, dataModel.REPORTID);


                //if (dataModel.REPORTID == 138) {
                //    $('.subTab').show();
                //    // Trade report
                //    let subModel = { ...dataModel, REPORTID: 139 };
                //    ajaxHelper.ajaxPostJsonData(subModel, "/SalesSamplingReports/GenerateReport", function (subData) {

                //        if (subData.msgType == 1) {
                //            empr_SalesSamplingReports.InitSubReportGrid(subData.data, 139);
                //        } else {
                //            empr_helper.notify(data.msg, data.msgType);
                //        }
                //    }, false, true);
                //} else {
                //    $('.subTab').hide();

                //}

            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

    InitReportGrid: function (dataSrc, reportId) {
        console.log("firstreport data", dataSrc);
        //empr_helper.typeOneTotal = 0;
        //empr_helper.typeTwoTotal = 0;
        //empr_helper.typeThreeTotal = 0;
        //empr_helper.typeFourTotal = 0;

        if (reportId == 140) {
            col = [
                { dataField: 'doc', caption: 'Doc', width: 20 },
                { dataField: 'iteM_NAME', caption: 'Item Name', width: 400 },
                { dataField: 'clienT_PO', caption: 'Enquiry Sub', width: 400 },
                { dataField: 'reF_ON_DATE', caption: 'Enquiry Date', width: 150 },
                { dataField: 'qty', caption: 'Qty', width: 150 },
                { dataField: 'iteM_DETAIL', caption: 'Product Detail', width: 400 },
                { dataField: 'sizE_NAME', caption: 'Size', width: 150 },
                { dataField: 'rate', caption: 'Prices', width: 100 },
            ]
        }
        
        empr_helper.DxGridBindingForReportsWithSetting_Aging('#ReportGridContainer', col, dataSrc, empr_helper.reportName, false, false, 'old' );

        setTimeout(function () {
            $('#OptionTab').removeClass('active');
            $('#OptionTabContent').removeClass('active');
            $('#ViewTab').click();
            $('.tab-pane').removeClass('fade');
            $('#ViewTab').addClass('active')
            $('#ViewTabContent').addClass('active');
        }, 500);
    },


    InitSubReportGrid(dataSrc, reportId) {

        if (reportId == 139) {
            col = [
                { dataField: 'suppName', caption: 'Supplier Name', width: 100, groupIndex: 0 },
                { dataField: 'sno', caption: 'Sno', width: 100, alignment: 'center', },
                { dataField: 'suppCode', caption: 'Supp Code', width: 120, visible: false },
                { dataField: 'suppActCode', caption: 'Supp Act Code', width: 100, visible: false },
                { dataField: 'size', caption: 'Size', width: 300 },
                { dataField: 'qty', caption: 'Qty', },
                { dataField: 'rate', caption: 'Rate' },
                { dataField: 'amt', caption: 'Amount' },
            ]
        }
        empr_helper.DxGridBindingForReportsWithSetting_Aging('#SubGridContainer', col, dataSrc, empr_helper.reportName);

        //setTimeout(function () {

        //    var selectedRowKey = $('#SubGridContainer').dxDataGrid('instance').getSelectedRowKeys();
        //    //if (selectedRowKey.length > 0) {
        //    //    $('#REPORT_NAME').text(selectedRowKey[0].reporT_NAME);
        //    //}
        //    $('#OptionTab').removeClass('active');
        //    $('#OptionTabContent').removeClass('active');
        //    $('#ViewTab').click();
        //    $('.tab-pane').removeClass('fade');
        //    $('#ViewTab').addClass('active')
        //    $('#ViewTabContent').addClass('active');
        //}, 500);
    },

    ValidateInfo: function () {

        var valid = true;
        var data = empr_SalesSamplingReports.GetDataToSave();


        var userfromDateObj = new Date(data.FROMDATE);
        var usertoDateObj = new Date(data.TODATE);

        var periodFromDateObj = new Date(data.HIDDENFROMDATE);
        var periodToDateObj = new Date(data.HIDDENTODATE);

        if (data.FROMDATE == "" || data.FROMDATE == null || data.FROMDATE == undefined) {
            empr_helper.notify("Please select FromDate.", 2);
            valid = false;
        } else {
            empr_helper.fromDate = data.FROMDATE;
        }

        if (data.TODATE == "" || data.TODATE == null || data.TODATE == undefined) {
            empr_helper.notify("Please select ToDate.", 2);
            valid = false;
        } else {
            empr_helper.toDate = data.TODATE;
        }

        if (data.FROMDATE > data.TODATE) {
            empr_helper.notify("'To Date' must be greater than or equal to 'From Date'.", 2);
            valid = false;
        }

        if (data.FROMDATE != null && data.HIDDENFROMDATE != null) {

            if (userfromDateObj < periodFromDateObj || userfromDateObj > periodToDateObj) {
                empr_helper.notify("Selected dates are outside the active period.", 2);
                valid = false;
            }
        }

        if (data.TODATE != null && data.HIDDENTODATE != null) {

            if (usertoDateObj > periodToDateObj || usertoDateObj < periodFromDateObj) {
                empr_helper.notify("Selected dates are outside the active period.", 2);
                valid = false;
            }
        }

        if (data.REPORTID < 1) {
            empr_helper.notify("Please select report type.", 2);
            valid = false;
        }
        $("#Loader").hide();
        return valid;
    },

    openVoucherPage(link, tran_Id) {
        var newWindow = window.open(link, '_blank');
        newWindow.addEventListener('load', function () {
            setTimeout(function () {
                newWindow.postMessage({ traN_ID: tran_Id }, '*');
            }, 1000);
            //newWindow.postMessage({ traN_ID: tran_Id }, '*');
        });
    },

    InitPartyCodeDDL: function (selectedValue) {
        $('#PARTY_CODE').dxSelectBox({
            dataSource: {
                store: Supplier,
                paginate: true,
                pageSize: 50
            },
            paging: {
                enabled: true,
                pageSize: 50,
            },
            displayExpr: 'value',
            valueExpr: 'customizedKey',
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
        });
    },

    InitSupplierCodeDDL: function (selectedValue) {
        $('#SPARTY_CODE').dxSelectBox({
            dataSource: {
                store: Supplier,
                paginate: true,
                pageSize: 50
            },
            paging: {
                enabled: true,
                pageSize: 50,
            },
            displayExpr: 'value',
            valueExpr: 'customizedKey',
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
        });
    },

    //InitPOJobGridDDL: function (_selectedValue) {

    //    ajaxHelper.ajaxGetJson('/MerchantPurchaseOrderDetail/POJobsDropdown', function (data) {
    //        debugger;
    //        if (data.msgType == 1) {
    //            var Datasource = data.data;

    //            selectedObject = [];
    //            selectedValue = _selectedValue;

    //            if (_selectedValue != null) {
    //                selectedObject = Datasource.filter(x => { return x.key == _selectedValue }) || [];
    //                if (selectedObject.length > 0) {
    //                    selectedValue = selectedObject[0].key;

    //                    $('#job_key_hidden').val(selectedObject[0].key);
    //                    $('#CLIENT_PO').val(selectedObject[0].clientPo);
    //                    $('#PARTY_CODE').val(selectedObject[0].partyName);
    //                }
    //            }

    //            let gridInstance;
    //            let currentSearchTerm = "";
    //            let isProgrammaticOpen = false;

    //            $("#JOB_NO").dxDropDownBox({
    //                value: selectedValue,
    //                valueExpr: "key",
    //                displayExpr: "value",
    //                placeholder: "Select a value...",
    //                dataSource: Datasource,
    //                acceptCustomValue: true,
    //                showClearButton: true,
    //                deferRendering: false,
    //                openOnFieldClick: true,
    //                onValueChanged: function (e) {
    //                    if (e.value && gridInstance) {
    //                        debugger;
    //                        const selectedData = gridInstance.getDataSource().items().find(item => item.key === e.value);
    //                        const selectedParty = Supplier.find(item => item.key === selectedData.partyCode && item.accountCode === selectedData.accountCode);
    //                        const selectedSupplier = Supplier.find(item => item.key === selectedData.spartyCode && item.accountCode === selectedData.saccountCode);
    //                        const selectedItem = Items.find(item => item.key === selectedData.itemCode);
    //                        const selectedSeason = Season.find(item => item.key === selectedData.seasonCode);
    //                        if (selectedData) {
    //                            empr_SalesSamplingReports.InitPartyCodeDDL(selectedParty.customizedKey);
    //                            empr_SalesSamplingReports.InitSupplierCodeDDL(selectedSupplier.customizedKey);
    //                            empr_SalesSamplingReports.InitItemDDL(selectedItem.key);
    //                            if (selectedSeason) {
    //                                empr_SalesSamplingReports.InitSeasonDDL(selectedSeason.key);
    //                            }


    //                            $('#job_key_hidden').val(selectedData.key);


    //                            //$('#CLIENT_PO').val(selectedData.clientPo);
    //                            //$('#PARTY_CODE').val(selectedData.partyName);
    //                        }
    //                    } else {
    //                        $('#job_key_hidden').val('');
    //                        empr_SalesSamplingReports.InitPartyCodeDDL();
    //                        empr_SalesSamplingReports.InitSupplierCodeDDL();
    //                        empr_SalesSamplingReports.InitItemDDL();
    //                        empr_SalesSamplingReports.InitSeasonDDL();
    //                        //$('#CLIENT_PO').val('');
    //                        //$('#PARTY_CODE').val('');
    //                    }
    //                },
    //                onOpened: function (e) {
    //                    if (!currentSearchTerm) {
    //                        if (gridInstance) {
    //                            gridInstance.getDataSource().filter(null);
    //                            gridInstance.refresh();
    //                        }
    //                    }

    //                    setTimeout(() => {
    //                        const input = e.component._$element.find(".dx-texteditor-input").first();
    //                        input.focus();
    //                        if (input.val()) {
    //                            //input.select();
    //                        }
    //                    }, 50);
    //                },
    //                onInput: function (e) {
    //                    currentSearchTerm = e.event.target.value;

    //                    if (!e.component.option("opened")) {
    //                        isProgrammaticOpen = true;
    //                        e.component.open();
    //                        setTimeout(() => { isProgrammaticOpen = false; }, 100);
    //                    }

    //                    if (gridInstance) {
    //                        applyGridFilter(gridInstance, currentSearchTerm);
    //                    }
    //                },
    //                contentTemplate: function (e) {
    //                    gridInstance = $("<div>").dxDataGrid({
    //                        dataSource: new DevExpress.data.DataSource({
    //                            store: Datasource,
    //                            key: "key"
    //                        }),
    //                        columns: [

    //                            {
    //                                dataField: "value",
    //                                caption: "Job No",
    //                                width: 200,
    //                                cellTemplate: function (container, options) {
    //                                    highlightText(container, options.value);
    //                                }
    //                            },
    //                            {
    //                                dataField: "clientPo",
    //                                caption: "Model# / PO#",
    //                                width: 200,
    //                                cellTemplate: function (container, options) {
    //                                    highlightText(container, options.value);
    //                                }
    //                            },
    //                            {
    //                                dataField: "partyName",
    //                                caption: "Client",
    //                                width: 370,
    //                                cellTemplate: function (container, options) {
    //                                    highlightText(container, options.value);
    //                                }
    //                            }
    //                        ],
    //                        selection: {
    //                            mode: "single",
    //                            showCheckBoxesMode: "always"
    //                        },
    //                        hoverStateEnabled: true,
    //                        height: 300,
    //                        keyboardNavigation: {
    //                            enabled: true,
    //                            enterKeyAction: "select",
    //                            editOnKeyPress: true
    //                        },
    //                        onSelectionChanged: function (selectedItems) {
    //                            const selected = selectedItems.selectedRowsData[0];
    //                            if (selected) {
    //                                e.component.option("value", selected.key);

    //                                e.component.close();

    //                                $('#job_key_hidden').val(selected.key);
    //                                $('#CLIENT_PO').val(selected.clientPo);
    //                                $('#PARTY_CODE').val(selected.partyName);
    //                            }
    //                        },
    //                        onContentReady: function (e) {
    //                            if (currentSearchTerm) {
    //                                const items = e.component.getDataSource().items();
    //                                if (items.length > 0) {
    //                                    e.component.selectRows([items[0].key], false);
    //                                }
    //                            }
    //                        }
    //                    }).dxDataGrid("instance");

    //                    gridInstance.element().on('click', function (event) {
    //                        event.stopPropagation();
    //                    });

    //                    return gridInstance.element();
    //                }
    //            });

    //            function applyGridFilter(grid, searchTerm) {
    //                const dataSource = grid.getDataSource();
    //                if (searchTerm) {
    //                    dataSource.filter([
    //                        ["value", "contains", searchTerm],
    //                        "or",
    //                        ["clientPo", "contains", searchTerm],
    //                        "or",
    //                        ["partyName", "contains", searchTerm]
    //                    ]);
    //                } else {
    //                    dataSource.filter(null);
    //                }
    //                dataSource.load();
    //            }

    //            function highlightText(container, value) {
    //                if (!value) return;

    //                const text = value.toString();
    //                if (!currentSearchTerm || !text.toLowerCase().includes(currentSearchTerm.toLowerCase())) {
    //                    container.text(text);
    //                    return;
    //                }

    //                const regex = new RegExp(currentSearchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), "gi");
    //                const highlighted = text.replace(regex, match =>
    //                    `<span style="background-color: #ffeb3b; font-weight: bold;">${match}</span>`
    //                );
    //                container.html(highlighted);
    //            }

    //            $(document).on("dxclick", "#JOB_NO .dx-clear-button-area", function (e) {
    //                currentSearchTerm = "";
    //                $('#ACT_PARENT_CODE_hidden').val('');
    //                $('#displayExprAccParent').val('');
    //                if (gridInstance) {
    //                    gridInstance.getDataSource().filter(null);
    //                    gridInstance.refresh();
    //                }
    //            });

    //        }
    //        else {
    //            empr_helper.notify('error', 2);
    //        }
    //    }, false, true);

    //},

    InitItemDDL: function (selectedValue) {
        $('#ITEM_CODE').dxSelectBox({
            dataSource: Items,
            displayExpr: 'value',
            valueExpr: 'key',
            value: selectedValue,
            searchEnabled: true,
            width: '100%',
            placeholder: 'Search',
            showClearButton: true,
            dropDownOptions: {
                height: 'auto',
            },
            pagingEnabled: true,
            searchTimeout: 500
        });
    },

    //InitSeasonDDL: function (selectedValue) {
    //    $('#SEASON').dxSelectBox({
    //        dataSource: Season,
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
    //        searchTimeout: 500
    //    });
    //},

}