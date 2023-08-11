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


