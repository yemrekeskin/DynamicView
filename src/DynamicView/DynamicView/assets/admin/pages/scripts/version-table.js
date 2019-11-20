var VersionTableAjax = function () {

    var initPickers = function () {
        //init date pickers
        $(".form_datetime").datetimepicker({
            isRTL: Metronic.isRTL(),
            format: GlobalConfig.DateTimePickerPattern,
            language: GlobalConfig.CultureShortCode,
            locale: GlobalConfig.CultureShortCode,
            autoclose: true,
            todayBtn: true,
            pickerPosition: (Metronic.isRTL() ? "bottom-right" : "bottom-left"),
            minuteStep: 5
        });

        $('body').removeClass("modal-open"); // fix bug when inline picker is used in modal
    }

    var handleRecords = function (tableConfig) {
   
        var filterEnable = false;
        var selectedRows = [];

        var table = {
            name: tableConfig.GridName + "_versions",
            get: function () { return $("#" + this.name); },
            compareButton: function () { return $("#" + this.name + "_compareButton"); },
            deleteButton: function () { return $("#" + this.name + "_deleteButton"); },
            searchButton: function () { return $("#" + this.name + "_filter_searchButton"); },
            resetButton: function () { return $("#" + this.name + "_filter_resetButton"); },
            searchQuery: function () {
                var filters = [];

                if (!filterEnable)
                    return filters;
              
                // filter by version
                var versionFilter = $("#" + this.name + "_filter_version_no");
                if (versionFilter != null) {
                    var filterVal = versionFilter.val();
                    if (filterVal.length > 0) {
                        var filterColumns = versionFilter.attr("data-filter");
                        if (filterColumns.length > 0) {
                            $.each(filterColumns.toString().split(";"), function (index, value) {
                                filters.push(value + " eq " + filterVal);
                            });
                        }
                    }
                }

                // filter by title
                var titleFilter = $("#" + this.name + "_filter_title");
                if (titleFilter != null) {
                    var filterVal = titleFilter.val();
                    if (filterVal.length > 0) {
                        var filterColumns = titleFilter.attr("data-filter");
                        if (filterColumns.length > 0) {
                            $.each(filterColumns.toString().split(";"), function (index, value) {
                                var prop = value.toString().split(":");
                                var propName = prop[0];
                                var propType = prop[1];

                                if(propType === "String")
                                    filters.push("substringof('" + filterVal + "', " + propName + ") eq true");
                                else
                                    filters.push(propName + " eq " + filterVal);
                            });
                        }
                    }
                }

                // filter by state
                var stateFilter = $("#" + this.name + "_filter_state");
                if (stateFilter != null) {
                    var filterVal = stateFilter.val();
                    if (filterVal.length > 0) {
                        var filterColumns = stateFilter.attr("data-filter");
                        if (filterColumns.length > 0) {
                            $.each(filterColumns.toString().split(";"), function (index, value) {
                                filters.push(value + (filterVal === "main" ? " eq null" : " ne null"));
                            });
                        }
                    }
                }

                // filter by CreatedAt From
                var createdAtFromFilter = $("#" + this.name + "_filter_version-date-from");
                if (createdAtFromFilter != null) {
                    var filterVal = createdAtFromFilter.val();
                    if (filterVal.length > 0) {
                        filterVal = getDateValue(filterVal, 00);
                        var filterColumns = createdAtFromFilter.attr("data-filter");
                        if (filterColumns.length > 0) {
                            $.each(filterColumns.toString().split(";"), function (index, value) {
                                filters.push(value + " ge DateTime'" + filterVal + "'");
                            });
                        }
                    }
                }

                // filter by CreatedAt To
                var createdAtToFilter = $("#" + this.name + "_filter_version-date-to");
                if (createdAtToFilter != null) {
                    var filterVal = createdAtToFilter.val();
                    if (filterVal.length > 0) {                        
                        filterVal = getDateValue(filterVal, 59);
                        var filterColumns = createdAtToFilter.attr("data-filter");
                        if (filterColumns.length > 0) {
                            $.each(filterColumns.toString().split(";"), function (index, value) {
                                filters.push(value + " le DateTime'" + filterVal + "'");
                            });
                        }
                    }
                }

                // filter by CreatedBy
                var createdByFilter = $("#" + this.name + "_filter_created-by");
                if (createdByFilter != null) {
                    var filterVal = createdByFilter.val();
                    if (filterVal.length > 0) {
                        var filterColumns = createdByFilter.attr("data-filter");
                        if (filterColumns.length > 0) {
                            $.each(filterColumns.toString().split(";"), function (index, value) {
                                filters.push("substringof('" + filterVal + "', " + value + ") eq true");
                            });
                        }
                    }
                }                
        
                return filters;
            },
            rowInfo: function () { return $("#" + this.name + "_rowInfo"); },
            rowInfoClear: function () { return $("#" + this.name + "_rowInfoClear"); },
            rowCheckBoxName: function () { return this.name + "_id[]"; },
            UnCheckedRows: function () { return $('#' + this.name + ' tbody input[name="' + this.rowCheckBoxName() + '"]:not(:checked)'); },
            CheckedRows: function () { return $('#' + this.name + ' tbody input[name="' + this.rowCheckBoxName() + '"]:checked'); },
            startLoadingBlock: function () {
                Metronic.blockUI({
                    target: "#" + table.name,
                    boxed: true,
                    message: TableLocalizations.loadingMessage
                });
            },
            stopLoadingBlock: function () { Metronic.unblockUI("#" + this.name); },
            filterVersionNo: function () { return $("#" + this.name + "_filter_version_no"); },
            selectAllCheckBox: function () { return $('#' + this.name + ' thead input[name="' + this.name + '_select_all"]'); }
        };

        var getDateValue = function(val, sec) {
            var dateFormat = "{yyyy}-{mm}-{dd}T{hh}:{ii}:" + minCharacter(sec, 2);
            var datetimes = val.toString().split(" - ");
            var shortDateArray = GlobalConfig.ShortDatePattern.toLowerCase().split(GlobalConfig.DateSeparator);
            var shortTimeArray = GlobalConfig.ShortTimePattern.toLowerCase().split(GlobalConfig.TimeSeparator);
            for (var i = 0; i < datetimes.length; i++) {
                var datetime = datetimes[i];
                if (i == 0) {
                    var dateValues = datetime.split(GlobalConfig.DateSeparator);
                    for (var j = 0; j < shortDateArray.length; j++) {
                        var pattern = shortDateArray[j];
                        var dateValue = dateValues[j];

                        if (pattern.indexOf("d") > -1)
                            dateFormat = dateFormat.replace("{dd}", minCharacter(dateValue, 2));
                        else if (pattern.indexOf("m") > -1)
                            dateFormat = dateFormat.replace("{mm}", minCharacter(dateValue, 2));
                        else if (pattern.indexOf("y") > -1)
                            dateFormat = dateFormat.replace("{yyyy}", dateValue.length == 4 ? dateValue : "20" + dateValue);
                    }
                } else {
                    var timeValues = datetime.split(GlobalConfig.TimeSeparator);
                    for (var j = 0; j < shortTimeArray.length; j++) {
                        var pattern = shortTimeArray[j];
                        var timeValue = timeValues[j];

                        if (pattern.indexOf("h") > -1)
                            dateFormat = dateFormat.replace("{hh}", minCharacter(timeValue, 2));
                        else if (pattern.indexOf("m") > -1)
                            dateFormat = dateFormat.replace("{ii}", minCharacter(timeValue, 2));
                    }
                }
            }
            return dateFormat;
        };

        var minCharacter = function (val, length) {
            while (val.toString().length < length) {
                val = "0" + val;
            }
            return val;
        };

        var toggleSelectedRowCount = function () {
            var paginate = $('#' + table.name + '_paginate');
            var length = $('#' + table.name + '_length');
            var info = $('#' + table.name + '_info');

            if (selectedRows.length > 0) {
                paginate.hide();
                length.hide();
                info.hide();

                var selectedRowInfo = "<span>" + TableLocalizations.rowInfoSelected.replace("_TotalCount_", selectedRows.length) + " | </span><a href=\"javascript:;\" id=\"" + table.name + "_rowInfoClear\">" + TableLocalizations.rowInfoClear + "</a>";

                var rowInfo = table.rowInfo();
                if (rowInfo.length) {
                    rowInfo.html(selectedRowInfo);
                    rowInfo.show();
                } else {
                    paginate.parent().append("<div id=\"" + table.name + "_rowInfo\" style=\"padding-top: 3px;\">" + selectedRowInfo + "</div>");
                }

                table.rowInfoClear().click(function () {
                    table.CheckedRows().trigger('click');
                    selectedRows = [];

                    paginate.show();
                    length.show();
                    info.show();

                    table.rowInfo().hide();
                });
            } else {
                paginate.show();
                length.show();
                info.show();

                table.rowInfo().hide();
            }

        };

        var grid = new Datatable();

        grid.init({
            src: table.get(),
            onSuccess: function (grid) {
                // execute some code after table records loaded
            },
            onError: function (grid) {
                // execute some code on network or other general error  
                table.stopLoadingBlock();
            },
            onDataLoad: function (grid) {
                // execute some code on ajax data load
                toggleSelectedRowCount();

                if (table.UnCheckedRows().length === 0) {
                    table.selectAllCheckBox().trigger('click');
                }
            },
            loadingMessage: TableLocalizations.loadingMessage,
            dataTable: { // here you can define a typical datatable settings from http://datatables.net/usage/options 

                // Uncomment below line("dom" parameter) to fix the dropdown overflow issue in the datatable cells. The default datatable layout
                // setup uses scrollable div(table-scrollable) with overflow:auto to enable vertical scroll(see: assets/global/scripts/datatable.js). 
                // So when dropdowns used the scrollable div should be removed. 
                //"dom": "<'row'<'col-md-8 col-sm-12'pli><'col-md-4 col-sm-12'<'table-group-actions pull-right'>>r>t<'row'<'col-md-8 col-sm-12'pli><'col-md-4 col-sm-12'>>",


                "bStateSave": false, // save datatable state(pagination, sort, etc) in cookie.
                "lengthMenu": [
                    [10, 20, 50, 100, 150, -1],
                    [10, 20, 50, 100, 150, 'All'] // change per page values here
                ],
                "pageLength": 10, // default record count per page
                "processing": false,
                "language": {
                    "processing": TableLocalizations.loadingMessage
                },
                "ajax": {
                    "url": "/api/" + tableConfig.EntityName + "/Versions/" + tableConfig.EntityId + "?f=dt&$inlinecount=allpages", // ajax source
                    "type": "GET",
                    "data": function (d) {
                        var params = new Object();
                        params.draw = d.draw;

                        var dTable = table.get().dataTable();
                        var dSettings = dTable.fnSettings();
                        
                        // orderby
                        var orderByParams = [];

                        //base orderby
                        orderByParams.push("MainId asc");

                        $.each(d.order, function (index, value) {
                            var orderedColumn = dSettings.aoColumns[value.column].data.replace(/\./g, '/');
                            if (orderedColumn === "Title") {
                                var titleFilter = $("#" + table.name + "_filter_title");
                                if (titleFilter != null) {                              
                                    var filterColumns = titleFilter.attr("data-filter");
                                    if (filterColumns.length > 0) {
                                        $.each(filterColumns.toString().split(";"), function (columnIndex, column) {
                                            var propName = column.toString().split(":")[0];
                                            orderByParams.push(propName + " " + value.dir);
                                        });
                                    }                                    
                                }
                            } else {
                                orderByParams.push(orderedColumn + " " + value.dir);
                            }
                        });

                        if (orderByParams.length > 0) {
                            var orderByQuery = "";

                            $.each(orderByParams, function (index, value) {
                                if (index > 0) {
                                    orderByQuery += ",";
                                }
                                orderByQuery += value;
                            });

                            params.$orderby = orderByQuery;
                        }

                        // skip top
                        if (d.length > 0) {
                            params.$skip = d.start;
                            params.$top = d.length;
                        }

                        // filter
                        var filters = table.searchQuery();

                        if (filters.length > 0) {
                            var filterQuery = "";

                            $.each(filters, function (index, value) {
                                if (index > 0) {
                                    filterQuery += " and ";
                                }
                                filterQuery += "(" + value + ")";
                            });

                            params.$filter = filterQuery;
                        }

                        return params;
                    }
                },
                "rowCallback": function (row, data) {
                    if ($.inArray(data.Id, selectedRows) !== -1) {
                        $(row).find('input[name="' + table.rowCheckBoxName() + '"]').prop('checked', true);
                    }
                },
                "columns": [
                    {
                        "data": "Id",
                        "render": function (data, type, row) {
                            return '<input type="checkbox" name="' + table.rowCheckBoxName() + '" value="' + data + '">';
                        }
                    },
                    {
                        "data": "Version",
                        "defaultContent": ""
                    },
                    {
                        "data": "Title",
                        "defaultContent": ""
                    },
                    {
                        "data": "MainId",
                        "orderable": false,
                        "render": function (data, type, row) {
                            return '<span class="label label-sm label-' + (data == null ? "success" : "info") + '">' + (data == null ? "Main" : "Version") + '</span>';
                        }
                    },
                    {
                        "data": "CreatedAt",
                        "defaultContent": "",
                        "type": "date",
                        "render": function (data, type, row) {
                            return moment(data).format(GlobalConfig.ShortDatePattern.toUpperCase() + " - " + GlobalConfig.LongTimePattern);
                        }
                    },
                    {
                        "data": "CreatedBy.UserName",
                        "defaultContent": ""
                    },
                    {
                        "data": "Id",
                        "orderable": false,
                        "visible": true,
                        "render": function (data, type, row) {
                            var actionButtons = '<a href="/Generic/Screen/Edit/' + data + '" class="btn btn-xs default btn-editable"><i class="fa fa-search"></i> ' + TableLocalizations.viewButton + '</a>';
                            if (row.MainId != null)
                                actionButtons += ' <a data-toggle="modal" data-target="#ajax" href="/Generic/Screen/More/' + tableConfig.EntityName + '/' + data + '" class="btn btn-xs grey-cascade btn-editable"><i class="fa fa-share"></i> ' + TableLocalizations.rollbackButton + '</a>';
                            return actionButtons;
                        }
                    }
                ],
                "order": [
                    [1, "desc"]
                ],// set first column as a default sort by asc
                "initComplete": function (settings, json) {

                }
            }
        });

        table.searchButton().click(function () {
            filterEnable = true;
            table.startLoadingBlock();
            table.get().DataTable().ajax.reload(function () { table.stopLoadingBlock(); }, true);
        });

        table.resetButton().click(function () {
            filterEnable = false;

            var tableObj = table.get();

            $('textarea.form-filter, select.form-filter, input.form-filter', tableObj).each(function () {
                $(this).val("");
            });

            $('input.form-filter[type="checkbox"]', tableObj).each(function () {
                $(this).attr("checked", false);
            });

            table.startLoadingBlock();
            tableObj.DataTable().ajax.reload(function () { table.stopLoadingBlock(); }, true);
        });

        table.filterVersionNo().keydown(function (e) {
            // Allow: backspace, delete, tab, escape, enter and .
            if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110]) !== -1 ||
                // Allow: Ctrl+A, Command+A
                (e.keyCode == 65 && (e.ctrlKey === true || e.metaKey === true)) ||
                // Allow: home, end, left, right, down, up
                (e.keyCode >= 35 && e.keyCode <= 40)) {
                // let it happen, don't do anything
                return;
            }
            // Ensure that it is a number and stop the keypress
            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                e.preventDefault();
            }
        });

        $(document).on("change", '[name="' + table.rowCheckBoxName() + '"]', function () {
            var index = $.inArray(this.value, selectedRows);
            if (/*this.checked && */index === -1) {
                selectedRows.push(this.value);
            } else /*if (!this.checked && index !== -1) */{
                selectedRows.splice(index, 1);
            }
            toggleSelectedRowCount();
        });

        table.selectAllCheckBox().on('click', function (e) {
            if (this.checked) {
                table.UnCheckedRows().trigger('click');
            } else {
                table.CheckedRows().trigger('click');
            }
        });

        table.deleteButton().click(function () {
            if (selectedRows.length > 0) {
                bootbox.dialog({
                    message: TableLocalizations.deleteConfirmMessage,
                    title: TableLocalizations.warning,
                    buttons: {
                        cancel: {
                            label: TableLocalizations.cancelText,
                            className: "btn-default",
                            callback: function () {

                            }
                        },
                        ok: {
                            label: TableLocalizations.okText,
                            className: "btn-primary",
                            callback: function () {

                                table.startLoadingBlock();

                                $.ajax({
                                    url: '/api/' + tableConfig.EntityName,
                                    type: 'DELETE',
                                    data: { "": selectedRows },
                                    success: function (result) {
                                        toastr["success"](selectedRows.length > 1 ? TableLocalizations.deletePluralSuccessMessage : TableLocalizations.deleteSingleSuccessMessage, TableLocalizations.success);
                                        table.get().DataTable().ajax.reload(function () { table.stopLoadingBlock(); }, false);
                                    },
                                    statusCode: {
                                        400: function (data) {
                                            var errors = "";
                                            $.each(data.responseJSON.ValidationResults, function (index, value) {
                                                errors += value.ErrorMessage + "<br />";
                                            });
                                            toastr["error"](errors, TableLocalizations.error);
                                            table.stopLoadingBlock();
                                        },
                                        401: function () {
                                            toastr["error"](TableLocalizations.unAuthorizedMessage, TableLocalizations.error);
                                            table.stopLoadingBlock();
                                        },
                                        403: function () {
                                            toastr["error"](TableLocalizations.forbiddenMessage, TableLocalizations.error);
                                            table.stopLoadingBlock();
                                        },
                                        404: function () {
                                            toastr["error"](TableLocalizations.notFoundMessage, TableLocalizations.error);
                                            table.stopLoadingBlock();
                                        },
                                        500: function (data) {
                                            toastr["error"](data, TableLocalizations.error);
                                            table.stopLoadingBlock();
                                        }
                                    }
                                });

                            }
                        }
                    }
                });

            } else {
                toastr["warning"](TableLocalizations.noRecordSelected, TableLocalizations.warning);
            }
        });
    }

    return {

        //main function to initiate the module
        init: function (tableConfig) {
            initPickers();
            handleRecords(tableConfig);
        }

    };

}();