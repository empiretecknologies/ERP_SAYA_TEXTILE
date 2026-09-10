var empr_PurchaseOrderReports = {
    reportDataSrc: [],
    InitEvents: function () {
        console.log('Supplier', Supplier);
        empr_PurchaseOrderReports.GetReportTypes();
        //empr_PurchaseOrderReports.InitControlDDL();
        //empr_PurchaseOrderReports.InitAccountDDL();
        empr_PurchaseOrderReports.InitPartyCodeDDL();
        empr_PurchaseOrderReports.InitSupplierCodeDDL();
        empr_PurchaseOrderReports.InitPOJobGridDDL(null);
        empr_PurchaseOrderReports.InitItemDDL();
        empr_PurchaseOrderReports.InitSeasonDDL();
        $('.subTab').hide();

        $('body').on('click', '#BtnGenerate', function () {
            $("#Loader").show();
            $("#Loader").css('display', 'flex');
            setTimeout(function () {
                if (empr_PurchaseOrderReports.ValidateInfo()) {
                    empr_PurchaseOrderReports.GenerateReport();
                    setTimeout(function () {
                        $("#Loader").hide();
                    }, 500);
                }
            }, 200);
        });

    },


    GetReportTypes: function () {
        ajaxHelper.ajaxGetJson("/PurchaseOrderReports/GetReportTypes", function (data) {
            if (data.msgType == 1) {
                empr_PurchaseOrderReports.InitReportTypeGrid(data.data);
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
        var SEASON = $('#SEASON').dxSelectBox('option', 'value');
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
            SEASON: SEASON,

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
        var dataModel = empr_PurchaseOrderReports.GetDataToSave();
        console.log(dataModel);
        ajaxHelper.ajaxPostJsonData(dataModel, "/PurchaseOrderReports/GenerateReport", function (data) {
            if (data.msgType == 1) {
                empr_PurchaseOrderReports.InitReportGrid(data.data, dataModel.REPORTID);


                if (dataModel.REPORTID == 138) {
                    $('.subTab').show();
                    // Trade report
                    let subModel = { ...dataModel, REPORTID: 139 };
                    ajaxHelper.ajaxPostJsonData(subModel, "/PurchaseOrderReports/GenerateReport", function (subData) {

                        if (subData.msgType == 1) {
                            empr_PurchaseOrderReports.InitSubReportGrid(subData.data, 139);
                        } else {
                            empr_helper.notify(data.msg, data.msgType);
                        }
                    }, false, true);
                } else {
                    $('.subTab').hide();

                }

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

        if (reportId == 138) {
            col = [
                { dataField: 'suppName', caption: 'Supplier Name', groupIndex: 0 },
                { dataField: 'clientName', caption: 'Client Name', groupIndex: 1 },
                { dataField: 'model', caption: 'Model', groupIndex: 2 },
                { dataField: 'orderNo', caption: 'Order No', width: 150 },
                { dataField: 'color', caption: 'Color', width: 150 },
                { dataField: 'size', caption: 'Size', width: 150 },
                {
                    dataField: 'qty',
                    caption: 'Qty',
                    width: 100,
                    format: { type: 'fixedPoint', precision: 2 }
                },
                {
                    dataField: 'rate',
                    caption: 'Rate',
                    width: 100,
                    format: { type: 'fixedPoint', precision: 2 }
                },
                {
                    dataField: 'amt',
                    caption: 'Amount',
                    width: 100,
                    format: { type: 'fixedPoint', precision: 2 } 
                },
                { dataField: 'lcPort', caption: 'LC Port', width: 100 },
                { dataField: 'shipDate', caption: 'Ship Date', width: 120 },
                { dataField: 'bookingDate', caption: 'Booking Date', width: 120 },
                { dataField: 'comment', caption: 'Comment', },
                { dataField: 'currSign', caption: 'Currency', visible: false, width: 150 },
                //{ dataField: 'clientCode', caption: 'Client Code', width: 120, visible: false },
                //{ dataField: 'clientActCode', caption: 'Client Act Code', width: 100, visible: false },
                //{ dataField: 'suppCode', caption: 'Supp Code', width: 120, visible: false  },
                //{ dataField: 'suppActCode', caption: 'Supp Act Code', width: 100, visible: false },
                //{ dataField: 'revRef', caption: 'Rev Ref Port', width: 100, visible: false },
                //{ dataField: 'dtCode', caption: 'DT Code', width: 120, visible: false },
                //{ dataField: 'tranId', caption: 'Tran Id', width: 120, visible: false },
            ]
        }
        //if (reportId == 139) {
        //    col = [
        //        { dataField: 'suppName', caption: 'Supplier Name', width: 100, groupIndex: 0 },
        //        { dataField: 'suppCode', caption: 'Supp Code', width: 120, visible: false },
        //        { dataField: 'suppActCode', caption: 'Supp Act Code', width: 100, visible: false },
        //        { dataField: 'size', caption: 'Size', width: 100 },
        //        { dataField: 'qty', caption: 'Qty', width: 100 },
        //        { dataField: 'rate', caption: 'Rate', width: 100, },
        //        { dataField: 'amt', caption: 'Amount', width: 100 },
        //    ]
        //}


        //if (reportId == 3) {
        //    let totalDebit = 0;
        //    let totalCredit = 0;
        //    dataSrc.forEach(item => {
        //        totalDebit += item.debit || 0;
        //        totalCredit += item.credit || 0;
        //        if (
        //            item.chqDate == '1900-01-01' || item.chqDate == '01-01-1900' || item.chqDate == '01-Jan-1900' || item.chqDate == '1/1/1900 12:00:00 AM' || item.chqDate == '01/01/1900 12:00:00 AM' || item.chqDate == '1/1/1900' ||
        //            item.chqDate == '2000-01-01' || item.chqDate == '01-01-2000' || item.chqDate == '01-Jan-2000' || item.chqDate == '1/1/2000 12:00:00 AM' || item.chqDate == '01/01/2000 12:00:00 AM' || item.chqDate == '1/1/2000' ||
        //            item.chqDate == '00-01-01' || item.chqDate == '01-01-00' || item.chqDate == '01-Jan-00' || item.chqDate == '1/1/00 12:00:00 AM' || item.chqDate == '01/01/00 12:00:00 AM' || item.chqDate == '1/1/00' || item.chqDate == '01-Jan-00 12:00:00 AM'
        //        ) {
        //            item.chqDate = null;
        //        }
        //    });
        //    empr_helper.balanceAmount = totalDebit - totalCredit;
        //    var col = [
        //        { dataField: 'voucherDate', caption: 'Date', width: 120 },
        //        {
        //            dataField: 'voucherNo', caption: 'Transaction #', width: 200,
        //            cellTemplate: function (container, options) {
        //                $('<a>')
        //                    .addClass('dx-link')
        //                    .text(options.value)
        //                    .attr('href', '#')
        //                    .attr('onclick', 'empr_PurchaseOrderReports.openVoucherPage(' + JSON.stringify(options.data.link) + ', ' + JSON.stringify(options.data.traN_ID) + ')')
        //                    .appendTo(container);
        //            }
        //        },
        //        { dataField: 'bilL_NO', caption: 'CHQ', width: 110 },
        //        { dataField: 'bilL_DATE', caption: 'CHQ.Date', width: 110 },
        //        { dataField: 'accountName', caption: 'A/c Name', groupIndex: 0 },
        //        {
        //            dataField: 'accountDescription',
        //            caption: 'Description',
        //            cellTemplate: function (container, options) {
        //                $('<div>')
        //                    .text(options.value)
        //                    .css({
        //                        'white-space': 'normal',
        //                        'word-wrap': 'break-word',
        //                        'overflow-wrap': 'break-word'
        //                    })
        //                    .appendTo(container);
        //            }
        //        },
        //        { dataField: 'debit', caption: 'Debit', dataType: 'number', width: 120 },
        //        { dataField: 'credit', caption: 'Credit', dataType: 'number', width: 120 },
        //        {
        //            caption: 'Balance',
        //            dataType: 'number',
        //            width: 120,
        //            calculateCellValue: function (data) {
        //                return data.balance < 0 ? `(${new Intl.NumberFormat().format(Math.abs(data.balance))})` : new Intl.NumberFormat().format(data.balance);
        //            },
        //            cellTemplate: function (container, options) {
        //                const isNegative = options.data.balance < 0;
        //                $('<div>')
        //                    .text(options.value)
        //                    .css({
        //                        color: isNegative ? 'red' : 'black',
        //                        'text-align': 'right'
        //                    })
        //                    .appendTo(container);
        //            },
        //        }

        //    ];
        //}
        //else if (reportId == 4 || reportId == 119) {
        //    var col = [
        //        { dataField: 'parentName', caption: 'Parent Name', groupIndex: 0 },
        //        { dataField: 'accountName', caption: 'A/c Name' },
        //        { dataField: 'debit', caption: 'Debit', dataType: 'number', width: 120 },
        //        { dataField: 'credit', caption: 'Credit', dataType: 'number', width: 120 }
        //    ];
        //}
        //else if (reportId == 6) {
        //    var col = [
        //        {
        //            dataField: 'accountCode',
        //            caption: 'Type',
        //            groupIndex: 0,
        //            calculateSortValue: function (rowData) {
        //                return rowData.accountCode; // sort code ke hisaab se hoga
        //            },
        //            customizeText: function (cellInfo) {
        //                switch (cellInfo.value) {
        //                    case 4: return 'Revenue';
        //                    case 5: return 'Expenses';
        //                    default: return 'Other';
        //                }
        //            }
        //        },
        //        /*{ dataField: 'accountType', caption: 'Type', , },*/
        //        { dataField: 'grCode', caption: 'GR Code', visible: false },
        //        { dataField: 'parentName', caption: 'Parent Name', groupIndex: 1, },
        //        { dataField: 'accountName', caption: 'Account', },
        //        {
        //            dataField: 'balances',
        //            caption: 'Balance',
        //            dataType: 'number',
        //            format: {
        //                type: 'fixedPoint', // ensures decimal formatting
        //                precision: 2        // 2 decimal places
        //            },
        //            alignment: 'right' // optional: aligns numbers neatly
        //        }
        //    ];
        //}
        //else if (reportId == 7) {
        //    var col = [
        //        { dataField: 'accountCode', caption: 'Code', visible: false },
        //        { dataField: 'accountType', caption: 'Type', groupIndex: 0 },
        //        { dataField: 'grCode', caption: 'GR Code', visible: false },
        //        { dataField: 'parentName', caption: 'Parent Name', groupIndex: 1, },
        //        { dataField: 'accountName', caption: 'Account', },
        //        { dataField: 'balance', caption: 'Balance', }
        //    ];
        //}
        //else if (reportId == 121) {

        //    $.each(dataSrc, function (index, item) {
        //        if (item.vC_TYPE === 1 && item.debit !== null) {
        //            empr_helper.typeOneTotal += item.debit;
        //        }
        //        if (item.vC_TYPE === 2 && item.debit !== null) {
        //            empr_helper.typeTwoTotal += item.debit;
        //        }
        //        if (item.vC_TYPE === 3 && item.debit !== null) {
        //            empr_helper.typeThreeTotal += item.debit;
        //        }
        //        if (item.vC_TYPE === 4 && item.debit !== null) {
        //            empr_helper.typeFourTotal += item.debit;
        //        }
        //    });
        //    console.log(empr_helper.typeOneTotal);
        //    console.log(empr_helper.typeTwoTotal);
        //    console.log(empr_helper.typeThreeTotal);
        //    console.log(empr_helper.typeFourTotal);

        //    var col = [
        //        { dataField: 'parentName', caption: 'Control Name', groupIndex: 1 },
        //        { dataField: 'accountName', caption: 'A/c Name' },
        //        {
        //            dataField: 'debit',
        //            caption: 'Amount',
        //            dataType: 'number',
        //            width: 120,
        //            cellTemplate: function (container, options) {
        //                const value = options.value;
        //                const isNegative = value < 0;
        //                const formattedValue = isNegative
        //                    ? `(${new Intl.NumberFormat().format(Math.abs(value))})`
        //                    : new Intl.NumberFormat().format(value);

        //                $('<div>')
        //                    .text(formattedValue)
        //                    .css({
        //                        color: isNegative ? 'red' : 'black',
        //                        'text-align': 'right'
        //                    })
        //                    .appendTo(container);
        //            },
        //        },
        //        {
        //            dataField: 'vC_TYPE',
        //            caption: '',
        //            dataType: 'number',
        //            width: 120,
        //            groupIndex: 0,
        //            visible: false,
        //            groupCellTemplate: function (container) {
        //                container.html("");
        //            }
        //        }
        //    ];
        //}
        //else if (reportId == 37) {
        //    empr_helper.balanceAmount = "";
        //    var col = [
        //        { dataField: 'voucherDate', caption: 'Date', width: 120 },
        //        { dataField: 'voucherNo', caption: 'Transaction #', width: 150, },
        //        { dataField: 'accountName', caption: 'A/c Name' },
        //        { dataField: 'debit', caption: 'Debit', dataType: 'number', width: 120 },
        //        { dataField: 'credit', caption: 'Credit', dataType: 'number', width: 120 },
        //        { dataField: 'accountDescription', caption: 'Description', width: 250 },
        //    ];
        //}
        //else if (reportId == 92) {
        //    var col = [
        //        { dataField: 'accountCode', caption: 'Code', width: 120, visible: false },
        //        { dataField: 'accountName', caption: 'Account Name' },
        //        { dataField: 'parentName', caption: 'Parent Name', groupIndex: 0, },
        //        { dataField: 'debit', caption: 'Amount', dataType: 'number', width: 120 },
        //    ]
        //}
        //else if (reportId == 101) {
        //    var col = [
        //        {
        //            dataField: 'voucherNo', caption: 'Transaction #', width: 200,
        //            cellTemplate: function (container, options) {
        //                $('<a>')
        //                    .addClass('dx-link')
        //                    .text(options.value)
        //                    .attr('href', '#')
        //                    .attr('onclick', 'empr_PurchaseOrderReports.openVoucherPage(' + JSON.stringify(options.data.link) + ', ' + JSON.stringify(options.data.traN_ID) + ')')
        //                    .appendTo(container);
        //            }
        //        },
        //        { dataField: 'chq', caption: 'CHQ', width: 70 },
        //        { dataField: 'chqDate', caption: 'CHQ.Date', width: 110 },
        //        { dataField: 'amt', caption: 'Amount', dataType: 'number', width: 80 },
        //        { dataField: 'bankName', caption: 'Bank Name', width: 200 },
        //        { dataField: 'desc', caption: 'Description' },
        //        { dataField: 'bType', caption: 'Type', groupIndex: 0 },
        //    ];
        //}
        //else if (reportId == 103) {
        //    let natures = [...new Set(dataSrc.map(x => x.accountNature))].sort((a, b) => a - b);
        //    let grossTotal = 0;
        //    let sales = 0;
        //    let salesReturn = 0;
        //    let purchase = 0;
        //    let formattedTotal;
        //    // loop karke har nature ka debit-credit calculate karo
        //    natures.forEach(nature => {
        //        let debitSum = dataSrc
        //            .filter(x => x.accountNature === nature)
        //            .reduce((sum, x) => sum + (x.debit || 0), 0);

        //        let creditSum = dataSrc
        //            .filter(x => x.accountNature === nature)
        //            .reduce((sum, x) => sum + (x.credit || 0), 0);

        //        if (nature == 5)
        //            sales = creditSum - debitSum;
        //        if (nature == 6)
        //            salesReturn = debitSum - creditSum;
        //        if (nature == 7)
        //            purchase = debitSum - creditSum;

        //    });
        //    grossTotal = (sales - salesReturn) - purchase;
        //    formattedTotal = grossTotal.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

        //    var col = [
        //        {
        //            dataField: 'accountNature',
        //            caption: 'Nature',
        //            groupIndex: 0,
        //            allowSorting: false,
        //            calculateGroupValue: function (rowData) {
        //                return rowData.accountNature;
        //            },
        //            groupCellTemplate: function (container, options) {
        //                var map = {
        //                    1: "CASH A/C",
        //                    2: "BANK A/C",
        //                    3: "VENDOR",
        //                    4: "CUSTOMER",
        //                    5: "SALES",
        //                    6: "SALES RETURN",
        //                    7: "PURCHASE",
        //                    7.2: `Gross Total :        ${formattedTotal}`,
        //                    8: "PURCHASE RETRUN",
        //                    9: "SALES PERSON",
        //                    10: "TRANSPOTERS",
        //                    11: "Fixed Assets",
        //                    15: "Stiching Unit",
        //                    20: "OTHERS",
        //                    21: "LABOUR",
        //                    22: "Cash Sales",
        //                    23: "Cash Sales Return",
        //                    24: "Advance",
        //                    25: "Transfer Shop",
        //                    26: "Expenses",
        //                    27: "Shop Expenses"
        //                };

        //                var div = $("<div>")
        //                    .css({
        //                        "text-align": "center",
        //                        "font-weight": "bold",
        //                        "width": "100%"
        //                    })
        //                    .text(map[options.value] || options.value)
        //                    .appendTo(container);

        //                if (options.value === 7.2) { // GROSS TOTAL
        //                    div.css("text-align", "right");
        //                    div.css("font-weight", "bold");
        //                    div.css("color", "rgba(51, 51, 51, .7)");
        //                }
        //            },
        //            rowTemplate: function (container, row) {
        //                var rowData = row.data;
        //                if (rowData.isGrossTotal) {
        //                    $("<tr>")
        //                        .css({ "font-weight": "bold", "background-color": "#eef" })
        //                        .append(
        //                            $("<td>").text(rowData.accountName),
        //                            $("<td>").text(""), // Account Name column empty
        //                            $("<td>").text(""), // Debit empty
        //                            $("<td>").text(rowData.credit.toLocaleString('en-US')) // Credit = grossTotal
        //                        )
        //                        .appendTo(container);
        //                } else {
        //                    // Normal row rendering
        //                    this.defaultRowTemplate(container, row);
        //                }
        //            }
        //        },
        //        { dataField: 'accountName', caption: 'Account Name'},
        //        { dataField: 'debit', caption: 'Debit', width: 200 },
        //        { dataField: 'credit', caption: 'Credit', width: 200 }
        //    ];


        //}
        //else if (reportId == 106 || reportId == 107 || reportId == 108 || reportId == 109) {
        //    var col = [
        //        { dataField: 'voucherDate', caption: 'Date', width: 120 },
        //        {
        //            dataField: 'voucherNo', caption: 'Transaction #', width: 200,
        //            cellTemplate: function (container, options) {
        //                $('<a>')
        //                    .addClass('dx-link')
        //                    .text(options.value)
        //                    .attr('href', '#')
        //                    .attr('onclick', 'empr_PurchaseOrderReports.openVoucherPage(' + JSON.stringify(options.data.link) + ', ' + JSON.stringify(options.data.traN_ID) + ')')
        //                    .appendTo(container);
        //            }
        //        },
        //        { dataField: 'chqDate', caption: 'CHQ.Date', width: 120 },
        //        { dataField: 'chqNo', caption: 'CHQ.No', width: 100 },
        //        { dataField: 'accountName', caption: 'Account Name', width: 200 },
        //        { dataField: 'debit', caption: 'Debit', dataType: 'number', width: 100 },
        //        { dataField: 'credit', caption: 'Credit', dataType: 'number', width: 100 },
        //        { dataField: 'accountDescription', caption: 'Account Description' },
        //        { dataField: 'bType', caption: 'Type', groupIndex: 0 },
        //    ];
        //}
        //else if (reportId == 110 || reportId == 111 || reportId == 116) {
        //    var col = [
        //        { dataField: 'voucherDate', caption: 'Date', width: 120 },
        //        {
        //            dataField: 'voucherNo', caption: 'Transaction #', width: 200,
        //            cellTemplate: function (container, options) {
        //                $('<a>')
        //                    .addClass('dx-link')
        //                    .text(options.value)
        //                    .attr('href', '#')
        //                    .attr('onclick', 'empr_PurchaseOrderReports.openVoucherPage(' + JSON.stringify(options.data.link) + ', ' + JSON.stringify(options.data.traN_ID) + ')')
        //                    .appendTo(container);
        //            }
        //        },
        //        { dataField: 'chqDate', caption: 'CHQ.Date', width: 120 },
        //        { dataField: 'chqNo', caption: 'CHQ.No', width: 100 },
        //        { dataField: 'accountName', caption: 'Account Name', width: 200, groupIndex: 0 },
        //        { dataField: 'bType', caption: 'Type' },
        //        { dataField: 'debit', caption: 'Debit', dataType: 'number', width: 100 },
        //        { dataField: 'credit', caption: 'Credit', dataType: 'number', width: 100 },
        //        { dataField: 'accountDescription', caption: 'Account Description' },
        //    ];
        //}
        //else if (reportId == 112 || reportId == 113 || reportId == 114 || reportId == 115) {
        //    var col = [
        //        { dataField: 'voucherDate', caption: 'Date', width: 120 },
        //        {
        //            dataField: 'voucherNo', caption: 'Transaction #', width: 200,
        //            cellTemplate: function (container, options) {
        //                $('<a>')
        //                    .addClass('dx-link')
        //                    .text(options.value)
        //                    .attr('href', '#')
        //                    .attr('onclick', 'empr_PurchaseOrderReports.openVoucherPage(' + JSON.stringify(options.data.link) + ', ' + JSON.stringify(options.data.traN_ID) + ')')
        //                    .appendTo(container);
        //            }
        //        },
        //        { dataField: 'chqDate', caption: 'CHQ.Date', width: 120 },
        //        { dataField: 'chqNo', caption: 'CHQ.No', width: 100 },
        //        { dataField: 'accountName', caption: 'Account Name', width: 200, groupIndex: 0 },
        //        { dataField: 'bType', caption: 'Type', width: 250 },
        //        { dataField: 'qty', caption: 'Qty', width: 70 },
        //        { dataField: 'rate', caption: 'Rate', width: 70 },
        //        { dataField: 'debit', caption: 'Debit', dataType: 'number', width: 100 },
        //        { dataField: 'credit', caption: 'Credit', dataType: 'number', width: 100 },
        //        { dataField: 'accountDescription', caption: 'A.Description' },
        //    ];
        //}
        //else if (reportId == 121) {
        //    var col = [
        //        { dataField: 'parentName', caption: 'Control Name', },
        //        { dataField: 'accountName', caption: 'Account Name', },
        //        { dataField: 'debit', caption: 'DEBIT', },
        //        { dataField: 'voucherType', caption: 'V_Type', groupIndex: 0, visible: false, },
        //    ];
        //}
        //if ($('#ReportGridContainer').data('dxDataGrid') != undefined) {
        //    $('#ReportGridContainer').data('dxDataGrid').dispose();
        //}
        //empr_helper.DxGridBindingForReportsWithSetting('#ReportGridContainer', col, dataSrc, empr_helper.reportName, true);
        //empr_helper.DxGridBindingForReportsWithSetting_Aging('#ReportGridContainer', col, dataSrc, empr_helper.reportName);
        empr_helper.DxGridBindingForReportsWithSetting_Aging('#ReportGridContainer', col, dataSrc, empr_helper.reportName, false, false, 'old');


        //if (reportId >= 106 && reportId <= 116) {
        //}
        //else {
        //    empr_helper.DxGridBindingForReportsWithSetting('#ReportGridContainer', col, dataSrc, empr_helper.reportName);
        //}

        //setTimeout(function () {
        //    empr_helper.DxGridBindingForReportsWithSetting_Aging('#ReportGridContainer', col, dataSrc, empr_helper.reportName);
        //}, 100);
        setTimeout(function () {
            var selectedRowKey = $('#GridContainer').dxDataGrid('instance').getSelectedRowKeys();
            //if (selectedRowKey.length > 0) {
            //    $('#REPORT_NAME').text(selectedRowKey[0].reporT_NAME);
            //}
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
                {
                    dataField: 'amt',
                    caption: 'Amount',
                    width: 100,
                    format: { type: 'fixedPoint', precision: 2 }
                },
            ]
        }
        empr_helper.DxGridBindingForReportsWithSetting_Aging('#SubGridContainer', col, dataSrc, empr_helper.reportName, true, true);

        setTimeout(function () {

            var selectedRowKey = $('#SubGridContainer').dxDataGrid('instance').getSelectedRowKeys();
            //if (selectedRowKey.length > 0) {
            //    $('#REPORT_NAME').text(selectedRowKey[0].reporT_NAME);
            //}
            $('#OptionTab').removeClass('active');
            $('#OptionTabContent').removeClass('active');
            $('#ViewTab').click();
            $('.tab-pane').removeClass('fade');
            $('#ViewTab').addClass('active')
            $('#ViewTabContent').addClass('active');
        }, 500);
    },

    ValidateInfo: function () {

        var valid = true;
        var data = empr_PurchaseOrderReports.GetDataToSave();


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

    InitPOJobGridDDL: function (_selectedValue) {

        ajaxHelper.ajaxGetJson('/MerchantPurchaseOrderDetail/POJobsDropdown', function (data) {
            debugger;
            if (data.msgType == 1) {
                var Datasource = data.data;

                selectedObject = [];
                selectedValue = _selectedValue;

                if (_selectedValue != null) {
                    selectedObject = Datasource.filter(x => { return x.key == _selectedValue }) || [];
                    if (selectedObject.length > 0) {
                        selectedValue = selectedObject[0].key;

                        $('#job_key_hidden').val(selectedObject[0].key);
                        $('#CLIENT_PO').val(selectedObject[0].clientPo);
                        $('#PARTY_CODE').val(selectedObject[0].partyName);
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
                            const selectedParty = Supplier.find(item => item.key === selectedData.partyCode && item.accountCode === selectedData.accountCode);
                            const selectedSupplier = Supplier.find(item => item.key === selectedData.spartyCode && item.accountCode === selectedData.saccountCode);
                            const selectedItem = Items.find(item => item.key === selectedData.itemCode);
                            const selectedSeason = Season.find(item => item.key === selectedData.seasonCode);
                            if (selectedData) {
                                empr_PurchaseOrderReports.InitPartyCodeDDL(selectedParty.customizedKey);
                                empr_PurchaseOrderReports.InitSupplierCodeDDL(selectedSupplier.customizedKey);
                                empr_PurchaseOrderReports.InitItemDDL(selectedItem.key);
                                if (selectedSeason) {
                                    empr_PurchaseOrderReports.InitSeasonDDL(selectedSeason.key);
                                }


                                $('#job_key_hidden').val(selectedData.key);


                                //$('#CLIENT_PO').val(selectedData.clientPo);
                                //$('#PARTY_CODE').val(selectedData.partyName);
                            }
                        } else {
                            $('#job_key_hidden').val('');
                            empr_PurchaseOrderReports.InitPartyCodeDDL();
                            empr_PurchaseOrderReports.InitSupplierCodeDDL();
                            empr_PurchaseOrderReports.InitItemDDL();
                            empr_PurchaseOrderReports.InitSeasonDDL();
                            //$('#CLIENT_PO').val('');
                            //$('#PARTY_CODE').val('');
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

                                    $('#job_key_hidden').val(selected.key);
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

    InitSeasonDDL: function (selectedValue) {
        $('#SEASON').dxSelectBox({
            dataSource: Season,
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

}