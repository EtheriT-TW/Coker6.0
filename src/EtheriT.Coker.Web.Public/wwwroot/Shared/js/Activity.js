function input_change(id) {
    var $self = $(`#${id}`);
    console.log($self);
    var start_date = $self.attr('data-date-strat'); 
    var end_date = $self.attr('data-date-end'); 
    var location = $self.attr('data-location'); 
    var addr = $self.attr('data-addr'); 
    var link = $self.attr('data-link'); 
    var tel = $self.attr('data-tel'); 

    console.log("start"+start_date);

    $self.find(".activity_time").html(start_date + ' ' + end_date);
    $self.find(".activity_location").html(location);
    $self.find(".activity_addr").html(addr); 
    $self.find(".activity_link").html(link);
    $self.find(".activity_tel").html(tel); 
}

function namecontrol(id) {
    var o = $("#"+id);
    var ischeckedname = o.attr("checkbox_name");
    if (ischeckedname == 1 ) {
        $(".checkbox_name").addClass("d-none");
    } else if (ischeckedname == 0) {
        $(".checkbox_name").removeClass("d-none");
    }
}
function picturecontrol(id) {
    var o = $("#" + id);
    var ischeckedpictur = o.attr("checkbox_picture");
    if (ischeckedpictur == 1 ) {
        $(".checkbox_picture").addClass("d-none");
    } else if ( ischeckedpictur == 0 ) {
        $(".checkbox_picture").removeClass("d-none");
    }
}
function graphiccontrol(id) {
    var o = $("#" + id);
    var ischeckedgraphic = o.attr("checkbox_graphic");
    if ( ischeckedgraphic == 1) {
        $(".checkbox_graphic").addClass("d-none");
    } else if (ischeckedgraphic == 0) {
        $(".checkbox_graphic").removeClass("d-none");
    }
}
   
