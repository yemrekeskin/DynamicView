var TableAjax = function () {

    var handleRecords = function () {

        var timeout;
        var delay = 200;
        var isLoading = false;
        var lastQuery = "";
        var entityName = "Screen";
        var isIVersionableEntity = true;
        var isIDraftEntity = true;

        var table = {
            name: "screen_table",
            get: function() { return $("#" + this.name); },
            filterSettingsKey: function () { return this.name + "_filterSettings_" + GlobalConfig.HashedValue; },
            quickSearch: function () { return $("#" + this.name + "_quickSearch"); },
            quickSearchQuery: function () {
                var searchTerm = $.trim(this.quickSearch().val());
                return searchTerm.length > 0
                    ? "(substringof('" + searchTerm + "', Name) eq true) or (substringof('" + searchTerm + "', Module/Name) eq true)"
                    : "";
            },
            columnToggler: function() { return $("#" + this.name + "_column_toggler"); },
            refreshButton: function () { return $("#" + this.name + "_refresh"); },
            deleteButton: function () { return $("#" + this.name + "_deleteButton"); },
            advancedSearchButton: function () { return $("#" + this.name + "_advanced"); },
            advancedSearchDropdownButton: function () { return $("#" + this.name + "_advanced_dropdown"); },
            advancedSearchItem: function () { return $("." + this.name + "_filter"); },
            advancedSearchClear: function () { return $("." + this.name + "_filter_remove"); },
            advancedSearchQuery: function () {
                var isAdvancedSearchTriggered = $("#" + this.name + "_advanced").hasClass("yellow");
                if (!isAdvancedSearchTriggered)
                    return "";
                /*
                    public enum PredicateCondition
                    { 
                        And = 1,
                        Or = 2
                    }

                    public enum FilterCondition
                    {
                        Contains = 1,
                        DoesNotContain = 2,
                        StartsWith = 3,
                        EndsWith = 4,
                        EqualTo = 5,
                        NotEqualTo = 6,
                        GreaterThan = 7,
                        LessThan = 8,
                        GreaterThanOrEqualTo = 9,
                        LessThanOrEqualTo = 10,
                        Between = 11,
                        NotBetween = 12,
                        IsEmpty = 13,
                        IsNotEmpty = 14,    
                        IsNull = 15,
                        IsNotNull = 16
                    }
                 */

                var filterScopeCriteria = function (predicateCondition, filterCriterias, filterScopeCriterias) {
                    var scopeCriteria = {
                        PredicateCondition: predicateCondition,
                        FilterCriterias: filterCriterias,
                        FilterScopeCriterias: filterScopeCriterias
                    };

                    return scopeCriteria;
                };

                var filterCriteria = function(propertyName, filterCondition, startValue, endValue) {
                    var criteria = {
                        PropertyName: propertyName,
                        FilterCondition: filterCondition,
                        StartValue: startValue,
                        EndValue: endValue
                    };

                    return criteria;
                };

                var searchCriterias = new filterScopeCriteria(2, null, [
                    new filterScopeCriteria(1, [
                        new filterCriteria("Title", 1, "Apple"),
                        new filterCriteria("UnitPrice", 11, 400, 700)                        
                    ], null),
                    new filterCriteria("AmountInStock", 7, 50)
                ]);

                //return "(substringof(Title, 'Apple') and (UnitPrice ge 400 and UnitPrice le 700)) or (AmountInStock gt 150)";
                return "substringof('Browse', Name) eq true";
            },
            rowCheckBoxName: function() { return this.name + "_id[]"; },
            startLoadingBlock: function () {
                Metronic.blockUI({
                    target: "#" + table.name,
                    boxed: true,
                    message: TableLocalizations.loadingMessage
                });
            },
            stopLoadingBlock: function () { Metronic.unblockUI("#" + table.name); }
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
            onDataLoad: function(grid) {
                // execute some code on ajax data load
                isLoading = false;
            },
            loadingMessage: TableLocalizations.loadingMessage,
            dataTable: { // here you can define a typical datatable settings from http://datatables.net/usage/options 

                // Uncomment below line("dom" parameter) to fix the dropdown overflow issue in the datatable cells. The default datatable layout
                // setup uses scrollable div(table-scrollable) with overflow:auto to enable vertical scroll(see: assets/global/scripts/datatable.js). 
                // So when dropdowns used the scrollable div should be removed. 
                //"dom": "<'row'<'col-md-8 col-sm-12'pli><'col-md-4 col-sm-12'<'table-group-actions pull-right'>>r>t<'row'<'col-md-8 col-sm-12'pli><'col-md-4 col-sm-12'>>",
               

                "bStateSave": GlobalConfig.IsAuthenticated, // save datatable state(pagination, sort, etc) in cookie.
                "stateSaveCallback": function (settings, data) {
                    // save the filter settings without connecting it to a unique url
                    localStorage.setItem(table.filterSettingsKey(), JSON.stringify(data));
                },
                "stateLoadCallback": function (settings) {
                    // read out the filter settings and apply
                    return JSON.parse(localStorage.getItem(table.filterSettingsKey()));
                },
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
                    "url": "/api/" + entityName + "?f=dt&$inlinecount=allpages", // ajax source
                    "type": "GET",
                    "data": function(d) {
                        var params = new Object();
                        params.draw = d.draw;
                    
                        var dTable = table.get().dataTable();
                        var dSettings = dTable.fnSettings();
   
                        // selectExpand
                        var select = [];
                        var expand = [];

                        $.each(dSettings.aoColumns, function (index, value) {
                            if (value.bVisible) {
                                var propName = value.data.replace(/\./g, '/');
                                if ($.inArray(propName, select) === -1) {
                                    select.push(propName);

                                    if (propName.indexOf("/") > -1) {
                                        var expandName = propName.substring(0, propName.indexOf("/"));
                                        if ($.inArray(expandName, expand) === -1) {
                                            expand.push(expandName);
                                        }
                                    }
                                }
                            }
                        });
                
                        if (select.length > 0)
                            params.$select = select.toString();

                        if (expand.length > 0)
                            params.$expand = expand.toString();

                        // orderby
                        var orderby = [];
                        
                        $.each(d.order, function(index, value) {
                            orderby.push(dSettings.aoColumns[value.column].data.replace(/\./g, '/') + " " + value.dir);
                        });

                        if (orderby.length > 0)
                            params.$orderby = orderby;

                        // skip top
                        if (d.length > 0) {
                            params.$skip = d.start;
                            params.$top = d.length;
                        }

                        // filter
                        var filters = [];

                        // baseFilter
                        if (isIVersionableEntity) {
                            filters.push("MainId eq null");
                        }

                        if (isIDraftEntity) {
                            filters.push("OriginalId eq null");
                        }

                        // quickSearch
                        var quickSearchQuery = table.quickSearchQuery();
                        if (quickSearchQuery.length > 0) {
                            filters.push(quickSearchQuery);
                        }

                        // advancedSearch
                        var advancedSearchQuery = table.advancedSearchQuery();
                        if (advancedSearchQuery.length > 0) {
                            filters.push(advancedSearchQuery);
                        }

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
                "columns": [
                    {
                        "data": "Id",
                        "render": function (data, type, row) {
                            return '<input type="checkbox" name="' + table.rowCheckBoxName() + '" value="' + data + '">';
                        }
                    },
                    {
                        "data": "Name",
                        "defaultContent": ""
                    },
                    {
                        "data": "Module.Name",
                        "defaultContent": ""
                    },
                    {
                        "data": "Id",
                        "orderable": false,
                        "visible": true,
                        "render": function (data, type, row) {
                            return '<a href="/Generic/Screen/Edit/' + data + '" class="btn btn-xs default btn-editable"><i class="fa fa-search"></i> ' + TableLocalizations.viewButton + '</a> <a data-toggle="modal" data-target="#ajax" href="/Generic/Screen/More/' + entityName + '/' + data + '?grid=' + table.name + '" class="btn btn-xs grey-cascade btn-editable"><i class="fa fa-share"></i> ' + TableLocalizations.moreButton + '</a>';
                        }
                    }
                ],
                "order": [
                    [1, "asc"]
                ],// set first column as a default sort by asc
                "initComplete": function (settings, json) {
                    $('input[type="checkbox"]', table.columnToggler()).each(function () {
                        var iCol = parseInt($(this).attr("data-column"));
                        var bVis = settings.aoColumns[iCol].bVisible;
                        if (bVis === false) {
                            $(this).attr("checked", false);
                            $(this).parent().removeClass("checked");
                        }
                    });
                }
            }
        });

        $('.portlet > .portlet-title .reload').tooltip({
            container: 'body',
            title: TableLocalizations.reload
        });

        table.refreshButton().click(function () {
            table.startLoadingBlock();
            table.get().DataTable().ajax.reload(function () { table.stopLoadingBlock(); }, false);
        });

        table.quickSearch().keyup(function () {
            var searchTerm = $.trim(table.quickSearch().val());
            if (searchTerm !== lastQuery) {
                lastQuery = searchTerm;
                
                if (timeout) {
                    clearTimeout(timeout);
                }

                if (!isLoading) {
                    timeout = setTimeout(function () {
                        isLoading = true;
                        table.get().DataTable().ajax.reload();
                    }, delay);
                }
            }
        });

        // handle show/hide columns
        $('input[type="checkbox"]', table.columnToggler()).change(function () {
            /* Get the DataTables object again - this is not a recreation, just a get of the object */
            var dTable = table.get().dataTable();
            var iCol = parseInt($(this).attr("data-column"));
            var bVis = dTable.fnSettings().aoColumns[iCol].bVisible;
            dTable.fnSetColumnVis(iCol, (bVis ? false : true));
        });

        table.deleteButton().click(function () {
            var checkedRows = $('input[name="' + table.rowCheckBoxName() + '"]:checked');
            if (checkedRows.length > 0) {
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

                                var allVals = [];
                                checkedRows.each(function () {
                                    allVals.push(this.value);
                                });

                                table.startLoadingBlock();

                                $.ajax({
                                    url: '/api/Screen',
                                    type: 'DELETE',
                                    data: { "": allVals },
                                    success: function (result) {
                                        toastr["success"](allVals.length > 1 ? TableLocalizations.deletePluralSuccessMessage : TableLocalizations.deleteSingleSuccessMessage, TableLocalizations.success);
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

        table.advancedSearchItem().click(function () {
            var advancedSearchButton = table.advancedSearchButton();
            advancedSearchButton.html("<i class=\"fa fa-filter\"></i> " + $(this).text());
            if (advancedSearchButton.hasClass("btn-info")) {
                advancedSearchButton.removeClass("btn-info").addClass("yellow");
   
                table.advancedSearchDropdownButton().removeClass("btn-info").addClass("yellow");
            }

            table.startLoadingBlock();
            table.get().DataTable().ajax.reload(function () { table.stopLoadingBlock(); }, false);
        });

        table.advancedSearchClear().click(function () {
            var advancedSearchButton = table.advancedSearchButton();
            if (advancedSearchButton.hasClass("yellow")) {
                advancedSearchButton.html("<i class=\"fa fa-search\"></i> " + TableLocalizations.advancedSearchText);
                advancedSearchButton.removeClass("yellow").addClass("btn-info");

                table.advancedSearchDropdownButton().removeClass("yellow").addClass("btn-info");

                table.startLoadingBlock();
                table.get().DataTable().ajax.reload(function () { table.stopLoadingBlock(); }, false);
            }
        });
    }

    return {

        //main function to initiate the module
        init: function () {
            handleRecords();
        }

    };

}();