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

    var parenttime = $self.find(".activity_start_time").parents("div").eq(1);
    if (start_date == "" && end_date=="") {
        $(parenttime).addClass("d-none");
    } else {
        $(parenttime).removeClass("d-none"); 
    }
    var parentlocation = $self.find(".activity_location").parents("div").eq(0);
   
    if (location == "" ) {
        $(parentlocation).addClass("d-none");
    } else {
        $(parentlocation).find("a")
            .attr({ href: `https://www.google.com.tw/maps/place/${location}`, title: `連結至：${location}(另開新視窗)`, target: "_blank" });
        $(parentlocation).removeClass("d-none");
    }
    var parentaddr = $self.find(".activity_addr").parents("div").eq(0);
    
    if (addr == "") {
        $(parentaddr).addClass("d-none");
    } else {
        $(parentaddr).find("a").attr({ href: `https://www.google.com.tw/maps/place/${addr}`, title: `連結至：${addr}(另開新視窗)`, target: "_blank" });
        $(parentaddr).removeClass("d-none");
    }
    var parentorganizer = $self.find(".activity-organizer").parent("div").eq(0);
    if (organizer == "") {
        $(parentorganizer).addClass("d-none");
    } else {
        $(parentorganizer).removeClass("d-none");
    }
    
    var parentaorganizer = $self.find(".activity-a-organizer").parent("div").eq(0);
    if (aorganizer == "") {
        $(parentaorganizer).addClass("d-none");
    } else {
        $(parentaorganizer).removeClass("d-none");
    }
       
    var parentlink = $self.find(".activity_link").parents("div").eq(0);
    if (link == "") {
        $(parentlink).addClass("d-none");
    } else {
        $(parentlink).find("a").attr({ href: link, title: `連結至：活動網站(另開新視窗)`, target: "_blank" });
        $(parentlink).removeClass("d-none");
    }
    var parenttel = $self.find(".activity_tel").parents("div").eq(0);
    
    if (tel == "") {
        $(parenttel).addClass("d-none");
    } else {
        $(parenttel).find("a").attr({ href: `tel:${tel}`, title: `播打電話：${tel}`, target: "_blank" });
        $(parenttel).removeClass("d-none");
    }

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


