using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DynamicView.Models;
using DynamicView.Attributes;
using Microsoft.AspNetCore.Http;

namespace DynamicView.Helpers
{
    public static class UIControlHelper
    {
        public static string CreateTable(FormMetaData formMetaData)
        {
            if(formMetaData.ListPageOutModel == null)
                return String.Empty;

            string tableControl = String.Empty;

            tableControl += @"var Table" + formMetaData.PropertyName + @"Ajax = function () {

            var handleRecords = function () {

                var timeout;
                var delay = 200;
                var isLoading = false;
                var lastQuery = """";
                var initComplete = false;

                var table = {
                    name: """ + string.Format("{0}_entity_table", formMetaData.PropertyName) + @""",
                    get: function () { return $(""#"" + this.name); },
                    filterSettingsKey: function () { return this.name + ""_filterSettings_"" + GlobalConfig.HashedValue; },
                    quickSearch: function () { return $(""#"" + this.name + ""_quickSearch""); },
                    quickSearchQuery: function () {                        
                        if (initComplete === false) {
                            return """";
                        }

                        var searchTerm = $.trim(this.quickSearch().val());
                        return searchTerm.length > 0
                            ? ""(substringof('"" + searchTerm + ""', Title) eq true)""
                            : """";
                    },
                    refreshButton: function () { return $(""#"" + this.name + ""_refresh""); },
                    deleteButton: function () { return $(""#"" + this.name + ""_deleteButton""); },
                    rowCheckBoxName: function () { return this.name + ""_id[]""; },
                    startLoadingBlock: function () {
                        Metronic.blockUI({
                            target: ""#"" + table.name,
                            boxed: true,
                            message: TableLocalizations.loadingMessage
                        });
                    },
                    stopLoadingBlock: function () { Metronic.unblockUI(""#"" + table.name); }
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
                        isLoading = false;
                    },
                    loadingMessage: TableLocalizations.loadingMessage,
                    dataTable: { // here you can define a typical datatable settings from http://datatables.net/usage/options

                        // Uncomment below line(""dom"" parameter) to fix the dropdown overflow issue in the datatable cells. The default datatable layout
                        // setup uses scrollable div(table-scrollable) with overflow:auto to enable vertical scroll(see: assets/global/scripts/datatable.js).
                        // So when dropdowns used the scrollable div should be removed.
                        //""dom"": ""<'row'<'col-md-8 col-sm-12'pli><'col-md-4 col-sm-12'<'table-group-actions pull-right'>>r>t<'row'<'col-md-8 col-sm-12'pli><'col-md-4 col-sm-12'>>"",


                        ""bStateSave"": GlobalConfig.IsAuthenticated, // save datatable state(pagination, sort, etc) in cookie.
                        ""stateSaveCallback"": function (settings, data) {
                            // save the filter settings without connecting it to a unique url
                            localStorage.setItem(table.filterSettingsKey(), JSON.stringify(data));
                        },
                        ""stateLoadCallback"": function (settings) {
                            // read out the filter settings and apply
                            return JSON.parse(localStorage.getItem(table.filterSettingsKey()));
                        },
                        ""lengthMenu"": [
                            [10, 20, 50, 100, 150, -1],
                            [10, 20, 50, 100, 150, 'All'] // change per page values here
                        ],
                        ""pageLength"": 10, // default record count per page
                        ""processing"": false,
                        ""language"": {
                            ""processing"": TableLocalizations.loadingMessage
                        },
                        ""ajax"": {
                            ""url"": """ + formMetaData.ListPageOutModel.MetaData.DataUrl + @"?f=dt&$inlinecount=allpages"", // ajax source
                            ""type"": ""GET"",
                            ""data"": function (d) {
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

                                            if (propName.indexOf(""/"") > -1) {
                                                var expandName = propName.substring(0, propName.indexOf(""/""));
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

                                $.each(d.order, function (index, value) {
                                    orderby.push(dSettings.aoColumns[value.column].data.replace(/\./g, '/') + "" "" + value.dir);
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
                                " + (string.IsNullOrEmpty(formMetaData.ListPageOutModel.MetaData.RelatedWith) ? "" : "filters.push(\"" + formMetaData.ListPageOutModel.MetaData.RelatedWith + " eq guid'" + HttpContext.Current.Request.RequestContext.RouteData.Values["id"] + "'\");") + @"

                                // quickSearch
                                var quickSearchQuery = table.quickSearchQuery();
                                if (quickSearchQuery.length > 0) {
                                    filters.push(quickSearchQuery);
                                }

                                if (filters.length > 0) {
                                    var filterQuery = """";

                                    $.each(filters, function (index, value) {
                                        if (index > 0) {
                                            filterQuery += "" and "";
                                        }
                                        filterQuery += ""("" + value + "")"";
                                    });

                                    params.$filter = filterQuery;
                                }

                                return params;
                            }
                        },
                        ""columns"": [
                            {
                                ""data"": ""Id"",
                                ""render"": function (data, type, row) {
                                    return '<input type=""checkbox"" name=""' + table.rowCheckBoxName() + '"" value=""' + data + '"">';
                                }
                            },
                            " + string.Join(",", formMetaData.ListPageOutModel.MetaData.ColumnMetaDatas.Where(c => c.ColumnName != "Id").Select(c => string.Format("{{\"data\":\"{0}\",\"orderable\":{1},\"defaultContent\":\"\"{2}{3}}}", c.ColumnName, c.Orderable.ToString().ToLower(), (string.IsNullOrEmpty(c.ColumnType) ? "" : string.Format(", \"type\":\"{0}\"", c.ColumnType)), (string.IsNullOrEmpty(c.Renderer) ? "" : string.Format(", \"render\": function (data, type, row) {{{0}}}", c.Renderer))))) + @"
                            ,
                            {
                                ""data"": ""Id"",
                                ""orderable"": false,
                                ""visible"": true,
                                ""render"": function (data, type, row) {
                                    return '<a href=""" + formMetaData.ListPageOutModel.MetaData.DetailPageUrl + @"' + data + '" + (string.IsNullOrEmpty(formMetaData.ListPageOutModel.MetaData.RelatedWith) ? "" : string.Format("?Relation={0}&RelationId={1}", formMetaData.ListPageOutModel.MetaData.RelationName, HttpContext.Current.Request.RequestContext.RouteData.Values["id"])) + @""" class=""btn btn-xs default btn-editable""><i class=""fa fa-search""></i> ' + TableLocalizations.viewButton + '</a>';
                                }
                            }
                        ],
                        ""order"": [
                            [1, ""asc""]
                        ],// set first column as a default sort by asc
                        ""initComplete"": function (settings, json) {
                            initComplete = true;
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

                table.deleteButton().click(function () {
                    var checkedRows = $('input[name=""' + table.rowCheckBoxName() + '""]:checked');
                    if (checkedRows.length > 0) {
                        bootbox.dialog({
                            message: TableLocalizations.deleteConfirmMessage,
                            title: TableLocalizations.warning,
                            buttons: {
                                cancel: {
                                    label: TableLocalizations.cancelText,
                                    className: ""btn-default"",
                                    callback: function () {

                                    }
                                },
                                ok: {
                                    label: TableLocalizations.okText,
                                    className: ""btn-primary"",
                                    callback: function () {

                                        var allVals = [];
                                        checkedRows.each(function () {
                                            allVals.push(this.value);
                                        });

                                        table.startLoadingBlock();

                                        $.ajax({
                                            url: '" + formMetaData.ListPageOutModel.MetaData.DeleteUrl + @"',
                                            type: 'DELETE',
                                            data: { """": allVals },
                                            success: function (result) {
                                                toastr[""success""](allVals.length > 1 ? TableLocalizations.deletePluralSuccessMessage : TableLocalizations.deleteSingleSuccessMessage, TableLocalizations.success);
                                                table.get().DataTable().ajax.reload(function () { table.stopLoadingBlock(); }, false);
                                            },
                                            statusCode: {
                                                400: function (data) {
                                                    var errors = """";
                                                    $.each(data.responseJSON.ValidationResults, function (index, value) {
                                                        errors += value.ErrorMessage + ""<br />"";
                                                    });
                                                    toastr[""error""](errors, TableLocalizations.error);
                                                    table.stopLoadingBlock();
                                                },
                                                401: function () {
                                                    toastr[""error""](TableLocalizations.unAuthorizedMessage, TableLocalizations.error);
                                                    table.stopLoadingBlock();
                                                },
                                                403: function () {
                                                    toastr[""error""](TableLocalizations.forbiddenMessage, TableLocalizations.error);
                                                    table.stopLoadingBlock();
                                                },
                                                404: function () {
                                                    toastr[""error""](TableLocalizations.notFoundMessage, TableLocalizations.error);
                                                    table.stopLoadingBlock();
                                                },
                                                500: function (data) {
                                                    toastr[""error""](data, TableLocalizations.error);
                                                    table.stopLoadingBlock();
                                                }
                                            }
                                        });

                                    }
                                }
                            }
                        });

                    } else {
                        toastr[""warning""](TableLocalizations.noRecordSelected, TableLocalizations.warning);
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

        Table" + formMetaData.PropertyName + @"Ajax.init(); // init table";

            return tableControl;
        }

        public static string CreateRules(List<FormMetaData> formMetaDatas)
        {
            StringBuilder rules = new StringBuilder();

            if (formMetaDatas == null)
                return rules.ToString();

            var excludedControls = new[]
            {
                UIFormInputType.None, 
                UIFormInputType.List, 
                UIFormInputType.ManyToMany, 
                UIFormInputType.Hidden
            };

            var formItems = formMetaDatas.Where(p => !excludedControls.Contains(p.UIFormInputType)).ToList();

            if(!formItems.Any())
                return rules.ToString();

            foreach (var formItem in formItems)
            {
                rules.Append(formItem.PropertyName);
                rules.Append(": {");

                if (formItem.MaxLength > 0 
                    && (formItem.UIFormInputType == UIFormInputType.Text
                        || formItem.UIFormInputType == UIFormInputType.TextArea
                        || formItem.UIFormInputType == UIFormInputType.DecimalField
                        || formItem.UIFormInputType == UIFormInputType.IntField))
                {
                    rules.Append(string.Format("maxlength: {0},", formItem.MaxLength));
                }

                if (formItem.UIFormInputType == UIFormInputType.PasswordConfirm)
                {
                    rules.Append(string.Format("equalTo: \"#{0}\",", formItem.PropertyName.Split('_')[0]));
                }

                if (formItem.UIFormInputType == UIFormInputType.DecimalField)
                {
                    rules.Append("number: true,");
                }

                if (formItem.UIFormInputType == UIFormInputType.IntField)
                {
                    rules.Append("digits: true,");
                }

                rules.Append(string.Format("required: {0}", formItem.IsRequired.ToString().ToLower()));
                rules.Append("},");
            }

            return rules.ToString().TrimEnd(',');
        }
    }
}