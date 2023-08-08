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

function namecontrol(isChecked) {
    console.log(isChecked);
    if (isChecked) {
        $(".checkbox_name").addClass("d-none");
    } else {
        $(".checkbox_name").removeClass("d-none");
    }
}


   
