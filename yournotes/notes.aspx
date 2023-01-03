<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="notes.aspx.cs"
    Inherits="_notes" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
    <style>
        .voce {
            padding-top: 3px;
            padding-bottom: 3px;
        }

        .voce-cut a {
            color: yellow !important;
        }

        .secondo:before {
            content: " ";
            border-bottom: 1px solid darkslategray;
            position: absolute;
            left: 0px;
            z-index: 1;
            width: 1000px;
            display: block;
        }

        .primo a {
            color: steelblue;
        }

        .secondo a {
            color: skyblue;
        }

        .terzo a {
            color: lightblue;
        }

        .quarto a {
            color: whitesmoke;
        }

        .task-cut {
            box-shadow: 6px 2px 4px 2px yellow;
        }
    </style>
    <script type="text/javascript" language="javascript">

        $(document).ready(function () {

            // synch
            if (__action_page && __action_page == "synch") {
                window.setTimeout(function () {
                    var res = post_action({ "action": "synch_folders", "user_id": __user_id, "user_name": __user_name });
                    $("#content").html($("#content").html() + (res.contents ? res.contents : ""));
                    if (res.data.err) $("#content").html($("#content").html() + "<span class='text-danger'>si &eacute; verificato un errore: " + res.data.err + "</span><br/>");
                    $("#content").html($("#content").html() + "<br/><br/><u>tempo di esecuzione: " + res.data.seconds.toString() + " secondi</u><br/>");
                }, 500);
                return;
            }

            if ($("#folder_id").val()) {
                window.setTimeout(function () {
                    $("#menu").scrollTop($("[tp-item='folder'][item-id='" + $("#folder_id").val() + "']").position().top + 100);
                }, 300);
            }

            // drag & drop attività
            $("html").on("dragover", function (e) {
                e.preventDefault(); e.stopPropagation();
            });
            $("html").on("drop", function (e) {
                e.preventDefault(); e.stopPropagation();
                try {
                    if (_drag_task_id) {
                        var fd = new FormData(), i = 0;
                        (e.originalEvent.dataTransfer.files).forEach(function (file) {
                            fd.append('task-file' + '_' + i + '_' + _drag_task_id, file); i++;
                        });
                        post_form_data(fd);
                        open_notes(_drag_task_id, true, true);
                    }
                } catch (e) { show_danger("Attenzione!", e.message); }
            });
        });

        // drag & drop attività
        var _drag_task_id = null;
        function drag_task_event(e, task_id, event) {
            try {
                var t = $("[task-id=" + task_id + "]")
                if (event == "over") { _drag_task_id = task_id; t.css("border-color", "Aquamarine").css("box-shadow", "3px 3px 3px Aquamarine"); e.preventDefault(); e.stopPropagation(); }
                else if (event == "leave") { _drag_task_id = null; t.css("border-color", "").css("box-shadow", ""); }
            } catch (e) { }
        }

        function add_att(task_id) {
            try {
                show_dyn_dlg({
                    title: "Aggiungi allegato", rows: [
                        { id: "name", icon: "file-alt", label: "Nome File" }]
                    , on_ok: function () {
                        if (!val_dyn("name")) return;
                        window.setTimeout(function () {
                            try {
                                var res = post_action({ "action": "add_att", "task_id": task_id, "name": val_dyn("name") });
                                if (res) { open_notes(task_id, true, true); hide_dyn_dlg(); }
                            } catch (e) { show_danger("Attenzione!", e.message); }
                        }, 500);
                    }
                });
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function task_state(task_id, assign_stato) {
            try {
                $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");
                window.setTimeout(function () {
                    try {
                        var t = $("[task-id=" + task_id + "]"), res = post_action({ "action": "task_state", "id": task_id, "stato": assign_stato });
                        if (res && res.html_element) {
                            var p = t.prev(); t.remove(); p.after(res.html_element);
                            t.css("border-color", "").css("box-shadow", "");
                        } else t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato");
                    } catch (e) { show_danger("Attenzione!", e.message); }
                }, 1500);
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function modify_task(task_id) {
            try {
                var t = $("[task-id=" + task_id + "]");
                show_dyn_dlg({
                    title: "Aggiorna Attivit\u00E0", rows: [
                        { id: "title", icon: "heading", label: "Titolo", valore: t.find("[tp-val='title']").text() }
                        , { id: "assegna", label: "Assegna a", valore: t.attr("task-assegna"), type: 'select', values: __task_assegna }
                        , { id: "priorita", label: "Priorit\u00E0", valore: t.attr("task-priorita"), type: 'select', values: __task_priorita }
                        , { id: "tipo", label: "Tipo", valore: t.attr("task-tipo"), type: 'select', values: __task_tipi }
                        , { id: "stima", label: "Stima", valore: t.attr("task-stima"), type: 'select', values: __task_stime }]
                    , on_ok: function () {

                        $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");

                        window.setTimeout(function () {
                            try {

                                var t = $("[task-id=" + task_id + "]"), res = post_action({
                                    "action": "update_task", "id": task_id, "title": val_dyn("title")
                                    , "assegna": val_dyn("assegna"), "priorita": val_dyn("priorita"), "tipo": val_dyn("tipo"), "stima": val_dyn("stima")
                                });
                                if (res && res.html_element) {
                                    hide_dyn_dlg();
                                    var p = t.prev(); t.remove(); p.after(res.html_element);
                                    t.css("border-color", "").css("box-shadow", "");
                                } else t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato");

                            } catch (e) { show_danger("Attenzione!", e.message); }
                        }, 1500);


                    }
                });
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function del_task(task_id) {
            show_warning_yn("Attenzione!", "Sei sicuro di voler eliminare l'attivit&aacute;?"
                , function () { remove_task(task_id) });
        }

        function remove_task(id) {
            try {
                var result = post_data({ "action": "remove_task", "id": id });
                if (result) {
                    if (result.des_result == "ok") window.location.reload();
                    else show_danger("Attenzione!", result.message);
                }
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function add_attivita(stato) {
            try {
                show_dyn_dlg({
                    title: "Aggiungi Attivit\u00E0", rows: [
                        { id: "title", icon: "heading", label: "Titolo", valore: "" }
                        , { id: "assegna", label: "Assegna a", valore: "", type: 'select', values: __task_assegna }
                        , { id: "priorita", label: "Priorit\u00E0", valore: "", type: 'select', values: __task_priorita }
                        , { id: "tipo", label: "Tipo", valore: "", type: 'select', values: __task_tipi }
                        , { id: "stima", label: "Stima", valore: "", type: 'select', values: __task_stime }]
                    , on_ok: function () {

                        window.setTimeout(function () {
                            try {
                                var res = post_action({
                                    "action": "add_task", "folder_id": get_param("id"), "synch_folder_id": get_param("sf"), "search_id": $("#search_id_active").val()
                                    , "stato": stato, "title": val_dyn("title"), "assegna": val_dyn("assegna"), "priorita": val_dyn("priorita"), "tipo": val_dyn("tipo"), "stima": val_dyn("stima")
                                });
                                if (res) window.location.reload();
                            } catch (e) { show_danger("Attenzione!", e.message); }
                        }, 1500);
                    }
                });
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function del_folder(id, tp_id) {
            show_danger_yn("Attenzione!",
                get_param("sf") || tp_id == "synch-folder" ? "Sei sicuro di voler cancellare il contenuto della cartella?"
                    : "Sei sicuro di voler cancellare la cartella e tutto il contenuto?"
                , function () { remove_folder(id, tp_id) });
        }

        function cut_element(id, tp_id, copy) {
            try {
                tp_id = tp_id ? tp_id : (get_param("sf") ? "synch-folder" : "folder");
                window.setTimeout(function () {
                    try {
                        var res = post_action({
                            "action": "cut_element", "tp_element": tp_id, "copy": copy ? true : false
                            , "element_id": id && (tp_id == "folder" || tp_id == "task" || tp_id == "att") ? id : get_param("id")
                            , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf")
                        });

                        if (res) {
                            var added = res.vars.added == "true";
                            if (tp_id == "folder" || tp_id == "synch-folder") {
                                $.each(res.list, function (index, value) {
                                    if (added) $("li[tp-item='folder'][item-id=" + value + "]").addClass("voce-cut");
                                    else $("li[tp-item='folder'][item-id=" + value + "]").removeClass("voce-cut");
                                });
                            } else if (tp_id == "task") {
                                if (added) $("[task-id=" + id + "]").addClass("task-cut");
                                else $("[task-id=" + id + "]").removeClass("task-cut");
                            }
                            else if (tp_id == "att") {
                                var tp_att = $("[tp=att-name-" + id + "]").attr("tp-att");
                                if (added) $("[tp=att-name-" + id + "]").removeClass(tp_att).addClass("badge-warning");
                                else $("[tp=att-name-" + id + "]").addClass(tp_att).removeClass("badge-warning");
                            }
                        }
                    } catch (e) { show_danger("Attenzione!", e.message); }
                }, 500);
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function paste_elements(id, tp_id) {
            try {
                tp_id = tp_id ? tp_id : "folder";
                window.setTimeout(function () {
                    try {
                        var res = post_action({
                            "action": "paste_elements", "folder_id": id && (tp_id == "folder" || tp_id == "task") ? id : get_param("id")
                            , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf"), "tp": tp_id
                        });
                        if (res) {
                            if (res.message)
                                show_danger("Nota Bene!", res.message, function () { window.location.reload(); });
                            else {
                                if (tp_id == "task") {
                                    open_notes(id, true, true);
                                    $("[tp-item='section-notes'][state=opened]").each(function () {
                                        var id2 = $(this).closest("[task-id]").attr("task-id");
                                        if (id2 != id) open_notes(id2, true, true);
                                    });
                                }
                                else window.location.reload();
                            }
                        }
                    } catch (e) { show_danger("Attenzione!", e.message); }
                }, 500);
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function remove_folder(id, tp_id) {
            try {
                window.setTimeout(function () {
                    try {
                        var res = post_action({
                            "action": "del_folder", "folder_id": id && tp_id == "folder" ? id : get_param("id")
                            , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf")
                        });
                        if (res) { if (tp_id) window.location.reload(); else window.location.href = res.contents; }
                    } catch (e) { show_danger("Attenzione!", e.message); }
                }, 500);
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function ren_folder(name, id, tp_id) {
            try {
                show_dyn_dlg({
                    title: "Rinomina la Cartella", rows: [
                        { id: "title", icon: "heading", label: "Titolo", valore: name ? name : $("[tp='name_folder']").text() }]
                    , on_ok: function () {
                        if (!val_dyn("title")) return;
                        window.setTimeout(function () {
                            try {
                                var res = post_action({
                                    "action": "ren_folder", "folder_id": id && tp_id == "folder" ? id : get_param("id")
                                    , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf"), "title": val_dyn("title")
                                });
                                if (res) window.location.reload();
                            } catch (e) { show_danger("Attenzione!", e.message); }
                        }, 500);
                    }
                });
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function add_folder(id, tp_id) {
            try {
                show_dyn_dlg({
                    title: "Aggiungi Cartella", rows: [
                        { id: "title", icon: "heading", label: "Titolo", valore: "" }]
                    , on_ok: function () {
                        if (!val_dyn("title")) return;
                        window.setTimeout(function () {
                            try {
                                var res = post_action({
                                    "action": "add_folder", "folder_id": id && tp_id == "folder" ? id : get_param("id")
                                    , "synch_folder_id": id && tp_id == "synch-folder" ? id : get_param("sf"), "title": val_dyn("title")
                                });
                                if (res) window.location.reload();
                            } catch (e) { show_danger("Attenzione!", e.message); }
                        }, 500);
                    }
                });
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function change_filter(filter_id) {
            try {
                var result = post_data({ "action": "set_filter_id", "filter_id": filter_id });
                if (result) {
                    if (result.des_result == "ok") window.location.reload();
                    else show_danger("Attenzione!", result.message);
                }
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function open_notes(task_id, force_open, force_read) {
            var sec = $("[task-id='" + task_id + "'] [tp-item='section-notes']"), ta = $("[task-id='" + task_id + "'] [tp-item='txt-notes']")
                , tf = $("[task-id='" + task_id + "'] [tp-item='section-allegati']"), btn = $("[task-id='" + task_id + "'] [tp-item='btn-notes']")
                , opened = sec.attr("state") == "opened";
            if (opened && !force_open) {
                sec.hide(350); tf.hide(350); sec.attr("state", "");
                if (ta.val()) btn.text("vedi note..."); else btn.text("aggiungi note...");
            }
            else {
                // read notes
                if (!sec.attr("readed") || force_read) {
                    try {
                        $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");
                        window.setTimeout(function () {
                            try {
                                var t = $("[task-id=" + task_id + "]"), ta = $("[task-id='" + task_id + "'] [tp-item='txt-notes']")
                                    , btn = $("[task-id='" + task_id + "'] [tp-item='btn-notes']");
                                var res = post_action({ "action": "get_details", "task_id": task_id, "search_id": $("#search_id_active").val() });
                                if (res) {
                                    ta.val(res.contents); sec.attr("readed", "1");
                                    if (!opened) { sec.show(350, "swing", function () { sec.attr("state", "opened"); ta.focus(); }); tf.show(350, "swing"); }
                                    if (res.html_element) tf.html(res.html_element); else tf.html("");
                                    t.css("border-color", "").css("box-shadow", "");
                                    btn.text("salva e nascondi le note...");
                                }
                                else t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato");
                            } catch (e) { show_danger("Attenzione!", e.message); }
                        }, 200);
                    } catch (e) { show_danger("Attenzione!", e.message); }
                }
                else {
                    var ta = $("[task-id='" + task_id + "'] [tp-item='txt-notes']")
                        , tf = $("[task-id='" + task_id + "'] [tp-item='section-allegati']")
                    if (!opened) { sec.show(350, "swing", function () { sec.attr("state", "opened"); ta.focus(); }); tf.show(350, "swing"); }
                    btn.text("salva e nascondi le note...");
                }
            }
        }

        var _notes = null;
        function focus_task_notes(task_id) {
            _notes = $("[task-id='" + task_id + "'] [tp-item='txt-notes']").val();
        }

        function blur_task_notes(task_id) {
            var notes_2 = $("[task-id='" + task_id + "'] [tp-item='txt-notes']").val();
            if (_notes != notes_2) { _notes = null; save_task_notes(task_id, notes_2); }
        }

        function save_task_notes(task_id, txt) {
            try {
                $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");
                window.setTimeout(function () {
                    var t = $("[task-id=" + task_id + "]"), ta = $("[task-id='" + task_id + "'] [tp-item='txt-notes']");
                    if (post_action({ "action": "save_task_notes", "task_id": task_id, "text": txt, "user_id": __user_id, "user_name": __user_name }))
                        t.css("border-color", "").css("box-shadow", "");
                    else
                        t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato");
                }, 200);

            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function show_menu(el) { $(el).find("[tp='menu-item']").show(); }

        function hide_menu(el) { $(el).find("[tp='menu-item']").hide(); }

        function title_task_focus(el) { if (!$(el).attr("err-title")) $(el).attr("bck-title", $(el).text()); }

        function title_task_blur(el) {
            if ($(el).attr("bck-title") == $(el).text())
                return;

            try {
                var task_id = $(el).closest("[task-id]").attr("task-id"), title = $(el).text();

                $("[task-id=" + task_id + "]").css("border-color", "lightgreen").css("box-shadow", "4px 4px 4px lightgreen");
                window.setTimeout(function () {
                    try {
                        var t = $("[task-id=" + task_id + "]"), res = post_action({ "action": "ren_task", "id": task_id, "title": title });
                        if (res) {
                            $(el).attr("bck-title", ""); $(el).attr("err-title", "");
                            t.css("border-color", "").css("box-shadow", "");
                        } else { t.css("border-color", "tomato").css("box-shadow", "3px 3px 3px tomato"); $(el).attr("err-title", "true"); }
                    } catch (e) { show_danger("Attenzione!", e.message); }
                }, 1500);
            } catch (e) { show_danger("Attenzione!", e.message); }

        }

        function open_att(file_id, user_id, user_name) {
            try {
                post_data({ "action": "client_cmd", "cmd": "open_att#file_id:" + file_id + "#user_id:" + user_id + "#user_name:" + user_name });
                return false;
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function del_att(file_id) {
            show_warning_yn("Attenzione!", "Sei sicuro di voler eliminare l'allegato?"
                , function () { remove_att(file_id) });
        }

        function remove_att(file_id) {
            try {
                var result = post_data({ "action": "remove_att", "id": file_id });
                if (result) {
                    if (result.des_result == "ok") $("*[tp='att-" + file_id + "'],*[tp='att-menu-" + file_id + "']").remove();
                    else show_danger("Attenzione!", result.message);
                }
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function modify_att(file_id) {
            try {
                show_dyn_dlg({
                    title: "Rinomina l'allegato", rows: [
                        { id: "name", icon: "heading", label: "Nome", valore: $("[tp='att-name-" + file_id + "']").text() }]
                    , on_ok: function () {
                        if (!val_dyn("name")) return;
                        window.setTimeout(function () {
                            try {
                                var res = post_action({
                                    "action": "ren_att", "file_id": file_id, "name": val_dyn("name")
                                });
                                if (res) { $("[tp='att-name-" + file_id + "']").text(val_dyn("name")); hide_dyn_dlg(); }
                            } catch (e) { show_danger("Attenzione!", e.message); }
                        }, 500);
                    }
                });
            } catch (e) { show_danger("Attenzione!", e.message); }
        }

        function del_search() { window.location = set_param("cmd", "tasks"); }

    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
    <input id='folder_id' type='hidden' runat='server' />
    <input id='search_id_active' type='hidden' runat='server' />
    <div class="container-fluid">
        <div class="row mb-4">
            <!-- sidebar menu -->
            <nav sidebar-tp='menu' class='d-none' sidebar-init='show'>
                <div id='menu' class='sidebar-sticky' runat='server'>
                </div>
            </nav>
            <!-- contenuti -->
            <div sidebar-tp='body'>
                <!-- content -->
                <div id='content' runat='server'>
                </div>
            </div>
            <!-- sidebar icon -->
            <div sidebar-tp='sh' onclick='sh_side_menu()'>
                <i sidebar-tp='icon'></i>
            </div>
        </div>
    </div>
</asp:Content>
