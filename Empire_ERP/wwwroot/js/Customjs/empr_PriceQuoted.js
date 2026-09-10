var empr_PriceQuoted = {
    totalCount: 0,
    rowsCount: 0,
    parties: [],
    PartiesData: [],
    pickId: 0,
    PickQty: 0,
    //BtnBatchPick SodaPickGridContainer
    //BtnAddBatch
    InitEvents: function () {
        $(document).ready(function () {
            empr_PriceQuoted.InitReportTypeDDL();
            empr_PriceQuoted.InitPartyCodeDDL();
            empr_PriceQuoted.InitPOJobGridDDL(null);
            empr_PriceQuoted.ResetForm();

            window.addEventListener('message', function (event) {
                if (event.origin !== window.location.origin) {
                    return;
                }
                var data = event.data;
                if (data && data.traN_ID) {
                    $('#Code').val(data.traN_ID);
                    empr_PriceQuoted.GetPriceQuotedByCode(data.traN_ID);
                }
            });

            //ajaxHelper.ajaxGetJson("/DeliveryFeeding/GetParties", function (data) {
            //    if (data.msgType == 1) {
            //        empr_PriceQuoted.PartiesData = data.data;
            //        empr_PriceQuoted.InitPartyCodeDDL(data.data);
            //        parties = data.data;
            //    }
            //    else {
            //        empr_helper.notify(data.data, data.msgType);
            //    }
            //}, false, true);

            $('body').on('click', '#BtnQuickSearch', function () {
                empr_PriceQuoted.InitQuickSearchGrid();
            });

            $('body').on('click', '.elm_edit', function () {
                var id = $(this).attr("reportid");
                $('#Code').val(id);
                $('.vHide').show();
                $('.modal').modal('hide');
                empr_PriceQuoted.GetPriceQuotedByCode(id);

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
                ajaxHelper.ajaxPostJsonData({ traN_ID: empr_helper.selectedBill, v_DATE: $('#updatedDate').val() }, "/PriceQuoted/CopyRecord", function (data) {
                    empr_helper.notify(data.msg, data.msgType);
                    if (data.msgType == 1) {
                        $('.modal').modal('hide');
                        empr_PriceQuoted.GetPriceQuotedByCode(data.data.code);
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
                        if (empr_PriceQuoted.ValidateInfo()) {
                            empr_PriceQuoted.SaveInfo();
                        }
                        setTimeout(function () {
                            $("#Loader").hide();
                        }, 500);
                    }
                } else {
                    if (empr_PriceQuoted.ValidateInfo()) {
                        empr_PriceQuoted.SaveInfo();
                    }
                    setTimeout(function () {
                        $("#Loader").hide();
                    }, 500);
                }
            });

            $('body').on('click', '#BtnNew', function () {
                empr_PriceQuoted.ResetForm();
            });

            $('body').on('click', '#BtnDelete', function () {
                empr_PriceQuoted.Delete();
            });

            $('body').on('click', '#BtnPrint, #BtnGenerateReport', function () {
                empr_PriceQuoted.GeneratePrintReport();
            });

            $('body').on('click', '.elm_print', function () {
                empr_helper.selectedBill = $(this).attr("reportid");
                empr_PriceQuoted.GeneratePrintReport();
            });

            $('body').on('click', '#BtnBatchPick', function () { // first  
                empr_PriceQuoted.InitBatchPickGrid();
            });

            $('body').on('click', '#BtnAddBatch', function () { // second
                //debugger;
                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                if (selectedSodas.length > 0) {

                    empr_PriceQuoted.AddPickRecordToDetail();

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

    AddPickRecordToDetail: function () {

        if ($('#SodaPickGridContainer').dxDataGrid('instance').hasEditData()) {
            $('#SodaPickGridContainer').dxDataGrid('instance').saveEditData().done(function () {
                var data = empr_PriceQuoted.GetDataToSave();
                var IsDataAvailableInGrid = false;
                $.each(data, function (index, item) {
                    if (item.supplieR_CODE != "" && item.supplieR_CODE != null && item.supplieR_CODE != undefined || item.yarN_CODE != "" && item.yarN_CODE != null && item.yarN_CODE != undefined) {
                        IsDataAvailableInGrid = true;
                    }
                });
                if (IsDataAvailableInGrid || empr_PriceQuoted.firstClick == 1) {
                    var existingData = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
                    var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                    var finalData = existingData.concat(selectedSodas);

                    finalData.forEach(function (row) {
                        row.__KEY__ = empr_PriceQuoted.GenerateKey(36);
                    });

                    $('#DetailContainer').dxDataGrid('instance').option('dataSource', finalData);
                }
                else {
                    var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                    selectedSodas.forEach(function (row) {
                        row.__KEY__ = empr_PriceQuoted.GenerateKey(36);
                    });

                    $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedSodas);
                }
                $('.modal').hide();
            });
        }
        else {
            var grid = $('#DetailContainer').dxDataGrid('instance');
            grid.saveEditData();
            var data = grid.option('dataSource');

            var IsDataAvailableInGrid = false;
            $.each(data, function (index, item) {
                if (item.supplieR_CODE != "" && item.supplieR_CODE != null && item.supplieR_CODE != undefined || item.yarN_CODE != "" && item.yarN_CODE != null && item.yarN_CODE != undefined) {
                    IsDataAvailableInGrid = true;
                }
            });

            if (IsDataAvailableInGrid || empr_PriceQuoted.firstClick == 1) {
                debugger;
                var existingData = $('#DetailContainer').dxDataGrid('instance').option('dataSource') || [];
                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();

                var finalData = existingData.concat(selectedSodas);

                finalData.forEach(function (row) {
                    row.__KEY__ = empr_PriceQuoted.GenerateKey(36);
                });

                $('#DetailContainer').dxDataGrid('instance').option('dataSource', finalData);

            }
            else {
                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                console.log('selectedSodas', selectedSodas);

                selectedSodas.forEach(function (row) {
                    row.__KEY__ = empr_PriceQuoted.GenerateKey(36);
                });

                $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedSodas);

            }
            $('.modal').hide();
        }
    },

    ResetForm: function () {
        empr_PriceQuoted.CreateGrid([{ __KEY__: empr_PriceQuoted.GenerateKey(36) }]);
        empr_PriceQuoted.pickId = 0;
        $('.Record input').not('#ASTATUS, .dx-texteditor-input, #V_DATE').val('');
        $('#REMARKS').val('');
        $('#COMM').val('');
        $('#key_hidden').val('');

        $('#BtnDelete').hide();
        $('#CLIENT_PO').val('');
        empr_PriceQuoted.InitPartyCodeDDL();
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
        empr_PriceQuoted.CreateGrid([{ __KEY__: empr_PriceQuoted.GenerateKey(36) }]);
        if ($('#FinishItem').data('dxSelectBox') != null) {
            $('#FinishItem').dxSelectBox('instance').dispose();
        }
        //empr_PriceQuoted.InitPartyCodeDDL();
        //$('#CLIENT_PO').prop('disabled', false);
    },
    CreateGrid: function (dataSrc) {
        console.log('CreateGrid', dataSrc);

        if (dataSrc.length > 0) {
            empr_PriceQuoted.rowsCount = dataSrc.length - 1;
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
                            : `<a href="javascript:;" class="grid-action-icon Clone" onclick="empr_PriceQuoted.CloneRow('${options.data.__KEY__}')" title="Duplicate"><i class="fa fa-clone"></i></a>`;
                        //const addAction = (!Permissions.r_ADD && !Permissions.r_EDIT)
                        //    ? ''
                        //    : `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_PriceQuoted.AddRow()" title="Add"><i class="fa fa-add"></i></a>`;
                        const deleteAction = !Permissions.r_DLT
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 18px" onclick="empr_PriceQuoted.DeleteRow('${options.data.__KEY__}')" title="Delete"><i class="fa fa-trash"></i></a>`;
                        const actions = `<div class="btn-group btn-group-sm">${copyAction}${deleteAction}</div>`;
                        $(actions).appendTo(container);
                    } else {
                        //console.log('insideGrid',options.data);
                        $(`<div class="btn-group btn-group-sm">
                   <a href="javascript:;" class="grid-action-icon" onclick="empr_PriceQuoted.CloneRow('${options.data.__KEY__}')" title="Duplicate"><i class="fa fa-clone"></i></a>
                   <a href="javascript:;" class="grid-action-icon" style="margin-left: 18px" onclick="empr_PriceQuoted.DeleteRow('${options.data.__KEY__}')" title="Delete"><i class="fa fa-trash"></i></a>
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

                            if (newValue === "N") empr_PriceQuoted.handleN(options.data, options.rowIndex);
                            else if (newValue === "R") empr_PriceQuoted.handleR(options.data, options.rowIndex);
                            else if (newValue === "C") empr_PriceQuoted.handleC(options.data, options.rowIndex);
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
                //width: 400,
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
                dataField: 'pQ_DATE',
                caption: 'Price Quoted Date',
                dataType: 'date',
                format: 'dd-MM-yyyy',
                alignment: 'center',
                width: 200,
                setCellValue: function (newData, value) {
                    if (value) {
                        var date = new Date(value);
                        var year = date.getFullYear();
                        if (year < 100) year += 2000;
                        newData.pQ_DATE = new Date(year, date.getMonth(), date.getDate());
                    } else {
                        newData.pQ_DATE = null;
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
        empr_helper.editableDxGridbindingForTransactions('#DetailContainer', col, dataSrc, "PriceQuoted", "ordeR_NO", '', true, 600);
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

                empr_PriceQuoted.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                var dataSource = gridInstance.option("dataSource");
                if (dataSource.length > 0) {
                    let clonedRowData = $.extend(true, {}, dataSource.find(x => x.__KEY__ === uniqueKey));
                    if (clonedRowData.hasOwnProperty('dT_CODE')) {
                        delete clonedRowData.dT_CODE;
                    }
                    var key = empr_PriceQuoted.GenerateKey(36);
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
            empr_PriceQuoted.rowsCount += 1;
            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            var dataSource = gridInstance.option("dataSource");
            if (dataSource.length > 0) {
                //let clonedRowData = $.extend(true, {}, dataSource[index]);
                let clonedRowData = $.extend(true, {}, dataSource.find(x => x.__KEY__ === uniqueKey));
                if (clonedRowData.hasOwnProperty('dT_CODE')) {
                    delete clonedRowData.dT_CODE;
                }
                var key = empr_PriceQuoted.GenerateKey(36);
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
                empr_PriceQuoted.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                const dataSource = gridInstance.option("dataSource");

                dataSource.unshift({ __KEY__: empr_PriceQuoted.GenerateKey(36) });
                gridInstance.option("dataSource", dataSource);
                gridInstance.refresh();
            });
        }
        else {
            empr_PriceQuoted.rowsCount += 1;
            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            const dataSource = gridInstance.option("dataSource");

            dataSource.unshift({ __KEY__: empr_PriceQuoted.GenerateKey(36) });
            gridInstance.option("dataSource", dataSource);
            gridInstance.refresh();
        }
    },

    DeleteRow: function (uniqueKey) {
        console.log('delete key', uniqueKey);
        debugger;
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

                    empr_PriceQuoted.rowsCount -= 1;
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
                        ajaxHelper.ajaxPostJsonData({ code: row.dT_CODE }, "/PriceQuoted/DeletePriceQuotedDetailByCode", function (data) {
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
    //            empr_PriceQuoted.BindDxDDL("FinishItem", response.data, selectedValue, "key", "value", "Select", function (d) {
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
        empr_PriceQuoted.GetPriceQuoted();
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
    GetPriceQuoted: function () {
        ajaxHelper.ajaxGetJson('/PriceQuoted/GetPriceQuoteds', function (data) {
            if (data.msgType == 1) {
                empr_PriceQuoted.CreateQuickSearchGrid(data.data);
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
            PICK_ID: empr_PriceQuoted.pickId,
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
            if (!(item.pQ_DATE == "" || item.pQ_DATE == null || item.pQ_DATE == undefined)) {
                item.pQ_DATE = empr_helper.PrepareDate(item.pQ_DATE);
            }
        });

        if (empr_PriceQuoted.rowsCount == detailRecords.length) {
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
        var data = empr_PriceQuoted.GetDataToSave();

        //if (data.Master.CLIENT_PO == "" || data.Master.CLIENT_PO == null || data.Master.CLIENT_PO == undefined) {
        //    empr_helper.notify("Model# is empty", 2);
        //    valid = false;
        //    return valid;
        //}

        data.Detail = $('#DetailContainer').dxDataGrid('instance').option("dataSource");

        //if (data.Detail.length == 0) {
        //    empr_helper.notify("Please add Order.", 2);
        //    valid = false;
        //    return valid;
        //}
        $.each(data.Detail, function (index, item) {
            debugger;

            if (data.Detail.length == 1 && item.reV_STATUS == "C") {
                empr_helper.notify("At least one active row is required in details.", 2);
                valid = false;
                return valid;
            }

            if (item.picK_ID == "" || item.picK_ID == null || item.picK_ID == undefined) {
                empr_helper.notify("You Cannot save entry without pick data at Line no " + (index + 1), 2);
                valid = false;
                return valid;
            }

            if (item.pQ_DATE == "" || item.pQ_DATE == null || item.pQ_DATE == undefined) {
                empr_helper.notify("Please select Price Quotated Date at Line no " + (index + 1), 2);
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
        var dataModel = empr_PriceQuoted.GetDataToSave();

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


        //if (totalQty > empr_PriceQuoted.PickQty) {
        //    empr_helper.notify(`Quantity is greater then PO Qty. It should be equal or less then ${empr_PriceQuoted.PickQty}`, 2);
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

        ajaxHelper.ajaxPostJsonData(dataModel, "/PriceQuoted/Save", function (data) {
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
                empr_PriceQuoted.GetPriceQuotedByCode(data.data.code);
                $('#BtnDelete').show();
            }
        }, false, true);
    },
    GetPriceQuotedByCode: function (code) {
        ajaxHelper.ajaxGetJson('/PriceQuoted/GetPriceQuotedByCode?code=' + code, function (data) {
            console.log('edit', data);
            if (data.master.msgType == 1) {
                var masterData = data.master.data;
                var detailData = data.detail.data;
                if (masterData.length == 1) {
                    var response = masterData[0];
                    $('#Code').val(response.traN_ID);
                    empr_helper.selectedBill = response.traN_ID;
                    empr_PriceQuoted.pickId = detailData[0].picK_ID;
                    //empr_PriceQuoted.PickQty = response.picK_QTY;
                    $('#ASTATUS').dxSelectBox('instance').option('value', response.astatus);
                    $('#REF').val(response.ref);
                    $('#REMARKS').val(response.remarks);
                    $('#V_DATE').val(response.v_DATE);
                    $('#VOUCHER_NO').val(response.voucheR_NO);
                    $('#CLIENT_PO').val(response.clienT_PO);
                    empr_PriceQuoted.InitPOJobGridDDL(response.joB_NO);
                    empr_PriceQuoted.InitPartyCodeDDL(response.partY_CODE);

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
                            row.__KEY__ = empr_PriceQuoted.GenerateKey(36);
                        });

                        empr_PriceQuoted.CreateGrid(data.detail.data);
                        $('.card-body').addClass('customHighlightForModifiedCells');
                    }

                    //console.log('After Adding Key', data.detail.data);
                    //empr_PriceQuoted.CreateGrid(data.detail.data);
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
            ajaxHelper.ajaxPostJsonData({ code: $('#Code').val() }, "/PriceQuoted/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_PriceQuoted.ResetForm();
                    $('#BtnDelete').hide();
                }
            }, false, true);
        });
    },

    InitBatchPickGrid: function () {
        debugger;

        var CLIENT_PO = $("#CLIENT_PO").val();
        var partyInstance = $('#PARTY_CODE').dxSelectBox('instance');
        var JOBNO = $('#key_hidden').val();
        var selectedParty = partyInstance.option('selectedItem');
        var PARTY_CODE = selectedParty ? selectedParty.key : null;
        var ACT_CODE = selectedParty ? selectedParty.accountCode : null;

        if (CLIENT_PO && PARTY_CODE && ACT_CODE && JOBNO) {
            empr_PriceQuoted.GetBatchDetailByProcess(CLIENT_PO, PARTY_CODE, ACT_CODE, JOBNO);
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

        //ajaxHelper.ajaxGetJson('/PriceQuoted/GetBatchDetailByProcess?clientPo=' + CLIENT_PO + "&partyCode=" + PARTY_CODE + "&actCode=" + ACT_CODE, function (data) {
        ajaxHelper.ajaxPostJsonData(dataModel, "/PriceQuoted/GetBatchDetailByProcess", function (data) {
            if (data.msgType == 1) {
                if (data.data.length > 0) {
                    if ($('#SodaPickGridContainer').data('dxDataGrid') != undefined) {
                        $('#SodaPickGridContainer').data('dxDataGrid').dispose();
                    }
                    empr_PriceQuoted.CreatePickGrid(data.data);
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
            { dataField: 'clienT_NAME', caption: 'Client', allowEditing: false, },
            { dataField: 'clienT_PO', caption: 'Model#', allowEditing: false, },
            { dataField: 'ref', caption: 'Ref', allowEditing: false, },
            { dataField: 'remarks', caption: 'Remarks', allowEditing: false, },

            { dataField: 'supplier', caption: 'Supplier', allowEditing: false, width: 150, },
            { dataField: 'yarn', caption: 'Yarn', allowEditing: false },
            { dataField: 'comM_RATE', caption: 'Comm %', allowEditing: false },
            //{ dataField: 'comM_RATE', caption: 'Comm %', allowEditing: false },
            { dataField: 'rate', caption: 'Rate', allowEditing: false },
        ];

        //empr_helper.dxGridbindingVouchers('#SodaPickGridContainer', col, dataSrc, "PurchaseBillPick", "single");
        empr_helper.editableDxGridbinding('#SodaPickGridContainer', col, dataSrc, "SodaPickDetailGrid");
        setTimeout(function () {
            $('#SodaPickGridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
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
    //            var data = empr_PriceQuoted.GetDataToSave();
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
    //        var data = empr_PriceQuoted.GetDataToSave();
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
    //                if (empr_PriceQuoted.Branch_RT_TYPE == 'Y') {
    //                    item.amt = item.baL_QTY * rate;
    //                    var perRate = parseFloat(rate / rT_TYPE) || 0;
    //                    if (perRate > -1 && perRate != 'Infinity') {
    //                        item.amt = (item.baL_QTY * perRate).toFixed(2);
    //                    }
    //                }

    //                if (empr_PriceQuoted.Branch_RT_TYPE == 'N') {
    //                    item.amt = qty * rate;
    //                }
    //                item.neT_AMT = item.amt;
    //            }
    //            else {
    //                item.baL_QTY = qty;
    //                if (empr_PriceQuoted.Branch_RT_TYPE == 'Y') {
    //                    var perRate = parseFloat(rate / rT_TYPE) || 0;
    //                    if (perRate > -1 && perRate != 'Infinity') {
    //                        item.amt = (item.baL_QTY * perRate).toFixed(2);
    //                    }
    //                }

    //                if (empr_PriceQuoted.Branch_RT_TYPE == 'N') {
    //                    item.amt = qty * rate;
    //                }
    //                item.neT_AMT = item.amt;
    //            }
    //        }

    //        item.__KEY__ = empr_PriceQuoted.GenerateKey(36);
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

    //    ajaxHelper.ajaxGetJson('/PriceQuoted/POJobsDropdown', function (data) {
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

        empr_PriceQuoted.InitReportTypeDDL();
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
        ajaxHelper.ajaxPostJsonData(dataModel, "/PriceQuoted/GetPrintReport", function (data) {
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
        ajaxHelper.ajaxGetJson("/PriceQuoted/GetReportTypes", function (data) {
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
    InitPOJobGridDDL: function (_selectedValue) {
        console.log(_selectedValue);

        ajaxHelper.ajaxGetJson('/PriceQuoted/POJobsDropdown', function (data) {
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
                        empr_PriceQuoted.InitPartyCodeDDL(`${selectedObject[0].partyCode}${selectedObject[0].accountCode}`);
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
                                empr_PriceQuoted.InitPartyCodeDDL(`${selectedData.partyCode}${selectedData.accountCode}`);
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
}