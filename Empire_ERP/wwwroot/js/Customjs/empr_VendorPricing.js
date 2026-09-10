var empr_VendorPricing = {
    totalCount: 0,
    rowsCount: 0,
    parties: [],
    PartiesData: [],
    //pickId: 0,
    //PickQty: 0,
    //BtnBatchPick
    //BtnAddBatch
    InitEvents: function () {
        $(document).ready(function () {
            empr_VendorPricing.InitReportTypeDDL();
            empr_VendorPricing.InitPOJobGridDDL(null);
            empr_VendorPricing.InitPartyCodeDDL();
            empr_VendorPricing.ResetForm();

            window.addEventListener('message', function (event) {
                if (event.origin !== window.location.origin) {
                    return;
                }
                var data = event.data;
                if (data && data.traN_ID) {
                    $('#Code').val(data.traN_ID);
                    empr_VendorPricing.GetVendorPricingByCode(data.traN_ID);
                }
            });

            //ajaxHelper.ajaxGetJson("/DeliveryFeeding/GetParties", function (data) {
            //    if (data.msgType == 1) {
            //        empr_VendorPricing.PartiesData = data.data;
            //        empr_VendorPricing.InitPartyCodeDDL(data.data);
            //        parties = data.data;
            //    }
            //    else {
            //        empr_helper.notify(data.data, data.msgType);
            //    }
            //}, false, true);

            $('body').on('click', '#BtnQuickSearch', function () {
                empr_VendorPricing.InitQuickSearchGrid();
            });

            $('body').on('click', '.elm_edit', function () {
                var id = $(this).attr("reportid");
                $('#Code').val(id);
                $('.vHide').show();
                $('.modal').modal('hide');
                empr_VendorPricing.GetVendorPricingByCode(id);

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
                ajaxHelper.ajaxPostJsonData({ traN_ID: empr_helper.selectedBill, v_DATE: $('#updatedDate').val() }, "/VendorPricing/CopyRecord", function (data) {
                    empr_helper.notify(data.msg, data.msgType);
                    if (data.msgType == 1) {
                        $('.modal').modal('hide');
                        empr_VendorPricing.GetVendorPricingByCode(data.data.code);
                    }
                }, false, true);
            });


            $('body').on('click', '#BtnSave', function () {
                $("#Loader").show();
                $("#Loader").css('display', 'flex');
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                        setTimeout(function () {
                            $("#Loader").hide();
                        }, 500);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                        setTimeout(function () {
                            $("#Loader").hide();
                        }, 500);
                    } else {
                        if (empr_VendorPricing.ValidateInfo()) {
                            empr_VendorPricing.SaveInfo();
                        }
                        setTimeout(function () {
                            $("#Loader").hide();
                        }, 500);
                    }
                } else {
                    if (empr_VendorPricing.ValidateInfo()) {
                        empr_VendorPricing.SaveInfo();
                    }
                    setTimeout(function () {
                        $("#Loader").hide();
                    }, 500);
                }
            });

            $('body').on('click', '#BtnNew', function () {
                empr_VendorPricing.ResetForm();
            });

            $('body').on('click', '#BtnDelete', function () {
                empr_VendorPricing.Delete();
            });

            $('body').on('click', '#BtnPrint, #BtnGenerateReport', function () {
                empr_VendorPricing.GeneratePrintReport();
            });

            $('body').on('click', '.elm_print', function () {
                empr_helper.selectedBill = $(this).attr("reportid");
                empr_VendorPricing.GeneratePrintReport();
            });

            $('body').on('click', '#BtnBatchPick', function () { // first  
                empr_VendorPricing.InitBatchPickGrid();
            });

            //$('body').on('click', '#BtnAddBatch', function () { // second
            //    //debugger;
            //    var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
            //    if (selectedSodas.length > 0) {
            //        var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();

            //        empr_VendorPricing.pickId = selectedSodas[0].picK_ID;
            //        empr_VendorPricing.PickQty = selectedSodas[0].qty;
            //        //empr_VendorPricing.InitSupplierDDL(Supplier.value.data, selectedSodas[0].spartY_CODE);

            //        selectedSodas[0].__KEY__ = empr_VendorPricing.GenerateKey(36);
            //        var abc = selectedSodas[0];
            //        empr_VendorPricing.CreateGrid([selectedSodas[0]]);

            //        //$('.modal').hide();
            //        // Model double clicks
            //        var modalEl = document.getElementById('SodaPickModal');
            //        var modalInstance = bootstrap.Modal.getInstance(modalEl);
            //        if (modalInstance) {
            //            modalInstance.hide(); // proper close
            //        }

            //        //$('#CLIENT_PO').prop('disabled', true);
            //        $('#V_DATE').focus();
            //    }
            //    else {
            //        empr_helper.notify("Please select the items first.", 2);
            //    }
            //});

            if (Permissions != "Admin") {
                !Permissions.r_ADD && $('#BtnNew').hide();
                !Permissions.r_VIEW && $('#BtnQuickSearch').hide();
                !Permissions.r_PRINT && $('.btn-print').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }

        });
    },

    ResetForm: function () {
        empr_VendorPricing.CreateGrid([{ __KEY__: empr_VendorPricing.GenerateKey(36) }]);
        //empr_VendorPricing.pickId = 0;
        $('.Record input').not('#ASTATUS, .dx-texteditor-input, #V_DATE').val('');
        $('#REMARKS').val('');
        $('#BtnBatchPick').prop('disabled', false);
        $('#key_hidden').val('');
        $('#pickTranId').val('');
        $('#CLIENT_PO').val('');
        $('#BtnDelete').hide();
        empr_VendorPricing.InitPartyCodeDDL();
        empr_VendorPricing.InitPOJobGridDDL(null);
        $('.card-body').removeClass('customHighlightForModifiedCells');
        if (Permissions != "Admin") {
            if (Permissions.r_ADD) {
                $('#BtnSave').show();
            } else {
                $('#BtnSave').hide();
            }
        } else {
            $('#BtnSave').show();
        }
        empr_VendorPricing.CreateGrid([{ __KEY__: empr_VendorPricing.GenerateKey(36) }]);
        if ($('#FinishItem').data('dxSelectBox') != null) {
            $('#FinishItem').dxSelectBox('instance').dispose();
        }
        //empr_VendorPricing.InitPartyCodeDDL();
        //$('#CLIENT_PO').prop('disabled', false);
    },
    CreateGrid: function (dataSrc) {
        console.log('CreateGrid', dataSrc);

        if (dataSrc.length > 0) {
            empr_VendorPricing.rowsCount = dataSrc.length - 1;
            dataSrc.forEach(item => {
                if (
                    item.shiP_DATE == '1900-01-01' || item.shiP_DATE == '01-01-1900' || item.shiP_DATE == '01-Jan-1900' || item.shiP_DATE == '1/1/1900 12:00:00 AM' || item.shiP_DATE == '01/01/1900 12:00:00 AM' || item.shiP_DATE == '1/1/1900' ||
                    item.shiP_DATE == '2000-01-01' || item.shiP_DATE == '01-01-2000' || item.shiP_DATE == '01-Jan-2000' || item.shiP_DATE == '1/1/2000 12:00:00 AM' || item.shiP_DATE == '01/01/2000 12:00:00 AM' || item.shiP_DATE == '1/1/2000' ||
                    item.shiP_DATE == '00-01-01' || item.shiP_DATE == '01-01-00' || item.shiP_DATE == '01-Jan-00' || item.shiP_DATE == '1/1/00 12:00:00 AM' || item.shiP_DATE == '01/01/00 12:00:00 AM' || item.shiP_DATE == '1/1/00' || item.shiP_DATE == '01-Jan-00 12:00:00 AM'
                ) {
                    item.shiP_DATE = null;
                }
                if (
                    item.bookinG_DATE == '1900-01-01' || item.bookinG_DATE == '01-01-1900' || item.bookinG_DATE == '01-Jan-1900' || item.bookinG_DATE == '1/1/1900 12:00:00 AM' || item.bookinG_DATE == '01/01/1900 12:00:00 AM' || item.bookinG_DATE == '1/1/1900' ||
                    item.bookinG_DATE == '2000-01-01' || item.bookinG_DATE == '01-01-2000' || item.bookinG_DATE == '01-Jan-2000' || item.bookinG_DATE == '1/1/2000 12:00:00 AM' || item.bookinG_DATE == '01/01/2000 12:00:00 AM' || item.bookinG_DATE == '1/1/2000' ||
                    item.bookinG_DATE == '00-01-01' || item.bookinG_DATE == '01-01-00' || item.bookinG_DATE == '01-Jan-00' || item.bookinG_DATE == '1/1/00 12:00:00 AM' || item.bookinG_DATE == '01/01/00 12:00:00 AM' || item.bookinG_DATE == '1/1/00' || item.bookinG_DATE == '01-Jan-00 12:00:00 AM'
                ) {
                    item.bookinG_DATE = null;
                }
                if (
                    item.handoveR_DATE == '1900-01-01' || item.handoveR_DATE == '01-01-1900' || item.handoveR_DATE == '01-Jan-1900' || item.handoveR_DATE == '1/1/1900 12:00:00 AM' || item.handoveR_DATE == '01/01/1900 12:00:00 AM' || item.handoveR_DATE == '1/1/1900' ||
                    item.handoveR_DATE == '2000-01-01' || item.handoveR_DATE == '01-01-2000' || item.handoveR_DATE == '01-Jan-2000' || item.handoveR_DATE == '1/1/2000 12:00:00 AM' || item.handoveR_DATE == '01/01/2000 12:00:00 AM' || item.handoveR_DATE == '1/1/2000' ||
                    item.handoveR_DATE == '00-01-01' || item.handoveR_DATE == '01-01-00' || item.handoveR_DATE == '01-Jan-00' || item.handoveR_DATE == '1/1/00 12:00:00 AM' || item.handoveR_DATE == '01/01/00 12:00:00 AM' || item.handoveR_DATE == '1/1/00' || item.handoveR_DATE == '01-Jan-00 12:00:00 AM'
                ) {
                    item.handoveR_DATE = null;
                }
            });
        }

        var maxSNoFromDB = 0;
        if (dataSrc && dataSrc.length > 0) {
            maxSNoFromDB = Math.max.apply(Math, dataSrc.map(o => o.s_NO || 0));
        }

        var col = [
            {
                dataField: "Action",
                width: 90,
                alignment: 'center',
                fixed: true,
                fixedPosition: "left",
                allowExporting: false,
                allowEditing: false,
                cellTemplate: function (container, options) {
                    if (Permissions != "Admin") {
                        const copyAction = !Permissions.r_COPY
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Clone" onclick="empr_VendorPricing.CloneRow('${options.data.__KEY__}')" title="Duplicate"><i class="fa fa-clone"></i></a>`;
                        //const addAction = (!Permissions.r_ADD && !Permissions.r_EDIT)
                        //    ? ''
                        //    : `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_VendorPricing.AddRow()" title="Add"><i class="fa fa-add"></i></a>`;
                        const deleteAction = !Permissions.r_DLT
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 18px" onclick="empr_VendorPricing.DeleteRow('${options.data.__KEY__}')" title="Delete"><i class="fa fa-trash"></i></a>`;
                        const actions = `<div class="btn-group btn-group-sm">${copyAction}${deleteAction}</div>`;
                        $(actions).appendTo(container);
                    } else {
                        //console.log('insideGrid',options.data);
                        $(`<div class="btn-group btn-group-sm">
                   <a href="javascript:;" class="grid-action-icon" onclick="empr_VendorPricing.CloneRow('${options.data.__KEY__}')" title="Duplicate"><i class="fa fa-clone"></i></a>
                   <a href="javascript:;" class="grid-action-icon" style="margin-left: 18px" onclick="empr_VendorPricing.DeleteRow('${options.data.__KEY__}')" title="Delete"><i class="fa fa-trash"></i></a>
                   </div>`).appendTo(container);
                    }
                }
            },
            {
                dataField: "reV_STATUS",
                caption: "Rev Status",
                width: 140,
                alignment: "center",
                allowEditing: false,
                cellTemplate: function (container, options) {
                    if (options.data.reV_STATUS === undefined || options.data.reV_STATUS === null) {
                        options.data.reV_STATUS = "N";
                    }

                    const currentValue = options.data.reV_STATUS;
                    const isDisabled = (options.data.dT_CODE == null);

                    const btnGroupHTML = `
                            <div class="btn-group nrcBtnCommon nrc_btns${options.rowIndex}" role="group">
                                <button type="button" class="btn btn-sm btn_n${options.rowIndex}" data-value="N" ${isDisabled ? 'disabled' : ''}>N</button>
                                <button type="button" class="btn btn-sm btn_r${options.rowIndex}" data-value="R" ${isDisabled ? 'disabled' : ''}>R</button>
                                <button type="button" class="btn btn-sm btn_c${options.rowIndex}" data-value="C" ${isDisabled ? 'disabled' : ''}>C</button>
                            </div>
                            `;

                    $(btnGroupHTML).appendTo(container);

                    const color = options.data.reV_COLOR === "green" ? "#51bb25" : options.data.reV_COLOR === "red" ? "#dc3545" : "#055a87";

                    if (currentValue === "N") {
                        $(container).find(".btn_n" + options.rowIndex).css({
                            "background-color": "#055a87",
                            "color": "white"
                        });
                    } else if (currentValue === "R") {
                        $(container).find(".btn_r" + options.rowIndex).css({
                            "background-color": color,
                            "color": "white"
                        });
                    } else if (currentValue === "C") {
                        $(container).find(".btn_c" + options.rowIndex).css({
                            "background-color": color,
                            "color": "white"
                        });
                    }

                    if (!isDisabled) {
                        $(container).find('button').on('click', function () {

                            const newValue = $(this).data('value');

                            options.data.reV_STATUS = newValue;

                            if (newValue === "N") empr_VendorPricing.handleN(options.data, options.rowIndex);
                            else if (newValue === "R") empr_VendorPricing.handleR(options.data, options.rowIndex);
                            else if (newValue === "C") empr_VendorPricing.handleC(options.data, options.rowIndex);
                        });
                    }
                }
            },
            {
                dataField: "reV_TOGGLE",
                caption: "Rev Allow",
                width: 100,
                allowEditing: false,
                alignment: "center",
                cellTemplate: function (container, options) {

                    let isChecked = options.data.reV_TOGGLE === true;

                    let checkbox = $("<input>")
                        .addClass("form-check-input")
                        .attr("type", "checkbox")
                        .attr("id", "nrc_toggle" + options.rowIndex)
                        .prop("checked", isChecked)
                        .prop("disabled", true);

                    $("<div>")
                        .addClass("form-check form-switch toggle-rev-status")
                        .append(checkbox)
                        .appendTo(container);
                }
            },
            {
                dataField: 'reV_COLOR',
                visible: false,
            },
            {
                dataField: 'reV_REF',
                visible: false,
            },
            {
                dataField: 'supplieR_CODE',
                caption: 'Supplier',
                lookup: {
                    dataSource: Parties,
                    displayExpr: 'value',
                    valueExpr: 'customizedKey'
                },
                minWidth: 400,
                alignment: 'center',
                setCellValue: function (newData, value, currentRowData) {
                    newData.supplieR_CODE = value;
                    debugger;
                    const selectedParty = Parties.find(p => p.customizedKey === value);

                    if (selectedParty) {
                        newData.spartY_CODE = selectedParty.key;
                        newData.sact_CODE = selectedParty.accountCode;
                    }
                }
            },
            {
                dataField: 'yarN_CODE',
                caption: 'Yarn',
                lookup: {
                    dataSource: Yarn,
                    displayExpr: 'value',
                    valueExpr: 'key',
                    allowClearing: true
                },
                width: 300,
                alignment: 'center',
            },
            {
                dataField: 'comM_RATE',
                caption: 'Comm %',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                //allowEditing: false,
                width: 200,
                alignment: 'center'
            },
            {
                dataField: 'rate',
                caption: 'Rate',
                dataType: 'number',
                width: 200,
                alignment: 'center'
            },
            {
                dataField: 'dT_CODE',
                caption: 'Code',
                visible: false,
            },
            {
                dataField: 'spartY_CODE',
                visible: false,
            },
            {
                dataField: 'sact_CODE',
                visible: false,
            },
            {
                dataField: 'picK_ID',
                caption: 'Pick Id',
                visible: false
            },

        ];
        empr_helper.editableDxGridbindingForTransactions('#DetailContainer', col, dataSrc, "VendorPricing", "ordeR_NO", '', true, 600);
        if (dataSrc.length == 0) {
            $('#DetailContainer').dxDataGrid('instance').addRow().done(function () {
                $('#DetailContainer').dxDataGrid('instance').saveEditData();
            });
        }

    },
    CloneRow: function (uniqueKey) {
        console.log('existing key', uniqueKey);
        //debugger;
        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {

                empr_VendorPricing.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                var dataSource = gridInstance.option("dataSource");
                if (dataSource.length > 0) {
                    let clonedRowData = $.extend(true, {}, dataSource.find(x => x.__KEY__ === uniqueKey));
                    if (clonedRowData.hasOwnProperty('dT_CODE')) {
                        delete clonedRowData.dT_CODE;
                    }
                    var key = empr_VendorPricing.GenerateKey(36);
                    clonedRowData.__KEY__ = key;
                    console.log('new generated key', key);

                    clonedRowData.reV_STATUS = "N";
                    clonedRowData.reV_TOGGLE = false;

                    let newDataSource = [clonedRowData].concat(dataSource);
                    gridInstance.option("dataSource", newDataSource);
                    gridInstance.refresh();
                }
            });
        }
        else {
            empr_VendorPricing.rowsCount += 1;
            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            var dataSource = gridInstance.option("dataSource");
            if (dataSource.length > 0) {
                //let clonedRowData = $.extend(true, {}, dataSource[index]);
                let clonedRowData = $.extend(true, {}, dataSource.find(x => x.__KEY__ === uniqueKey));
                if (clonedRowData.hasOwnProperty('dT_CODE')) {
                    delete clonedRowData.dT_CODE;
                }
                var key = empr_VendorPricing.GenerateKey(36);
                clonedRowData.__KEY__ = key;
                console.log('new generated key', key);
                clonedRowData.reV_STATUS = "N";
                clonedRowData.reV_TOGGLE = false;

                let newDataSource = [clonedRowData].concat(dataSource);
                gridInstance.option("dataSource", newDataSource);
                gridInstance.refresh();
            }
        }
    },
    AddRow: function () {

        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                empr_VendorPricing.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                const dataSource = gridInstance.option("dataSource");

                dataSource.unshift({ __KEY__: empr_VendorPricing.GenerateKey(36) });
                gridInstance.option("dataSource", dataSource);
                gridInstance.refresh();
            });
        }
        else {
            empr_VendorPricing.rowsCount += 1;
            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            const dataSource = gridInstance.option("dataSource");

            dataSource.unshift({ __KEY__: empr_VendorPricing.GenerateKey(36) });
            gridInstance.option("dataSource", dataSource);
            gridInstance.refresh();
        }
    },

    DeleteRow: function (uniqueKey) {
        console.log('delete key', uniqueKey);
        //debugger;
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        var dataSource = gridInstance.option("dataSource");

        if (dataSource.length > 0) {
            if (dataSource.length > 1) {

                var rowIndex = dataSource.findIndex(x => x.__KEY__ === uniqueKey);
                if (rowIndex === -1) return; // row not found

                var row = dataSource[rowIndex];

                if (!row.dT_CODE) {

                    const gridInstance = $('#DetailContainer').dxDataGrid('instance');

                    let visibleRow = gridInstance.getVisibleRows().find(r => r.data.__KEY__ === uniqueKey);

                    if (visibleRow) {
                        gridInstance.deleteRow(visibleRow.rowIndex);
                    }

                    empr_VendorPricing.rowsCount -= 1;
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
                        ajaxHelper.ajaxPostJsonData({ code: row.dT_CODE }, "/VendorPricing/DeleteVendorPricingDetailByCode", function (data) {
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
    //InitFinishItemsDDL: function (selectedValue) {
    //    $.ajax({
    //        url: "DailyProduction/GetFinishItems",
    //        type: "GET",
    //        success: function (response) {
    //            empr_VendorPricing.BindDxDDL("FinishItem", response.data, selectedValue, "key", "value", "Select", function (d) {
    //                $('#FinishItem_Hidden').val(d.value)
    //                if (d.value == null) {
    //                    $('#FinishItem_Hidden').val('');
    //                }
    //            });
    //        }
    //    });
    //},
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
    //BindDxDDL: function (divId, data, selectedvalues, keyExp, dataField, placeholder, onvalueChangeFun) {
    //    ati_dxHelper.createDropdownSingle(divId, data, selectedvalues, keyExp, dataField, placeholder, onvalueChangeFun);
    //},
    InitQuickSearchGrid: function () {
        empr_VendorPricing.GetVendorPricing();
    },
    CreateQuickSearchGrid: function (dataSrc) {
        console.log('Quick', dataSrc);
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
                           <a href="javascript:;"  class="grid-action-icon elm_copy" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.traN_ID} title="COPY"><i class="fa fa-copy"></i></a>
                           </div>`).appendTo(container);
                }
            }
        },
        { dataField: 'traN_ID', caption: 'Code', alignment: 'center' },
        { dataField: 'v_DATE', caption: 'Voucher Date', dataType: 'date', format: 'dd-MM-yyy', alignment: 'center' },
        { dataField: 'astatus', caption: 'Status', alignment: 'center' },
        { dataField: 'voucheR_NO', caption: 'Voucher No', alignment: 'center', },
        { dataField: 'ref', caption: 'Reference No', alignment: 'center' },
        { dataField: 'clienT_PO', caption: 'Model# / PO#', alignment: 'center' },
        { dataField: 'partY_NAME', caption: 'Client', alignment: 'center' },
        { dataField: 'remarks', caption: 'Remarks', alignment: 'center' },
        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "DailyProductionQS");
    },
    GetVendorPricing: function () {
        ajaxHelper.ajaxGetJson('/VendorPricing/GetVendorPricings', function (data) {
            if (data.msgType == 1) {
                empr_VendorPricing.CreateQuickSearchGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetDataToSave: function () {

        var CODE = $("#Code").val();
        var ASTATUS = $('#ASTATUS').dxSelectBox('option', 'value');
        var V_DATE = $("#V_DATE").val();
        var CLIENT_PO = $("#CLIENT_PO").val();
        var JOBNO = $("#key_hidden").val();
        var REF = $("#REF").val();
        var REMARKS = $("#REMARKS").val();
        var PICK_ID = $("#pickTranId").val();
        var PARTY_CODE = $('#PARTY_CODE').dxSelectBox('option', 'value');

        var masterRecord = {
            TRAN_ID: CODE,
            ASTATUS: ASTATUS,
            V_DATE: V_DATE,
            CLIENT_PO: CLIENT_PO,
            REF: REF,
            PARTY_CODE: PARTY_CODE,
            REMARKS: REMARKS,
            JOBNO: JOBNO,
            PICK_ID: PICK_ID,
            //PICK_ID: empr_VendorPricing.pickId,
        }

        // validate data
        var detailRecords = [];
        //debugger;
        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
            });
        }
        else {
            detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
        }

        $.each(detailRecords, function (index, item) {
            if (!(item.shiP_DATE == "" || item.shiP_DATE == null || item.shiP_DATE == undefined)) {
                item.shiP_DATE = empr_helper.PrepareDate(item.shiP_DATE);
            }

            if (!(item.handoveR_DATE == "" || item.handoveR_DATE == null || item.handoveR_DATE == undefined)) {
                item.handoveR_DATE = empr_helper.PrepareDate(item.handoveR_DATE);
            }

            if (!(item.bookinG_DATE == "" || item.bookinG_DATE == null || item.bookinG_DATE == undefined)) {
                item.bookinG_DATE = empr_helper.PrepareDate(item.bookinG_DATE);
            }

        });

        if (empr_VendorPricing.rowsCount == detailRecords.length) {
            var modelRecord = {
                Master: masterRecord,
                Detail: detailRecords
            };
            return modelRecord;
        }
        else {
            var modelRecord = {
                Master: masterRecord,
                Detail: $('#DetailContainer').dxDataGrid('instance').option("dataSource")
            };
            return modelRecord;
        }
    },
    ValidateInfo: function () {
        debugger;
        var valid = true;
        var data = empr_VendorPricing.GetDataToSave();

        if (data.Master.PICK_ID == "" || data.Master.PICK_ID == null || data.Master.PICK_ID == undefined) {
            empr_helper.notify("You Cannot save entry without mapping data", 2);
            valid = false;
            return valid;
        }

        data.Detail = $('#DetailContainer').dxDataGrid('instance').option("dataSource");

        if (data.Detail.length == 0) {
            empr_helper.notify("Please add Order.", 2);
            valid = false;
            return valid;
        }
        $.each(data.Detail, function (index, item) {
            debugger;

            if (data.Detail.length == 1 && item.reV_STATUS == "C") {
                empr_helper.notify("At least one active row is required in details.", 2);
                valid = false;
                return valid;
            }

            if (item.spartY_CODE == "" || item.spartY_CODE == null || item.spartY_CODE == undefined) {
                empr_helper.notify("Please select Supplier at Line no " + (index + 1), 2);
                valid = false;
                return valid;
            }

            //if (item.reV_STATUS == "R") {
            //    if (item.reV_REF == "" || item.reV_REF == null || item.reV_REF == undefined || item.reV_REF <= 0) {
            //        empr_helper.notify("You must need to select S-NO# of Revised entry at Line no " + (index + 1), 2);
            //        valid = false;
            //        return valid;
            //    }
            //}

            //if (item.qty == "" || item.qty == null || item.qty == undefined) {
            //    empr_helper.notify("Please enter quantity at Line no " + (index + 1), 2);
            //    valid = false;
            //    return valid;
            //}

            //if (item.qty <= 0) {
            //    empr_helper.notify("Please enter correct item quantity at Line no " + (index + 1), 2);
            //    valid = false;
            //    return valid;
            //}

            //if (item.picK_ID <= 0 || item.picK_ID == "" || item.picK_ID == null || item.picK_ID == undefined) {
            //    empr_helper.notify("You Cannot add record without pick", 2);
            //    valid = false;
            //    return valid;
            //}

            //if (item.doc == "" || item.doc == null || item.doc == undefined) {
            //    empr_helper.notify("Please select Document Line no " + (index + 1), 2);
            //    valid = false;
            //    return valid;
            //}
        });

        return valid;
    },
    SaveInfo: function () {
        debugger;
        var dataModel = empr_VendorPricing.GetDataToSave();

        //var totalQty = (dataModel.Detail || []).reduce(function (sum, item) {
        //    return sum + (parseFloat(item.qty) || 0);
        //}, 0);
        //debugger;
        //var totalQty = (dataModel.Detail || []).reduce(function (sum, item) {
        //    if (item.reV_STATUS !== "C") {
        //        return sum + (parseFloat(item.qty) || 0);
        //    } else {
        //        return sum;
        //    }
        //}, 0);


        //if (totalQty > empr_VendorPricing.PickQty) {
        //    empr_helper.notify(`Quantity is greater then PO Qty. It should be equal or less then ${empr_VendorPricing.PickQty}`, 2);
        //    valid = false;
        //    return valid;
        //}

        console.log('SaveInfo', dataModel);
        if (dataModel.Master.TRAN_ID == 0
            || dataModel.Master.TRAN_ID == null
            || dataModel.Master.TRAN_ID == undefined
            || dataModel.Master.TRAN_ID == "") {
            dataModel.Detail.reverse();
        }

        ajaxHelper.ajaxPostJsonData(dataModel, "/VendorPricing/Save", function (data) {
            console.log('Save', data);
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                empr_helper.selectedBill = data.data.code;
                if (dataModel.Master.TRAN_ID == 0
                    || dataModel.Master.TRAN_ID == null
                    || dataModel.Master.TRAN_ID == undefined) {
                    $('#Code').val(data.data.code);
                    $('#VOUCHER_NO').val(data.data.voucherNo);
                }
                empr_VendorPricing.GetVendorPricingByCode(data.data.code);
                $('#BtnDelete').show();
            }
        }, false, true);
    },
    GetVendorPricingByCode: function (code) {
        ajaxHelper.ajaxGetJson('/VendorPricing/GetVendorPricingByCode?code=' + code, function (data) {
            console.log('edit', data);
            if (data.master.msgType == 1) {
                var masterData = data.master.data;
                var detailData = data.detail.data;
                if (masterData.length == 1) {
                    var response = masterData[0];
                    $('#Code').val(response.traN_ID);
                    empr_helper.selectedBill = response.traN_ID;
                    //empr_VendorPricing.pickId = detailData[0].picK_ID;
                    //empr_VendorPricing.PickQty = response.picK_QTY;
                    $('#ASTATUS').dxSelectBox('instance').option('value', response.astatus);
                    empr_VendorPricing.InitPOJobGridDDL(response.joB_NO);
                    $('#pickTranId').val(detailData[0].picK_ID);
                    $('#REF').val(response.ref);
                    $('#REMARKS').val(response.remarks);
                    $('#V_DATE').val(response.v_DATE);
                    $('#VOUCHER_NO').val(response.voucheR_NO);
                    $('#P_VOUCHER_NO').val(response.pvoucheR_NO);
                    $('#BtnBatchPick').prop('disabled', true);
                    empr_VendorPricing.InitPartyCodeDDL(response.partY_CODE);

                    if (Permissions != "Admin") {
                        if (Permissions.r_DLT) {
                            $('#BtnDelete').show();
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
                    }
                }

                if (data.detail.msgType == 1) {

                    if (data.detail.msgType == 1) {

                        data.detail.data.forEach(function (row) {
                            row.__KEY__ = empr_VendorPricing.GenerateKey(36);
                        });

                        empr_VendorPricing.CreateGrid(data.detail.data);
                        $('.card-body').addClass('customHighlightForModifiedCells');
                    }

                    //console.log('After Adding Key', data.detail.data);
                    //empr_VendorPricing.CreateGrid(data.detail.data);
                    //$('.card-body').addClass('customHighlightForModifiedCells');
                }
                else {
                    empr_helper.notify(data.msg, data.msgType);
                }
            }
            else {
                empr_helper.notify(data.master.msg, data.master.msgType);
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
            ajaxHelper.ajaxPostJsonData({ code: $('#Code').val() }, "/VendorPricing/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_VendorPricing.ResetForm();
                    $('#BtnDelete').hide();
                }
            }, false, true);
        });
    },
    InitBatchPickGrid: function () {
        debugger;

        var CLIENT_PO = $("#CLIENT_PO").val();
        var partyInstance = $('#PARTY_CODE').dxSelectBox('instance');
        var jobNo = $('#key_hidden').val();
        var selectedParty = partyInstance.option('selectedItem');
        var PARTY_CODE = selectedParty ? selectedParty.key : null;
        var ACT_CODE = selectedParty ? selectedParty.accountCode : null;

        if (CLIENT_PO && PARTY_CODE && ACT_CODE && jobNo) {
            empr_VendorPricing.GetBatchDetailByProcess(CLIENT_PO, PARTY_CODE, ACT_CODE, jobNo);
        }
        else {
            empr_helper.notify("Please select Model#, Client & Job first.", 2);
        }
    },
    GetBatchDetailByProcess: function (CLIENT_PO, PARTY_CODE, ACT_CODE, JOBNO) {

        var dataModel = {
            CLIENT_PO: CLIENT_PO,
            PARTY_CODE: PARTY_CODE,
            ACT_CODE: ACT_CODE,
            JOBNO: JOBNO
        }

        //ajaxHelper.ajaxGetJson('/VendorPricing/GetBatchDetailByProcess?clientPo=' + CLIENT_PO + "&partyCode=" + PARTY_CODE + "&actCode=" + ACT_CODE, function (data) {
        ajaxHelper.ajaxPostJsonData(dataModel, "/VendorPricing/GetBatchDetailByProcess", function (data) {
            debugger;
            if (data.msgType == 1) {
                if (data.data == '' || data.data == null || data.data == undefined) {
                    empr_helper.notify("Data not Mapped, Record not found with entered PO and Client.", 2);
                    

                } else {
                    empr_helper.notify("Data Mapped.", 1);

                    console.log('Pick Id', data.data);
                    $('#BtnBatchPick').prop('disabled', true);
                    $('#pickTranId').val(data.data);
                    $('#P_VOUCHER_NO').val(data.data2);
                }
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    //CreatePickGrid: function (dataSrc) {
    //    var col = [
    //        { dataField: 'v_DATE', caption: 'Date', dataType: 'date', allowEditing: false, format: 'dd-MM-yyy' }, // pick grid
    //        { dataField: 'voucheR_NO', caption: 'Voucher', allowEditing: false, },
    //        { dataField: 'iteM_NAME', caption: 'Item', allowEditing: false, width: 150, },
    //        { dataField: 'clienT_NAME', caption: 'Client', allowEditing: false, },
    //        { dataField: 'ref', caption: 'Ref', allowEditing: false, },
    //        { dataField: 'joB_NO', caption: 'Job #', allowEditing: false, },
    //        { dataField: 'ename', caption: 'Employee', allowEditing: false, },
    //        { dataField: 'deP_NAME', caption: 'Department', allowEditing: false, },
    //        { dataField: 'supplieR_NAME', caption: 'Supplier', allowEditing: false, width: 150, },
    //        { dataField: 'qty', caption: 'Quantity', allowEditing: false, },
    //        { dataField: 'uniT_NAME', caption: 'Unit', allowEditing: false, },
    //        { dataField: 'rate', caption: 'Rate', allowEditing: false, },
    //        { dataField: 'amt', caption: 'Amount', allowEditing: false, },
    //        { dataField: 'remarks', caption: 'Remarks', allowEditing: false, },
    //        { dataField: 'comM_TYPE', allowEditing: false, visible: false, },
    //        { dataField: 'comM_RATE', allowEditing: false, visible: false, },
    //        { dataField: 'comM_AMT', allowEditing: false, visible: false, },
    //        { dataField: 'status', allowEditing: false, visible: false, },
    //    ];
    //    empr_helper.dxGridbindingVouchers('#SodaPickGridContainer', col, dataSrc, "PurchaseBillPick", "single");
    //    setTimeout(function () {
    //        $('#SodaPickGridContainer').dxDataGrid('instance').resize();
    //    }, 500);
    //},
    //CreateBatchGrid: function (dataSrc) {
    //    var col = [
    //        { dataField: 'traN_ID', caption: 'Code', visible: false, },
    //        { dataField: 'process', caption: 'Process', allowEditing: false, },
    //        { dataField: 'iteM_NAME', caption: 'Item Name', allowEditing: false, },
    //        { dataField: 'batch', caption: 'Batch', allowEditing: false, },
    //        { dataField: 'uniT_NAME', caption: 'Unit', allowEditing: false },
    //        { dataField: 'iteM_CODE', caption: 'Item Code', visible: false, },
    //        { dataField: 'qty', caption: 'Batch Qty', allowEditing: false, },
    //        { dataField: 'baL_QTY', caption: 'B.Qty', allowEditing: false, visible: false },
    //        { dataField: 'unit', caption: 'Unit', allowEditing: false, visible: false },
    //        { dataField: 'mfG_DATE', caption: 'MFG Date', dataType: 'date', allowEditing: false, format: 'MM-yyy' },
    //        { dataField: 'exP_DATE', caption: 'EXP Date', dataType: 'date', allowEditing: false, format: 'MM-yyy' },
    //    ];
    //    empr_helper.editableDxGridbindingForTransactionsVouchers('#SodaPickGridContainer', col, dataSrc, "DailyProductionPick", "v_DATE", 'multiple');
    //    setTimeout(function () {
    //        $('#SodaPickGridContainer').dxDataGrid('instance').resize();
    //    }, 500);
    //},
    //AddBatchToDailyProduction: function () {
    //    if ($('#SodaPickGridContainer').dxDataGrid('instance').hasEditData()) {
    //        $('#SodaPickGridContainer').dxDataGrid('instance').saveEditData().done(function () {
    //            var data = empr_VendorPricing.GetDataToSave();
    //            var IsDataAvailableInGrid = false;
    //            $.each(data.Detail, function (index, item) {
    //                if (item.iteM_CODE != "" && item.iteM_CODE != null && item.iteM_CODE != undefined) {
    //                    IsDataAvailableInGrid = true;
    //                }
    //            });

    //            if (IsDataAvailableInGrid) {
    //                var existingData = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
    //                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
    //                var finalData = existingData.concat(selectedSodas);
    //                $('#DetailContainer').dxDataGrid('instance').option('dataSource', finalData);
    //            }
    //            else {
    //                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
    //                $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedSodas);
    //            }
    //            $('.modal').hide();
    //            $('#V_DATE').focus();
    //        });
    //    }
    //    else {
    //        var data = empr_VendorPricing.GetDataToSave();
    //        var IsDataAvailableInGrid = false;
    //        $.each(data.Detail, function (index, item) {
    //            if (item.iteM_CODE != "" && item.iteM_CODE != null && item.iteM_CODE != undefined) {
    //                IsDataAvailableInGrid = true;
    //            }
    //        });

    //        if (IsDataAvailableInGrid) {
    //            var existingData = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
    //            var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
    //            var finalData = existingData.concat(selectedSodas);
    //            $('#DetailContainer').dxDataGrid('instance').option('dataSource', finalData);
    //        }
    //        else {
    //            var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
    //            $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedSodas);
    //        }

    //        $('.modal').hide();
    //        $('#V_DATE').focus();
    //    }
    //},
    //SetData: function (dataSource) {
    //    $.each(dataSource, function (index, item) {
    //        item.chK1 = false;
    //        item.chk = "0";
    //        if (item.unit != '') {
    //            var selectedUnit = Units.filter(u => u.key == item.unit);
    //            if (selectedUnit.length > 0) {
    //                item.qtY2 = selectedUnit[0].qty;
    //            }
    //        }
    //        else {
    //            item.qtY2 = 0;
    //        }

    //        var qty = parseFloat(item.qty) || 0;
    //        var qtY2 = parseFloat(item.qtY2) || 1;
    //        var rate = parseFloat(item.rate) || 0;
    //        var rT_TYPE = parseFloat(item.rT_TYPE) || 0;
    //        if (!isNaN(qty) && !isNaN(qtY2)) {
    //            if (item.chK1) {
    //                item.baL_QTY = qty * qtY2;
    //                if (empr_VendorPricing.Branch_RT_TYPE == 'Y') {
    //                    item.amt = item.baL_QTY * rate;
    //                    var perRate = parseFloat(rate / rT_TYPE) || 0;
    //                    if (perRate > -1 && perRate != 'Infinity') {
    //                        item.amt = (item.baL_QTY * perRate).toFixed(2);
    //                    }
    //                }

    //                if (empr_VendorPricing.Branch_RT_TYPE == 'N') {
    //                    item.amt = qty * rate;
    //                }
    //                item.neT_AMT = item.amt;
    //            }
    //            else {
    //                item.baL_QTY = qty;
    //                if (empr_VendorPricing.Branch_RT_TYPE == 'Y') {
    //                    var perRate = parseFloat(rate / rT_TYPE) || 0;
    //                    if (perRate > -1 && perRate != 'Infinity') {
    //                        item.amt = (item.baL_QTY * perRate).toFixed(2);
    //                    }
    //                }

    //                if (empr_VendorPricing.Branch_RT_TYPE == 'N') {
    //                    item.amt = qty * rate;
    //                }
    //                item.neT_AMT = item.amt;
    //            }
    //        }

    //        item.__KEY__ = empr_VendorPricing.GenerateKey(36);
    //    });

    //    return dataSource;
    //},

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

    //InitPOJobGridDDL: function (_selectedValue) {
    //    console.log(_selectedValue);

    //    ajaxHelper.ajaxGetJson('/VendorPricing/POJobsDropdown', function (data) {
    //        console.log('InitPOJobGridDDL', data);
    //        if (data.msgType == 1) {
    //            var Datasource = data.data;
    //            console.log('Datasource', Datasource);
    //            selectedObject = [];
    //            selectedValue = _selectedValue;

    //            if (_selectedValue != null) {
    //                selectedObject = Datasource.filter(x => { return x.key == _selectedValue }) || [];
    //                if (selectedObject.length > 0) {
    //                    selectedValue = selectedObject[0].key;

    //                    $('#key_hidden').val(selectedObject[0].key);
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
    //                        const selectedData = gridInstance.getDataSource().items().find(item => item.key === e.value);
    //                        if (selectedData) {
    //                            $('#key_hidden').val(selectedData.key);
    //                            $('#CLIENT_PO').val(selectedData.clientPo);
    //                            $('#PARTY_CODE').val(selectedData.partyName);
    //                        }
    //                    } else {
    //                        $('#key_hidden').val('');
    //                        $('#CLIENT_PO').val('');
    //                        $('#PARTY_CODE').val('');
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

    //                                $('#key_hidden').val(selected.key);
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

    handleN: function (rowData, rowIndex) {
        console.log("N clicked", rowData);

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

    GeneratePrintReport: function () {

        empr_VendorPricing.InitReportTypeDDL();
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
        ajaxHelper.ajaxPostJsonData(dataModel, "/VendorPricing/GetPrintReport", function (data) {
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
    InitPOJobGridDDL: function (_selectedValue) {
        console.log(_selectedValue);

        ajaxHelper.ajaxGetJson('/VendorPricing/POJobsDropdown', function (data) {
            console.log('InitPOJobGridDDL', data);
            if (data.msgType == 1) {
                var Datasource = data.data;
                console.log('Datasource', Datasource);
                selectedObject = [];
                selectedValue = _selectedValue;
                debugger;
                if (_selectedValue != null) {
                    selectedObject = Datasource.filter(x => { return x.key == _selectedValue }) || [];
                    if (selectedObject.length > 0) {
                        selectedValue = selectedObject[0].key;

                        $('#key_hidden').val(selectedObject[0].key);
                        $('#CLIENT_PO').val(selectedObject[0].clientPo);
                        $('#PARTY_CODE').val(selectedObject[0].partyName);
                        empr_VendorPricing.InitPartyCodeDDL(`${selectedObject[0].partyCode}${selectedObject[0].accountCode}`);
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
                            debugger;
                            const selectedData = gridInstance.getDataSource().items().find(item => item.key === e.value);
                            if (selectedData) {
                                $('#key_hidden').val(selectedData.key);
                                $('#CLIENT_PO').val(selectedData.clientPo);
                                $('#PARTY_CODE').val(selectedData.partyName);
                                empr_VendorPricing.InitPartyCodeDDL(`${selectedData.partyCode}${selectedData.accountCode}`);
                            }
                        } else {
                            $('#key_hidden').val('');
                            $('#CLIENT_PO').val('');
                            $('#PARTY_CODE').val('');
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
    InitReportTypeDDL: function (selectedValue) {
        ajaxHelper.ajaxGetJson("/VendorPricing/GetReportTypes", function (data) {
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

    InitPartyCodeDDL: function (selectedValue) {
        $('#PARTY_CODE').dxSelectBox({
            dataSource: {
                store: Parties,
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
            //disabled: true
        });
    },
}