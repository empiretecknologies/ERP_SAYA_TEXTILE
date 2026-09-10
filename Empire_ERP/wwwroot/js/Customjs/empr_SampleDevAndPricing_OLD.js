var empr_SampleDevAndPricing_OLD = {

    PartiesData: [],

    initEvents: function () {

        $(document).ready(function () {

            empr_SampleDevAndPricing_OLD.resetForm();

            window.addEventListener('message', function (event) {
                if (event.origin !== window.location.origin) {
                    return;
                }
                var data = event.data;
                if (data && data.traN_ID) {
                    $('#Code').val(data.traN_ID);
                    empr_SampleDevAndPricing_OLD.GetSampleDevAndPricingByID(data.traN_ID);
                }
            });

            $('#saveAttempt').click(function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_SampleDevAndPricing_OLD.validateForm()) {
                            empr_SampleDevAndPricing_OLD.saveAttempt();
                        }
                    }
                } else {
                    if (empr_SampleDevAndPricing_OLD.validateForm()) {
                        empr_SampleDevAndPricing_OLD.saveAttempt();
                    }
                }
            })

            $('body').on('click', '#quicksearch', function () {
                empr_SampleDevAndPricing_OLD.InintQuickSearch();
            })

            $('body').on('click', '.elm_print', function () {
                empr_helper.selectedBill = $(this).attr("reportid");
                empr_SampleDevAndPricing_OLD.GeneratePrintReport();
            });

            $('body').on('click', '.elm_edit', function () {

                var rportid = $(this).attr("rportid")
                empr_helper.selectedBill = rportid;
                empr_SampleDevAndPricing_OLD.GetSampleDevAndPricingByID(rportid);

            })

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
                ajaxHelper.ajaxPostJsonData({ traN_ID: empr_helper.selectedBill, v_DATE: $('#updatedDate').val() }, "/SampleDevAndPricing/CopyRecord", function (data) {
                    //console.log(data);
                    empr_helper.notify(data.msg, data.msgType);
                    if (data.msgType == 1) {
                        $('.modal').modal('hide');
                        empr_SampleDevAndPricing_OLD.GetSampleDevAndPricingByID(data.data);
                    }
                }, false, true);
            });

            $('body').on('click', '#resetall', function () {
                $('.btn-delete').hide();
                $('.btn-print').hide();
                empr_SampleDevAndPricing_OLD.resetForm();

            })

            $('.btn-delete').click(function () {
                empr_SampleDevAndPricing_OLD.DeleteRecord();
            });

            $('body').on('click', '#BtnSodaPick', function () {
                empr_SampleDevAndPricing_OLD.InitSodaPickGrid();
            });

            $('body').on('click', '#BtnPrint, #BtnGenerateReport', function () {
                empr_SampleDevAndPricing_OLD.GeneratePrintReport();
            });

            if (Permissions != "Admin") {
                !Permissions.r_VIEW && $('#quicksearch').hide();
                !Permissions.r_PRINT && $('#BtnPrint').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#saveAttempt').hide();
            }
        });

    },

    resetForm: function () {

        $("#Code").val('')
        $('#ASTATUS').dxSelectBox('instance').option("value", "Y");
        $("#VOUCHER_NO").val('');
        $("#REF").val('');
        $("#CLIENT_PO").val('');
        $("#STYLE_DESC").val('');
        $("#BLEND").val('');
        $("#GSM").val('');
        $("#SIZE_RANGE").val('');
        $("#RATIO").val('');
        $("#PROJ_QTY").val('');
        $("#TARGET_PRICE").val('');
        $("#REMARKS").val('');


        $("#hdnDOC").val('');
        $('#DOCName').val('');
        $("#hdnDOCPFR").val('');
        $('#DOCNamePFR').val('');
        $('#V_DATE').val(todayDate);
        if (Permissions != "Admin") {
            if (Permissions.r_ADD) {
                $('#saveAttempt').show();
            } else {
                $('#saveAttempt').hide();
            }
        } else {
            $('#saveAttempt').show();
        }

        empr_SampleDevAndPricing_OLD.InitPartyCodeDDL();
        empr_SampleDevAndPricing_OLD.InitItemCodeDDL();
        empr_SampleDevAndPricing_OLD.InitFabricDDL();
        empr_SampleDevAndPricing_OLD.InitReportTypeDDL();
    },

    validateForm: function () {

        var valid = true;

        var ASTATUS = $('#ASTATUS').dxSelectBox('option', 'value')
        var ITEM_CODE = $('#ITEM_CODE').dxSelectBox('option', 'value')
        var PARTY_CODE = $('#PARTY_CODE').dxSelectBox('option', 'value')
        var CLIENT_PO = $('#CLIENT_PO').val();
        var PROJ_QTY = $('#PROJ_QTY').val();
        var FABRIC = $('#FABRIC').dxSelectBox('option', 'value');
        var DOC = $('#hdnDOC').val();

        if (CLIENT_PO == '' || CLIENT_PO == null) {
            valid = false;
            empr_helper.notify("Please Enter Client PO#.", 2);
        }

        //if (DOC == '' || DOC == null) {
        //    valid = false;
        //    empr_helper.notify("Please Select Document.", 2);
        //}

        //if (SPARTY_CODE == '' || SPARTY_CODE == null || SPARTY_CODE == '00') {
        //    valid = false;
        //    empr_helper.notify("Please select Supplier.", 2);
        //}

        if (ASTATUS == '' || ASTATUS == null) {
            valid = false;
            empr_helper.notify("Please select active.", 2);
        }

        if (FABRIC == '' || FABRIC == null) {
            valid = false;
            empr_helper.notify("Please select Fabric.", 2);
        }

        if (PROJ_QTY == '' || PROJ_QTY == null) {
            valid = false;
            empr_helper.notify("Please select Proj.Qty.", 2);
        }
        if (ITEM_CODE == '' || ITEM_CODE == null) {
            valid = false;
            empr_helper.notify("Please select Item.", 2);
        }
        if (PARTY_CODE == '' || PARTY_CODE == null) {
            valid = false;
            empr_helper.notify("Please select Client Account.", 2);
        }

        if (!valid) return;

        valid = empr_helper.validateDateRange($("#V_DATE").val(), minDate, maxDate);

        return valid;
    },

    getDataToSave: function () {
        var TRAN_ID = $("#Code").val().trim();
        var ASTATUS = $('#ASTATUS').dxSelectBox('option', 'value');
        var V_DATE = $("#V_DATE").val();
        var REF = $("#REF").val();
        var CLIENT_PO = $('#CLIENT_PO').val();
        var PARTY_CODE = $('#PARTY_CODE').dxSelectBox('option', 'value');
        var REC_ON_DATE = $("#REC_ON_DATE").val();
        var ITEM_CODE = $('#ITEM_CODE').dxSelectBox('option', 'value');
        var FABRIC = $('#FABRIC').dxSelectBox('option', 'value');
        var STYLE_DESC = $("#STYLE_DESC").val();
        var DOC = $('#hdnDOC').val();
        var BLEND = $('#BLEND').val();
        var GSM = $('#GSM').val();
        var SIZE_RANGE = $('#SIZE_RANGE').val();
        var DOC_PFR = $('#hdnDOCPFR').val();
        var RATIO = $('#RATIO').val();
        var PROJ_QTY = $('#PROJ_QTY').val();
        var TARGET_PRICE = $('#TARGET_PRICE').val();
        var REMARKS = $("#REMARKS").val();

        var modelRecord = {
            TRAN_ID: TRAN_ID,
            ASTATUS: ASTATUS,
            V_DATE: V_DATE,
            REF: REF,
            CLIENT_PO: CLIENT_PO,
            PARTY_CODE: PARTY_CODE,
            REC_ON_DATE: REC_ON_DATE,
            ITEM_CODE: ITEM_CODE,
            FABRIC: FABRIC,
            STYLE_DESC: STYLE_DESC,
            DOC: DOC,
            BLEND: BLEND,
            GSM: GSM,
            SIZE_RANGE: SIZE_RANGE,
            DOC_PFR: DOC_PFR,
            RATIO: RATIO,
            PROJ_QTY: PROJ_QTY,
            TARGET_PRICE: TARGET_PRICE,
            REMARKS: REMARKS,
        }

        return modelRecord;

    },

    saveAttempt: function () {

        var obj = empr_SampleDevAndPricing_OLD.getDataToSave();
        console.log('saveattempt', obj);
        var xhr = ajaxHelper.ajaxPostJsonData(obj, "/SampleDevAndPricing/save", function (data) {
            empr_helper.notify(data.msg, data.msgType);

            if (data.msgType == 1) {

                empr_helper.selectedBill = data.data;

                if (dataClear == 1) {
                    empr_SampleDevAndPricing_OLD.GetSampleDevAndPricingByID(data.data);
                    $('.btn-delete').show();
                    $('.btn-print').show();
                }
                else {
                    empr_SampleDevAndPricing_OLD.resetForm();
                }
            }


        }, false, true);

    },

    InintQuickSearch: function () {
        empr_SampleDevAndPricing_OLD.GetQuickSearch();
    },

    GetQuickSearch: function () {
        ajaxHelper.ajaxGetJson('/SampleDevAndPricing/QuickSearch', function (data) {
            if (data.msgType == 1) {
                empr_SampleDevAndPricing_OLD.CreateGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

    GetSampleDevAndPricingByID: function (id) {
        var xhr = ajaxHelper.ajaxGetJson('/SampleDevAndPricing/SampleDevAndPricingByid?id=' + id, function (data) {
            debugger;
            if (data.msgType == 1) {
                console.log('edit', data);
                if (Permissions != "Admin") {
                    if (Permissions.r_DLT) {
                        $('.btn-delete').show();
                    }
                    if (Permissions.r_PRINT) {
                        $('.btn-print').show();
                    }
                    if (Permissions.r_ADD) {
                        $('#resetall').show();
                    }
                    if (Permissions.r_EDIT) {
                        $('#saveAttempt').show();
                    }
                    else {
                        $('#saveAttempt').hide();
                    }
                } else {
                    $('#saveAttempt').show();
                    $('.btn-delete').show();
                    $('.btn-print').show();
                    $('#resetall').show();
                }

                $('.modal').modal('hide')
                debugger;
                $('#Code').val(data.data.traN_ID); 
                $("#V_DATE").val(data.data.v_DATE);
                $("#REC_ON_DATE").val(data.data.reC_ON_DATE);
                $("#VOUCHER_NO").val(data.data.voucheR_NO);
                $("#REF").val(data.data.ref);
                $("#CLIENT_PO").val(data.data.clienT_PO);
                $("#PROJ_QTY").val(data.data.proJ_QTY);
                $("#STYLE_DESC").val(data.data.stylE_DESC);
                $("#BLEND").val(data.data.blend);
                $("#GSM").val(data.data.gsm);
                $("#SIZE_RANGE").val(data.data.sizE_RANGE);
                $("#RATIO").val(data.data.ratio);
                $("#TARGET_PRICE").val(data.data.targeT_PRICE);


                empr_SampleDevAndPricing_OLD.InitPartyCodeDDL(data.data.partY_CODE);
                empr_SampleDevAndPricing_OLD.InitItemCodeDDL(data.data.iteM_CODE);
                empr_SampleDevAndPricing_OLD.InitFabricDDL(data.data.fabric);
                
                $("#REMARKS").val(data.data.remarks);
                $('#activestatushidden').val(data.data.astatus);
                $('#ASTATUS').dxSelectBox('instance').option("value", data.data.astatus);

                $('#hdnDOC').val(data.data.doc);
                var fullPath = data.data.doc;
                var fileName = fullPath.split('/').pop();
                $('#DOCName').val(fileName);

                $('#hdnDOCPFR').val(data.data.doC_PFR);
                var fullPathPFR = data.data.doC_PFR;
                var fileNamePFR = fullPathPFR.split('/').pop();
                $('#DOCNamePFR').val(fileNamePFR);

            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }


        }, false, true);

    },

    CreateGrid: function (dataSrc) {

        var col = [{
            dataField: "Action",
            width: 100,
            alignment: 'center',
            fixed: true,
            fixedPosition: "left",
            allowExporting: false,
            cellTemplate: function (container, options) {
                var data = JSON.stringify(options.data);
                var data_ = encodeURI(data);
                if (Permissions != "Admin" && !Permissions.r_PRINT) {
                    $(`<div class="btn-group btn-group-sm">
                            <a href="javascript:;"  class="grid-action-icon elm_edit" rportid=${options.data.traN_ID} title="Edit"><i class="fa fa-edit"></i></a>
                            <a href="javascript:;"  class="grid-action-icon elm_copy" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.traN_ID} title="COPY"><i class="fa fa-copy"></i></a>
                            </div>`).appendTo(container);
                } else {
                    $(`<div class="btn-group btn-group-sm">
                            <a href="javascript:;"  class="grid-action-icon elm_edit" rportid=${options.data.traN_ID} title="Edit"><i class="fa fa-edit"></i></a>
                            <a href="javascript:;"  class="grid-action-icon elm_print" style="margin-left: 8px" reportid=${options.data.traN_ID} title="PRINT"><i class="fa fa-print"></i></a>
                            <a href="javascript:;"  class="grid-action-icon elm_copy" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.traN_ID} title="COPY"><i class="fa fa-copy"></i></a>
                            </div>`).appendTo(container);
                }
            }
        },
        {
            dataField: 'doc',
            caption: 'Doc',
            width: 100,
            alignment: 'center',
            cellTemplate: function (container, options) {
                var html = '<div class="btn-group btn-group-sm">';
                if (options.data.doc != null && options.data.doc != '' && options.data.doc != undefined) {
                    html += `<a href="javascript:;" class="grid-action-icon" title="View Pic" onclick="window.open('${options.data.doc}', '_blank')"><i class="fa fa-eye"></i></a>`;
                }
                html += '</div>';
                $(html).appendTo(container);
            }
        },
        { dataField: 'traN_ID', caption: 'Code', width: 80, alignment: "center" },
        { dataField: 'astatus', caption: 'Status' },
        { dataField: 'v_DATE', caption: 'Voucher Date' },
        { dataField: 'voucheR_NO', caption: 'Voucher Number' },
        { dataField: 'ref', caption: 'Refrence No #' },
        { dataField: 'clienT_PO', caption: 'Model# / PO#', alignment: 'center' },
        { dataField: 'partY_NAME', caption: 'Client Name' },
        { dataField: 'reC_ON_DATE', caption: 'Rec On Date', alignment: 'center' },
        { dataField: 'iteM_NAME', caption: 'Item' },
        { dataField: 'fabric', caption: 'Fabric' },
        { dataField: 'stylE_DESC', caption: 'Style Disc' },
        { dataField: 'blend', caption: 'Blend' },
        { dataField: 'gsm', caption: 'GSM' },
        { dataField: 'sizE_RANGE', caption: 'Size Range' },
        { dataField: 'ratio', caption: 'Ratio' },
        { dataField: 'proJ_QTY', caption: 'Proj.Qty' },
        { dataField: 'targeT_PRICE', caption: 'Target Price' },
        { dataField: 'remarks', caption: 'Remarks' },
        { dataField: 'ediT_USER_ID', caption: 'User' },
        { dataField: 'ediT_DATE', caption: 'Add Date'},

        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "DeliveryFormatQS");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
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

    InitItemCodeDDL: function (selectedValue) {
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

    InitReportTypeDDL: function (selectedValue) {
        ajaxHelper.ajaxGetJson("/SampleDevAndPricing/GetReportTypes", function (data) {
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

    GeneratePrintReport: function () {
        empr_SampleDevAndPricing_OLD.InitReportTypeDDL();
        let TRAN_ID = empr_helper.selectedBill;
        let MD_ID = $('#ReportType').dxSelectBox('option', 'value');
        if (TRAN_ID == 0 || TRAN_ID == null || TRAN_ID == undefined || TRAN_ID == "") {
            empr_helper.notify("Please open the delivery in edit mode.", 2);
            return;
        }
        var dataModel = {
            TRAN_ID: TRAN_ID,
            MD_ID: MD_ID,
        }


        ajaxHelper.ajaxPostJsonData(dataModel, "/SampleDevAndPricing/GetPrintReport", function (data) {
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
    
    InitFabricDDL: function (selectedValue) {

        $('#FABRIC').dxSelectBox({
            dataSource: Fabric,
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
            searchTimeout: 500,
        });
    },

    DeleteRecord: function () {

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
            ajaxHelper.ajaxPostJsonData({ id: $('#Code').val() }, "/SampleDevAndPricing/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_SampleDevAndPricing_OLD.resetForm();
                    $('#optmodal').modal('hide');
                    $('.btn-delete').hide();
                    $('.btn-print').hide();
                    //$('#resetall').hide();
                }
            }, false, true);

        });

    },

    UploadDoc: function () {
        debugger;
        $('#saveAttempt').prop('disabled', true);
        var files = document.getElementById('DOC').files;
        if (files.length > 0) {
            $('#DOCName').val(files[0].name); 
        }

        var formData = new FormData();
        for (var i = 0; i !== files.length; i++) {
            formData.append("model", files[i]);
        }

        $.ajax({
            url: "/Common/UploadVoucherDocs",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                if (data.msgType == 1) {
                    $("#hdnDOC").val(data.data);
                    empr_helper.notify("File uploaded successfully.", 1);
                } else {
                    $('#DOCName').val("No File");
                    empr_helper.notify("Something went wrong while saving the file. Please re-upload.", 2);
                }
                $('#saveAttempt').prop('disabled', false);
            }
        });
    },

    OpenDoc: function () {
        var hdnUrl = $('#hdnDOC').val();
        if (!hdnUrl) {
            empr_helper.notify("Please upload a file to view.", 2);
        } else {
            const fileURL = window.location.origin + hdnUrl;
            window.open(fileURL, '_blank');
        }
    },

    UploadDocPFR: function () {
        debugger;
        $('#saveAttempt').prop('disabled', true);
        var files = document.getElementById('DOCPFR').files;
        if (files.length > 0) {
            $('#DOCNamePFR').val(files[0].name);
        }

        var formData = new FormData();
        for (var i = 0; i !== files.length; i++) {
            formData.append("model", files[i]);
        }

        $.ajax({
            url: "/Common/UploadVoucherDocs",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                debugger
                if (data.msgType == 1) {
                    $("#hdnDOCPFR").val(data.data);
                    empr_helper.notify("File uploaded successfully.", 1);
                } else {
                    $('#DOCNamePFR').val("No File");
                    empr_helper.notify("Something went wrong while saving the file. Please re-upload.", 2);
                }
                $('#saveAttempt').prop('disabled', false);
            }
        });
    },

    OpenDocPFR: function () {
        var hdnUrl = $('#hdnDOCPFR').val();
        if (!hdnUrl) {
            empr_helper.notify("Please upload a file to view.", 2);
        } else {
            const fileURL = window.location.origin + hdnUrl;
            window.open(fileURL, '_blank');
        }
    },


}