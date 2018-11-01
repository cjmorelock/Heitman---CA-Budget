var SmartSheet = (function () {
    var subscribers = [];

    var cancel = function (e) {
        e.preventDefault();
        e.stopPropagation();
        return false;
    }

    var dragenter_file = function (e) {
        cancel(e);
    }

    var dragover_file = function (e) {
        cancel(e);
        this.classList.add("over");
    }

    var dragleave_file = function (e) {
        cancel(e);
        this.classList.remove("over");
    }

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
    }

    var change_file = function (e) {
        cancel(e);

        var droppedFiles = this.files;
        if (droppedFiles) {
            refreshTable(droppedFiles[0]);
        }
    }

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
            complete: function () {
                $("#ssLoading").fadeOut(600);
                $("#ssContainer").fadeIn(1000);
            },
            success: function (data) {
                // data = {"isError" : <true|false>, "message" : <message>, "html" : <html>}
                $("#ssMessage").html(data.message);
                $("#ssContainer").html(data.html);
                // DataTable jQuery plugin adds searching, paging, sorting to <table>s
                $("#ssContainer .data-table").DataTable({
                    "ordering": false
                });
                if (data.isError) {
                    $(".save-container").hide(); // hide the Save button
                    $("#ssMessage").addClass("text-danger");

                } else {
                    $("#ssMessage").removeClass("text-danger");
                    $(".save-container").show();
                    notify();
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                // Log the error, show an alert, whatever works for you
                $(".save-container").hide();
                $("#ssMessage").addClass("text-danger");
                $("#ssMessage").html(textStatus + " : " + errorThrown);
                $("#ssContainer").html("");
            }
        });
    }
    
    var notify = function () {
        for (var i = 0; i < subscribers.length; i++) {
            subscribers[i].call(null); // subscriber function is called with no parameters. We're just notifying, not passing data
        }
    }

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

                var ssData = $form.serialize(); // not sure I even need this
                
                // submit data
                $.ajax({
                    url: $form.attr("action"),
                    type: $form.attr("method"),
                    data: ssData,
                    //dataType: "json",
                    cache: false,
                    complete: function () {
                        $("#ssLoading").fadeOut(600);
                        $("#ssContainer").fadeIn(1000);
                        $(".save-container").hide(); // hide the Save button
                    },
                    success: function (data) {
                        // data = {"isError" : <true|false>, "message" : <message>, "html" : <html>}
                        $("#ssMessage").html(data.message);
                        $("#ssContainer").html(data.html);
                        // DataTable jQuery plugin adds searching, paging, sorting to <table>s
                        $("#ssContainer .data-table").DataTable({
                            "ordering": false
                        });
                        if (data.isError) {
                            $("#ssMessage").addClass("text-danger");
                            $(".save-container").hide();
                        } else {
                            $("#ssMessage").removeClass("text-danger");
                            $(".save-container").show();
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        // Log the error, show an alert, whatever works for you
                        $("#ssMessage").html(textStatus + " : " + errorThrown);
                    }
                });
            });

        },

        SubscribeTableRefresh: function (fn) {
            subscribers.push(fn);
        }
    }
})();