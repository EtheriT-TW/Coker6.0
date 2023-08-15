function date_input_change(id) {
    var $self = $(`#${id}`);
    var start_date = $self.attr('date-date-strat-date'); 
    var end_date = $self.attr('data-date-end'); 
    var location = $self.attr('data-location'); 
    var addr = $self.attr('data-addr'); 
    var organizer = $self.attr('data-organizer');
    var aorganizer = $self.attr('data-a-organizer'); 
    var link = $self.attr('data-link'); 
    var tel = $self.attr('data-tel'); 

    $self.find(".activity_start_time").html(start_date);
    $self.find(".activity_end_time").html(end_date); 
    $self.find(".activity_location").html(location);
    $self.find(".activity_addr").html(addr); 
    $self.find(".activity-organizer").html(organizer); 
    $self.find(".activity-a-organizer").html(aorganizer); 
    $self.find(".activity_link").html(link);
    $self.find(".activity_tel").html(tel); 
    return $self.html();

}

function namecontrol(id) {
    console.log(id);
    const o = $("#"+id);
    const btn1 = o.find(".btn_text");
    const btn2 = o.find(".btn_grid");
    const btn3 = o.find(".btn_list");
    const $content = o.parents(".type_change_frame").first().find(".content").first();

    if (!btn1.hasClass("d-none") ) {
        typeChange(btn1, btn2, btn3, $content, "Text");
    } else if(btn1.hasClass("d-none") && !btn2.hasClass("d-none")){
        typeChange(btn2, btn3, btn1, $content, "Grid");
    }else if(btn1.hasClass("d-none") && btn2.hasClass("d-none") && !btn3.hasClass("d-none") ){
        typeChange(btn3, btn2, btn1, $content, "List");
    }
    
}


