var tinyMCEmode = true;

function hideStatusMessage() {
    jQuery('#' + aspClientID + 'lblStatusMessage').delay(1000).fadeOut(3000); //.promise().done(function () { alert(jQuery('#MainContent_lblGroupStatusMessage').size()); })
}

function hideFormStatusMessage() {
    jQuery('#lblFormStatusMessage').fadeIn(1);
    jQuery('#lblFormStatusMessage').delay(5000).fadeOut(1000);
}

function redirect(object) {
    window.location = object;
}

// pop up window to confirm deletion or cancel of change/add
function ConfirmFormAction() {
    jQuery("#divConfirmAction").attr("title", "Confirm");
    jQuery("#divConfirmAction").dialog({
        resizable: false,
        height: 150,
        width: 200,
        modal: true,
        buttons: {
            "OK": function () {
                if (jQuery("#spnActionType").html() == "Cancel") {
                    jQuery(this).dialog("close");
                    jQuery("#divAddUpdate").dialog("close");
                } else {
                    Recid = jQuery('#recid').val();
                    //                            var Action = "delete";
                    jQuery(this).dialog("close");
                    strFields = BuildFormData('divAddUpdate', -1);
                    jQuery("#divAddUpdate").dialog("close");
                    jQuery.post(strPostDelete, { values: strFields },
                                function (data) {
                                    if (blnFromProjects == false) {
                                        getList();
                                    }
                                });
                }
                clearNewForm("divAddUpdate");
                // here we need to determineif here from projects if so return
                // this funtion is also built in the vb code
                if (blnFromProjects == true) {
                    setProjectReturnData();
                }
            },
            "Cancel": function () {
                jQuery(this).dialog("close");
            }
        }
    })
}

// pop up window to add a new
function AddNew(strFormTitle, frmHeight, frmWidth, strPostAdd, blnProjectReturnData) {
    AddNew(strFormTitle, frmHeight, frmWidth, strPostAdd, blnProjectReturnData, -1);
}
function AddNew(strFormTitle, frmHeight, frmWidth, strPostAdd, blnProjectReturnData, intProjectId) {
    jQuery("#divAddUpdate").attr("title", "Add New " + strFormTitle);
    if (intProjectId != -1) {
        jQuery("#project_id option").filter(function () {
            return jQuery(this).val() == intProjectId;
        }).attr('selected', true);
    };
//    ;
    // look to see if this has a user id & fill with session variable
    jQuery("#user_id option").filter(function () {
        return jQuery(this).text() == document.getElementById(aspClientID + "txtFilter").value;
    }).attr('selected', true);
    
    jQuery("#divAddUpdate").dialog({
        resizable: false,
        height: frmHeight,
        width: frmWidth,
        modal: true,
        buttons: {
            "Add": function () {
                var error = checkRequiredFields('divAddUpdate');
                if (error == false) {
                    //Get the new information
                    strFields = BuildFormData('divAddUpdate');
//                    alert(strFields);  //Uncomment for use when debugging: shows fields and values being sent to postFormUpdate.aspx for sqlString
                    jQuery.post(strPostAdd, { values: strFields },
                                function (data) {
                                    if (data.indexOf("error") == -1) {
                                        jQuery("#divAddUpdate").dialog("close");
                                        clearNewForm("divAddUpdate");
                                        if (blnProjectReturnData) {
                                            setProjectReturnData();
                                        };

                                        getList();
                                    } else {
                                        alert(data.replace("<span class=error>", "").replace("</span>", ""));
                                    };
                                    hideFormStatusMessage();
                                });
                }
            },
            "Cancel": function () {
                jQuery("#spnActionType").html("Cancel");
                ConfirmFormAction();
            }
        }
    });
}


function UpdateFormFromGrid(objName, strFormTitle, frmHeight, frmWidth, strPostUpdate, blnFromParent) {
    var Recid = jQuery(objName).closest('tr').attr('id');

    //Loop grid table row & prepare with data
    var arrTypes = g_FormObjectTypes.split('+');
    var String = ''
    var objTr = jQuery(objName).closest('tr');
    var arrSelectNames = Array();
    var arrSelectValues = Array();

    //loop all objects within the table row
    jQuery(objTr).find("[class*='cls_']").each(function () {
        arrDBname = this.className.split(' ');

        for (clsIndex = 0; clsIndex <= arrDBname.length - 1; clsIndex++) {
            if (arrDBname[clsIndex].indexOf("cls_") > -1) {
                DBname = arrDBname[clsIndex].replace('cls_', '');
                clsIndex = arrDBname.length;
            }
        }

        var FieldValue = this.innerHTML.replace('&amp;', '&').replace('&nbsp;',' ').replace('&quot;','"');

        //check dd box 
        if (jQuery('#' + DBname).attr('id')) {
            if (jQuery('#' + DBname).is('select')) {
                arrSelectNames.push(DBname);
                arrSelectValues.push(FieldValue);
            } else {
                jQuery('#' + DBname).val(FieldValue);
            }
        } else {
            jQuery('#' + aspClientID + DBname).val(FieldValue);
        }
        //}
    });

    //Set the selected value of the select fields
    for (var curSelect = 0; curSelect <= arrSelectNames.length - 1; curSelect++) {
        jQuery("#" + arrSelectNames[curSelect] + " option").filter(function () {
            return jQuery(this).text() == arrSelectValues[curSelect];
        }).attr('selected', true);
    }

    //Prepare the form
    jQuery("#divAddUpdate").attr("title", "Update " + strFormTitle);
    jQuery('#recid').val(Recid.trim());

    //Show the Update dialog
    jQuery("#divAddUpdate").dialog({
        resizable: false,
        height: frmHeight,
        width: frmWidth,
        modal: true,
        close: function () { clearNewForm("divAddUpdate"); },
        buttons: {
            "Update": function () {
                var error = checkRequiredFields('divAddUpdate')
                if (error == false) {
                    removeTinyMCE('formContent','all');
                    strFields = BuildFormData('divAddUpdate', -1);
                    var Action = "update";
                    jQuery(this).dialog("close");
                    jQuery.post(strPostUpdate, { values: strFields },
                                function (data) {
                                    if (blnFromParent == false) {
                                        getList();
                                    }
                                });
                    // here we need to determineif here from projects if so return
                    // this funtion is also built in the vb code
                    if (blnFromParent == true) {
                        setProjectReturnData();
                    }
                }
            },
            "Delete": function () {
                jQuery("#spnActionType").html("Delete");
                ConfirmFormAction();
            },
            "Cancel": function () {
                jQuery("#spnActionType").html("Cancel");
                ConfirmFormAction();
            }
        }
    });
}

function showUpdateDialog(frmHeight, frmWidth) {
    //Show the Update dialog
    jQuery("#divAddUpdate").dialog({
        resizable: false,
        height: frmHeight,
        width: frmWidth,
        modal: true,
        close: function () { clearNewForm("divAddUpdate"); hideStatusMessage(); },
        buttons: {
            "Update": function () {
                removeTinyMCE('formContent','all');
                var error = checkRequiredFields('divAddUpdate')

                if (error == false) {
                    strFields = BuildFormData('divAddUpdate', -1);
                    var Action = "update";
                    jQuery(this).dialog("close");
                    jQuery.post(strPostUpdate, { values: strFields },
                                function (data) {
                                    if (blnFromProjects == false) {
                                        getList();
                                    }
                                });
                    // here we need to determineif here from projects if so return
                    // this funtion is also built in the vb code
                    setProjectReturnData();
                }
            },
            "Delete": function () {
                jQuery("#spnActionType").html("Delete");
                ConfirmFormAction();
            },
            "Cancel": function () {
                jQuery("#spnActionType").html("Cancel");
                ConfirmFormAction();
            }
        }
    });
}

// clear all fields on the form
function clearNewForm(objName) {
    var arrTypes = g_FormObjectTypes.split('+');
    jQuery.each(arrTypes, function () {
        jQuery('#' + objName + ' ' + this).each(function () {
            if (GetFieldType(this) == 'checkbox' || GetFieldType(this) == 'radio') {
                //jQuery(this).attr('checked') = 'unchecked';
                this.checked = 'unchecked';
            } else if (jQuery(this).is('select')) {
                //clear selects--***set default of selects to zero somehow?
                this.selectedIndex = 0;
            }
            else {
                if (jQuery(this).attr('class') == 'db_num') {
                    //jQuery(this).val(0)
                    this.value = 0;
                }
                else {
                    //jQuery(this).val("")
                    this.value = "";
                }
            };
        });
    });
}

// verify required fields
function checkRequiredFields(objName) {
    var error = false;
    var arrTypes = g_FormObjectTypes.split('+');
    var strErrs = "";
    var delim = "";
    var firstErrFound = "";
    jQuery.each(arrTypes, function () {
        jQuery('#' + objName + ' ' + this + '.required').each(function () {
            if (((GetFieldType(this) == 'checkbox' || GetFieldType(this) == 'radio') && this.checked == 'unchecked') ||
                            (GetFieldType(this) == 'select' && this.selectedIndex == 0) ||
                            (this.value.replace(' ', '') == '')) {
                error = true;
                jQuery(this).addClass("requirederror");
                strErrs += delim + jQuery(this).attr('reqmsg');
                delim = ",";
                if (firstErrFound == "") {
                    firstErrFound = this.id;
                }
            }
            else {
                jQuery(this).removeClass("requirederror");
            }
        });
    });
    if (strErrs == "") {
    } else {
        alert('The following fields cannot be empty:\n\n' + strErrs);
        jQuery('#' + firstErrFound).focus();
    }
    return error;
}

// build list of fields from the form for the database insert and/or update
function BuildFormData(objName) {
    var arrTypes = g_FormObjectTypes.split('+');
    var String = ''
    jQuery.each(arrTypes, function () {
        jQuery('#' + objName + ' ' + this).each(function () {
            if (jQuery(this).hasClass('ignore') || jQuery(this).attr('id').indexOf('ignore') > 0 || jQuery(this).attr('name').indexOf('ignore') > 0) {
            } else {

                if (GetFieldType(this) == 'checkbox' || GetFieldType(this) == "radio") {
                    if (jQuery(this).attr('checked') == 'checked') {
                        String += jQuery(this).attr('name').replace(aspClientID, '') + "~~1||";
                    }
                    else {
                        String += jQuery(this).attr('name').replace(aspClientID, '') + "~~||";
                    }
                }
                else {
                    if (jQuery(this).val() == '') {
                        if (jQuery(this).hasClass('db_num')) {
                            String += jQuery(this).attr('id').replace(aspClientID, '') + "~~0||";
                        } else {
                            String += jQuery(this).attr('id').replace(aspClientID, '') + "~~ ||";
                        }
                    }
                    else {
                        if (jQuery(this).hasClass('db_char_upper')) {
                            String += jQuery(this).attr('id').replace(aspClientID, '') + "~~" + jQuery(this).val().toUpperCase() + "||";  //This is used to prevent errors when dealing with posting html code from a wysiwyg
                        }
                        else {
                            String += jQuery(this).attr('id').replace(aspClientID, '') + "~~" + jQuery(this).val() + "||";
                        }
                    }
                }
            }
        });
    });
    String = String.replace(/</g, '&lt;');  //Replaces a '<' with '&lt;' because ASP.NET throws danger 
    //error otherwise. This must be handled when coming back from the db.
   


    return String;
}

// determine the field type
function GetFieldType(objField) {
    if (jQuery(objField).is('select')) {
        return 'select';
    }
    else if (jQuery(objField).is('textarea')) {
        return 'textarea';
    }
    else {
        return jQuery(objField).attr('type');
    }
}

function addTinyMCE(objectID) {
    if (tinyMCEmode == false) {
        tinyMCE.init({
            mode: "exact",
            convert_fonts_to_spans: false,
            elements: objectID,
            theme: "advanced",
            theme_advanced_buttons1: "bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,|,formatselect,fontselect,fontsizeselect",
            theme_advanced_buttons2: "cut,copy,paste,pastetext,pasteword,|,search,replace,|,bullist,numlist,|,outdent,indent,blockquote,|,undo,redo,|,link,unlink,|,insertdate,inserttime,|,forecolor,backcolor"
        });
        tinyMCEmode = true;
    }
}

function removeTinyMCE(objectID, mode) {
    if (tinyMCEmode == true) {
        if (mode == 'all') {
            var arrTypes = g_FormObjectTypes.split('+');
            jQuery.each(arrTypes, function () {
                jQuery('#' + objectID + ' ' + this).each(function () {
                    if (jQuery(this).hasClass('wysiwyg')) {
                        //alert(jQuery(this).attr('id'));
                        var itemName = jQuery(this).attr('id');
                        tinyMCE.execCommand('mceFocus', false, itemName);
                        tinyMCE.execCommand('mceRemoveControl', false, itemName);
                    };
                });
            });
        } else {
            if (tinyMCEmode == true) {
                alert(this.id);
                alert('objectID: ' + objectID);
                alert('#' + objectID + '.val()');
                tinyMCE.execCommand('mceFocus', false, objectID);
                tinyMCE.execCommand('mceRemoveControl', false, objectID);
            }
        }
        tinyMCEmode = false;
    }
}