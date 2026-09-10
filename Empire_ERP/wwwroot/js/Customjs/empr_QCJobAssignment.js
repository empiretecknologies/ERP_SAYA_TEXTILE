var empr_QCJobAssignment = {
    InitEvents: function () {
        $(document).ready(function () {
            empr_QCJobAssignment.InitGrid();
        });

        $('a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
            var target = $(e.target).attr("href");
            if (target === '#pills-warninghome') {
                $('#tab1Wrapper').show();
                $('#tab2Wrapper').hide();
            }
            if (target === '#pills-warningcontact') {
                $('#tab1Wrapper').hide();
                $('#tab2Wrapper').show();
            }
        });


        $('body').on('click', '#BtnSave', function () {
            empr_QCJobAssignment.Save();
        });

        if (Permissions != "Admin") {
            (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            !Permissions.r_VIEW && $('#gridContainer').hide();
        }
    },
    InitGrid: function () {
        empr_QCJobAssignment.GetQCJobAssignments();
    },
    GetQCJobAssignments: function () {

        ajaxHelper.ajaxGetJson(`/QCJobAssignment/GetQCJobAssignments`, function (data) {
            if (data.msgType == 1) {
                console.log('QCJobAssignments', data);
                empr_QCJobAssignment.CreateGrid(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

    CreateGrid: function (dataSrc) {

        var col = [
            //{
            //    dataField: 't_SIZES',
            //    caption: 'AQL',
            //    lookup: {
            //        dataSource: TextileSizes,
            //        displayExpr: 'value',
            //        valueExpr: 'key'
            //    },
            //    width: 120,
            //    alignment: 'center',
            //},
            { dataField: 'picK_ID', caption: 'id', visible: false, allowEditing: false },
            { dataField: 'voucheR_NO', caption: 'Transaction #', width: 150, allowEditing: false, alignment: 'center' },
            { dataField: 'clienT_PO', caption: 'Model# / PO#', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'partY_NAME', caption: 'Party' , width: 170, allowEditing: false, alignment: 'center' },
            { dataField: 'supplier', caption: 'Supplier', width: 250, allowEditing: false, alignment: 'center' },
            { dataField: 'joB_NAME', caption: 'Job #', width: 120, allowEditing: false, alignment: 'center' },

            { dataField: 'ordeR_NO', caption: 'Order #', width: 120, allowEditing: false, alignment: 'center' },
            {
                dataField: 'color',
                caption: 'Color',
                lookup: {
                    dataSource: Colors,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 170,
                alignment: 'center',
                allowEditing: false
            },
            {
                dataField: 'size',
                caption: 'Size',
                lookup: {
                    dataSource: Sizes,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 170,
                alignment: 'center',
                allowEditing: false
            },
            { dataField: 'qty', caption: 'Qty', width: 100, alignment: 'right', allowEditing: false },
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
            { dataField: 'rate', caption: 'Rate', allowEditing: false, alignment: 'right', width: 120 },
            {
                dataField: 'amt', caption: 'Amt', allowEditing: false,
                alignment: 'right',
                dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                },
                width: 120
            },
            {
                dataField: 'handoveR_DATE',
                caption: 'HandOver Date',
                dataType: 'date',
                format: 'dd-MM-yyyy',
                alignment: 'center',
                width: 120,
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
                dataField: 'shiP_DATE',
                caption: 'Ship Date',
                dataType: 'date',
                format: 'dd-MM-yyyy',
                alignment: 'center',
                width: 120,
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
                dataField: 'port',
                caption: 'Port',
                lookup: {
                    dataSource: Ports,
                    displayExpr: 'value',
                    valueExpr: 'key'
                },
                width: 120,
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
            { dataField: 'dT_DESC', caption: 'Desc', allowEditing: false, width: 120 },
            {
                dataField: 'intake',
                caption: 'Intake#',
                dataType: 'number',
                width: 120,
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
                width: 120,
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

            { dataField: 'carton', caption: 'Carton', allowEditing: false },
            { dataField: 'totaL_CARTON', caption: 'T.Carton', allowEditing: false },
            
        ];


        empr_helper.dxGridForQCForm('#gridContainer', col, dataSrc[0].unApprovedData, 'UnApproved', 'multi');

        empr_QCJobAssignment.createApprovedGrid(dataSrc[0].approvedData);
    },

    createApprovedGrid: function (dataSrc) {

        var col = [
            { dataField: 'ordeR_NO', caption: 'Order #', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'aql', caption: 'AQL', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'item', caption: 'Item', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'color', caption: 'Color', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'size', caption: 'Size', width: 120, allowEditing: false, alignment: 'center' },

            {
                dataField: 'qty', caption: 'Qty', width: 120, allowEditing: false, alignment: 'center', dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                }, },
            { dataField: 'unit', caption: 'Unit', width: 120, allowEditing: false, alignment: 'center' },

            {
                dataField: 'rate', caption: 'Rate', width: 120, allowEditing: false, alignment: 'center', dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                }, },
            {
                dataField: 'amt', caption: 'Amt', width: 120, allowEditing: false, alignment: 'center', dataType: 'number',
                format: {
                    type: 'fixedPoint',
                    precision: 2
                }, },

            { dataField: 'handoveR_DATE', caption: 'Handover Date', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'shiP_DATE', caption: 'Ship Date', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'bookinG_DATE', caption: 'Booking Date', width: 120, allowEditing: false, alignment: 'center' },

            { dataField: 'port', caption: 'Port', width: 120, allowEditing: false, alignment: 'center' },

            { dataField: 'dT_DESC', caption: 'Desc', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'entity', caption: 'Entity', width: 120, allowEditing: false, alignment: 'center' },

            { dataField: 'grade', caption: 'Grade', width: 120, allowEditing: false, alignment: 'center' },
            { dataField: 'season', caption: 'Season', width: 120, allowEditing: false, alignment: 'center' },

            { dataField: 'carton', caption: 'Carton', allowEditing: false },
            { dataField: 'totaL_CARTON', caption: 'T.Carton', allowEditing: false },
        ];

        empr_helper.dxGridForQCForm('#ApprovedgridContainer', col, dataSrc, 'Approved', 'single');
    },
    ValidateInfo: function () {

        var valid = true;
        var CreditAcc = $("#bookType2").dxSelectBox('option', 'value');
        var DebitAcc = $("#bookType1").dxSelectBox('option', 'value');

        if (CreditAcc == "" || CreditAcc == null || CreditAcc == undefined) {
            empr_helper.notify("Please select Credit Account.", 2);
            valid = false;
        }
        if (DebitAcc == "" || DebitAcc == null || DebitAcc == undefined) {
            empr_helper.notify("Please select Debit Account.", 2);
            valid = false;
        }

        $("#Loader").hide();
        return valid;
    },

    GetData: function () {
        var gridInstance = $('#gridContainer').dxDataGrid('instance');

        gridInstance.saveEditData();

        var detailRecords = gridInstance.getSelectedRowsData();

        return detailRecords;
    },

    ValidateData: function (data) {
        if (!data || data.length === 0) {
            return "Select atleast one record ...!";
        }

        //for (var i = 0; i < data.length; i++) {
        //    var record = data[i];

        //    if (record.t_SIZES === null || record.t_SIZES === undefined || record.t_SIZES === "" || record.t_SIZES === 0) {
        //        return "Please select AQL for all selected records...!";
        //    }
        //}

        return "";
    },

    Save: function () {
        var data = empr_QCJobAssignment.GetData();

        var validationMsg = empr_QCJobAssignment.ValidateData(data);
        if (validationMsg !== "") {
            empr_helper.notify(validationMsg, 2); 
            return;
        }
        var postData = {
            Master: data
        }
        ajaxHelper.ajaxPostJsonData(postData, "/QCJobAssignment/Save", function (data) {
            empr_helper.notify(data.msg, data.msgType);

            if (data.msgType == 1) {
                // 🔥 STEP 1: Dispose UnApproved Grid
                var grid1 = $('#gridContainer').data('dxDataGrid');
                if (grid1) {
                    grid1.dispose();
                    $('#gridContainer').empty();
                }

                // 🔥 STEP 2: Dispose Approved Grid
                var grid2 = $('#ApprovedgridContainer').data('dxDataGrid');
                if (grid2) {
                    grid2.dispose();
                    $('#ApprovedgridContainer').empty();
                }

                empr_QCJobAssignment.InitGrid();
            }
        }, false, true);
    },

    GetTJVRecord: function (code) {
        ajaxHelper.ajaxGetJson('/QCJobAssignment/GetTJVRecord?code=' + code, function (data) {
            if (data.data.msgType == 1) {
                var data = data.data.data[0];
                console.log('data', data);
                //empr_QCJobAssignment.InitBranchTo(data.debiT_AC, data.crediT_AC);
                empr_QCJobAssignment.InitDebitDDL(data.debiT_AC);
                empr_QCJobAssignment.InitCreditDDL(data.crediT_AC);
                $("#REMARKS").val(data.ddesc);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
}