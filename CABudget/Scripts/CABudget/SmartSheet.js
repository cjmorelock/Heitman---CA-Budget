var SmartSheet = (function () {
    var subscribers = [];

    var cancel = function (e) {
        e.preventDefault();
        e.stopPropagation();
        return false;
    };

    var dragenter_file = function (e) {
        cancel(e);
    };

    var dragover_file = function (e) {
        cancel(e);
        this.classList.add("over");
    };

    var dragleave_file = function (e) {
        cancel(e);
        this.classList.remove("over");
    };

    var drop_file = function (e) {
        cancel(e);
        this.classList.remove("over");
        var types = e.dataTransfer.types;
        for (var i = 0; i < types.length; i++) {
            if (types[i] === "Files") {
                if (e.dataTransfer.files.length > 1) {
                    alert("Error! You can only drag one file in at a time.");
                } else {
                    refreshTable(e.dataTransfer.files[0]);
                }
            }
        }
    };

    var change_file = function (e) {
        cancel(e);

        var droppedFiles = this.files;
        if (droppedFiles) {
            refreshTable(droppedFiles[0]);
        }
    };

    var ajax_success = function (data, textStatus, jqXHR) {
        // data = {"isError" : <true|false>, "message" : <message>, "html" : <html>}
        $("#ssMessage").html(data.message);
        if (data.html) {
            $("#ssContainer").html(data.html);
            // DataTable jQuery plugin adds searching, paging, sorting to <table>s
            $("#ssContainer .data-table").DataTable({
                "ordering": false
            });
        }
    };

    var ajax_error = function (jqXHR, textStatus, errorThrown) {
        // Log the error, show an alert, whatever works for you
        $(".save-container").hide();
        $("#ssMessage").removeClass("alert-success").addClass("alert-danger");
        $("#ssMessage").html(textStatus + " : " + errorThrown);
        $("#ssContainer").html("");
    }

    var ajax_complete = function (jqXHR, textStatus) {
        $("#ssLoading").fadeOut(600);
        $("#ssContainer").fadeIn(1000);
        $("input[type='submit']").attr("disabled", false);
    }

    /* Call when file is selected/dropped to be uploaded. */
    var refreshTable = function (f) {
        // Internet Explorer's FormData object *only* provides an append() method, not the set() method Chrome provides.
        var $form = $("form"); // assume only 1 form
        //var ajaxData = new FormData($form.get(0));
        //ajaxData.set("ExcelSmartSheet", f);
        var ajaxData = new FormData();

        // add dropped/uploaded file
        ajaxData.append("BudgetFile", f);

        $("#ssContainer").hide();
        $("#ssLoading").show();
        $.ajax({
            url: $form.attr("action"),
            type: $form.attr("method"),
            data: ajaxData,
            //dataType: "json",
            cache: false,
            contentType: false,
            processData: false,
            complete: ajax_complete,
            success: function (data, textStatus, jqXHR) {
                // data = {"isError" : <true|false>, "message" : <message>, "html" : <html>}
                ajax_success(data, textStatus, jqXHR);

                if (data.isError) {
                    $(".save-container").hide(); // data did not validate, hide the Save button
                    $("#ssMessage").removeClass("alert-success").addClass("alert-danger");

                } else {
                    $(".save-container").show(); // data validation passed, show the Save button
                    $("#ssMessage").removeClass("alert-danger").addClass("alert-success");
                    notify();
                }
            },
            error: ajax_error
        });
    };
    
    var notify = function () {
        for (var i = 0; i < subscribers.length; i++) {
            subscribers[i].call(null); // subscriber function is called with no parameters. We're just notifying, not passing data
        }
    };

    return {
        Init: function () {
            // file upload management
            var dropzone = document.getElementById("ssDrop");
            dropzone.addEventListener("dragenter", dragenter_file, false);
            dropzone.addEventListener("dragover", dragover_file, false);
            dropzone.addEventListener("dragleave", dragleave_file, false);
            dropzone.addEventListener("drop", drop_file, false);

            var fileinput = document.getElementById("BudgetFile");
            fileinput.addEventListener("click", function () { this.value = null; }, false); // make sure when re-selecting the same file the onchange event handler fires!
            fileinput.addEventListener("change", change_file, false);

            // form submission
            $('form').on('submit', null, null, function (event) {
                cancel(event);
                var $form = $(this);

                $form.find("input[type='submit']").attr("disabled", true); // prevent double-click double-posting

                var ssData = $form.serialize(); // not sure I even need this

                // submit data - Called when the "Commit & Refresh SL Cube" button is pressed
                $.ajax({
                    url: $form.attr("action"),
                    type: $form.attr("method"),
                    data: ssData,
                    //dataType: "json",
                    cache: false,
                    complete: ajax_complete,
                    success: function (data, textStatus, jqXHR) {
                        // data = {"isError" : <true|false>, "message" : <message>, "html" : <html>}
                        ajax_success(data, textStatus, jqXHR);

                        if (data.isError) {
                            $(".save-container").show(); // commit failed, allow re-try later!
                            $("#ssMessage").removeClass("alert-success").addClass("alert-danger");
                        } else {
                            $(".save-container").hide(); // commit worked, hide the Save button
                            $("#ssMessage").removeClass("alert-danger").addClass("alert-success");
                        }
                    },
                    error: ajax_error
                });
            });

        },

        SubscribeTableRefresh: function (fn) {
            subscribers.push(fn);
        }
    };
})();